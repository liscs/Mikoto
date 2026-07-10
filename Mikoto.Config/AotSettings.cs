using Microsoft.Extensions.Configuration;
using System.Globalization;

namespace Mikoto.Config
{
    public class AotSettings : IAppSettings
    {
        private readonly IConfiguration _config;

        // 预缓存各个 Section 以简化内部 Key 的访问
        private readonly IConfigurationSection _sw;      // Software
        private readonly IConfigurationSection _glob;    // Globalization
        private readonly IConfigurationSection _txt;     // Textractor
        private readonly IConfigurationSection _jb;      // JBeijing
        private readonly IConfigurationSection _ks;      // KingsoftFastAIT
        private readonly IConfigurationSection _dr;      // Dreye
        private readonly IConfigurationSection _txo;     // TencentOldTranslator
        private readonly IConfigurationSection _az;      // AzureTranslator
        private readonly IConfigurationSection _vol;     // VolcanoTranslator
        private readonly IConfigurationSection _aws;     // AwsTranslator
        private readonly IConfigurationSection _bd;      // BaiduTranslator
        private readonly IConfigurationSection _goog;    // GoogleCloudTranslator
        private readonly IConfigurationSection _deepl;   // DeepLTranslator
        private readonly IConfigurationSection _tx;      // TencentTranslator
        private readonly IConfigurationSection _cy;      // CaiyunTranslator
        private readonly IConfigurationSection _gpt;     // ChatGPTTranslator
        private readonly IConfigurationSection _xn;      // XiaoniuTranslator
        private readonly IConfigurationSection _ibm;     // IBMTranslator
        private readonly IConfigurationSection _yan;     // YandexTranslator
        private readonly IConfigurationSection _yd;      // YoudaoZhiyun
        private readonly IConfigurationSection _tall;    // Translate_All
        private readonly IConfigurationSection _le;      // LE
        private readonly IConfigurationSection _tts;     // TTS
        private readonly IConfigurationSection _tf;      // TranslateFormSettings
        private readonly IConfigurationSection _tfrom;   // TranslateFromSettings
        private readonly IConfigurationSection _art;     // ArtificialTrans
        private readonly IConfigurationSection _mecab;   // Mecab

        public AotSettings(IConfiguration config, string iniPath)
        {
            _config = config;
            _sw = config.GetSection("Software");
            _glob = config.GetSection("Globalization");
            _txt = config.GetSection("Textractor");
            _jb = config.GetSection("JBeijing");
            _ks = config.GetSection("KingsoftFastAIT");
            _dr = config.GetSection("Dreye");
            _txo = config.GetSection("TencentOldTranslator");
            _az = config.GetSection("AzureTranslator");
            _vol = config.GetSection("VolcanoTranslator");
            _aws = config.GetSection("AwsTranslator");
            _bd = config.GetSection("BaiduTranslator");
            _goog = config.GetSection("GoogleCloudTranslator");
            _deepl = config.GetSection("DeepLTranslator");
            _tx = config.GetSection("TencentTranslator");
            _cy = config.GetSection("CaiyunTranslator");
            _gpt = config.GetSection("ChatGPTTranslator");
            _xn = config.GetSection("XiaoniuTranslator");
            _ibm = config.GetSection("IBMTranslator");
            _yan = config.GetSection("YandexTranslator");
            _yd = config.GetSection("YoudaoZhiyun");
            _tall = config.GetSection("Translate_All");
            _le = config.GetSection("LE");
            _tts = config.GetSection("TTS");
            _tf = config.GetSection("TranslateFormSettings");
            _tfrom = config.GetSection("TranslateFromSettings");
            _art = config.GetSection("ArtificialTrans");
            _mecab = config.GetSection("Mecab");
        }

