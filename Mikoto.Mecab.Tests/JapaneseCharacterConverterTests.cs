using System.Collections.Generic;
using Xunit; 

namespace Mikoto.Mecab.Tests;

public class JapaneseCharacterConverterTests
{
    [Theory]
    [InlineData("かっこ", "kakko")]
    [InlineData("きゃ", "kya")]
    [InlineData("さっき", "sakki")]
    [InlineData("ふぁ", "fa")]
    [InlineData("まって", "matte")]
    [InlineData("ぜったい", "zettai")]
    [InlineData("あ", "a")]
    [InlineData("お", "o")]
    [InlineData("さようなら", "sayounara")]
    [InlineData("ありがとう", "arigatou")]
    [InlineData("はい", "hai")]
    [InlineData("ありがとう123", "arigatou123")]
    public void HiraganaToRomajiStringTest(string hiragana, string expectedRomaji)
    {
        string result = JapaneseCharacterConverter.HiraganaToRomajiString(hiragana);
        Assert.Equal(expectedRomaji, result);
    }


    [Theory]
    [InlineData("アカ", "あか")]
    [InlineData("サシスセソ", "さしすせそ")]
    [InlineData("マメモ", "まめも")]
    [InlineData("ヤユヨ", "やゆよ")]
    [InlineData("ガギグゲゴ", "がぎぐげご")]
    public void KatakanaToHiraganaStringTest(string katakana, string expectedHiragana)
    {
        string result = JapaneseCharacterConverter.KatakanaToHiraganaString(katakana);
        Assert.Equal(expectedHiragana, result);
    }
}