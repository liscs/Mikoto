using Mikoto.DataAccess;
using Mikoto.Resource;
using Moq;
using Xunit;

namespace Mikoto.TextHook.Tests;

public class HistoryExporterTest
{
    [Fact]
    public void HistoryExporter_ShouldExportAllLines()
    {
        // mock hook
        var hookMock = new Mock<ITextHookService>();
        hookMock.Setup(h => h.TextractorOutPutHistory)
            .Returns(new Queue<string>(new[] { "A", "B" }));

        // mock resource
        var resMock = new Mock<IResourceService>();
        resMock.Setup(r => r.Get("Common_TextractorHistory"))
            .Returns("HEADER");

        // mock file
        var fileMock = new Mock<IFileService>();
        var captured = new List<string>();

        fileMock
            .Setup(f => f.WriteAllLines(It.IsAny<string>(), It.IsAny<IEnumerable<string>>()))
            .Callback<string, IEnumerable<string>>((p, lines) => captured = lines.ToList());

        var exporter = new HistoryExporter(
            hookMock.Object,
            fileMock.Object,
            resMock.Object);

        bool ok = exporter.Export("test.txt");

        Assert.True(ok);
        Assert.Equal(
            ["HEADER", "A", "B"],
            captured);
    }
}
