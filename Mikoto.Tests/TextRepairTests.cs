using System;
using Xunit;

namespace Mikoto.Tests;

public class TextRepairTests
{
    private void TestSingleWordRepeat(int repeat, string src, string expected)
    {
        TextRepair.SingleWordRepeatTimes = repeat;
        string actual = TextRepair.RepairFun_RemoveSingleWordRepeat(src);
        Assert.Equal(expected, actual);
    }

    private void TestSentenceRepeat(int repeat, string src, string expected)
    {
        TextRepair.SentenceRepeatFindCharNum = repeat;
        string actual = TextRepair.RepairFun_RemoveSentenceRepeat(src);
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
        string src = "FFFFFFFFFFoooooooooouuuuuuuuuurrrrrrrrrr          ssssssssssccccccccccoooooooooorrrrrrrrrreeeeeeeeee          aaaaaaaaaannnnnnnnnndddddddddd          sssssssssseeeeeeeeeevvvvvvvvvveeeeeeeeeennnnnnnnnn          yyyyyyyyyyeeeeeeeeeeaaaaaaaaaarrrrrrrrrrssssssssss          aaaaaaaaaaggggggggggoooooooooo";
        string expected = "Four score and seven years ago";
        TestSingleWordRepeat(10, src, expected);
    }

    [Fact]
    public void RepairFun_RemoveSingleWordRepeatTest_AboveThreshold()
    {
        string src = "aaaaaaaaaannnnnnnnnndddddddddd          tttttttttthhhhhhhhhhaaaaaaaaaatttttttttt          ggggggggggoooooooooovvvvvvvvvveeeeeeeeeerrrrrrrrrrnnnnnnnnnnmmmmmmmmmmeeeeeeeeeennnnnnnnnntttttttttt          ooooooooooffffffffff          tttttttttthhhhhhhhhheeeeeeeeee          ppppppppppeeeeeeeeeeoooooooooopppppppppplllllllllleeeeeeeeee,,,,,,,,,,          bbbbbbbbbbyyyyyyyyyy          tttttttttthhhhhhhhhheeeeeeeeee          ppppppppppeeeeeeeeeeoooooooooopppppppppplllllllllleeeeeeeeee,,,,,,,,,,          ffffffffffoooooooooorrrrrrrrrr          tttttttttthhhhhhhhhheeeeeeeeee          ppppppppppeeeeeeeeeeoooooooooopppppppppplllllllllleeeeeeeeee,,,,,,,,,,          sssssssssshhhhhhhhhhaaaaaaaaaallllllllllllllllllll          nnnnnnnnnnooooooooootttttttttt          ppppppppppeeeeeeeeeerrrrrrrrrriiiiiiiiiisssssssssshhhhhhhhhh          ffffffffffrrrrrrrrrroooooooooommmmmmmmmm          tttttttttthhhhhhhhhheeeeeeeeee          eeeeeeeeeeaaaaaaaaaarrrrrrrrrrtttttttttthhhhhhhhhh";
        string expected = "and that government of the people, by the people, for the people, shall not perish from the earth";
        TestSingleWordRepeat(9, src, expected);
    }

    [Fact]
    public void RepairFun_RemoveSingleWordRepeatTest_ExceedsThreshold()
    {
        string src = "sssssssssshhhhhhhhhhaaaaaaaaaallllllllllllllllllll          nnnnnnnnnnooooooooootttttttttt          ppppppppppeeeeeeeeeerrrrrrrrrriiiiiiiiiisssssssssshhhhhhhhhh          ffffffffffrrrrrrrrrroooooooooommmmmmmmmm          tttttttttthhhhhhhhhheeeeeeeeee          eeeeeeeeeeaaaaaaaaaarrrrrrrrrrtttttttttthhhhhhhhhh";
        string expected = "shall not perish from the earth";
        TestSingleWordRepeat(11, src, expected);
    }

    [Fact]
    public void RepairFun_RemoveSentenceRepeatTest_NoRepetition()
    {
        string src = "shall not perish from the earth";
        TestSentenceRepeat(1, src, src);
    }

    [Fact]
    public void RepairFun_RemoveSentenceRepeatTest_MultipleRepetitions()
    {
        string src = "shall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earth";
        string expected = "shall not perish from the earth";
        TestSentenceRepeat(10, src, expected);
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
        string src = "";
        TestSentenceRepeat(1, src, src);
    }
}