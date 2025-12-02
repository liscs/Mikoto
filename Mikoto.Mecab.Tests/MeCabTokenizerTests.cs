using MeCab;
using Moq;
using Xunit;
namespace Mikoto.Mecab.Tests;

public class MeCabTokenizerTests
{
    // 模拟一个 MeCabNode
    private static MeCabNode MakeNode(string surface, string feature)
    {
        return new MeCabNode
        {
            Surface = surface,
            Feature = feature
        };
    }

    [Fact]
    public void SentenceHandle_ShouldParseWords_WhenMecabEnabled()
    {
        // Arrange
        var mock = new Mock<IMeCabTagger>();

        mock.Setup(t => t.ParseToNodes("テスト"))
            .Returns(new List<MeCabNode>
            {
                MakeNode("テスト", "名詞,一般,*,*,*,*,テスト,テスト,テスト,*,*,*,*,*,*,*,*,*,*,*,テスト")
            });

        var svc = new MeCabTokenizer(mock.Object);

        // Act
        var result = svc.Parse("テスト");

        // Assert
        Assert.Single(result);
        Assert.Equal("テスト", result[0].Word);
        Assert.Equal("名詞", result[0].PartOfSpeech);

        mock.Verify(t => t.ParseToNodes("テスト"), Times.Once);
    }

    [Fact]
    public void SentenceHandle_ShouldFallback_WhenMecabDisabled()
    {
        // Arrange
        var svc = new MeCabTokenizer((IMeCabTagger?)null);

        // Act
        var result = svc.Parse("中国上海");

        // Assert
        Assert.Single(result);
        Assert.Equal("中国上海", result[0].Word);
    }

    [Fact]
    public void Dispose_ShouldCallTaggerDispose()
    {
        // Arrange
        var mock = new Mock<IMeCabTagger>();
        var svc = new MeCabTokenizer(mock.Object);

        // Act
        svc.Dispose();

        // Assert
        mock.Verify(t => t.Dispose(), Times.Once);
    }
}
