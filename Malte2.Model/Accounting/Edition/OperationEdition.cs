using MigraDocCore.DocumentObjectModel;
using MigraDocCore.Rendering;

namespace Malte2.Model.Accounting.Edition
{
    public static class OperationEdition
    {
        public static MemoryStream CreateEditionPdf(IEnumerable<Operation> operations)
        {
            // TODO
            // GlobalFontSettings.FontResolver = new FontResolver();
            Document document = CreateDocument(operations);
            
            PdfDocumentRenderer renderer = new PdfDocumentRenderer(true);
            renderer.Document = document;
            renderer.RenderDocument();
            MemoryStream stream = new MemoryStream();
            renderer.PdfDocument.Save(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return stream;
        }
        
        private static Document CreateDocument(IEnumerable<Operation> operations)
        {
            // Create a new MigraDoc document
            Document document = new Document();
            document.Info.Title = "Opérations";
            document.Info.Subject = "Édition des opérations";
            document.Info.Author = "Au Coin de Malte";
        
            // DefineStyles(document);

            // DefineCover(document);
            // DefineTableOfContents(document);
            
            // DefineContentSection(document);
            
            // DefineParagraphs(document);
            // DefineTables(document);
            // DefineCharts(document);
        
            return document;
        }
    }

}
