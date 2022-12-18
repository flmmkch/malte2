using ClosedXML.Excel;

namespace Malte2.Model.Accounting.Edition
{
    public abstract class OperationEdition : Malte2.Model.Edition.XlsxEdition
    {
        public abstract XLWorkbook ProduceWorkbook();

        public const int LEFT_COL_START = 2;
        public const int TOP_ROW_START = 2;

        public static readonly XLColor HEADER_BG = XLColor.LightGray;
        public static readonly XLColor SECTION_HEADER_BG = XLColor.AliceBlue;
        public static readonly XLColor SHEET_HEADER_BG = XLColor.AntiqueWhite;

        public struct WorksheetSectionCoordinates {
            public int TotalsRow;
            public int RevenueColumn;
            public int ExpenseColumn;
        }

        protected static WorksheetSectionCoordinates addTableToSheet(IEnumerable<OperationTableColumn> columns, IXLWorksheet worksheet, ref int row, ref int column, IEnumerable<Operation> operations)
        {
            // Operations table headers
            row++;
            column = LEFT_COL_START;
            worksheet.Column(column).AdjustToContents();
            foreach (OperationTableColumn tableColumn in columns) {
                IXLCell headerCell = worksheet.Cell(row, column++);
                tableColumn.OnHeader(headerCell);
            }
            IXLStyle operationTableHeaderStyle = worksheet.Range(row, LEFT_COL_START, row, LEFT_COL_START + columns.Count() - 1).Style;
            SetTableBorders(operationTableHeaderStyle);
            SetTitleTextStyle(operationTableHeaderStyle);
            operationTableHeaderStyle.Fill.BackgroundColor = HEADER_BG;
            int revenueColumn = LEFT_COL_START + 1;
            int expenseColumn = revenueColumn + 1;

            int operationStartRow = row + 1;
            
            // Operations table
            foreach (Operation operation in operations) {
                row++;
                column = LEFT_COL_START;
                foreach (OperationTableColumn tableColumn in columns) {
                    IXLCell operationCell = worksheet.Cell(row, column);
                    tableColumn.OnOperation(operation, operationCell);
                    column += 1;
                }
            }
            int operationEndRow = row;
            IXLStyle operationTableContentsStyle = worksheet.Range(operationStartRow, LEFT_COL_START, operationEndRow, LEFT_COL_START + columns.Count() - 1).Style;
            SetTableBorders(operationTableContentsStyle);

            // totals
            row++;
            column = LEFT_COL_START;
            worksheet.Cell(row, column++).SetValue("Total");
            worksheet.Cell(row, revenueColumn).SetFormulaR1C1($"SUM(R{operationStartRow}C{revenueColumn}:R{operationEndRow}C{revenueColumn})");
            worksheet.Cell(row, expenseColumn).SetFormulaR1C1($"SUM(R{operationStartRow}C{expenseColumn}:R{operationEndRow}C{expenseColumn})");
            int totalsRow = row;
            row++;
            column = LEFT_COL_START;
            worksheet.Cell(row, column++).SetValue("Balance");
            worksheet.Cell(row, column++).SetFormulaR1C1($"R{totalsRow}C{revenueColumn}:R{totalsRow}C{revenueColumn} - R{totalsRow}C{expenseColumn}:R{totalsRow}C{expenseColumn}");
            SetTotalsSubtableStyle(worksheet.Range(totalsRow, LEFT_COL_START, totalsRow + 1, LEFT_COL_START).Style);
            SetTableBorders(worksheet.Range(totalsRow, LEFT_COL_START, totalsRow, expenseColumn).Style);
            SetTableBorders(worksheet.Range(totalsRow + 1, LEFT_COL_START, totalsRow + 1, expenseColumn).Style);

            // number format
            SetMonetaryNumberFormat(worksheet.Range(operationStartRow, revenueColumn, totalsRow + 1, expenseColumn).Style);

            return new WorksheetSectionCoordinates {
                TotalsRow = totalsRow,
                RevenueColumn = revenueColumn,
                ExpenseColumn = expenseColumn,
            };
        }

        protected static void SetTotalsSubtableStyle(IXLStyle style)
        {
            style.Fill.BackgroundColor = HEADER_BG;
            SetTitleTextStyle(style);
        }

        protected static void SetTableBorders(IXLStyle style)
        {
            style.Border.InsideBorder = XLBorderStyleValues.Thin;
            style.Border.InsideBorderColor = XLColor.Gray;
            style.Border.OutsideBorder = XLBorderStyleValues.Medium;
            style.Border.OutsideBorderColor = XLColor.Black;
        }

        protected static void SetTitleTextStyle(IXLStyle style)
        {
            style.Font.Bold = true;
            style.Alignment.Horizontal = XLAlignmentHorizontalValues.Center;
        }

        protected static void SetMonetaryNumberFormat(IXLStyle style)
        {
            style.NumberFormat.Format = "0.00 €";
        }

        protected static void SetSectionTitleStyle(IXLStyle style)
        {
            style.Fill.BackgroundColor = SECTION_HEADER_BG;
            SetTitleTextStyle(style);
            SetTableBorders(style);
        }

        protected static void SetWorksheetTitleStyle(IXLStyle style)
        {
            style.Fill.BackgroundColor = SHEET_HEADER_BG;
            SetTitleTextStyle(style);
            SetTableBorders(style);
        }

