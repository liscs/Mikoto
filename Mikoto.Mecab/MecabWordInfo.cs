namespace Mikoto.Mecab
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
}
