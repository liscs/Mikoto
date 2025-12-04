using System.Globalization;

namespace Mikoto.Translators.LanguageCode;

/// <summary>
/// 将标准的 System.Globalization.CultureInfo 转换为 Google Cloud Translation API 所需的语言代码。
/// Google Cloud V2/V3 API 主要使用 ISO 639-1 (两位小写) 或 BCP-47 标准（部分特殊语言）。
/// </summary>
public static class GoogleLanguageCodeConverter
{
    // Google V2 API 接受的特殊语言代码映射，如果有需要可以放在这里
    private static readonly Dictionary<string, string> SpecialCodeMap = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        // 示例：某些 API 可能对中文有特殊要求，但 Google 默认支持 BCP-47 标准
        // { "zh-Hans", "zh-CN" }, // 简化为 zh
        // { "zh-Hant", "zh-TW" }, // 简化为 zh-TW (可选)

        // 示例：Google API 要求的特殊代码，如果与 CultureInfo.TwoLetterISOLanguageName 不一致
        // { "iw", "he" } // 希伯来语的旧 ISO 639-1 是 iw，Google 推荐 he
    };

    /// <summary>
    /// 获取 Google Cloud Translation API 所需的语言代码。
    /// </summary>
    /// <param name="cultureInfo">标准的 CultureInfo 对象。</param>
    /// <returns>Google API 兼容的语言代码（通常是两位 ISO 639-1 或 BCP-47 格式）。</returns>
    public static string GetLanguageCode(CultureInfo cultureInfo)
    {
        // 1. 获取完整的 BCP-47 格式，通常这是 Google API 最推荐的格式
        string bcp47Code = cultureInfo.Name; // 例如: "en-US", "zh-Hans-CN"

        // 2. 检查特殊映射（处理不兼容 ISO 639-1 的情况）
        if (SpecialCodeMap.TryGetValue(bcp47Code, out string? specialCode))
        {
            return specialCode;
        }

        // 3. 尝试简化为两字母代码 (ISO 639-1)，这是最常见的格式
        string isoCode = cultureInfo.TwoLetterISOLanguageName; // 例如: "en", "zh"

        // 谷歌 API 可以在大多数情况下识别 ISO 639-1 代码
        if (!string.IsNullOrEmpty(isoCode))
        {
            // 针对中文：Google API 通常推荐使用 zh 或 zh-CN/zh-TW
            // 这里选择返回 ISO 639-1 代码，让 Google API 自动识别

            // 如果 CultureInfo 的 Name 不等于 TwoLetterISOLanguageName，
            // 且 TwoLetterISOLanguageName 不足以区分 (例如 zh 无法区分简繁)，
            // 我们可以返回 BCP-47 格式以保持区分度（如 zh-CN, zh-TW, en-US）。

            // 经验表明，对于 Google API，使用两字母代码通常已经足够，除非您需要明确区分区域或脚本（如简繁体）

            // 对于中文 (zh) 建议保留脚本信息 (zh-CN 或 zh-TW) 以区分简繁体
            if (isoCode.Equals("zh", StringComparison.OrdinalIgnoreCase))
            {
                // 谷歌 V3 推荐的中文代码是 zh-CN (简体) 和 zh-TW (繁体)
                // 如果 CultureInfo.Name 包含 "Hans" 或 "Hant"，最好返回区分简繁的代码。
                if (bcp47Code.IndexOf("hans", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "zh-CN";
                if (bcp47Code.IndexOf("hant", StringComparison.OrdinalIgnoreCase) >= 0)
                    return "zh-TW";

                // 如果无法判断简繁，退回到基础代码
                return "zh";
            }

            // 对于其他语言，直接使用小写的两字母代码
            return isoCode.ToLowerInvariant();
        }

        // 4. 如果两字母代码不可用或存在问题，退回到完整的 CultureInfo 名称 (BCP-47)
        return bcp47Code.ToLowerInvariant();
    }

    // 谷歌支持的完整语言代码列表可以在其官方文档中查看
    public static string GetGoogleSupportedLanguagesUrl()
    {
        return "https://cloud.google.com/translate/docs/languages";
    }
}