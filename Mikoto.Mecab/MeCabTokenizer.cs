using System.Text.RegularExpressions;

namespace Mikoto.Mecab;

public partial class MeCabTokenizer : IDisposable
{
    private readonly IMeCabTagger? _tagger;
    private bool disposedValue;

    public bool EnableMecab { get; protected set; }

    // 正常构造：生产使用
    public MeCabTokenizer(string dicPath)
    {
        try
        {
            _tagger = new MeCabTaggerWrapper(dicPath);
            EnableMecab = true;
        }
        catch
        {
            _tagger = null;
            EnableMecab = false;
        }
    }

    // 测试用构造：注入 mock
    public MeCabTokenizer(IMeCabTagger? mockTagger)
    {
        _tagger = mockTagger;
        EnableMecab = mockTagger != null;
    }

    /// <summary>
    /// 处理句子，对句子进行分词，得到结果
    /// </summary>
    /// <param name="sentence"></param>
    /// <returns></returns>
    public List<MecabWordInfo> Parse(string sentence)
    {
        List<MecabWordInfo> ret = [];
        if (EnableMecab && _tagger != null)
        {
            foreach (var node in _tagger.ParseToNodes(sentence))
            {
                if (node.Feature == null) { continue; }
                var features = CommaSeparateRegex().Split(node.Feature);
                MecabWordInfo mwi = new()
                {
                    Word = node.Surface,
                    PartOfSpeech = features[0],
                    Description = features[1],
                    Feature = node.Feature
                };

                if (features.Length >= 21 && mwi.PartOfSpeech != "補助記号" && mwi.PartOfSpeech != "空白")
                {
                    mwi.Katakana = features[20];
                    mwi.Hiragana = JapaneseCharacterConverter.KatakanaToHiraganaString(mwi.Katakana);
                    mwi.Romaji = JapaneseCharacterConverter.HiraganaToRomajiString(mwi.Hiragana);
                }

                ret.Add(mwi);
            }
        }
        else
        {
            ret.Add(new MecabWordInfo { Word = sentence });
        }
        return ret;
    }

    [GeneratedRegex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")]
    private static partial Regex CommaSeparateRegex();

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                _tagger?.Dispose();
            }
            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
