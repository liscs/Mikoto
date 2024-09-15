using BenchmarkDotNet.Attributes;
using Mikoto.Mecab;

namespace Mikoto.Benchmark;

public class JapaneseRomajiConverterVs
{
    [Params("さっき")]
    public string Hiragana { get; set; } = string.Empty;

    [Benchmark]
    public string HiraganaToRomajiString()
        => JapaneseCharacterConverter.HiraganaToRomajiString(Hiragana);

    [Benchmark]
    public string HiraganaToRomajiString2()
        => JapaneseCharacterConverter.HiraganaToRomajiString(Hiragana);
}

