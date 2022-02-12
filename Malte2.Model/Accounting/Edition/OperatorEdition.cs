using System.IO;
using PdfSharpCore.Pdf;
using PdfSharpCore.Drawing;

namespace Malte2.Model.Accounting.Edition
{
    public static class OperatorEdition
    {
        public static MemoryStream CreateEdition()
        {
            // TODO
            // GlobalFontSettings.FontResolver = new FontResolver();

            var document = new PdfDocument();
            var page = document.AddPage();
            var gfx = XGraphics.FromPdfPage(page);
            var font = new XFont("OpenSans", 20, XFontStyle.Bold);

            gfx.DrawString("Hello World!", font, XBrushes.Black, new XRect(20, 20, page.Width, page.Height), XStringFormats.Center);

            MemoryStream stream = new MemoryStream();
            document.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }

}
