using ClosedXML.Excel;

namespace Malte2.Model.Accounting.Edition
{
    public record OperationTableColumn {
        public Action<IXLCell> OnHeader { get; set; }
        public Action<Operation, IXLCell> OnOperation { get; set; }

        public OperationTableColumn(Action<IXLCell> onHeader, Action<Operation, IXLCell> onOperation)
        {
            OnHeader = onHeader;
            OnOperation = onOperation;
        }

        public static OperationTableColumn DateColumn()
        {
            return new OperationTableColumn(
                headerCell => {
                headerCell.SetValue("Date");
                headerCell.Style.NumberFormat.NumberFormatId = 14;
            },
                (Operation operation, IXLCell cell) => cell.SetValue(operation.OperationDateTime)
            );
        }

        public static OperationTableColumn AmountColumn(AccountingEntryType expectedAccountingEntryType, OperationEdition.TryGetOperationAccountingEntryTypeDelegate tryGetOperationAccountingEntryType)
        {
            string header;
            switch (expectedAccountingEntryType) {
                case AccountingEntryType.Expense:
                    header = "Dépenses";
                    break;
                case AccountingEntryType.Revenue:
                    header = "Recettes";
                    break;
                default:
                    throw new Exception("Unknown accounting entry type.");
            }
            return new OperationTableColumn(
                headerCell => headerCell.SetValue(header),
                (Operation operation, IXLCell cell) => cell.Value = OperationEdition.GetOperationAmount(operation, expectedAccountingEntryType, tryGetOperationAccountingEntryType)?.ToString()
            );
        }

        public static OperationTableColumn PaymentMethodColumn()
        {
            return new OperationTableColumn(
                headerCell => headerCell.SetValue("Type"),
                (Operation operation, IXLCell cell) => cell.SetValue(operation.PaymentMethod.GetDisplayString())
            );
        }

        public static OperationTableColumn AccountingEntryColumn(OperationEdition.TryGetOperationAccountingEntryDelegate tryGetOperationAccountingEntryDelegate)
        {
            return new OperationTableColumn(
                headerCell => headerCell.SetValue("Imputation"),
                (Operation operation, IXLCell cell) => {
                    if (tryGetOperationAccountingEntryDelegate(operation, out AccountingEntry? accountingEntry)) {
                        cell.SetValue(accountingEntry.Label);
                    }
                }
            );
        }

        public static OperationTableColumn BoarderColumn(OperationEdition.TryGetOperationBoarderNameDelegate tryGetOperationBoarderNameDelegate)
        {
            return new OperationTableColumn(
                headerCell => headerCell.SetValue("Pensionnaire"),
                (Operation operation, IXLCell cell) => {
                    if (tryGetOperationBoarderNameDelegate(operation, out string boarderName)) {
                        cell.SetValue(boarderName);
                    }
                }
            );
        }

        public static OperationTableColumn LabelColumn()
        {
            return new OperationTableColumn(
                headerCell => headerCell.SetValue("Libellé"),
                (Operation operation, IXLCell cell) => cell.SetValue(operation.Label)
            );
        }

        public static OperationTableColumn DetailsColumn()
        {
            return new OperationTableColumn(
                headerCell => headerCell.SetValue("Détails"),
                (Operation operation, IXLCell cell) => cell.SetValue(operation.Details)
            );
        }
    }
}
