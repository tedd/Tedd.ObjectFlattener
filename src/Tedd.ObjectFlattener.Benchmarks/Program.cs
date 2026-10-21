extern alias TeddObjFlat;

using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using System.Text.Json;
using System.Collections.Generic;

namespace Tedd.ObjectFlattener.Benchmarks_App;

public class TestObject
{
    public string Name { get; set; } = "Test";
    public int Age { get; set; } = 30;
    public double Score { get; set; } = 99.9;
    public bool IsActive { get; set; } = true;
    public List<int> Numbers { get; set; } = new List<int> { 1, 2, 3, 4, 5 };
    public NestedObject Nested { get; set; } = new NestedObject();
}

public class NestedObject
{
    public string Description { get; set; } = "Nested";
    public int Value { get; set; } = 100;
}

[MemoryDiagnoser]
public class ObjectFlattenerBenchmarks
{
    private TestObject _testObject = null!;
    private Dictionary<string, string?> _flattenedDict = null!;

    [GlobalSetup]
    public void Setup()
    {
        _testObject = new TestObject();
        _flattenedDict = TeddObjFlat::Tedd.ObjectFlattener.Flatten(_testObject);
    }

    [Benchmark(Baseline = true)]
    public Dictionary<string, string?> Flatten_Archive()
    {
        return global::Tedd.ObjectFlattener.Archive.ObjectFlattener.Flatten(_testObject);
    }

    [Benchmark]
    public Dictionary<string, string?> Flatten_Optimized()
    {
        return TeddObjFlat::Tedd.ObjectFlattener.Flatten(_testObject);
    }

    [Benchmark]
    public TestObject? Unflatten_Archive()
    {
        return global::Tedd.ObjectFlattener.Archive.ObjectFlattener.Unflatten<TestObject>(_flattenedDict);
    }

    [Benchmark]
    public TestObject? Unflatten_Optimized()
    {
        return TeddObjFlat::Tedd.ObjectFlattener.Unflatten<TestObject>(_flattenedDict);
    }
}

class Program
{
    static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<ObjectFlattenerBenchmarks>();
    }
}
