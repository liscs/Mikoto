using Config.Net;

namespace MisakaTranslator
{
    public interface IAppSettings
    {
        [Option(Alias = "Software.OnCloseMainWindow", DefaultValue = "Exit")]
        string OnClickCloseButton
        {
            get;
            set;
        }

        [Option(Alias = "Globalization.Language", DefaultValue = "zh-CN")]
        string AppLanguage
        {
            get;
            set;
        }

        [Option(Alias = "Textractor.AutoHook", DefaultValue = false)]
        bool AutoHook
        {
            get;
            set;
        }

        [Option(Alias = "Textractor.AutoDetach", DefaultValue = true)]
        bool AutoDetach
        {
            get;
            set;
        }

        [Option(Alias = "JBeijing.JBJCTDllPath", DefaultValue = "")]
        string JBJCTDllPath
        {
            get;
            set;
        }

        [Option(Alias = "KingsoftFastAIT.KingsoftFastAITPath", DefaultValue = "")]
        string KingsoftFastAITPath
        {
            get;
            set;
        }

        [Option(Alias = "Dreye.DreyePath", DefaultValue = "")]
        string DreyePath
        {
            get;
            set;
        }

        [Option(Alias = "TencentOldTranslator.SecretId", DefaultValue = "")]
        string TXOSecretId
        {
            get;
            set;
        }

        [Option(Alias = "TencentOldTranslator.SecretKey", DefaultValue = "")]
        string TXOSecretKey
        {
            get;
            set;
        }

        [Option(Alias = "AzureTranslator.SecretKey", DefaultValue = "")]
        string AzureSecretKey
        {
            get;
            set;
        }

        [Option(Alias = "AzureTranslator.Location", DefaultValue = "")]
        string AzureLocation
        {
            get;
            set;
        }

        [Option(Alias = "VolcanoTranslator.VolcanoId", DefaultValue = "")]
        string VolcanoId { get; set; }

        [Option(Alias = "VolcanoTranslator.VolcanoKey", DefaultValue = "")]
        string VolcanoKey { get; set; }

        [Option(Alias = "AwsTranslator.AwsAccessKey", DefaultValue = "")]
        string AwsAccessKey { get; set; }

        [Option(Alias = "AwsTranslator.AwsSecretKey", DefaultValue = "")]
        string AwsSecretKey { get; set; }

        [Option(Alias = "BaiduTranslator.AppID", DefaultValue = "")]
        string BDappID
        {
            get;
            set;
        }

        [Option(Alias = "BaiduTranslator.SecretKey", DefaultValue = "")]
        string BDsecretKey
        {
            get;
            set;
        }

        [Option(Alias = "DeepLTranslator.SecretKey", DefaultValue = "")]
        string DeepLsecretKey
        {
            get;
            set;
        }

        [Option(Alias = "TencentTranslator.AppID", DefaultValue = "")]
        string TXappID
        {
            get;
            set;
        }

        [Option(Alias = "TencentTranslator.AppKey", DefaultValue = "")]
        string TXappKey
        {
            get;
            set;
        }

        [Option(Alias = "CaiyunTranslator.CaiyunToken", DefaultValue = "")]
        string CaiyunToken
        {
            get;
            set;
        }

        [Option(Alias = "ChatGPTTranslator.ApiKey", DefaultValue = "")]
        string ChatGPTapiKey
        {
            get;
            set;
        }

        [Option(Alias = "ChatGPTTranslator.ApiUrl", DefaultValue = "https://api.openai.com/v1/chat/completions")]
        string ChatGPTapiUrl
        {
            get;
            set;
        }

        [Option(Alias = "XiaoniuTranslator.XiaoniuApiKey", DefaultValue = "")]
        string xiaoniuApiKey
        {
            get;
            set;
        }

        [Option(Alias = "IBMTranslator.IBMApiKey", DefaultValue = "")]
        string IBMApiKey
        {
            get;
            set;
        }

        [Option(Alias = "IBMTranslator.IBMURL", DefaultValue = "")]
        string IBMURL
        {
            get;
            set;
        }

        [Option(Alias = "YandexTranslator.YandexApiKey", DefaultValue = "")]
        string YandexApiKey
        {
            get;
            set;
        }

        [Option(Alias = "YoudaoZhiyun.YDZYAppId", DefaultValue = "")]
        string YDZYAppId
        {
            get;
            set;
        }

        [Option(Alias = "YoudaoZhiyun.YDZYAppSecret", DefaultValue = "")]
        string YDZYAppSecret
        {
            get;
            set;
        }

