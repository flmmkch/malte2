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
            tableColumns.Add(new TableColumn("Catégorie", Unit.FromCentimeter(4), operation => GetAccountingCategoryLabel(operation, accountingCategories)));
            tableColumns.Add(new TableColumn("Libellé", Unit.FromCentimeter(6), operation => operation.Label));

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
            
            List<(AccountBook, List<Operation>)> accountBooksOperations = operationsByAccountBookId.Select(keyValue => (accountBooks[keyValue.Key], keyValue.Value)).Where(item => item.Item1 != null).ToList();
            accountBooksOperations.Sort((left, right) => (int) (left.Item1.Id - right.Item1.Id).GetValueOrDefault());

            foreach ((AccountBook accountBook, List<Operation> currentOperations) in accountBooksOperations) {
                var operationsTable = new OperationTable(tableColumns, currentOperations, accountingEntries);
                var editionGroup = new TitledContentGroup(paragraph => {
                    paragraph.AddText("Livre comptable ");
                    paragraph.AddFormattedText(accountBook.Label, TextFormat.Bold);
                }, 2, operationsTable);
                Content.Add(editionGroup);
            }

        }
    }
}
