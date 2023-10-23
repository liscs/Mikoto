extern alias Tesseract;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Threading.Tasks;
using Tesseract.TesseractOCR;

namespace OCRLibrary
{
    public class TesseractOCR : OCREngine
    {
        private string srcLangCode;  //OCR识别语言 jpn=日语 eng=英语
        private Engine engine;

        public override Task<string> OCRProcessAsync(Bitmap img)
        {
            try
            {
                var stream = new MemoryStream();
                img.Save(stream, ImageFormat.Bmp);
                var pix = Tesseract.TesseractOCR.Pix.Image.LoadFromMemory(stream);

                var recog = engine.Process(pix);
                string text = recog.Text;
                stream.Dispose();

                /* 项目“OCRLibrary (netframework4.7.2)”的未合并的更改
                在此之前:
                                recog.Dispose();

                                return Task.FromResult(text);
                在此之后:
                                recog.Dispose();

                                return Task.FromResult(text);
                */
                recog.Dispose();

                return Task.FromResult(text);
            }
            catch (Exception ex)
            {
                errorInfo = ex.Message;
                return Task.FromResult<string>(null);
            }
        }

        public override bool OCR_Init(string param1 = "", string param2 = "")
        {
            try
            {
                engine = new Tesseract.TesseractOCR.Engine(Environment.CurrentDirectory + "\\tessdata", srcLangCode);
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