        #region 1. 基础与路径类
        public string OnClickCloseButton { get => _sw["OnCloseMainWindow"] ?? "Exit"; set => _sw["OnCloseMainWindow"] = value; }
        public string AppLanguage { get => _glob["Language"] ?? "zh-CN"; set => _glob["Language"] = value; }
        public bool AutoHook { get => GetV(_txt, "AutoHook", false); set => SetV(_txt, "AutoHook", value); }
        public bool AutoDetach { get => GetV(_txt, "AutoDetach", true); set => SetV(_txt, "AutoDetach", value); }
        public string Textractor_Path32 { get => _txt["Path32"] ?? ""; set => _txt["Path32"] = value; }
        public string Textractor_Path64 { get => _txt["Path64"] ?? ""; set => _txt["Path64"] = value; }
        public string JBJCTDllPath { get => _jb["JBJCTDllPath"] ?? ""; set => _jb["JBJCTDllPath"] = value; }
        public string KingsoftFastAITPath { get => _ks["KingsoftFastAITPath"] ?? ""; set => _ks["KingsoftFastAITPath"] = value; }
        public string DreyePath { get => _dr["DreyePath"] ?? ""; set => _dr["DreyePath"] = value; }
        public string LEPath { get => _le["LEPath"] ?? ""; set => _le["LEPath"] = value; }
        public string Mecab_DicPath { get => _mecab["DicPath"] ?? ""; set => _mecab["DicPath"] = value; }
        #endregion

        #region 2. 翻译 API 密钥类
        public string TXOSecretId { get => _txo["SecretId"] ?? ""; set => _txo["SecretId"] = value; }
        public string TXOSecretKey { get => _txo["SecretKey"] ?? ""; set => _txo["SecretKey"] = value; }
        public string AzureSecretKey { get => _az["SecretKey"] ?? ""; set => _az["SecretKey"] = value; }
        public string AzureLocation { get => _az["Location"] ?? ""; set => _az["Location"] = value; }
        public string VolcanoId { get => _vol["VolcanoId"] ?? ""; set => _vol["VolcanoId"] = value; }
        public string VolcanoKey { get => _vol["VolcanoKey"] ?? ""; set => _vol["VolcanoKey"] = value; }
        public string AwsAccessKey { get => _aws["AwsAccessKey"] ?? ""; set => _aws["AwsAccessKey"] = value; }
        public string AwsSecretKey { get => _aws["AwsSecretKey"] ?? ""; set => _aws["AwsSecretKey"] = value; }
        public string BDappID { get => _bd["AppID"] ?? ""; set => _bd["AppID"] = value; }
        public string BDsecretKey { get => _bd["SecretKey"] ?? ""; set => _bd["SecretKey"] = value; }
        public string GoogleSecretKey { get => _goog["SecretKey"] ?? ""; set => _goog["SecretKey"] = value; }
        public string DeepLsecretKey { get => _deepl["SecretKey"] ?? ""; set => _deepl["SecretKey"] = value; }
        public string TXappID { get => _tx["AppID"] ?? ""; set => _tx["AppID"] = value; }
        public string TXappKey { get => _tx["AppKey"] ?? ""; set => _tx["AppKey"] = value; }
        public string CaiyunToken { get => _cy["CaiyunToken"] ?? ""; set => _cy["CaiyunToken"] = value; }
        public string ChatGPTapiKey { get => _gpt["ApiKey"] ?? ""; set => _gpt["ApiKey"] = value; }
        public string ChatGPTapiUrl { get => _gpt["ApiUrl"] ?? "https://api.openai.com/v1/chat/completions"; set => _gpt["ApiUrl"] = value; }
        public string ChatGPTapiModel { get => _gpt["ApiModel"] ?? "gpt-3.5-turbo"; set => _gpt["ApiModel"] = value; }
        public string XiaoniuApiKey { get => _xn["XiaoniuApiKey"] ?? ""; set => _xn["XiaoniuApiKey"] = value; }
        public string IBMApiKey { get => _ibm["IBMApiKey"] ?? ""; set => _ibm["IBMApiKey"] = value; }
        public string IBMURL { get => _ibm["IBMURL"] ?? ""; set => _ibm["IBMURL"] = value; }
        public string YandexApiKey { get => _yan["YandexApiKey"] ?? ""; set => _yan["YandexApiKey"] = value; }
        public string YDZYAppId { get => _yd["YDZYAppId"] ?? ""; set => _yd["YDZYAppId"] = value; }
        public string YDZYAppSecret { get => _yd["YDZYAppSecret"] ?? ""; set => _yd["YDZYAppSecret"] = value; }
        #endregion

