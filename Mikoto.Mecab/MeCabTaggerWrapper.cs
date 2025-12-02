using MeCab;

namespace Mikoto.Mecab
{
    public class MeCabTaggerWrapper : IMeCabTagger
    {
        private readonly MeCabTagger _tagger;

        public MeCabTaggerWrapper(string dicPath)
        {
            _tagger = MeCabTagger.Create(new MeCabParam() { DicDir = dicPath });
        }

        public IEnumerable<MeCabNode> ParseToNodes(string sentence)
            => _tagger.ParseToNodes(sentence);

        public void Dispose() => _tagger.Dispose();
    }
}
