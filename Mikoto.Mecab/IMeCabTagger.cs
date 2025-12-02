using MeCab;

namespace Mikoto.Mecab
{
    public interface IMeCabTagger
    {
        IEnumerable<MeCabNode> ParseToNodes(string sentence);
        void Dispose();
    }
}