        #region 3. 翻译通用策略类
        public string HttpProxy { get => _tall["HttpProxy"] ?? ""; set => _tall["HttpProxy"] = value; }
        public bool EachRowTrans { get => GetV(_tall, "EachRowTrans", true); set => SetV(_tall, "EachRowTrans", value); }
        public string FirstTranslator { get => _tall["FirstTranslator"] ?? "NoTranslate"; set => _tall["FirstTranslator"] = value; }
        public string SecondTranslator { get => _tall["SecondTranslator"] ?? "NoTranslate"; set => _tall["SecondTranslator"] = value; }
        public int TransLimitNums { get => GetV(_tall, "TransLimitNums", 100); set => SetV(_tall, "TransLimitNums", value); }
        #endregion

        #region 4. TTS 设置类
        public TTSMode SelectedTTS { get => GetV(_tts, "SelectedTTS", TTSMode.Local); set => SetV(_tts, "SelectedTTS", value); }
        public string AzureTTSProxy { get => _tts["AzureTTSProxy"] ?? ""; set => _tts["AzureTTSProxy"] = value; }
        public string AzureTTSSecretKey { get => _tts["AzureTTSSecretKey"] ?? ""; set => _tts["AzureTTSSecretKey"] = value; }
        public string AzureTTSLocation { get => _tts["AzureTTSLocation"] ?? ""; set => _tts["AzureTTSLocation"] = value; }
        public string AzureTTSVoice { get => _tts["AzureTTSVoice"] ?? "Microsoft Server Speech (ja-JP, NanamiNeural)"; set => _tts["AzureTTSVoice"] = value; }
        public double AzureTTSVoiceVolume { get => GetV(_tts, "AzureTTSVoiceVolume", 100.0); set => SetV(_tts, "AzureTTSVoiceVolume", value); }
        public string AzureTTSVoiceStyle { get => _tts["AzureTTSVoiceStyle"] ?? ""; set => _tts["AzureTTSVoiceStyle"] = value; }
        public bool AzureEnableAutoSpeak { get => GetV(_tts, "AzureEnableAutoSpeak", false); set => SetV(_tts, "AzureEnableAutoSpeak", value); }
        public string LocalTTSVoice { get => _tts["LocalTTSVoice"] ?? ""; set => _tts["LocalTTSVoice"] = value; }
        public int LoaclTTSVolume { get => GetV(_tts, "LoaclTTSVolume", 80); set => SetV(_tts, "LoaclTTSVolume", value); }
        public int LocaTTSRate { get => GetV(_tts, "LocaTTSRate", 0); set => SetV(_tts, "LocaTTSRate", value); }
        #endregion

