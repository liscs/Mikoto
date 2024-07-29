using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MecabHelperLibrary.Tests
{
    [TestClass()]
    public class JapaneseCharacterConverterTests
    {
        [TestMethod()]
        [DataRow("かっこ", "kakko")]
        [DataRow("きゃ", "kya")]
        [DataRow("さっき", "sakki")]
        [DataRow("ふぁ", "fa")]
        [DataRow("まって", "matte")]
        [DataRow("ぜったい", "zettai")]
        [DataRow("あ", "a")]
        [DataRow("お", "o")]
        [DataRow("さようなら", "sayounara")]
        [DataRow("ありがとう", "arigatou")]
        [DataRow("はい", "hai")]
        [DataRow("ありがとう123", "arigatou123")]
        public void HiraganaToRomajiStringTest(string hiragana, string expectedRomaji)
        {
            string result = JapaneseCharacterConverter.HiraganaToRomajiString(hiragana);
            Assert.AreEqual(expectedRomaji, result);
        }



        [TestMethod()]
        [DataRow("アカ", "あか")]
        [DataRow("サシスセソ", "さしすせそ")]
        [DataRow("マメモ", "まめも")]
        [DataRow("ヤユヨ", "やゆよ")]
        [DataRow("ガギグゲゴ", "がぎぐげご")]
        public void KatakanaToHiraganaStringTest(string katakana, string expectedHiragana)
        {
            string result = JapaneseCharacterConverter.KatakanaToHiraganaString(katakana);
            Assert.AreEqual(expectedHiragana, result);
        }
    }
}