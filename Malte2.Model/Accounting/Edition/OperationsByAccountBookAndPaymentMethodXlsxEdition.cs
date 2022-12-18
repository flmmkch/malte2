using ClosedXML.Excel;

namespace Malte2.Model.Accounting.Edition
{
    using OperationByAccountBookAndPaymentMethodList = List<(AccountBook accountBook, Dictionary<PaymentMethod, List<Operation>> operationMap)>;

    public class OperationByAccountBookAndPaymentMethodXlsxEdition : OperationEdition
    {
        private OperationByAccountBookAndPaymentMethodList operations;

        private readonly Dictionary<long, AccountingCategory> categoryById;
        private readonly Dictionary<long, AccountingEntry> entryById;
        private readonly Dictionary<long, string> boarderNameById;

        public string Title { get; set; }

        public OperationByAccountBookAndPaymentMethodXlsxEdition(string title, IEnumerable<Operation> operations, Dictionary<long, AccountBook> accountBooks, Dictionary<long, AccountingEntry> entryById, Dictionary<long, AccountingCategory> categoryById, Dictionary<long, string> boarderNameById)
        {
            this.categoryById = categoryById;
            this.entryById = entryById;
            this.boarderNameById = boarderNameById;
            Title = title;

            // group operations by account book
            Dictionary<long, Dictionary<PaymentMethod, List<Operation>>> operationsMapsId = new Dictionary<long, Dictionary<PaymentMethod, List<Operation>>>();
            foreach (Operation operation in operations)
            {
                if (!operationsMapsId.TryGetValue(operation.AccountBookId, out Dictionary<PaymentMethod, List<Operation>>? accountBookOperations))
                {
                    accountBookOperations = new Dictionary<PaymentMethod, List<Operation>>();
                    operationsMapsId.Add(operation.AccountBookId, accountBookOperations);
                }

                PaymentMethod paymentMethod = operation.PaymentMethod;

                if (!accountBookOperations.TryGetValue(paymentMethod, out List<Operation>? accountingCategoryOperations))
                {
                    accountingCategoryOperations = new List<Operation>();
                    accountBookOperations.Add(paymentMethod, accountingCategoryOperations);
                }
                
                accountingCategoryOperations.Add(operation);
            }
            
            this.operations = operationsMapsId.Select(keyValue => (accountBook: accountBooks[keyValue.Key], operationsMap: keyValue.Value)).Where(item => item.accountBook != null).ToList();
            this.operations.Sort((left, right) => IHasObjectIdHelper.ItemsByIdComparer(left.accountBook, right.accountBook));
        }

        private WorksheetSectionCoordinates addPaymentMethodTableToSheet(IXLWorksheet worksheet, ref int row, ref int column, PaymentMethod paymentMethod, IEnumerable<Operation> operations)
        {
            // Section header label
            worksheet.Cell(row, column).SetValue(paymentMethod.GetDisplayString());
            SetSectionTitleStyle(worksheet.Cell(row, column).Style);
            
            // Operations table
            List<OperationTableColumn> tableColumns = new List<OperationTableColumn>();
            tableColumns.Add(OperationTableColumn.DateColumn());
            tableColumns.Add(OperationTableColumn.AmountColumn(AccountingEntryType.Revenue, MakeTryGetOperationAccountingEntryTypeDelegateFromDict(this.entryById)));
            tableColumns.Add(OperationTableColumn.AmountColumn(AccountingEntryType.Expense, MakeTryGetOperationAccountingEntryTypeDelegateFromDict(this.entryById)));
            tableColumns.Add(OperationTableColumn.AccountingEntryColumn(MakeTryGetOperationAccountingEntryDelegateFromDict(this.entryById)));
            tableColumns.Add(OperationTableColumn.BoarderColumn(MakeTryGetOperationBoarderNameDelegateFromDict(this.boarderNameById)));
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
            foreach ((AccountBook accountBook, Dictionary<PaymentMethod, List<Operation>> operationsByPaymentMethod) in this.operations) {
                IXLWorksheet worksheet = workbook.AddWorksheet($"Livre {accountBook.Label}");
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

                foreach ((PaymentMethod paymentMethod, List<Operation> operations) in operationsByPaymentMethod) {
                    row += 2;
                    column = LEFT_COL_START;
                    WorksheetSectionCoordinates coordinates = addPaymentMethodTableToSheet(worksheet, ref row, ref column, paymentMethod, operations);
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
