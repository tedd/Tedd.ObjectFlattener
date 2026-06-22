# Tedd.ObjectFlattener
Serialize complex objects into a key value format preserving hierarchy

# Architectural Execution Flow

The framework operates via a dual-serializer paradigm to ensure robust hierarchical traversal and structural reconstruction:

1. **Flattening (Serialization):** `System.Text.Json` is utilized to serialize the complex object structure into a JSON string, which is then parsed using `JsonDocument`. The resulting structure is deterministically traversed to produce a flat Key-Value format using a `:` separator.
2. **Unflattening (Deserialization):** The Key-Value pairs are mapped back into an intermediate JSON structure utilizing `Newtonsoft.Json.Linq` (`JObject`, `JArray`). The final deserialization into the strongly-typed target object is executed by `Newtonsoft.Json`.

### Clarification on Capabilities

* **Implemented Facts:** Hierarchical object flattening/unflattening, preservation of sequence order, and serialization of primitives, arrays, and objects.
* **Roadmap Hypotheses:** "Hierarchical data binding" and "routed event infrastructure" are currently theoretical constructs and are **not** present in the operational codebase. The framework strictly focuses on structural serialization without active runtime binding or event delegation.

# Example

## Simplified

```c#
// Serialization
var flattened = ObjectFlattener.Flatten(root); 

// Deserialization
var rootBack = ObjectFlattener.Unflatten<Root>(flattened); 
```

## Full sample

```c#
// Define some test objects
public enum TestStatus { Pending, InProgress, Completed, Failed }
public record Root
{
	public TestStatus Status { get; set; }
	public string @String { get; set; } = string.Empty;
	public List<string> List { get; set; } = new();
	public SubLevel1 SubLevel1_1 { get; set; } = new();
	public SubLevel1 SubLevel1_2 { get; set; } = new();
}
public record SubLevel1
{
	public SubLevel2 SubLevel2_1 { get; set; } = new();
	public SubLevel2 SubLevel2_2 { get; set; } = new();
}
public record SubLevel2
{
    // For easy comparison we need to preserve order, so using SortedDictionary
	public SortedDictionary<string, int>? Dict { get; set; } = new();
	public List<TestStatus>? List { get; set; } = new();
}

// Set up and populate test object
var root = new Root() 
{ 
  Status = TestStatus.InProgress,
  @String = "Hello world",
  List = ["First", "Second"],
  SubLevel1_1 = new() 
  { 
    SubLevel2_1 = new() { 
        Dict = new() { { "First", 1 }, { "Second", 2 }, { "Third", 3 } },
        List = [TestStatus.Pending] },
    SubLevel2_2 = new() { 
        Dict = new() { { "Four", 4 }, { "Five", 5 } },
        List = [TestStatus.Pending, TestStatus.Completed] }
  }, 
  SubLevel1_2 = new() 
  { 
    SubLevel2_1 = new() { Dict = null, List = null }, 
    SubLevel2_2 = new() {
        Dict = new() { { "A", 10 }, { "B", 20 } }, 
        List = [TestStatus.Failed] } 
  }
};

// Serialization
var flattened = ObjectFlattener.Flatten(root); 

// Dump debug
foreach (var kvp in flattened) 
  Console.WriteLine(kvp.Key + " -> " + kvp.Value); 

// Deserialization
var rootBack = ObjectFlattener.Unflatten<Root>(flattened); 

// Compare
var options = new JsonSerializerOptions { WriteIndented = true }; 
string jsonRoot = System.Text.Json.JsonSerializer.Serialize(root, options); 
string jsonRootBack = System.Text.Json.JsonSerializer.Serialize(rootBack, options); 
bool areEqual = jsonRoot == jsonRootBack; 
Console.WriteLine(); 
Console.WriteLine("Are equal: " + areEqual); 
```

### Outputs

```
Status -> 1
String -> Hello world
List:0 -> First
List:1 -> Second
SubLevel1_1:SubLevel2_1:Dict:First -> 1
SubLevel1_1:SubLevel2_1:Dict:Second -> 2
SubLevel1_1:SubLevel2_1:Dict:Third -> 3
SubLevel1_1:SubLevel2_1:List:0 -> 0
SubLevel1_1:SubLevel2_2:Dict:Five -> 5
SubLevel1_1:SubLevel2_2:Dict:Four -> 4
SubLevel1_1:SubLevel2_2:List:0 -> 0
SubLevel1_1:SubLevel2_2:List:1 -> 2
SubLevel1_2:SubLevel2_1:Dict -> 
SubLevel1_2:SubLevel2_1:List -> 
SubLevel1_2:SubLevel2_2:Dict:A -> 10
SubLevel1_2:SubLevel2_2:Dict:B -> 20
SubLevel1_2:SubLevel2_2:List:0 -> 3

Are equal: True
```
