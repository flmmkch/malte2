using ClosedXML.Excel;

namespace Malte2.Model.Accounting.Edition
{
    using OperationByAccountBook = List<(AccountBook accountBook, List<Operation> operations)>;

    public class OperationByAccountBookXlsxEdition : OperationEdition
    {
        private OperationByAccountBook operations;

        private readonly Dictionary<long, AccountingCategory> categoryById;
        private readonly Dictionary<long, AccountingEntry> entryById;
        private readonly Dictionary<long, string> boarderNameById;

        public string Title { get; set; }

        public OperationByAccountBookXlsxEdition(string title, IEnumerable<Operation> operations, Dictionary<long, AccountBook> accountBooks, Dictionary<long, AccountingEntry> entryById, Dictionary<long, AccountingCategory> categoryById, Dictionary<long, string> boarderNameById)
        {
            this.categoryById = categoryById;
            this.entryById = entryById;
            this.boarderNameById = boarderNameById;
            Title = title;

            // group operations by account book
            Dictionary<long, List<Operation>> operationsMapsId = new Dictionary<long, List<Operation>>();
            foreach (Operation operation in operations)
            {
                if (!operationsMapsId.TryGetValue(operation.AccountBookId, out List<Operation>? accountBookOperations))
                {
                    accountBookOperations = new List<Operation>();
                    operationsMapsId.Add(operation.AccountBookId, accountBookOperations);
                }

                accountBookOperations.Add(operation);
            }
            
            this.operations = operationsMapsId.Select(keyValue => (accountBook: accountBooks[keyValue.Key], operationsMap: keyValue.Value)).Where(item => item.accountBook != null).ToList();
            this.operations.Sort((left, right) => IHasObjectIdHelper.ItemsByIdComparer(left.accountBook, right.accountBook));
        }

        public override XLWorkbook ProduceWorkbook()
        {
            XLWorkbook workbook = new XLWorkbook();
            foreach ((AccountBook accountBook, List<Operation> operations) in this.operations) {
                IXLWorksheet worksheet = workbook.AddWorksheet(Malte2.Model.Edition.XlsxEditionHelper.ShortenSheetName($"Livre {accountBook.Label}"));
                int row = TOP_ROW_START;
                int column = LEFT_COL_START;
                int worksheetTitleTopRow = row;
                int worksheetTitleBottomRow = row;
                worksheet.Cell(row, column).SetValue("Opérations comptables");
                if (!string.IsNullOrWhiteSpace(Title)) {
                    row++;
                    column = LEFT_COL_START;
                    worksheet.Cell(row, column).SetValue(Title);
                    worksheetTitleBottomRow = row;
                    row++;
                }
                SetWorksheetTitleStyle(worksheet.Range(worksheetTitleTopRow, LEFT_COL_START, worksheetTitleBottomRow, LEFT_COL_START).Style);
                int bookTotalsRow = row;
                // leave 2 rows for the totals and skip 1 row
                row += 2;

                List<WorksheetSectionCoordinates> sectionCoordinates = new List<WorksheetSectionCoordinates>();

                // Operations table
                List<OperationTableColumn> tableColumns = new List<OperationTableColumn>();
                tableColumns.Add(OperationTableColumn.DateColumn());
                tableColumns.Add(OperationTableColumn.AmountColumn(AccountingEntryType.Revenue, MakeTryGetOperationAccountingEntryTypeDelegateFromDict(this.entryById)));
                tableColumns.Add(OperationTableColumn.AmountColumn(AccountingEntryType.Expense, MakeTryGetOperationAccountingEntryTypeDelegateFromDict(this.entryById)));
                tableColumns.Add(OperationTableColumn.PaymentMethodColumn());
                tableColumns.Add(OperationTableColumn.AccountingEntryColumn(MakeTryGetOperationAccountingEntryDelegateFromDict(this.entryById)));
                tableColumns.Add(OperationTableColumn.BoarderColumn(MakeTryGetOperationBoarderNameDelegateFromDict(this.boarderNameById)));
                tableColumns.Add(OperationTableColumn.LabelColumn());
                tableColumns.Add(OperationTableColumn.DetailsColumn());

                WorksheetSectionCoordinates worksheetSectionCoordinates = addTableToSheet(tableColumns, worksheet, ref row, ref column, operations);
                sectionCoordinates.Add(worksheetSectionCoordinates);

                for (int col = LEFT_COL_START; col < LEFT_COL_START + tableColumns.Count; col++) {
                    worksheet.Column(col).AdjustToContents();
                }

                // add the totals
                addWorksheetTotals(worksheet, bookTotalsRow, sectionCoordinates, LEFT_COL_START + 1, LEFT_COL_START + 2);
                worksheet.Column(LEFT_COL_START + 1).AdjustToContents();
                worksheet.Column(LEFT_COL_START + 2).AdjustToContents();
            }
            return workbook;
        }

        private bool hasBoarder(AccountingCategory? category) {
            return (category?.AccountingEntryId == null)
                || (this.entryById.TryGetValue(category.AccountingEntryId.Value, out AccountingEntry? entry) && entry.HasBoarder);
        }
    }
}
