using ClosedXML.Excel;

namespace Malte2.Model.Accounting.Edition
{
    public class RemissionEdition : Malte2.Model.Edition.XlsxEdition
    {

        public static readonly XLColor HEADER_BG = XLColor.LightGray;
        public static readonly XLColor SHEET_HEADER_BG = XLColor.AntiqueWhite;

        public const int LEFT_COL_START = 2;
        public const int TOP_ROW_START = 2;
        public string Title { get; set; }

        public List<Remission> remissions { get; set; }

        public Dictionary<long, Operator> operators { get; set; }

        public RemissionEdition(string title, IEnumerable<Remission> remissions, Dictionary<long, Operator> operators)
        {
            Title = title;
            
            this.remissions = new List<Remission>(remissions);
            this.operators = operators;
        }

        public XLWorkbook ProduceWorkbook()
        {
            XLWorkbook workbook = new XLWorkbook();
            
            IXLWorksheet worksheet = workbook.AddWorksheet(Malte2.Model.Edition.XlsxEditionHelper.ShortenSheetName($"{Title}"));

            const int COLS_COUNT = 4;
            
            int row = TOP_ROW_START;
            int column = LEFT_COL_START;
            int worksheetTitleTopRow = row;
            int worksheetTitleBottomRow = row;
            worksheet.Cell(row, column).SetValue("Dépôts bancaires");
            if (!string.IsNullOrWhiteSpace(Title)) {
                row++;
                column = LEFT_COL_START;
                worksheet.Cell(row, column).SetValue(Title);
                worksheetTitleBottomRow = row;
                row++;
            }
            SetWorksheetTitleStyle(worksheet.Range(worksheetTitleTopRow, LEFT_COL_START, worksheetTitleBottomRow, LEFT_COL_START).Style);

            // skip 1 row
            row++;

            // Remissions table

            // Remissions table headers
            row++;
            column = LEFT_COL_START;
            worksheet.Column(column).AdjustToContents();
            worksheet.Cell(row, column++).SetValue("Date");
            worksheet.Cell(row, column++).SetValue("Total");
            worksheet.Cell(row, column++).SetValue("Espèces");
            worksheet.Cell(row, column++).SetValue("Chèques");
            IXLStyle operationTableHeaderStyle = worksheet.Range(row, LEFT_COL_START, row, LEFT_COL_START + COLS_COUNT - 1).Style;
            SetTableBorders(operationTableHeaderStyle);
            SetTitleTextStyle(operationTableHeaderStyle);
            operationTableHeaderStyle.Fill.BackgroundColor = HEADER_BG;

            int operationStartRow = row + 1;
            int operationEndRow = operationStartRow;

            if (remissions.Count > 0) {
                // Remission rows
                foreach (Remission remission in remissions) {
                    row++;
                    column = LEFT_COL_START;
                    // date
                    worksheet.Cell(row, column).Value = remission.DateTime;
                    column++;

                    // total
                    worksheet.Cell(row, column).SetFormulaR1C1($"SUM(R{row}C{column + 1}:R{row}C{column + 2})");
                    column++;

                    // cash
                    worksheet.Cell(row, column).Value = remission.CashDeposits.Select(cashDeposit => cashDeposit.CalculateAmount()).Aggregate(new Amount(), (sum, amount) => sum + amount);
                    column++;

                    // checks
                    worksheet.Cell(row, column).Value = remission.CheckRemissions.Select(checkRemission => checkRemission.Amount).Aggregate(new Amount(), (sum, amount) => sum + amount);
                    column++;
                }
            }
            else {
                row++;
            }
            operationEndRow = row;
            IXLStyle operationTableContentsStyle = worksheet.Range(operationStartRow, LEFT_COL_START, operationEndRow, LEFT_COL_START + COLS_COUNT - 1).Style;
            SetTableBorders(operationTableContentsStyle);

            // totals
            row++;
            worksheet.Cell(row, LEFT_COL_START).SetValue("Total");
            worksheet.Cell(row, LEFT_COL_START + 1).SetFormulaR1C1($"R{row}C{LEFT_COL_START + 2} + R{row}C{LEFT_COL_START + 3}");
            worksheet.Cell(row, LEFT_COL_START + 2).SetFormulaR1C1($"SUM(R{operationStartRow}C{LEFT_COL_START + 2}:R{operationEndRow}C{LEFT_COL_START + 2})");
            worksheet.Cell(row, LEFT_COL_START + 3).SetFormulaR1C1($"SUM(R{operationStartRow}C{LEFT_COL_START + 3}:R{operationEndRow}C{LEFT_COL_START + 3})");
            int totalsRow = row;
            row++;
            SetTotalsSubtableStyle(worksheet.Range(totalsRow, LEFT_COL_START, totalsRow, LEFT_COL_START).Style);
            SetTableBorders(worksheet.Range(totalsRow, LEFT_COL_START, totalsRow, LEFT_COL_START + COLS_COUNT - 1).Style);

            // number format
            SetMonetaryNumberFormat(worksheet.Range(operationStartRow, LEFT_COL_START + 1, totalsRow, LEFT_COL_START + 3).Style);

            for (int col = LEFT_COL_START; col < LEFT_COL_START + COLS_COUNT - 1; col++) {
                worksheet.Column(col).AdjustToContents();
            }

            return workbook;
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
            style.NumberFormat.Format = "0.00 €;[RED]-0.00 €";
        }

        protected static void SetWorksheetTitleStyle(IXLStyle style)
        {
            style.Fill.BackgroundColor = SHEET_HEADER_BG;
            SetTitleTextStyle(style);
            SetTableBorders(style);
        }
    }
}
