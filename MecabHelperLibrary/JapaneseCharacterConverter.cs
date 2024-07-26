using System.Text;

namespace MecabHelperLibrary
{
    public class JapaneseCharacterConverter
    {
        public static string KatakanaToHiraganaString(string katakana)
        {
            StringBuilder sb = new();
            char[] target = katakana.ToCharArray();
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

        private static string HiraganaRomajiMap(string hiragana)
        {
            return hiragana switch
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
        public static string HiraganaToRomajiString(string hiragana)
        {
            string romaji = "";
            for (int i = 0; i < hiragana.Length; i++)
            {

                if (i + 1 < hiragana.Length)
                {
                    // 有「っ」的情况下
                    if (hiragana.Substring(i, 1).CompareTo("っ") == 0)
                    {
                        romaji += HiraganaRomajiMap(hiragana.Substring(i + 1, 1)).Substring(0, 1);
                        continue;
                    }

                    // 出现其他小假名的情况
                    string combineConvertResult = HiraganaRomajiMap(hiragana.Substring(i, 2));
                    if (combineConvertResult != "")
                    {
                        romaji += combineConvertResult;
                        i++;
                        continue;
                    }
                }
                romaji += HiraganaRomajiMap(hiragana.Substring(i, 1));
            }
            return romaji;
        }
    }
}
