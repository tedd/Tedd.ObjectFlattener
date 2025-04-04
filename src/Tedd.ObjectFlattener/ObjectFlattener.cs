using Newtonsoft.Json.Linq;
using System.Globalization;
using System.Text.Json;

namespace Tedd;

/// <summary>
/// Provides static methods to flatten a C# object into a dictionary
/// and unflatten such a dictionary back into an object, using JSON as an intermediate format.
/// Uses System.Text.Json for Flattening, Newtonsoft.Json.Linq for intermediate structure building,
/// and **Newtonsoft.Json** for final Unflattening deserialization step.
/// </summary>
/// <remarks>
/// The Unflatten reconstruction uses heuristics to determine Array vs Object creation.
/// The reliance on Newtonsoft.Json's lenient final deserialization is key to handling potential
/// structural discrepancies arising from these heuristics.
/// </remarks>
public static class ObjectFlattener
{
    internal const char Separator = '|';

    /// <summary>
    /// Serializes an object to JSON (respecting provided System.Text.Json options, e.g., for enum conversion)
    /// and then flattens its structure into a dictionary using the '|' separator.
    /// </summary>
    public static Dictionary<string, string> Flatten<T>(T obj, JsonSerializerOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(obj);
        var flattenedDict = new Dictionary<string, string?>();
        try
        {
            var jsonString = JsonSerializer.Serialize(obj, options);
            using var jsonDocument = JsonDocument.Parse(jsonString);
            FlattenElement(jsonDocument.RootElement, string.Empty, flattenedDict);
        }
        catch (System.Text.Json.JsonException jsonEx)
        {
            throw new InvalidOperationException("System.Text.Json Exception during Flatten (Serialize/Parse step).", jsonEx);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unexpected error during Flatten's recursive element processing.", ex);
        }
        return flattenedDict;
    }

    /// <summary>
    /// Recursive helper method to traverse the JsonElement structure for Flattening.
    /// </summary>
    private static void FlattenElement(JsonElement element, string currentPath, Dictionary<string, string?> flattenedDict)
    {
        if (element.ValueKind == JsonValueKind.Object)
        {
            foreach (var property in element.EnumerateObject())
            {
                var newPath = string.IsNullOrEmpty(currentPath)
                    ? property.Name
                    : $"{currentPath}{Separator}{property.Name}";

                FlattenElement(property.Value, newPath, flattenedDict);
            }
        }
        else if (element.ValueKind == JsonValueKind.Array)
        {
            int index = 0;
            foreach (var item in element.EnumerateArray())
            {
                var newPath = $"{currentPath}{Separator}{index}";
                FlattenElement(item, newPath, flattenedDict);
                index++;
            }
        }
        else if (element.ValueKind == JsonValueKind.String)
            flattenedDict[currentPath] = element.GetString();
        else if (element.ValueKind is JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False)
            flattenedDict[currentPath] = element.GetRawText();
        else if (element.ValueKind == JsonValueKind.Null)
            flattenedDict[currentPath] = null;
    }

