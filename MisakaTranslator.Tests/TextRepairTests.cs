using Microsoft.VisualStudio.TestTools.UnitTesting;
using TextRepairLibrary;

namespace MisakaTranslator.Tests
{
    [TestClass()]
    public class TextRepairTests
    {
        [TestMethod()]
        public void RepairFun_RemoveSingleWordRepeatTest_One()
        {
            int repeat = 1;
            string src = "Four score and seven years ago";
            string expected = "Four score and seven years ago";

            TextRepair.SingleWordRepeatTimes = repeat;
            string actual = TextRepair.RepairFun_RemoveSingleWordRepeat(src);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RepairFun_RemoveSingleWordRepeatTest_Ten()
        {
            int repeat = 10;
            string src = "FFFFFFFFFFoooooooooouuuuuuuuuurrrrrrrrrr          ssssssssssccccccccccoooooooooorrrrrrrrrreeeeeeeeee          aaaaaaaaaannnnnnnnnndddddddddd          sssssssssseeeeeeeeeevvvvvvvvvveeeeeeeeeennnnnnnnnn          yyyyyyyyyyeeeeeeeeeeaaaaaaaaaarrrrrrrrrrssssssssss          aaaaaaaaaaggggggggggoooooooooo";
            string expected = "Four score and seven years ago";

            TextRepair.SingleWordRepeatTimes = repeat;
            string actual = TextRepair.RepairFun_RemoveSingleWordRepeat(src);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RepairFun_RemoveSingleWordRepeatTest_Nine()
        {
            int repeat = 9;
            //10次重复但只设置9次，健壮性测试
            string src = "aaaaaaaaaannnnnnnnnndddddddddd          tttttttttthhhhhhhhhhaaaaaaaaaatttttttttt          ggggggggggoooooooooovvvvvvvvvveeeeeeeeeerrrrrrrrrrnnnnnnnnnnmmmmmmmmmmeeeeeeeeeennnnnnnnnntttttttttt          ooooooooooffffffffff          tttttttttthhhhhhhhhheeeeeeeeee          ppppppppppeeeeeeeeeeoooooooooopppppppppplllllllllleeeeeeeeee,,,,,,,,,,          bbbbbbbbbbyyyyyyyyyy          tttttttttthhhhhhhhhheeeeeeeeee          ppppppppppeeeeeeeeeeoooooooooopppppppppplllllllllleeeeeeeeee,,,,,,,,,,          ffffffffffoooooooooorrrrrrrrrr          tttttttttthhhhhhhhhheeeeeeeeee          ppppppppppeeeeeeeeeeoooooooooopppppppppplllllllllleeeeeeeeee,,,,,,,,,,          sssssssssshhhhhhhhhhaaaaaaaaaallllllllllllllllllll          nnnnnnnnnnooooooooootttttttttt          ppppppppppeeeeeeeeeerrrrrrrrrriiiiiiiiiisssssssssshhhhhhhhhh          ffffffffffrrrrrrrrrroooooooooommmmmmmmmm          tttttttttthhhhhhhhhheeeeeeeeee          eeeeeeeeeeaaaaaaaaaarrrrrrrrrrtttttttttthhhhhhhhhh";
            string expected = "and that government of the people, by the people, for the people, shall not perish from the earth";

            TextRepair.SingleWordRepeatTimes = repeat;
            string actual = TextRepair.RepairFun_RemoveSingleWordRepeat(src);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RepairFun_RemoveSingleWordRepeatTest_Eleven()
        {
            int repeat = 11;
            //10次重复但设置11次，健壮性测试
            string src = "sssssssssshhhhhhhhhhaaaaaaaaaallllllllllllllllllll          nnnnnnnnnnooooooooootttttttttt          ppppppppppeeeeeeeeeerrrrrrrrrriiiiiiiiiisssssssssshhhhhhhhhh          ffffffffffrrrrrrrrrroooooooooommmmmmmmmm          tttttttttthhhhhhhhhheeeeeeeeee          eeeeeeeeeeaaaaaaaaaarrrrrrrrrrtttttttttthhhhhhhhhh";
            string expected = "shall not perish from the earth";

            TextRepair.SingleWordRepeatTimes = repeat;
            string actual = TextRepair.RepairFun_RemoveSingleWordRepeat(src);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RepairFun_RemoveSentenceRepeatTest_One()
        {
            int repeat = 1;
            string src = "shall not perish from the earth";
            string expected = "shall not perish from the earth";

            TextRepair.SentenceRepeatFindCharNum = repeat;
            string actual = TextRepair.RepairFun_RemoveSentenceRepeat(src);

            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void RepairFun_RemoveSentenceRepeatTest_Two()
        {
            int repeat = 10;
            string src = "shall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earthshall not perish from the earth";
            string expected = "shall not perish from the earth";

            TextRepair.SentenceRepeatFindCharNum = repeat;
            string actual = TextRepair.RepairFun_RemoveSentenceRepeat(src);

            Assert.AreEqual(expected, actual);
        }
    }
}