        [Option(Alias = "Translate_All.HttpProxy", DefaultValue = "")]
        string HttpProxy
        {
            get;
            set;
        }

        [Option(Alias = "Translate_All.EachRowTrans", DefaultValue = true)]
        bool EachRowTrans
        {
            get;
            set;
        }

        [Option(Alias = "Translate_All.FirstTranslator", DefaultValue = "NoTranslate")]
        string FirstTranslator
        {
            get;
            set;
        }

        [Option(Alias = "Translate_All.SecondTranslator", DefaultValue = "NoTranslate")]
        string SecondTranslator
        {
            get;
            set;
        }

        [Option(Alias = "Translate_All.TransLimitNums", DefaultValue = 100)]
        int TransLimitNums
        {
            get;
            set;
        }

        [Option(Alias = "OCR_All.OCRsource", DefaultValue = "BaiduOCR")]
        string OCRsource
        {
            get;
            set;
        }

        [Option(Alias = "OCR_All.GlobalOCRHotkey", DefaultValue = "Ctrl + Alt + Q")]
        string GlobalOCRHotkey
        {
            get;
            set;
        }

        [Option(Alias = "OCR_All.GlobalOCRLang", DefaultValue = "jpn")]
        string GlobalOCRLang
        {
            get;
            set;
        }

        [Option(Alias = "BaiduOCR.APIKEY", DefaultValue = "")]
        string BDOCR_APIKEY
        {
            get;
            set;
        }

        [Option(Alias = "BaiduOCR.SecretKey", DefaultValue = "")]
        string BDOCR_SecretKey
        {
            get;
            set;
        }

        [Option(Alias = "TesseractCli.Path", DefaultValue = "C:\\Program Files\\Tesseract-OCR\\tesseract.exe")]
        string TesseractCli_Path
        {
            get;
            set;
        }

        [Option(Alias = "TesseractCli.Mode", DefaultValue = "jpn")]
        string TesseractCli_Mode
        {
            get;
            set;
        }

        [Option(Alias = "TesseractCli.Args", DefaultValue = "")]
        string TesseractCli_Args
        {
            get;
            set;
        }

        [Option(Alias = "LE.LEPath", DefaultValue = "")]
        string LEPath
        {
            get;
            set;
        }

        [Option(Alias = "TTS.SelectedTTS", DefaultValue = "Local")]
        TTSMode SelectedTTS
        {
            get;
            set;
        }

        [Option(Alias = "TTS.AzureTTSProxy", DefaultValue = "")]
        string AzureTTSProxy
        {
            get;
            set;
        }

        [Option(Alias = "TTS.AzureTTSSecretKey", DefaultValue = "")]
        string AzureTTSSecretKey
        {
            get;
            set;
        }

        [Option(Alias = "TTS.AzureTTSLocation", DefaultValue = "")]
        string AzureTTSLocation
        {
            get;
            set;
        }

        [Option(Alias = "TTS.AzureTTSVoice", DefaultValue = "ja-JP-NanamiNeural")]
        string AzureTTSVoice
        {
            get;
            set;
        }

        [Option(Alias = "TTS.LocalTTSVoice", DefaultValue = "")]
        string LocalTTSVoice
        {
            get;
            set;
        }

        [Option(Alias = "TTS.LoaclTTSVolume", DefaultValue = "80")]
        int LoaclTTSVolume
        {
            get;
            set;
        }

        [Option(Alias = "TTS.LocaTTSRate", DefaultValue = "0")]
        int LocaTTSRate
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.Opacity", DefaultValue = "100")]
        double TF_Opacity
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.BackColor", DefaultValue = "#4BFFFFFF")]
        string TF_BackColor
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.SizeW", DefaultValue = "0")]
        string TF_SizeW
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.SizeH", DefaultValue = "0")]
        string TF_SizeH
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.LocX", DefaultValue = "-1")]
        string TF_LocX
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.LocY", DefaultValue = "-1")]
        string TF_LocY
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.ShowSourceText", DefaultValue = true)]
        bool TF_ShowSourceText
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.SrcAnimationCheckEnabled", DefaultValue = true)]
        bool TF_SrcAnimationCheckEnabled { get; set; }

        [Option(Alias = "TranslateFormSettings.SrcSingleLineDisplay", DefaultValue = true)]
        bool TF_SrcSingleLineDisplay { get; set; }


        [Option(Alias = "TranslateFormSettings.TransAnimationCheckEnabled", DefaultValue = false)]
        bool TF_TransAnimationCheckEnabled { get; set; }

