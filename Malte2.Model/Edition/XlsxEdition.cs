using ClosedXML.Excel;

namespace Malte2.Model.Edition
{
    public interface XlsxEdition
    {
        public XLWorkbook ProduceWorkbook();
    }

    public static class XlsxEditionHelper
    {
        public static MemoryStream ProduceXlsx(this XlsxEdition xlsxEdition)
        {
            XLWorkbook workbook = xlsxEdition.ProduceWorkbook();

            MemoryStream stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}
