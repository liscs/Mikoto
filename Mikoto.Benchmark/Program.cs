using BenchmarkDotNet.Running;

namespace Mikoto.Benchmark;

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<JapaneseRomajiConverterVs>();
    }
}

