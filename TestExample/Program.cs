using System;
using System.Collections.Generic;
using System.Text.Json;
using Tedd;

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

public class Program
{
    public static void Main()
    {
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
    }
}