        #region 5. 翻译窗体 UI 设置类
        public double TF_Opacity { get => GetV(_tf, "Opacity", 100.0); set => SetV(_tf, "Opacity", value); }
        public string TF_BackColor { get => _tf["BackColor"] ?? "#4BFFFFFF"; set => _tf["BackColor"] = value; }
        public string TF_SizeW { get => _tf["SizeW"] ?? "0"; set => _tf["SizeW"] = value; }
        public string TF_SizeH { get => _tf["SizeH"] ?? "0"; set => _tf["SizeH"] = value; }
        public string TF_LocX { get => _tf["LocX"] ?? "-1"; set => _tf["LocX"] = value; }
        public string TF_LocY { get => _tf["LocY"] ?? "-1"; set => _tf["LocY"] = value; }
        public bool TF_ShowSourceText { get => GetV(_tf, "ShowSourceText", true); set => SetV(_tf, "ShowSourceText", value); }
        public bool TF_SrcAnimationCheckEnabled { get => GetV(_tf, "SrcAnimationCheckEnabled", true); set => SetV(_tf, "SrcAnimationCheckEnabled", value); }
        public bool TF_SrcSingleLineDisplay { get => GetV(_tf, "SrcSingleLineDisplay", true); set => SetV(_tf, "SrcSingleLineDisplay", value); }
        public bool TF_TransAnimationCheckEnabled { get => GetV(_tf, "TransAnimationCheckEnabled", false); set => SetV(_tf, "TransAnimationCheckEnabled", value); }
        public bool TF_BackgroundBlurCheckEnabled { get => GetV(_tf, "BackgroundBlurCheckEnabled", true); set => SetV(_tf, "BackgroundBlurCheckEnabled", value); }
        public string TF_SrcTextFont { get => _tf["SrcTextFont"] ?? "Microsoft YaHei"; set => _tf["SrcTextFont"] = value; }
        public double TF_SrcTextSize { get => GetV(_tf, "SrcTextSize", 26.0); set => SetV(_tf, "SrcTextSize", value); }
        public string TF_SrcTextColor { get => _tf["SrcTextColor"] ?? "#FFFFFFFF"; set => _tf["SrcTextColor"] = value; }
        public string TF_FirstTransTextFont { get => _tf["FirstTransTextFont"] ?? "Microsoft YaHei"; set => _tf["FirstTransTextFont"] = value; }
        public double TF_FirstTransTextSize { get => GetV(_tf, "FirstTransTextSize", 30.0); set => SetV(_tf, "FirstTransTextSize", value); }
        public string TF_FirstTransTextColor { get => _tf["FirstTransTextColor"] ?? "#ffFFFFFF"; set => _tf["FirstTransTextColor"] = value; }
        public int TF_FirstTextFontWeight { get => GetV(_tf, "FirstTransFontWeight", 700); set => SetV(_tf, "FirstTransFontWeight", value); }
        public double TF_FirstTextStrokeThickness { get => GetV(_tf, "FirstTransStrokeThickness", 0.7); set => SetV(_tf, "FirstTransStrokeThickness", value); }
        public string TF_SecondTransTextFont { get => _tf["SecondTransTextFont"] ?? "Microsoft YaHei"; set => _tf["SecondTransTextFont"] = value; }
        public double TF_SecondTransTextSize { get => GetV(_tf, "SecondTransTextSize", 30.0); set => SetV(_tf, "SecondTransTextSize", value); }
        public string TF_SecondTransTextColor { get => _tf["SecondTransTextColor"] ?? "#ffFFFFFF"; set => _tf["SecondTransTextColor"] = value; }
        public int TF_SecondTextFontWeight { get => GetV(_tf, "SecondTransFontWeight", 700); set => SetV(_tf, "SecondTransFontWeight", value); }
        public double TF_SecondTextStrokeThickness { get => GetV(_tf, "SecondTransStrokeThickness", 0.7); set => SetV(_tf, "SecondTransStrokeThickness", value); }
        public bool TF_FirstWhiteStrokeIsChecked { get => GetV(_tf, "FirstWhiteStrokeIsChecked", false); set => SetV(_tf, "FirstWhiteStrokeIsChecked", value); }
        public bool TF_SecondWhiteStrokeIsChecked { get => GetV(_tf, "SecondWhiteStrokeIsChecked", false); set => SetV(_tf, "SecondWhiteStrokeIsChecked", value); }
        public bool TF_EnablePhoneticNotation { get => GetV(_tf, "EnablePhoneticNotation", true); set => SetV(_tf, "EnablePhoneticNotation", value); }

        public bool TF_EnableDropShadow { get => GetV(_tfrom, "EnableDropShadow", true); set => SetV(_tfrom, "EnableDropShadow", value); }
        public PhoneticNotationType TF_PhoneticNotationType { get => GetV(_tfrom, "PhoneticNotationType", PhoneticNotationType.Hiragana); set => SetV(_tfrom, "PhoneticNotationType", value); }
        public bool TF_EnableSuperBold { get => GetV(_tfrom, "EnableSuperBold", true); set => SetV(_tfrom, "EnableSuperBold", value); }
        public bool TF_EnableColorful { get => GetV(_tfrom, "EnableColorful", true); set => SetV(_tfrom, "EnableColorful", value); }
        #endregion

        #region 6. 其他设置类
        public string ArtificialPatchPath { get => _art["PatchPath"] ?? ""; set => _art["PatchPath"] = value; }
        public bool ATon { get => GetV(_art, "ATon", false); set => SetV(_art, "ATon", value); }
        public bool GrowlEnabled { get => GetV(_config, "GrowlEnabled", true); set => SetV(_config, "GrowlEnabled", value); }
        public bool UpdateCheckEnabled { get => GetV(_config, "UpdateCheckEnabled", true); set => SetV(_config, "UpdateCheckEnabled", value); }
        #endregion

        #region AOT 安全的辅助方法
        private static T GetV<T>(IConfiguration section, string key, T @default)
        {
            var val = section[key];
            if (string.IsNullOrEmpty(val)) return @default;
            try
            {
                if (typeof(T).IsEnum) return (T)Enum.Parse(typeof(T), val, true);
                return (T)Convert.ChangeType(val, typeof(T), CultureInfo.InvariantCulture);
            }
            catch { return @default; }
        }

        private static void SetV<T>(IConfiguration section, string key, T val) => section[key] = val?.ToString();
        #endregion
    }
}