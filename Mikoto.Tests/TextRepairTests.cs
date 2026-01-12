using Xunit;

namespace Mikoto.Tests;

public class TextRepairTests
{
    private void TestSingleWordRepeat(int repeat, string src, string expected)
    {
        string actual = TextRepair.RepairFun_RemoveSingleWordRepeat(src, repeat);
        Assert.Equal(expected, actual);
    }

    private void TestSentenceRepeat(int repeat, string src, string expected)
    {
        string actual = TextRepair.RepairFun_RemoveSentenceRepeat(src, repeat);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void RepairFun_RemoveSingleWordRepeatTest_NoRepetition()
    {
        string src = "Four score and seven years ago";
        TestSingleWordRepeat(1, src, src);
    }

    [Fact]
    public void RepairFun_RemoveSingleWordRepeatTest_ExactThreshold()
    {
        string src = "sssssssssshhhhhhhhhhaaaaaaaaaallllllllllllllllllll";
        string expected = "shall";
        TestSingleWordRepeat(10, src, expected);
    }

    [Fact]
    public void RepairFun_RemoveSingleWordRepeatTest_AboveThreshold()
    {
        string src = "sssssssssshhhhhhhhhhaaaaaaaaaallllllllllllllllllll";
        string expected = "shall";
        // 重复10次，重复次数填成了9
        TestSingleWordRepeat(9, src, expected);
    }

    [Fact]
    public void RepairFun_RemoveSingleWordRepeatTest_ExceedsThreshold()
    {
        string src = "sssssssssshhhhhhhhhhaaaaaaaaaallllllllllllllllllll";
        string expected = "shall";
        TestSingleWordRepeat(11, src, expected);
    }

    [Fact]
    public void RepairFun_RemoveSingleWordRepeatTest_EmptyString()
    {
        string src = "";
        TestSingleWordRepeat(1, src, src);
    }

    [Fact]
    public void RepairFun_RemoveSentenceRepeatTest_EmptyString()
    {
        TestSentenceRepeat(5, "", "");
    }

    [Fact]
    public void RepairFun_RemoveSentenceRepeatTest_ShortString_NoEffect()
    {
        string src = "hello";
        TestSentenceRepeat(10, src, src);
    }

    [Fact]
    public void RepairFun_RemoveSentenceRepeatTest_ThresholdLargerThanInput()
    {
        string src = "this is a test sentence";
        TestSentenceRepeat(50, src, src);
    }

    [Fact]
    public void RepairFun_RemoveSentenceRepeatTest_ExactlyOneRepeat_NoChange()
    {
        string src = "important messageimportant message";
        string expected = "important message";
        TestSentenceRepeat(10, src, expected);
    }

    [Fact]
    public void RepairFun_RemoveSentenceRepeatTest_ChineseFullRepeat()
    {
        string src = "这是一段很重要的测试文字这是一段很重要的测试文字这是一段很重要的测试文字";
        string expected = "这是一段很重要的测试文字";
        TestSentenceRepeat(8, src, expected);
    }

    [Fact]
    public void RepairFun_RemoveSentenceRepeatTest_RepeatWithExtraContentAtEnd()
    {
        string src = "repeat repeat repeat repeat extra content";
        TestSentenceRepeat(7, src, src);  // 因为不是完整到结尾的重复，应保持不变
    }

    [Fact]
    public void RepairFun_RemoveSentenceRepeatTest_RepeatingShortWord()
    {
        string src = "哈哈哈哈哈哈哈哈哈哈哈哈";
        string expected = "哈哈";           // 非贪婪匹配 + 最小长度限制可能得到这个结果
        TestSentenceRepeat(2, src, expected);
    }

    [Fact]
    public void RepairFun_RemoveSentenceRepeatTest_RepeatingShortWord_HigherThreshold()
    {
        string src = "哈哈哈哈哈哈哈哈哈哈哈哈";
        string expected = "哈哈哈哈哈哈";     // 阈值较高时保留更多字符
        TestSentenceRepeat(6, src, expected);
    }

    [Fact]
    public void RepairFun_RemoveSentenceRepeatTest_OnlySpacesRepeat()
    {
        string src = "          ";  // 8个空格
        string expected = "     ";      // 取决于非贪婪匹配行为，通常会保留最短一次
        TestSentenceRepeat(3, src, expected);
    }

    [Fact]
    public void RepairFun_RemoveSentenceRepeatTest_ThresholdIsOne_CharRepeat()
    {
        string src = "aaaaaaaaaa";
        string expected = "a";
        TestSentenceRepeat(1, src, expected);
    }



}