using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace MecabHelperLibrary.Tests
{
    [TestClass()]
    public class JapaneseCharacterConverterTests
    {
        [TestMethod()]
        [DataRow("かっこ", "kakko")]
        [DataRow("きゃ", "kya")]
        [DataRow("きゅ", "kyu")]
        [DataRow("きょ", "kyo")]
        [DataRow("さっき", "sakki")]
        [DataRow("ふぁ", "fa")]
        [DataRow("ふぃ", "fi")]
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