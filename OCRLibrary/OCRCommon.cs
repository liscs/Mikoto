namespace OCRLibrary
{
    public static class OCRCommon
    {
        public static List<string> lstOCR = new List<string>()
        {
            "BaiduOCR",
            "BaiduFanyiOCR",
            "TencentOCR",
            "TesseractOCR",
            "TesseractCli"
        };

        static OCRCommon()
        {
            if (Environment.OSVersion.Version.Build >= 10240)
            {
                lstOCR.Add("WindowsOCR");
            }
        }

        public static List<string> GetOCRList()
        {
            return lstOCR;
        }

        public static OCREngine OCRAuto(string ocr)
        {
            switch (ocr)
            {
                case "BaiduOCR":
                    return new BaiduGeneralOCREngine();
                case "BaiduFanyiOCR":
                    return new BaiduFanyiOCREngine();
                case "TencentOCR":
                    return new TencentOCR();
                case "TesseractOCR":
                    return new TesseractOCREngine();
                case "TesseractCli":
                    return new TesseractCommandLineEngine();
                case "WindowsOCR":
                    return new WindowsOCREngine();
            }
            return new WindowsOCREngine();
        }

        public static System.Text.Json.JsonSerializerOptions JsonOP = new()
        {
            IncludeFields = true
        };
    }
}
