using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;
using MigraDocCore.Rendering;

namespace Malte2.Model.Accounting.Edition
{
    public static class OperationEdition
    {
        public static MemoryStream RenderDocumentPdf(Document document)
        {
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

        public static Document CreateOperationsDocument(string title, IEnumerable<Operation> operations, Dictionary<long, AccountBook> accountBooks, Dictionary<long, AccountingEntry> accountingEntries, Dictionary<long, AccountingCategory> categories)
        {
            // Create a new MigraDoc document
            Document document = new Document();
            document.Info.Title = "Opérations";
            document.Info.Subject = "Édition des opérations comptables";
            document.Info.Author = "Au Coin de Malte";

            // styles
            // Get the predefined style Normal.
            Style style = document.Styles["Normal"];
            // Because all styles are derived from Normal, the next line changes the 
            // font of the whole document. Or, more exactly, it changes the font of
            // all styles and paragraphs that do not redefine the font.
            style.Font.Name = "Arial";
            
            style = document.Styles[StyleNames.Heading1];
            style.ParagraphFormat.Alignment = ParagraphAlignment.Center;
            style.Font.Size = 24;
            style.Font.Bold = true;
            style.ParagraphFormat.SpaceAfter = 6;

            style = document.Styles[StyleNames.Header];
            style.ParagraphFormat.Alignment = ParagraphAlignment.Right;

            style = document.Styles[StyleNames.Footer];
            style.ParagraphFormat.Alignment = ParagraphAlignment.Right;

            // content section
            Section section = document.AddSection();
            section.PageSetup.StartingNumber = 1;

            Paragraph headerParagraph = section.Headers.Primary.AddParagraph();
            headerParagraph.AddText("Document généré le ");
            headerParagraph.AddDateField(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);

            // Create a paragraph with centered page number. See definition of style "Footer".
            Paragraph footerPagesParagraph = section.Footers.Primary.AddParagraph();
            footerPagesParagraph.AddTab();
            footerPagesParagraph.AddText("Page ");
            footerPagesParagraph.AddPageField();
            footerPagesParagraph.AddText("/");
            footerPagesParagraph.AddNumPagesField();
            footerPagesParagraph.AddText("\nAu Coin de Malte, 1 rue de Malte");
            footerPagesParagraph.AddText("\n75011 Paris");

            // tables
            Paragraph titleParagraph = document.LastSection.AddParagraph(title, "Heading1");

            Table operationsTable = new Table();
            operationsTable.Borders.Left.Width = 0.25;
            operationsTable.Borders.Left.Color = Colors.DarkGray;
            operationsTable.Borders.Right.Width = 0.25;
            operationsTable.Borders.Right.Color = Colors.DarkGray;
            document.LastSection.Add(operationsTable);

            Column column = operationsTable.AddColumn(Unit.FromCentimeter(2));
            column.Format.Alignment = ParagraphAlignment.Center;

            operationsTable.AddColumn(Unit.FromCentimeter(2));
            operationsTable.AddColumn(Unit.FromCentimeter(2));

            operationsTable.AddColumn(Unit.FromCentimeter(2.5));
            operationsTable.AddColumn(Unit.FromCentimeter(2.5));

            operationsTable.AddColumn(Unit.FromCentimeter(4));

            Row headerRow = operationsTable.AddRow();
            headerRow.Shading.Color = Colors.LightGray;

            List<string> headers = new List<string>(new string[] { "Date", "Recettes", "Dépenses", "Livre", "Catégorie", "Libellé" });
            foreach (var headerWithCell in Enumerable.Range(0, headers.Count).Select(i => (headers[i], headerRow.Cells[i]))) {
                headerWithCell.Item2.AddParagraph(headerWithCell.Item1);
            }

            List<Operation> sortedOperations = operations.ToList();
            sortedOperations.Sort((op1, op2) => (int) (op1.OperationDateTime - op2.OperationDateTime).TotalMinutes + (op1.OperationDateTime == op2.OperationDateTime ? (int) (op1.Id.GetValueOrDefault() - op2.Id.GetValueOrDefault()) : 0));
            bool alternateRow = true;
            foreach (Operation operation in operations) {
                Row operationRow = operationsTable.AddRow();
                operationRow.Shading.Color = alternateRow ? Colors.WhiteSmoke : Colors.Transparent;
                BuildOperationRow(operation, operationRow, accountBooks, accountingEntries, categories);
                alternateRow = !alternateRow;
            }
            if (!operations.Any()) {
                // add an empty row
                operationsTable.AddRow();
            }

            document.LastSection.AddParagraph().AddLineBreak();

            // add an invisible row
            operationsTable.AddRow().Borders.Visible = false;
            
            BuildTotalRow(operations, operationsTable.AddRow(), "Total recettes", op => (accountingEntries[op.AccountingEntryId].EntryType == AccountingEntryType.Revenue ? op.Amount : new Amount(0)), amount => amount > 0 ? Colors.DarkGreen : Color.Empty);
            BuildTotalRow(operations, operationsTable.AddRow(), "Total dépenses", op => (accountingEntries[op.AccountingEntryId].EntryType == AccountingEntryType.Expense ? op.Amount : new Amount(0)), amount => amount > 0 ? Colors.DarkRed : Color.Empty);
            BuildTotalRow(operations, operationsTable.AddRow(), "Balance", op => (accountingEntries[op.AccountingEntryId].EntryType == AccountingEntryType.Revenue ? op.Amount : - op.Amount), amount => amount > 0 ? Colors.DarkGreen : (amount < 0 ? Colors.DarkRed : Color.Empty));

            operationsTable.SetEdge(operationsTable.Columns.Count - 3, operationsTable.Rows.Count - 3, 3, 3, Edge.Box, BorderStyle.Single, 0.75, Colors.Black);
            
            return document;
        }

        private static void BuildTotalRow(IEnumerable<Operation> operations, Row row, string rowLabel, Func<Operation, Amount> amountSelector, Func<Amount, Color> fontColor)
        {
            // content
            Amount amount = operations.Select(amountSelector).Aggregate(new Amount(0), (accumulation, amount) => accumulation + amount);
            row.Cells[3].AddParagraph(rowLabel);
            string totalAmountString = amount.ToCultureString(System.Globalization.CultureInfo.CurrentCulture);
            row.Cells[5].AddParagraph($"{totalAmountString} €");
            // format
            foreach (int i in Enumerable.Range(0, 3)) {
                row.Cells[i].Borders.Visible = false;
            }
            row.Cells[3].Format.Alignment = ParagraphAlignment.Left;
            row.Cells[3].Shading.Color = Colors.LightGray;
            row.Cells[3].MergeRight = 1;
            row.Cells[3].Borders.Bottom.Width = 0.5;
            row.Cells[3].Borders.Bottom.Color = Colors.DarkGray;
            row.Cells[5].Format.Alignment = ParagraphAlignment.Right;
            row.Cells[5].Borders.Bottom.Color = Colors.DarkGray;
            row.Cells[5].Format.Font.Bold = true;
            row.Cells[5].Format.Font.Color = fontColor(amount);
        }

        private static void BuildOperationRow(Operation operation, Row row, Dictionary<long, AccountBook> accountBooks, Dictionary<long, AccountingEntry> accountingEntries, Dictionary<long, AccountingCategory> categories)
        {
            Cell cell;
            cell = row.Cells[0];
            cell.AddParagraph(operation.OperationDateTime.ToShortDateString());
            if (accountingEntries.TryGetValue(operation.AccountingEntryId, out AccountingEntry? accountingEntry)) {
                if (accountingEntry.EntryType == AccountingEntryType.Revenue) {
                    cell = row.Cells[1];
                }
                else if (accountingEntry.EntryType == AccountingEntryType.Expense) {
                    cell = row.Cells[2];
                }
                cell.AddParagraph(operation.Amount.ToCultureString(System.Globalization.CultureInfo.CurrentCulture));
            }
            else {
                throw new Exception($"Failed to get accounting entry {operation.AccountingEntryId} for operation {operation.Id}");
            }
            if (accountBooks.TryGetValue(operation.AccountBookId, out AccountBook? accountBook)) {
                cell = row.Cells[3];
                cell.AddParagraph(accountBook.Label);
            }
            else {
                throw new Exception($"Failed to get account book {operation.AccountBookId} for operation {operation.Id}");
            }
            if (operation.AccountingCategoryId != null && categories.TryGetValue(operation.AccountingCategoryId.Value, out AccountingCategory? category)) {
                cell = row.Cells[4];
                cell.AddParagraph(category.Label);
            }
            else if (operation.AccountingCategoryId != null) {
                throw new Exception($"Failed to get accounting category {operation.AccountingCategoryId} for operation {operation.Id}");
            }
            cell = row.Cells[5];
            cell.AddParagraph(operation.Label);
        }
    }

}
