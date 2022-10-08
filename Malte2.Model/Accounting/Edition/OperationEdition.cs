using MigraDocCore.DocumentObjectModel;
using MigraDocCore.DocumentObjectModel.Tables;

namespace Malte2.Model.Accounting.Edition
{
    public class OperationEdition : DocumentEdition
    {
        protected const long NULL_OBJECT_ID_KEY = long.MaxValue;

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

        protected static Amount GetOperationAmount(Operation operation, Dictionary<long, AccountingEntry> accountingEntries)
        {
            if (accountingEntries.TryGetValue(operation.AccountingEntryId, out AccountingEntry? accountingEntry)) {
                if (accountingEntry.EntryType == AccountingEntryType.Revenue) {
                    return operation.Amount;
                }
                else {
                    return - operation.Amount;
                }
            }
            else {
                throw new Exception($"Failed to get accounting entry {operation.AccountingEntryId} for operation {operation.Id}");
            }
        }

        protected static Amount GetOperationsAmount(IEnumerable<Operation> operations, Dictionary<long, AccountingEntry> accountingEntries)
        {
            return operations.Select(op => GetOperationAmount(op, accountingEntries)).Aggregate(new Amount(0), (accumulation, amount) => accumulation + amount);
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
            bool PageBreakBefore { get; }

            void AddDocumentContent(DocumentElements documentElements);
        }

        public class OperationTable : EditionContent {
            public OperationTable(List<TableColumn> tableColumns, IEnumerable<Operation> operations, Dictionary<long, AccountingEntry> accountingEntries)
            {
                TableColumns = tableColumns;
                Operations = new List<Operation>(operations);
                AccountingEntries = accountingEntries;
            }

            public bool PageBreakBefore { get => false; }

            public List<TableColumn> TableColumns { get; set; } = new List<TableColumn>();

            public List<Operation> Operations { get; private set; }

            public Dictionary<long, AccountingEntry> AccountingEntries { get; set; }


            public void SortOperationsByDate()
            {
                Operations.Sort((op1, op2) => (int) (op1.OperationDateTime - op2.OperationDateTime).TotalMinutes + (op1.OperationDateTime == op2.OperationDateTime ? (int) (op1.Id.GetValueOrDefault() - op2.Id.GetValueOrDefault()) : 0));
            }

            public void AddDocumentContent(DocumentElements documentElements)
            {
                // operations table
                // tables
                Table operationsTable = new Table();
                operationsTable.Borders.Left.Width = 0.25;
                operationsTable.Borders.Left.Color = Colors.DarkGray;
                operationsTable.Borders.Right.Width = 0.25;
                operationsTable.Borders.Right.Color = Colors.DarkGray;
                documentElements.Add(operationsTable);

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

                documentElements.AddParagraph().AddLineBreak();
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
        }

        public class TitledContentGroup : EditionContent {
            private void internalConstructor(string typeLabel, string groupName, int level, IEnumerable<EditionContent> content, Amount totalAmount, bool pageBreakBefore)
            {
                TypeLabel = typeLabel;
                GroupName = groupName;
                Level = level;
                Content = content;
                TotalAmount = totalAmount;
                PageBreakBefore = pageBreakBefore;
            }

            public TitledContentGroup(string typeLabel, string groupName, int level, IEnumerable<EditionContent> content, Amount totalAmount, bool pageBreakBefore = false)
            {
                internalConstructor(typeLabel, groupName, level, content, totalAmount, pageBreakBefore);
            }

            public TitledContentGroup(string typeLabel, string groupName, int level, EditionContent content, Amount totalAmount, bool pageBreakBefore = false)
            {
                internalConstructor(typeLabel, groupName, level, new EditionContent[] { content }, totalAmount, pageBreakBefore);
            }

            public string TypeLabel { get; set; } = "";

            public string GroupName { get; set; } = "";

            public bool Border { get; set; } = false;

            public bool PageBreakBefore { get; set; } = false;

            public int Level { get; set; } = 1;

            public IEnumerable<EditionContent> Content { get; set; } = Enumerable.Empty<EditionContent>();

            public Amount TotalAmount { get; set; } = new Amount(0);

            public List<DocumentObject> Details { get; private set; } = new List<DocumentObject>();

            private void ParagraphAddTitle(Paragraph paragraph)
            {
                if (!string.IsNullOrWhiteSpace(TypeLabel)) {
                    paragraph.AddText(TypeLabel);
                }
                if (!string.IsNullOrWhiteSpace(TypeLabel) && !string.IsNullOrEmpty(GroupName)) {
                    paragraph.AddText(" ");
                }
                if (!string.IsNullOrEmpty(GroupName)) {
                    paragraph.AddFormattedText(GroupName, TextFormat.Bold);
                }
            }

