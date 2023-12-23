using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using TesseractOCR;

namespace OCRLibrary
{
    public class TesseractOCREngine : OCREngine
    {
        private string srcLangCode = string.Empty;  //OCR识别语言 jpn=日语 eng=英语
        private Engine engine = default!;

        public override Task<string?> OCRProcessAsync(Bitmap img)
        {
            try
            {
                var stream = new MemoryStream();
                img.Save(stream, ImageFormat.Bmp);
                var pix = TesseractOCR.Pix.Image.LoadFromMemory(stream);

                var recog = engine.Process(pix);
                string text = recog.Text;
                stream.Dispose();
                recog.Dispose();

                return Task.FromResult<string?>(text);
            }
            catch (Exception ex)
            {
                errorInfo = ex.Message;
                return Task.FromResult<string?>(null);
            }
        }

        public override bool OCR_Init(string param1 = "", string param2 = "")
        {
            try
            {
                engine = new TesseractOCR.Engine(Environment.CurrentDirectory + "\\tessdata", srcLangCode);
                return true;
            }
            catch (Exception ex)
            {
                errorInfo = ex.Message;
                return false;
            }
        }

        public override void SetOCRSourceLang(string lang)
        {
            srcLangCode = lang;
        }
    }
}