        [Option(Alias = "TranslateFormSettings.BackgroundBlurCheckEnabled", DefaultValue = true)]
        bool TF_BackgroundBlurCheckEnabled { get; set; }

        [Option(Alias = "TranslateFormSettings.SrcTextFont", DefaultValue = "Microsoft YaHei")]
        string TF_SrcTextFont
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.SrcTextSize", DefaultValue = "26")]
        double TF_SrcTextSize
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.SrcTextColor", DefaultValue = "#FFFFFFFF")]
        string TF_SrcTextColor { get; set; }

        [Option(Alias = "TranslateFormSettings.FirstTransTextFont", DefaultValue = "Microsoft YaHei")]
        string TF_FirstTransTextFont
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.FirstTransTextSize", DefaultValue = "30")]
        double TF_FirstTransTextSize
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.FirstTransTextColor", DefaultValue = "#ffFFFFFF")]
        string TF_FirstTransTextColor
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.FirstTransFontWeight", DefaultValue = "700")]
        int TF_FirstTextFontWeight { get; set; }

        [Option(Alias = "TranslateFormSettings.FirstTransStrokeThickness", DefaultValue = "0.7")]
        double TF_FirstTextStrokeThickness { get; set; }


        [Option(Alias = "TranslateFormSettings.SecondTransTextFont", DefaultValue = "Microsoft YaHei")]
        string TF_SecondTransTextFont
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.SecondTransTextSize", DefaultValue = "30")]
        double TF_SecondTransTextSize
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.SecondTransTextColor", DefaultValue = "#ffFFFFFF")]
        string TF_SecondTransTextColor
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.SecondTransFontWeight", DefaultValue = "700")]
        int TF_SecondTextFontWeight { get; set; }

        [Option(Alias = "TranslateFormSettings.SecondTransStrokeThickness", DefaultValue = "0.7")]
        double TF_SecondTextStrokeThickness { get; set; }

        [Option(Alias = "TranslateFormSettings.FirstWhiteStrokeIsChecked", DefaultValue = false)]
        bool TF_FirstWhiteStrokeIsChecked
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.SecondWhiteStrokeIsChecked", DefaultValue = false)]
        bool TF_SecondWhiteStrokeIsChecked
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFormSettings.EnablePhoneticNotation", DefaultValue = true)]
        bool TF_EnablePhoneticNotation
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFromSettings.EnableDropShadow", DefaultValue = true)]
        bool TF_EnableDropShadow
        {
            get;
            set;
        }

        [Option(Alias = "TranslateFromSettings.PhoneticNotationType", DefaultValue = PhoneticNotationType.Hiragana)]
        PhoneticNotationType TF_PhoneticNotationType
        {
            get;
            set;
        }
        [Option(Alias = "TranslateFromSettings.EnableSuperBold", DefaultValue = true)]
        bool TF_EnableSuperBold
        {
            get;
            set;
        }
        [Option(Alias = "TranslateFromSettings.EnableColorful", DefaultValue = true)]
        bool TF_EnableColorful
        {
            get;
            set;
        }

        [Option(Alias = "ArtificialTrans.PatchPath", DefaultValue = "")]
        string ArtificialPatchPath
        {
            get;
            set;
        }

        [Option(Alias = "ArtificialTrans.ATon", DefaultValue = false)]
        bool ATon
        {
            get;
            set;
        }

        [Option(DefaultValue = true)]
        bool GrowlEnabled { get; set; }

        [Option(DefaultValue = true)]
        bool UpdateCheckEnabled { get; set; }

        [Option(Alias = "Mecab.DicPath", DefaultValue = "")]
        string Mecab_DicPath { get; set; }

        [Option(Alias = "Textractor.Path32", DefaultValue = "")]
        string Textractor_Path32 { get; set; }

        [Option(Alias = "Textractor.Path64", DefaultValue = "")]
        string Textractor_Path64 { get; set; }

    }

    public interface IRepeatRepairSettings
    {
        [Option(Alias = "RepairFun_RemoveSingleWordRepeat.RepeatTimes", DefaultValue = 0)]
        int SingleWordRepeatTimes
        {
            get;
            set;
        }

        [Option(Alias = "RepairFun_RemoveSentenceRepeat.FindCharNum", DefaultValue = 4)]
        int SentenceRepeatFindCharNum
        {
            get;
            set;
        }

        [Option(Alias = "RepairFun_Regex.Regex", DefaultValue = "")]
        string Regex
        {
            get;
            set;
        }

        [Option(Alias = "RepairFun_Regex.Replace", DefaultValue = "")]
        string Regex_Replace
        {
            get;
            set;
        }
    }
}
