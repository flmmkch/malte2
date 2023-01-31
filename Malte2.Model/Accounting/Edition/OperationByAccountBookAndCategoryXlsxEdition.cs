using ClosedXML.Excel;

namespace Malte2.Model.Accounting.Edition
{
    using OperationByAccountBookAndCategoryList = List<(AccountBook accountBook, Dictionary<long, List<Operation>> operationMap)>;

    public class OperationByAccountBookAndCategoryXlsxEdition : OperationEdition
    {
        private OperationByAccountBookAndCategoryList operations;

        private readonly Dictionary<long, AccountingCategory> categoryById;
        private readonly Dictionary<long, AccountingEntry> entryById;
        private readonly Dictionary<long, string> boarderNameById;

        public string Title { get; set; }

        public OperationByAccountBookAndCategoryXlsxEdition(string title, IEnumerable<Operation> operations, Dictionary<long, AccountBook> accountBooks, Dictionary<long, AccountingEntry> entryById, Dictionary<long, AccountingCategory> categoryById, Dictionary<long, string> boarderNameById)
        {
            this.categoryById = categoryById;
            this.entryById = entryById;
            this.boarderNameById = boarderNameById;
            Title = title;

            // group operations by account book
            Dictionary<long, Dictionary<long, List<Operation>>> operationsMapsId = new Dictionary<long, Dictionary<long, List<Operation>>>();
            foreach (Operation operation in operations)
            {
                if (!operationsMapsId.TryGetValue(operation.AccountBookId, out Dictionary<long, List<Operation>>? accountBookOperations))
                {
                    accountBookOperations = new Dictionary<long, List<Operation>>();
                    operationsMapsId.Add(operation.AccountBookId, accountBookOperations);
                }

                long accountingCategoryKey = operation.AccountingCategoryId.GetValueOrDefault(IHasObjectIdHelper.NULL_OBJECT_ID_KEY);

                if (!accountBookOperations.TryGetValue(accountingCategoryKey, out List<Operation>? accountingCategoryOperations))
                {
                    accountingCategoryOperations = new List<Operation>();
                    accountBookOperations.Add(accountingCategoryKey, accountingCategoryOperations);
                }
                
                accountingCategoryOperations.Add(operation);
            }
            
            this.operations = operationsMapsId.Select(keyValue => (accountBook: accountBooks[keyValue.Key], operationsMap: keyValue.Value)).Where(item => item.accountBook != null).ToList();
            this.operations.Sort((left, right) => IHasObjectIdHelper.ItemsByIdComparer(left.accountBook, right.accountBook));
        }

        private WorksheetSectionCoordinates addCategoryTableToSheet(IXLWorksheet worksheet, ref int row, ref int column, long categoryId, IEnumerable<Operation> operations)
        {
            // Category header label
            string categoryLabel;
            if (this.categoryById.TryGetValue(categoryId, out AccountingCategory? category)) {
                categoryLabel = category.Label;
            } else {
                categoryLabel = "Autre";
            }
            worksheet.Cell(row, column).SetValue(categoryLabel);
            SetSectionTitleStyle(worksheet.Cell(row, column).Style);
            
            // Operations table
            List<OperationTableColumn> tableColumns = new List<OperationTableColumn>();
            tableColumns.Add(OperationTableColumn.DateColumn());
            tableColumns.Add(OperationTableColumn.AmountColumn(AccountingEntryType.Revenue, MakeTryGetOperationAccountingEntryTypeDelegateFromDict(this.entryById)));
            tableColumns.Add(OperationTableColumn.AmountColumn(AccountingEntryType.Expense, MakeTryGetOperationAccountingEntryTypeDelegateFromDict(this.entryById)));
            tableColumns.Add(OperationTableColumn.PaymentMethodColumn());
            if (category == null) {
                tableColumns.Add(OperationTableColumn.AccountingEntryColumn(MakeTryGetOperationAccountingEntryDelegateFromDict(this.entryById)));
            }
            if (hasBoarder(category)) {
                tableColumns.Add(OperationTableColumn.BoarderColumn(MakeTryGetOperationBoarderNameDelegateFromDict(this.boarderNameById)));
            }
            tableColumns.Add(OperationTableColumn.LabelColumn());
            tableColumns.Add(OperationTableColumn.DetailsColumn());

            WorksheetSectionCoordinates worksheetSectionCoordinates = addTableToSheet(tableColumns, worksheet, ref row, ref column, operations);

            for (int col = LEFT_COL_START; col < LEFT_COL_START + tableColumns.Count; col++) {
                worksheet.Column(col).AdjustToContents();
            }

            return worksheetSectionCoordinates;
        }

        public override XLWorkbook ProduceWorkbook()
        {
            XLWorkbook workbook = new XLWorkbook();
            foreach ((AccountBook accountBook, Dictionary<long, List<Operation>> operationsByCategory) in this.operations) {
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

                foreach ((long categoryId, List<Operation> categoryOperations) in operationsByCategory.Where((categoryOperationsPair) => categoryOperationsPair.Key != IHasObjectIdHelper.NULL_OBJECT_ID_KEY)) {
                    row += 2;
                    column = LEFT_COL_START;
                    WorksheetSectionCoordinates coordinates = addCategoryTableToSheet(worksheet, ref row, ref column, categoryId, categoryOperations);
                    sectionCoordinates.Add(coordinates);
                }
                if (operationsByCategory.TryGetValue(IHasObjectIdHelper.NULL_OBJECT_ID_KEY, out List<Operation>? noCategoryOperations)) {
                    row += 2;
                    column = LEFT_COL_START;
                    WorksheetSectionCoordinates coordinates = addCategoryTableToSheet(worksheet, ref row, ref column, IHasObjectIdHelper.NULL_OBJECT_ID_KEY, noCategoryOperations);
                    sectionCoordinates.Add(coordinates);
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
