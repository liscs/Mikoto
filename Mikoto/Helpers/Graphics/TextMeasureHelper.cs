using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using RichTextBox = System.Windows.Controls.RichTextBox;

namespace Mikoto.Helpers.Graphics
{
    internal static class TextMeasureHelper
    {
        /// <summary>
        /// 计算源文本RichTextBox的尺寸
        /// </summary>
        public static double GetContentWidth(RichTextBox richTextBox)
        {
            double result = 0;
            FlowDocument flowDocument = richTextBox.Document;
            if (flowDocument.Blocks.FirstBlock is Paragraph paragraph)
            {
                System.Collections.IList list = paragraph.Inlines;

                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] is Run run)
                    {
                        //Run列表，没有注音符号的情况
                        result += GetDesiredWidth(run, richTextBox);
                    }
                    if (list[i] is InlineUIContainer item && item.Child is StackPanel stackPanel)
                    {
                        //注音和正文组成的StackPanel列表，每个Panel内有两个元素
                        result += double.Max(GetDesiredWidth((TextBlock)stackPanel.Children[0]), GetDesiredWidth((TextBlock)stackPanel.Children[1]));
                    }
                }
            }
            return result * 1.1 + 50;
        }

        private static double GetDesiredWidth(TextBlock textBlock)
        {
            var formattedText = new FormattedText(
                textBlock.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(textBlock.FontFamily, textBlock.FontStyle, textBlock.FontWeight, textBlock.FontStretch),
                textBlock.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                VisualTreeHelper.GetDpi(textBlock).PixelsPerDip);
            formattedText.Trimming = TextTrimming.None;
            return formattedText.Width;
        }
        private static double GetDesiredWidth(Run run, RichTextBox richTextBox)
        {
            var formattedText = new FormattedText(
                run.Text,
                CultureInfo.CurrentCulture,
                FlowDirection.LeftToRight,
                new Typeface(run.FontFamily, run.FontStyle, run.FontWeight, run.FontStretch),
                run.FontSize,
                Brushes.Black,
                new NumberSubstitution(),
                VisualTreeHelper.GetDpi(richTextBox).PixelsPerDip);
            formattedText.Trimming = TextTrimming.None;
            return formattedText.Width;
        }
    }
}