    /// <summary>
    /// Unflattens a dictionary (with pipe-separated keys '|') back into a target object structure of type T.
    /// Builds an intermediate JSON structure using Newtonsoft.Json.Linq.
    /// Uses **Newtonsoft.Json** for the final deserialization step.
    /// </summary>
    public static T Unflatten<T>(Dictionary<string, string> flattenedDict, JsonSerializerOptions? options = null) where T : class
    {
        ArgumentNullException.ThrowIfNull(flattenedDict);

        JToken? root = null;
        try
        {
            var orderedKeys = flattenedDict.Keys.OrderBy(k => k, new PathComparer());
            foreach (var key in orderedKeys)
            {
                var value = flattenedDict[key]; if (string.IsNullOrEmpty(key)) continue;

               var parts = key.Split(Separator);

                root ??= new JObject(); // Assume object root
                var currentNode = root;

                for (int i = 0; i < parts.Length - 1; i++)
                {
                    var currentPart = parts[i]; var nextPart = parts[i + 1];
                    if (currentNode is JObject currentObj)
                    {
                        JToken nextNode = currentObj[currentPart];
                        if (nextNode == null || nextNode.Type == JTokenType.Null)
                        {
                            nextNode = (nextPart == "0") ? (JToken)new JArray() : new JObject();
                            currentObj[currentPart] = nextNode;
                        }
                        // Conflict check 
                        bool nextAccessIsIndex = int.TryParse(nextPart, out _);
                        if (nextAccessIsIndex) { if (nextNode.Type != JTokenType.Array && nextNode.Type != JTokenType.Object) { throw new InvalidOperationException($"Path conflict(Obj-Idx): Key='{key}', Seg='{nextPart}', Path='{string.Join(Separator.ToString(), parts.Take(i + 1))}', FoundType={nextNode.Type}"); } }
                        else { if (nextNode.Type != JTokenType.Object) { throw new InvalidOperationException($"Path conflict(Obj-Prop): Key='{key}', Seg='{nextPart}', Path='{string.Join(Separator.ToString(), parts.Take(i + 1))}', FoundType={nextNode.Type}"); } }
                        currentNode = nextNode;
                    }
                    else if (currentNode is JArray currentArr)
                    {
                        if (!int.TryParse(currentPart, out int currentIndex) || currentIndex < 0) { throw new InvalidOperationException($"Invalid index '{currentPart}' in key '{key}'."); }
                        EnsureJArraySlot(currentArr, currentIndex); JToken elementNode = currentArr[currentIndex];
                        if (elementNode == null || elementNode.Type == JTokenType.Null)
                        {
                            bool nextNodeShouldBeArray = int.TryParse(nextPart, out _);
                            elementNode = nextNodeShouldBeArray ? (JToken)new JArray() : new JObject();
                            currentArr[currentIndex] = elementNode;
                        }
                        bool nextAccessIsIndex = int.TryParse(nextPart, out _);
                        if (nextAccessIsIndex && elementNode.Type != JTokenType.Array) { throw new InvalidOperationException($"Path conflict(Arr-Idx): Key='{key}', Idx={currentIndex}, NextSeg='{nextPart}', FoundType={elementNode.Type}"); }
                        else if (!nextAccessIsIndex && elementNode.Type != JTokenType.Object) { throw new InvalidOperationException($"Path conflict(Arr-Prop): Key='{key}', Idx={currentIndex}, NextSeg='{nextPart}', FoundType={elementNode.Type}"); }
                        currentNode = elementNode;
                    }
                    else { throw new InvalidOperationException($"Cannot navigate further in key '{key}' at '{string.Join(Separator.ToString(), parts.Take(i))}'. Type is {currentNode?.Type}."); }
                } // --- End Navigation Loop ---

                var finalPart = parts.Last(); JToken finalValue = ParseValueString(value);
                if (currentNode is JObject finalObj) { finalObj[finalPart] = finalValue; }
                else if (currentNode is JArray finalArr) { if (!int.TryParse(finalPart, out int finalIndex) || finalIndex < 0) { throw new InvalidOperationException($"Invalid final index '{finalPart}' in key '{key}'."); } EnsureJArraySlot(finalArr, finalIndex); finalArr[finalIndex] = finalValue; }
                else { throw new InvalidOperationException($"Cannot set final value for key '{key}'. Container is {currentNode?.Type}."); }
            } // --- End foreach key loop ---

            if (root == null) { return default(T); }

            string jsonString = root.ToString(Newtonsoft.Json.Formatting.None); // Intermediate JSON

            // --- FINAL DESERIALIZATION USING NEWTONSOFT.JSON ---
            try
            {
                var newtonsoftSettings = new Newtonsoft.Json.JsonSerializerSettings
                {
                    Converters = { new Newtonsoft.Json.Converters.StringEnumConverter() },
                };
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString, newtonsoftSettings);
            }
            catch (Newtonsoft.Json.JsonException newtonsoftEx)
            {
                string intermediateJsonPreview = root?.ToString(Newtonsoft.Json.Formatting.Indented) ?? "[Structure was null]";
                string targetTypeName = typeof(T).Name;
                throw new InvalidOperationException($"Newtonsoft.Json failed to deserialize intermediate JSON to '{targetTypeName}'. Intermediate JSON:\n---\n{intermediateJsonPreview}\n---", newtonsoftEx);
            } // --- END NEWTONSOFT DESERIALIZATION ---
        }
        catch (Exception ex)
        { // Catch errors during reconstruction
            throw new InvalidOperationException($"Unexpected error during unflattening structure reconstruction: {ex.Message}", ex);
        }
    }

    /// <summary> Parses the string value into an appropriate JToken primitive. </summary>
    private static JToken ParseValueString(string stringValue)
    { 
        if (stringValue == null) { return JValue.CreateNull(); }
        if (bool.TryParse(stringValue, out bool boolVal)) { return new JValue(boolVal); }
        if (long.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out long longVal)) { return new JValue(longVal); }
        if (decimal.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal decVal)) { return new JValue(decVal); }
        if (double.TryParse(stringValue, NumberStyles.Any, CultureInfo.InvariantCulture, out double dblVal) && !double.IsNaN(dblVal) && !double.IsInfinity(dblVal)) { return new JValue(dblVal); }
        return new JValue(stringValue);
    }

    /// <summary> Ensures a JArray has elements up to the specified index. </summary>
    private static void EnsureJArraySlot(JArray array, int index) { while (array.Count <= index) { array.Add(JValue.CreateNull()); } }

    /// <summary> Custom comparer for sorting flattened keys correctly.</summary>
    internal class PathComparer : IComparer<string>
    {
        public int Compare(string x, string y)
        {
            if (x == null && y == null) return 0; if (x == null) return -1; if (y == null) return 1;
            // *** Use updated Separator ***
            var partsX = x.Split(ObjectFlattener.Separator);
            var partsY = y.Split(ObjectFlattener.Separator);
            int len = Math.Min(partsX.Length, partsY.Length);
            for (int i = 0; i < len; i++)
            {
                bool isNumX = int.TryParse(partsX[i], out int numX);
                bool isNumY = int.TryParse(partsY[i], out int numY);
                if (isNumX && isNumY) { if (numX != numY) return numX.CompareTo(numY); }
                else { int stringCompare = String.Compare(partsX[i], partsY[i], StringComparison.Ordinal); if (stringCompare != 0) return stringCompare; }
            }
            return partsX.Length.CompareTo(partsY.Length);
        }
    }
}