        protected static string GetCellSumFormula(IEnumerable<(int row, int col)> cellsCoordinates) {
            return cellsCoordinates.Select(((int row, int col) cellCoordinates) => $"R{cellCoordinates.row}C{cellCoordinates.col}").Aggregate((string acc, string cellRefFormulaStr) => $"{acc} + {cellRefFormulaStr}");
        }

        public delegate bool TryGetOperationAccountingEntryTypeDelegate(Operation operation, out AccountingEntryType accountingEntryType);

        public static TryGetOperationAccountingEntryTypeDelegate MakeTryGetOperationAccountingEntryTypeDelegateFromDict(Dictionary<long, AccountingEntry> accountingEntryById)
        {
            return (Operation operation, out AccountingEntryType entryType) => {
                bool result = accountingEntryById.TryGetValue(operation.AccountingEntryId, out AccountingEntry? accountingEntry);
                entryType = (accountingEntry?.EntryType).GetValueOrDefault();
                return result;
            };
        }

        public static Amount? GetOperationAmount(Operation operation, AccountingEntryType expectedAccountingEntryType, TryGetOperationAccountingEntryTypeDelegate tryGetOperationAccountingEntryType)
        {
            Amount? operationAmount = null;
            if (tryGetOperationAccountingEntryType(operation, out AccountingEntryType operationAccountingEntryType)) {
                if (operationAccountingEntryType == expectedAccountingEntryType) {
                    if (operationAccountingEntryType == AccountingEntryType.Revenue) {
                        operationAmount = operation.Amount;
                    } else {
                        operationAmount = - operation.Amount;
                    }
                }
            }
            else {
                throw new Exception($"Failed to get accounting entry {operation.AccountingEntryId} for operation {operation.Id}");
            }
            return operationAmount;
        }

        public static Amount? GetOperationAmount(Operation operation, TryGetOperationAccountingEntryTypeDelegate tryGetOperationAccountingEntryType)
        {
            Amount? operationAmount = null;
            if (tryGetOperationAccountingEntryType(operation, out AccountingEntryType operationAccountingEntryType)) {
                if (operationAccountingEntryType == AccountingEntryType.Revenue) {
                    operationAmount = operation.Amount;
                } else {
                    operationAmount = - operation.Amount;
                }
            }
            else {
                throw new Exception($"Failed to get accounting entry {operation.AccountingEntryId} for operation {operation.Id}");
            }
            return operationAmount;
        }

        protected Amount GetOperationsAmount(IEnumerable<Operation> operations, TryGetOperationAccountingEntryTypeDelegate tryGetOperationAccountingEntryType)
        {
            return operations.Select(op => GetOperationAmount(op, tryGetOperationAccountingEntryType)).Aggregate(new Amount(0), (accumulation, amount) => accumulation + amount.GetValueOrDefault(new Amount()));
        }

        protected static void addWorksheetTotals(IXLWorksheet worksheet, int row, IEnumerable<WorksheetSectionCoordinates> sectionCoordinates, int expenseColumn, int revenueColumn)
        {
            // add the totals
            int column = LEFT_COL_START;

            // libellés totaux page
            worksheet.Cell(row, column++).SetValue("Balance");
            worksheet.Cell(row, column++).SetValue("Total recettes");
            worksheet.Cell(row, column++).SetValue("Total dépenses");

            // montants totaux page
            worksheet.Cell(row + 1, LEFT_COL_START).SetFormulaR1C1($"R{row + 1}C{revenueColumn} - R{row + 1}C{expenseColumn}");
            worksheet.Cell(row + 1, revenueColumn).SetFormulaR1C1(GetCellSumFormula(sectionCoordinates.Select(c => (c.TotalsRow, c.RevenueColumn))));
            worksheet.Cell(row + 1, expenseColumn).SetFormulaR1C1(GetCellSumFormula(sectionCoordinates.Select(c => (c.TotalsRow, c.ExpenseColumn))));

            int lastColumn = Math.Max(revenueColumn, expenseColumn);
            IXLStyle labelsStyle = worksheet.Range(row, LEFT_COL_START, row, lastColumn).Style;
            SetTotalsSubtableStyle(labelsStyle);
            SetTableBorders(labelsStyle);
            IXLStyle valuesStyle = worksheet.Range(row + 1, LEFT_COL_START, row + 1, lastColumn).Style;
            SetTableBorders(valuesStyle);
            SetMonetaryNumberFormat(valuesStyle);
        }

        public delegate bool TryGetOperationBoarderNameDelegate(Operation operation, out string boarderName);

        public static TryGetOperationBoarderNameDelegate MakeTryGetOperationBoarderNameDelegateFromDict(Dictionary<long, string> boarderNameById)
        {
            return (Operation operation, out string boarderName) => {
                bool result = false;
                boarderName = string.Empty;
                if (operation.BoarderId != null) {
                    if (boarderNameById.TryGetValue(operation.BoarderId.Value, out string? boarderNameOpt)) {
                        boarderName = boarderNameOpt;
                        result = true;
                    }
                }
                return result;
            };
        }

        public delegate bool TryGetOperationAccountingEntryDelegate(Operation operation, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out AccountingEntry accountingEntry);

        public static TryGetOperationAccountingEntryDelegate MakeTryGetOperationAccountingEntryDelegateFromDict(Dictionary<long, AccountingEntry> accountingEntryById)
        {
            return (Operation operation, [System.Diagnostics.CodeAnalysis.MaybeNullWhen(false)] out AccountingEntry accountingEntry) => {
                return accountingEntryById.TryGetValue(operation.AccountingEntryId, out accountingEntry);
            };
        }
    }
}
