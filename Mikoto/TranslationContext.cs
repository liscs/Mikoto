using Mikoto.Enums;

namespace Mikoto;

public class TranslationContext
{
    public TransMode TransMode { get; set; }
    public Guid GameID { get; set; }
    public string UsingSrcLang { get; set; } = "ja";
    public string UsingDstLang { get; set; } = "zh";
    public string? UsingRepairFunc { get; set; }
}
