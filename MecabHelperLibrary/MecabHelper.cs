using MeCab;
using System.Text.RegularExpressions;

namespace MecabHelperLibrary
{
    public partial class MecabHelper : IDisposable
    {
        private readonly MeCabTagger? Tagger;
        private bool disposedValue;

        public bool EnableMecab { get; protected set; }

        public MecabHelper(string dicPath)
        {
            try
            {
                Tagger = MeCabTagger.Create(
                    new MeCabParam()
                    {
                        DicDir = dicPath
                    }
                );
                EnableMecab = true;
            }
            catch
            {
                Tagger = null;
                EnableMecab = false;
            }
        }

        /// <summary>
        /// 处理句子，对句子进行分词，得到结果
        /// </summary>
        /// <param name="sentence"></param>
        /// <returns></returns>
        public List<MecabWordInfo> SentenceHandle(string sentence)
        {
            List<MecabWordInfo> ret = new();
            if (EnableMecab && Tagger != null)
            {
                foreach (var node in Tagger.ParseToNodes(sentence))
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
                    Tagger?.Dispose();
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
}
