using MigraDocCore.DocumentObjectModel;

namespace Malte2.Model.Accounting.Edition
{
    public class OperationsByAccountBookEdition : OperationEdition
    {
        public OperationsByAccountBookEdition(string title, IEnumerable<Operation> operations, Dictionary<long, AccountBook> accountBooks, Dictionary<long, AccountingEntry> accountingEntries, Dictionary<long, AccountingCategory> accountingCategories)
        {
            Title = title;
            List<TableColumn> tableColumns = new List<TableColumn>();
            tableColumns.Add(new TableColumn("Date", Unit.FromCentimeter(2), operation => operation.OperationDateTime.ToShortDateString()));

            tableColumns.Add(new TableColumn("Recettes", Unit.FromCentimeter(2), operation => GetOperationAmount(operation, AccountingEntryType.Revenue, accountingEntries)?.ToCultureString(System.Globalization.CultureInfo.CurrentCulture)));
            tableColumns.Add(new TableColumn("Dépenses", Unit.FromCentimeter(2), operation => GetOperationAmount(operation, AccountingEntryType.Expense, accountingEntries)?.ToCultureString(System.Globalization.CultureInfo.CurrentCulture)));
            tableColumns.Add(new TableColumn("Type", Unit.FromCentimeter(1.6), operation => operation.PaymentMethod.GetDisplayString()));
            tableColumns.Add(new TableColumn("Catégorie", Unit.FromCentimeter(3), operation => GetAccountingCategoryLabel(operation, accountingCategories)));
            tableColumns.Add(new TableColumn("Libellé", Unit.FromCentimeter(4), operation => operation.Label));

            // group operations by account book
            Dictionary<long, List<Operation>> operationsByAccountBookId = new Dictionary<long, List<Operation>>();
            foreach (Operation operation in operations)
            {
                if (!operationsByAccountBookId.TryGetValue(operation.AccountBookId, out List<Operation>? accountBookOperations))
                {
                    accountBookOperations = new List<Operation>();
                    operationsByAccountBookId.Add(operation.AccountBookId, accountBookOperations);
                }
                accountBookOperations.Add(operation);
            }
            
            List<(AccountBook accountBook, List<Operation>)> accountBooksOperations = operationsByAccountBookId.Select(keyValue => (accountBook: accountBooks[keyValue.Key], keyValue.Value)).Where(item => item.accountBook != null).ToList();
            accountBooksOperations.Sort((left, right) => ItemsByIdComparer(left.accountBook, right.accountBook));

            foreach ((AccountBook accountBook, List<Operation> currentOperations) in accountBooksOperations) {
                var operationsTable = new OperationTable(tableColumns, currentOperations, accountingEntries);
                Amount totalAmount = GetOperationsAmount(currentOperations, accountingEntries);
                EditionContent editionContent = new TitledContentGroup("Livre comptable", accountBook.Label, 2, operationsTable, totalAmount, true);
                Content.Add(editionContent);
            }
        }
    }
}
