using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;

namespace Malte2.Model.Accounting.Edition
{
    public interface DocumentEdition
    {
        public Document ProduceDocument();
    }

    public static class DocumentEditionHelper
    {
        public static MemoryStream ProducePdf(this DocumentEdition documentEdition)
        {
            Document document = documentEdition.ProduceDocument();

            // TODO use a font resolver maybe
            // GlobalFontSettings.FontResolver = new FontResolver();
            
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;
            renderer.RenderDocument();
            MemoryStream stream = new MemoryStream();
            renderer.PdfDocument.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
    }
}