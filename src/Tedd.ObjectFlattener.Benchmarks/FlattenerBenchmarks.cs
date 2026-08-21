using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Order;
using System.Collections.Generic;

namespace Tedd.Benchmarks;

[MemoryDiagnoser]
[Orderer(SummaryOrderPolicy.FastestToSlowest)]
[RankColumn]
public class FlattenerBenchmarks
{
    private List<string>? _keys;
    private Tedd.ObjectFlattener.PathComparer? _optimizedComparer;
    private Tedd.Archive.ObjectFlattenerArchive.PathComparer? _archiveComparer;

    [GlobalSetup]
    public void Setup()
    {
        _keys = new List<string>
        {
            "Status",
            "String",
            "List:0",
            "List:1",
            "SubLevel1_1:SubLevel2_1:Dict:First",
            "SubLevel1_1:SubLevel2_1:Dict:Second",
            "SubLevel1_1:SubLevel2_1:Dict:Third",
            "SubLevel1_1:SubLevel2_1:List:0",
            "SubLevel1_1:SubLevel2_2:Dict:Five",
            "SubLevel1_1:SubLevel2_2:Dict:Four",
            "SubLevel1_1:SubLevel2_2:List:0",
            "SubLevel1_1:SubLevel2_2:List:1",
            "SubLevel1_2:SubLevel2_1:Dict",
            "SubLevel1_2:SubLevel2_1:List",
            "SubLevel1_2:SubLevel2_2:Dict:A",
            "SubLevel1_2:SubLevel2_2:Dict:B",
            "SubLevel1_2:SubLevel2_2:List:0"
        };

        // Ensure random order to force sorting
        var rnd = new System.Random(42);
        for (int i = _keys.Count - 1; i > 0; i--)
        {
            int j = rnd.Next(i + 1);
            var temp = _keys[i];
            _keys[i] = _keys[j];
            _keys[j] = temp;
        }

        _optimizedComparer = new Tedd.ObjectFlattener.PathComparer();
        _archiveComparer = new Tedd.Archive.ObjectFlattenerArchive.PathComparer();
    }

    [Benchmark(Baseline = true)]
    public void ArchiveSort()
    {
        var list = new List<string>(_keys);
        list.Sort(_archiveComparer);
    }

    [Benchmark]
    public void OptimizedSort()
    {
        var list = new List<string>(_keys);
        list.Sort(_optimizedComparer);
    }
}
