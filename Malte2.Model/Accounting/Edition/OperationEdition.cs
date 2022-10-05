using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace Malte2.Model.Accounting.Edition
{
    public class OperationEdition : DocumentEdition
    {
        public string Title { get; set; } = "Opérations";

        public string Subject { get; set; } = "Édition des opérations comptables";

        public string Author { get; set; } = "Au coin de Malte";

        public string? FooterDetails { get; set; } = "Au Coin de Malte, 1 rue de Malte\n75011 Paris";

        protected static Amount? GetOperationAmount(Operation operation, AccountingEntryType accountingEntryType, Dictionary<long, AccountingEntry> accountingEntries)
        {
            if (accountingEntries.TryGetValue(operation.AccountingEntryId, out AccountingEntry? accountingEntry)) {
                if (accountingEntry.EntryType == accountingEntryType) {
                    return operation.Amount;
                }
                else {
                    return null;
                }
            }
            else {
                throw new Exception($"Failed to get accounting entry {operation.AccountingEntryId} for operation {operation.Id}");
            }
        }

        protected static string GetAccountBookLabel(Operation operation, Dictionary<long, AccountBook> accountBooks)
        {
            if (accountBooks.TryGetValue(operation.AccountBookId, out AccountBook? accountBook)) {
                return accountBook.Label;
            }
            else {
                throw new Exception($"Failed to get account book {operation.AccountBookId} for operation {operation.Id}");
            }
        }

        protected static string GetAccountingEntryLabel(Operation operation, Dictionary<long, AccountingEntry> accountingEntries)
        {
            if (accountingEntries.TryGetValue(operation.AccountingEntryId, out AccountingEntry? accountingEntry)) {
                return accountingEntry.Label;
            }
            else {
                throw new Exception($"Failed to get accounting entry {operation.AccountingEntryId} for operation {operation.Id}");
            }
        }
        
        protected static string? GetAccountingCategoryLabel(Operation operation, Dictionary<long, AccountingCategory> accountingCategories)
        {
            if (operation.AccountingCategoryId != null && accountingCategories.TryGetValue(operation.AccountingCategoryId.Value, out AccountingCategory? category)) {
                return category.Label;
            }
            else if (operation.AccountingCategoryId != null) {
                throw new Exception($"Failed to get accounting category {operation.AccountingCategoryId} for operation {operation.Id}");
            }
            return null;
        }

        public class TableColumn {
            public TableColumn(string header, Unit columnWidth, Func<Operation, string?> columnValueFunc)
            {
                Header = header;
                ColumnWidth = columnWidth;
                ColumnValueFunc = columnValueFunc;
            }

            public string Header { get; set; }

            public Unit ColumnWidth { get; set; }

            public Func<Operation, string?> ColumnValueFunc { get; set; }
        }

        public interface EditionContent {
            void AddDocumentContent(Section documentSection);
        }

        public class OperationTable : EditionContent {
            public OperationTable(List<TableColumn> tableColumns, IEnumerable<Operation> operations, Dictionary<long, AccountingEntry> accountingEntries)
            {
                TableColumns = tableColumns;
                Operations = new List<Operation>(operations);
                AccountingEntries = accountingEntries;
            }

            public List<TableColumn> TableColumns { get; set; } = new List<TableColumn>();

            public List<Operation> Operations { get; private set; }

            public Dictionary<long, AccountingEntry> AccountingEntries { get; set; }


            public void SortOperationsByDate()
            {
                Operations.Sort((op1, op2) => (int) (op1.OperationDateTime - op2.OperationDateTime).TotalMinutes + (op1.OperationDateTime == op2.OperationDateTime ? (int) (op1.Id.GetValueOrDefault() - op2.Id.GetValueOrDefault()) : 0));
            }

            public void AddDocumentContent(Section documentSection)
            {
                // operations table
                // tables
                Table operationsTable = new Table();
                operationsTable.Borders.Left.Width = 0.25;
                operationsTable.Borders.Left.Color = Colors.DarkGray;
                operationsTable.Borders.Right.Width = 0.25;
                operationsTable.Borders.Right.Color = Colors.DarkGray;
                documentSection.Add(operationsTable);

                foreach (TableColumn tableColumn in TableColumns)
                {
                    operationsTable.AddColumn(tableColumn.ColumnWidth);
                }

                Row headerRow = operationsTable.AddRow();
                headerRow.Shading.Color = Colors.LightGray;

                foreach (var (tableColumn, cell) in Enumerable.Range(0, TableColumns.Count).Select(i => (TableColumns[i], headerRow.Cells[i])))
                {
                    cell.AddParagraph(tableColumn.Header);
                }
                
                SortOperationsByDate();

                bool alternateRow = true;
                foreach (Operation operation in Operations)
                {
                    Row operationRow = operationsTable.AddRow();
                    operationRow.Shading.Color = alternateRow ? Colors.WhiteSmoke : Colors.Transparent;
                    BuildOperationRow(operation, operationRow);
                    alternateRow = !alternateRow;
                }
                if (!Operations.Any()) {
                    // add an empty row
                    operationsTable.AddRow();
                }

                documentSection.AddParagraph().AddLineBreak();

                // add an invisible row
                operationsTable.AddRow().Borders.Visible = false;
                
                // subtotals
                Row subtotalsRow = operationsTable.AddRow();
                foreach (int i in Enumerable.Range(3, Math.Max(TableColumns.Count - 3, 0))) {
                    subtotalsRow.Cells[i].Borders.Visible = false;
                }
                SetTotalLabelCell(subtotalsRow.Cells[0], "Total");
                // recettes
                SetTotalValueCell(subtotalsRow.Cells[1], Operations, op => (AccountingEntries[op.AccountingEntryId].EntryType == AccountingEntryType.Revenue ? op.Amount : new Amount(0)), amount => amount > 0 ? Colors.DarkGreen : Color.Empty);
                SetTotalValueCell(subtotalsRow.Cells[2], Operations, op => (AccountingEntries[op.AccountingEntryId].EntryType == AccountingEntryType.Expense ? op.Amount : new Amount(0)), amount => amount > 0 ? Colors.DarkRed : Color.Empty);
                
                // total
                Row balanceRow = operationsTable.AddRow();
                foreach (int i in Enumerable.Range(3, Math.Max(TableColumns.Count - 3, 0))) {
                    balanceRow.Cells[i].Borders.Visible = false;
                }
                SetTotalLabelCell(balanceRow.Cells[0], "Balance");
                balanceRow.Cells[1].MergeRight = 1;
                SetTotalValueCell(balanceRow.Cells[1], Operations, op => (AccountingEntries[op.AccountingEntryId].EntryType == AccountingEntryType.Revenue ? op.Amount : - op.Amount), amount => amount > 0 ? Colors.DarkGreen : (amount < 0 ? Colors.DarkRed : Color.Empty));

                operationsTable.SetEdge(0, operationsTable.Rows.Count - 2, 3, 2, Edge.Box, BorderStyle.Single, 0.75, Colors.Black);
            }

            private void BuildOperationRow(Operation operation, Row row)
            {
                foreach ((TableColumn tableColumn, Cell cell) in Enumerable.Range(0, TableColumns.Count).Select(i => (TableColumns[i], row.Cells[i])))
                {
                    string? cellValue = tableColumn.ColumnValueFunc(operation);
                    if (cellValue != null) {
                        cell.AddParagraph(cellValue);
                    }
                }
            }

            protected void SetTotalLabelCell(Cell cell, string label)
            {
                cell.AddParagraph(label);
                cell.Format.Alignment = ParagraphAlignment.Left;
                cell.Shading.Color = Colors.LightGray;
                cell.Borders.Bottom.Width = 0.5;
            }

            protected void SetTotalValueCell(Cell cell, IEnumerable<Operation> operations, Func<Operation, Amount> amountSelector, Func<Amount, Color> fontColor)
            {
                // content
                Amount amount = operations.Select(amountSelector).Aggregate(new Amount(0), (accumulation, amount) => accumulation + amount);
                string totalAmountString = amount.ToCultureString(System.Globalization.CultureInfo.CurrentCulture);
                cell.AddParagraph($"{totalAmountString} €");
                // format
                cell.Borders.Bottom.Color = Colors.DarkGray;
                cell.Format.Alignment = ParagraphAlignment.Right;
                cell.Borders.Bottom.Color = Colors.DarkGray;
                cell.Format.Font.Bold = true;
                cell.Format.Font.Color = fontColor(amount);
            }
        }

        public class TitledContentGroup : EditionContent {
            public TitledContentGroup(Action<Paragraph> onParagraph, int level, IEnumerable<EditionContent> content)
            {
                OnParagraph = onParagraph;
                Level = level;
                Content = content;
            }

            public TitledContentGroup(Action<Paragraph> onParagraph, int level, EditionContent content)
            {
                OnParagraph = onParagraph;
                Level = level;
                Content = new EditionContent[] { content };
            }

            public Action<Paragraph> OnParagraph { get; set; }

            public int Level { get; set; }

            public IEnumerable<EditionContent> Content { get; set; }

            public void AddDocumentContent(Section documentSection)
            {
                Paragraph contentGroupTitle = documentSection.AddParagraph("", $"Heading{Level}");
                OnParagraph(contentGroupTitle);
                foreach (EditionContent content in Content)
                {
                    content.AddDocumentContent(documentSection);
                }
            }
        }

        public List<EditionContent> Content { get; private set; } = new List<EditionContent>();

        public Document ProduceDocument()
        {
            // Create a new MigraDoc document
            Document document = new Document();
            document.Info.Title = Title;
            document.Info.Subject = Subject;
            document.Info.Author = Author;

            ConfigureStyles(document);

            Section mainSection = document.AddSection();
            mainSection.PageSetup.StartingNumber = 1;
            ConfigureHeader(mainSection);
            ConfigureFooter(mainSection);

            Paragraph titleParagraph = mainSection.AddParagraph(Title, "Heading1");

            foreach (EditionContent content in Content)
            {
                content.AddDocumentContent(mainSection);
            }

            return document;
        }

        private void ConfigureStyles(Document document)
        {
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

            style = document.Styles[StyleNames.Heading2];
            style.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            style.Font.Size = 20;
            style.Font.Bold = false;
            style.ParagraphFormat.LeftIndent = Unit.FromCentimeter(2);

            style = document.Styles[StyleNames.Heading3];
            style.ParagraphFormat.Alignment = ParagraphAlignment.Left;
            style.Font.Size = 16;
            style.ParagraphFormat.LeftIndent = Unit.FromCentimeter(1.3);

            style = document.Styles[StyleNames.Header];
            style.ParagraphFormat.Alignment = ParagraphAlignment.Right;

            style = document.Styles[StyleNames.Footer];
            style.ParagraphFormat.Alignment = ParagraphAlignment.Right;
        }

        private void ConfigureHeader(Section section)
        {
            Paragraph headerParagraph = section.Headers.Primary.AddParagraph();
            headerParagraph.AddText("Document généré le ");
            headerParagraph.AddDateField(System.Globalization.CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern);
        }

        private void ConfigureFooter(Section section)
        {
            Paragraph footerPagesParagraph = section.Footers.Primary.AddParagraph();
            footerPagesParagraph.AddTab();
            footerPagesParagraph.AddText("Page ");
            footerPagesParagraph.AddPageField();
            footerPagesParagraph.AddText("/");
            footerPagesParagraph.AddNumPagesField();
            if (FooterDetails != null) {
                footerPagesParagraph.AddText("\n");
                footerPagesParagraph.AddText(FooterDetails);
            }
        }
    }
}
