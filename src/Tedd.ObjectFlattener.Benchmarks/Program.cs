using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using Tedd;
using Tedd.Archive;

[MemoryDiagnoser]
public class FlattenBenchmark
{
    public Root _testObj = default!;

    [GlobalSetup]
    public void Setup()
    {
        _testObj = new Root()
        {
            Status = TestStatus.InProgress,
            String = "Hello world",
            List = new List<string> { "First", "Second" },
            SubLevel1_1 = new SubLevel1
            {
                SubLevel2_1 = new SubLevel2
                {
                    Dict = new SortedDictionary<string, int> { { "First", 1 }, { "Second", 2 }, { "Third", 3 } },
                    List = new List<TestStatus> { TestStatus.Pending }
                },
                SubLevel2_2 = new SubLevel2
                {
                    Dict = new SortedDictionary<string, int> { { "Four", 4 }, { "Five", 5 } },
                    List = new List<TestStatus> { TestStatus.Pending, TestStatus.Completed }
                }
            },
            SubLevel1_2 = new SubLevel1
            {
                SubLevel2_1 = new SubLevel2 { Dict = null, List = null },
                SubLevel2_2 = new SubLevel2
                {
                    Dict = new SortedDictionary<string, int> { { "A", 10 }, { "B", 20 } },
                    List = new List<TestStatus> { TestStatus.Failed }
                }
            }
        };
    }

    [Benchmark(Baseline = true)]
    public Dictionary<string, string?> ArchiveFlatten()
    {
        return global::Tedd.Archive.ObjectFlattener.Flatten(_testObj);
    }

    [Benchmark]
    public Dictionary<string, string?> CurrentFlatten()
    {
        return global::Tedd.ObjectFlattener.Flatten(_testObj);
    }
}

public enum TestStatus { Pending, InProgress, Completed, Failed }
public class Root
{
    public TestStatus Status { get; set; }
    public string String { get; set; } = string.Empty;
    public List<string> List { get; set; } = new();
    public SubLevel1 SubLevel1_1 { get; set; } = new();
    public SubLevel1 SubLevel1_2 { get; set; } = new();
}
public class SubLevel1
{
    public SubLevel2 SubLevel2_1 { get; set; } = new();
    public SubLevel2 SubLevel2_2 { get; set; } = new();
}
public class SubLevel2
{
    public SortedDictionary<string, int>? Dict { get; set; } = new();
    public List<TestStatus>? List { get; set; } = new();
}

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<FlattenBenchmark>();
        BenchmarkRunner.Run<UnflattenBenchmark>();
    }
}

[MemoryDiagnoser]
public class UnflattenBenchmark
{
    public Dictionary<string, string?> _testDict = default!;

    [GlobalSetup]
    public void Setup()
    {
        var testObj = new Root()
        {
            Status = TestStatus.InProgress,
            String = "Hello world",
            List = new List<string> { "First", "Second" },
            SubLevel1_1 = new SubLevel1
            {
                SubLevel2_1 = new SubLevel2
                {
                    Dict = new SortedDictionary<string, int> { { "First", 1 }, { "Second", 2 }, { "Third", 3 } },
                    List = new List<TestStatus> { TestStatus.Pending }
                },
                SubLevel2_2 = new SubLevel2
                {
                    Dict = new SortedDictionary<string, int> { { "Four", 4 }, { "Five", 5 } },
                    List = new List<TestStatus> { TestStatus.Pending, TestStatus.Completed }
                }
            },
            SubLevel1_2 = new SubLevel1
            {
                SubLevel2_1 = new SubLevel2 { Dict = null, List = null },
                SubLevel2_2 = new SubLevel2
                {
                    Dict = new SortedDictionary<string, int> { { "A", 10 }, { "B", 20 } },
                    List = new List<TestStatus> { TestStatus.Failed }
                }
            }
        };
        _testDict = Tedd.Archive.ObjectFlattener.Flatten(testObj);
    }

    [Benchmark(Baseline = true)]
    public Root ArchiveUnflatten()
    {
        return Tedd.Archive.ObjectFlattener.Unflatten<Root>(_testDict!);
    }

    [Benchmark]
    public Root CurrentUnflatten()
    {
        return Tedd.ObjectFlattener.Unflatten<Root>(_testDict!);
    }
}
