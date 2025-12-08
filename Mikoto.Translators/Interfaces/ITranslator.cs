namespace Mikoto.Translators.Interfaces
{
    /// <summary>
    /// 翻译器接口，继承此接口的类会被代码生成器自动加入到翻译器列表
    /// </summary>
    public interface ITranslator
    {
        /// <summary>
        /// 翻译器用于显示的本地化名称
        /// </summary>
        string DisplayName { get; }

        /// <summary>
        /// 翻译一条语句
        /// </summary>
        /// <param name="sourceText">源文本</param>
        /// <param name="desLang">目标语言</param>
        /// <param name="srcLang">源语言</param>
        /// <returns>翻译后的语句,如果翻译有错误会返回空，可以通过GetLastError来获取错误</returns>
        Task<string?> TranslateAsync(string sourceText, string desLang, string srcLang);

        /// <summary>
        /// 返回最后一次错误的ID或原因
        /// </summary>
        /// <returns></returns>
        string GetLastError();
    }
}
