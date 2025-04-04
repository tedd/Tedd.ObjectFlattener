# Tedd.ObjectFlattener
Serialize complex objects into a key value format preserving hierarchy

# How it works

ObjectFlattener uses Newtonsoft.Json to serialize an object structure. Then it flattens the hierarchy into a Key-Value format.

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
	public string @String { get; set; }
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
	public SortedDictionary<string, int> Dict { get; set; } = new();
	public List<TestStatus> List { get; set; } = new();
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
SubLevel1_3:SubLevel2_1:Dict:First -> 1
SubLevel1_3:SubLevel2_1:Dict:Second -> 2
SubLevel1_3:SubLevel2_1:Dict:Third -> 3
SubLevel1_3:SubLevel2_1:List:0 -> 0
SubLevel1_3:SubLevel2_2:Dict:Five -> 5
SubLevel1_3:SubLevel2_2:Dict:Four -> 4
SubLevel1_3:SubLevel2_2:List:0 -> 0
SubLevel1_3:SubLevel2_2:List:1 -> 2

Are equal: True
```
