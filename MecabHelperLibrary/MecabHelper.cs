using MeCab;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace MecabHelperLibrary
{
    public struct MecabWordInfo
    {

        /// <summary>
        /// 单词
        /// </summary>
        public string Word;

        /// <summary>
        /// 词性
        /// </summary>
        public string PartOfSpeech;

        /// <summary>
        /// 词性说明
        /// </summary>
        public string Description;

        /// <summary>
        /// 片假名
        /// </summary>
        public string Katakana;

        /// <summary>
        /// 平假名
        /// </summary>
        public string Hiragana;

        /// <summary>
        /// 罗马音
        /// </summary>
        public string Romaji;

        /// <summary>
        /// Mecab能提供的关于这个词的详细信息 CSV表示
        /// </summary>
        public string Feature;
    }

    public partial class MecabHelper : IDisposable
    {
        private readonly MeCabTagger? Tagger;

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

        public void Dispose()
        {
            Tagger?.Dispose();
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
                        mwi.Hiragana = KatakanaToHiragana(mwi.Katakana);
                        mwi.Romaji = HiraganaToAlphabet(mwi.Hiragana);
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

        static string KatakanaToHiragana(string s)
        {
            StringBuilder sb = new();
            char[] target = s.ToCharArray();
            char c;
            for (int i = 0; i < target.Length; i++)
            {
                c = target[i];
                if (c >= 'ァ' && c <= 'ヴ')
                { // 筛选片假名范围内的字符
                    c = (char)(c - 0x0060);  // 片假名转换为平假名
                }
                sb.Append(c);
            }
            return sb.ToString();
        }

        static string ConvertHiraganaToRomaji(string s)
        {
            return s switch
            {
                "あ" => "a",
                "い" => "i",
                "う" => "u",
                "え" => "e",
                "お" => "o",
                "か" => "ka",
                "き" => "ki",
                "く" => "ku",
                "け" => "ke",
                "こ" => "ko",
                "さ" => "sa",
                "し" => "shi",
                "す" => "su",
                "せ" => "se",
                "そ" => "so",
                "た" => "ta",
                "ち" => "chi",
                "つ" => "tsu",
                "て" => "te",
                "と" => "to",
                "な" => "na",
                "に" => "ni",
                "ぬ" => "nu",
                "ね" => "ne",
                "の" => "no",
                "は" => "ha",
                "ひ" => "hi",
                "ふ" => "hu",
                "へ" => "he",
                "ほ" => "ho",
                "ま" => "ma",
                "み" => "mi",
                "む" => "mu",
                "め" => "me",
                "も" => "mo",
                "や" => "ya",
                "ゆ" => "yu",
                "よ" => "yo",
                "ら" => "ra",
                "り" => "ri",
                "る" => "ru",
                "れ" => "re",
                "ろ" => "ro",
                "わ" => "wa",
                "を" => "wo",
                "ん" => "n",
                "が" => "ga",
                "ぎ" => "gi",
                "ぐ" => "gu",
                "げ" => "ge",
                "ご" => "go",
                "ざ" => "za",
                "じ" => "ji",
                "ず" => "zu",
                "ぜ" => "ze",
                "ぞ" => "zo",
                "だ" => "da",
                "ぢ" => "ji",
                "づ" => "du",
                "で" => "de",
                "ど" => "do",
                "ば" => "ba",
                "び" => "bi",
                "ぶ" => "bu",
                "べ" => "be",
                "ぼ" => "bo",
                "ぱ" => "pa",
                "ぴ" => "pi",
                "ぷ" => "pu",
                "ぺ" => "pe",
                "ぽ" => "po",
                "きゃ" => "kya",
                "きぃ" => "kyi",
                "きゅ" => "kyu",
                "きぇ" => "kye",
                "きょ" => "kyo",
                "しゃ" => "sha",
                "しぃ" => "syi",
                "しゅ" => "shu",
                "しぇ" => "she",
                "しょ" => "sho",
                "ちゃ" => "cha",
                "ちぃ" => "cyi",
                "ちゅ" => "chu",
                "ちぇ" => "che",
                "ちょ" => "cho",
                "にゃ" => "nya",
                "にぃ" => "nyi",
                "にゅ" => "nyu",
                "にぇ" => "nye",
                "にょ" => "nyo",
                "ひゃ" => "hya",
                "ひぃ" => "hyi",
                "ひゅ" => "hyu",
                "ひぇ" => "hye",
                "ひょ" => "hyo",
                "みゃ" => "mya",
                "みぃ" => "myi",
                "みゅ" => "myu",
                "みぇ" => "mye",
                "みょ" => "myo",
                "りゃ" => "rya",
                "りぃ" => "ryi",
                "りゅ" => "ryu",
                "りぇ" => "rye",
                "りょ" => "ryo",
                "ぎゃ" => "gya",
                "ぎぃ" => "gyi",
                "ぎゅ" => "gyu",
                "ぎぇ" => "gye",
                "ぎょ" => "gyo",
                "じゃ" => "ja",
                "じぃ" => "ji",
                "じゅ" => "ju",
                "じぇ" => "je",
                "じょ" => "jo",
                "ぢゃ" => "dya",
                "ぢぃ" => "dyi",
                "ぢゅ" => "dyu",
                "ぢぇ" => "dye",
                "ぢょ" => "dyo",
                "びゃ" => "bya",
                "びぃ" => "byi",
                "びゅ" => "byu",
                "びぇ" => "bye",
                "びょ" => "byo",
                "ぴゃ" => "pya",
                "ぴぃ" => "pyi",
                "ぴゅ" => "pyu",
                "ぴぇ" => "pye",
                "ぴょ" => "pyo",
                "ぐぁ" => "gwa",
                "ぐぃ" => "gwi",
                "ぐぅ" => "gwu",
                "ぐぇ" => "gwe",
                "ぐぉ" => "gwo",
                "つぁ" => "tsa",
                "つぃ" => "tsi",
                "つぇ" => "tse",
                "つぉ" => "tso",
                "ふぁ" => "fa",
                "ふぃ" => "fi",
                "ふぇ" => "fe",
                "ふぉ" => "fo",
                "うぁ" => "wha",
                "うぃ" => "whi",
                "うぅ" => "whu",
                "うぇ" => "whe",
                "うぉ" => "who",
                "ヴぁ" => "va",
                "ヴぃ" => "vi",
                "ヴ" => "vu",
                "ヴぇ" => "ve",
                "ヴぉ" => "vo",
                "でゃ" => "dha",
                "でぃ" => "dhi",
                "でゅ" => "dhu",
                "でぇ" => "dhe",
                "でょ" => "dho",
                "てゃ" => "tha",
                "てぃ" => "thi",
                "てゅ" => "thu",
                "てぇ" => "the",
                "てょ" => "tho",
                _ => "",
            };
        }

        //平假名转罗马音
        static string HiraganaToAlphabet(string kana)
        {
            string romaji = "";
            for (int i = 0; i < kana.Length; i++)
            {

                if (i + 1 < kana.Length)
                {
                    // 有「っ」的情况下
                    if (kana.Substring(i, 1).CompareTo("っ") == 0)
                    {
                        romaji += ConvertHiraganaToRomaji(kana.Substring(i + 1, 1)).Substring(0, 1);
                        continue;
                    }

                    // 出现其他小假名的情况
                    string combineConvertResult = ConvertHiraganaToRomaji(kana.Substring(i, 2));
                    if (combineConvertResult != "")
                    {
                        romaji += combineConvertResult;
                        i++;
                        continue;
                    }
                }
                romaji += ConvertHiraganaToRomaji(kana.Substring(i, 1));
            }
            return romaji;
        }

        [GeneratedRegex(",(?=(?:[^\"]*\"[^\"]*\")*[^\"]*$)")]
        private static partial Regex CommaSeparateRegex();
    }
}