            public void AddDocumentContent(DocumentElements documentElements)
            {
                Table contentGroupTable = new Table();
                documentElements.Add(contentGroupTable);
                contentGroupTable.Format.Borders.Visible = false;
                contentGroupTable.AddColumn(Unit.FromCentimeter(Level * 0.4));
                contentGroupTable.AddColumn(Unit.FromCentimeter(18 - (Level * 0.8)));
                int contentColumn = contentGroupTable.Columns.Count - 1;

                Row titleRow = contentGroupTable.AddRow();
                Paragraph contentGroupTitle = new Paragraph();
                titleRow.Cells[contentColumn].Add(contentGroupTitle);
                contentGroupTitle.Style = $"Heading{Level}";
                ParagraphAddTitle(contentGroupTitle);
                if (Details.Count > 0) {
                    Row detailsRow = contentGroupTable.AddRow();
                    foreach (DocumentObject detailObject in Details)
                    {
                        detailsRow.Cells[contentColumn].Elements.Add(detailObject);
                    }
                }
                // content
                foreach (EditionContent content in Content)
                {
                    Row contentRow = contentGroupTable.AddRow();
                    content.AddDocumentContent(contentRow.Cells[contentColumn].Elements);
                }
                
                Row totalRow = contentGroupTable.AddRow();
                // total
                Paragraph totalParagraph = new Paragraph();
                totalParagraph.Style = "ContentGroupTotal";
                totalParagraph.AddText("Total");
                totalParagraph.AddSpace(1);
                if (!string.IsNullOrEmpty(GroupName)) {
                    totalParagraph.AddFormattedText(GroupName, TextFormat.Bold);
                    totalParagraph.AddSpace(1);
                }
                string totalAmountString = TotalAmount.ToCultureString(System.Globalization.CultureInfo.CurrentCulture);
                string totalAmountValueStyle = (TotalAmount > 0) ? "ContentGroupTotalPositiveAmount" : ((TotalAmount < 0) ? "ContentGroupTotalNegativeAmount" : "ContentGroupTotalZeroAmount");
                totalParagraph.AddFormattedText($"{totalAmountString} €", totalAmountValueStyle);
                totalRow.Cells[contentColumn].Add(totalParagraph);

                if (Border)
                {
                    contentGroupTable.SetEdge(1, 0, 1, contentGroupTable.Rows.Count, Edge.Box, BorderStyle.Single, 0.5, Colors.LightGray);
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

            var contentEnumerator = Content.GetEnumerator();
            if (contentEnumerator.MoveNext()) {
                // no page break for the first content item
                contentEnumerator.Current.AddDocumentContent(mainSection.Elements);
            }
            while (contentEnumerator.MoveNext()) {
                if (contentEnumerator.Current.PageBreakBefore) {
                    mainSection.AddPageBreak();
                }
                contentEnumerator.Current.AddDocumentContent(mainSection.Elements);
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

            foreach (int level in Enumerable.Range(2, 5)) {
                style = document.Styles[$"Heading{level}"];
                style.ParagraphFormat.Alignment = ParagraphAlignment.Left;
                style.Font.Size = 24 - 2 * level;
                style.Font.Bold = false;
            }

            style = document.Styles[StyleNames.Header];
            style.ParagraphFormat.Alignment = ParagraphAlignment.Right;

            style = document.Styles[StyleNames.Footer];
            style.ParagraphFormat.Alignment = ParagraphAlignment.Right;

            style = document.AddStyle("ContentGroupTotal", "Normal");

            style = document.AddStyle("ContentGroupTotalZeroAmount", "Normal");
            style.Font.Bold = true;

            style = document.AddStyle("ContentGroupTotalPositiveAmount", "ContentGroupTotalZeroAmount");
            style.Font.Color = Colors.DarkGreen;

            style = document.AddStyle("ContentGroupTotalNegativeAmount", "ContentGroupTotalZeroAmount");
            style.Font.Color = Colors.DarkRed;
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

        protected static int ItemsByIdComparer<T>(T? left, T? right) where T: IHasObjectId
        {
            long comparison = (left?.Id).GetValueOrDefault(NULL_OBJECT_ID_KEY).CompareTo((right?.Id).GetValueOrDefault(NULL_OBJECT_ID_KEY));
            return (int) comparison;
        }
    }
}
