using MigraDocCore.DocumentObjectModel;

namespace Malte2.Model.Accounting.Edition
{
    public class OperationsByAccountBookAndPaymentMethodEdition : OperationEdition
    {
        public OperationsByAccountBookAndPaymentMethodEdition(string title, IEnumerable<Operation> operations, Dictionary<long, AccountBook> accountBooks, Dictionary<long, AccountingEntry> accountingEntries, Dictionary<long, AccountingCategory> accountingCategories)
        {
            Title = title;
            List<TableColumn> tableColumns = new List<TableColumn>();
            tableColumns.Add(new TableColumn("Date", Unit.FromCentimeter(2), operation => operation.OperationDateTime.ToShortDateString()));

            tableColumns.Add(new TableColumn("Recettes", Unit.FromCentimeter(2), operation => GetOperationAmount(operation, AccountingEntryType.Revenue, accountingEntries)?.ToCultureString(System.Globalization.CultureInfo.CurrentCulture)));
            tableColumns.Add(new TableColumn("Dépenses", Unit.FromCentimeter(2), operation => GetOperationAmount(operation, AccountingEntryType.Expense, accountingEntries)?.ToCultureString(System.Globalization.CultureInfo.CurrentCulture)));
            tableColumns.Add(new TableColumn("Catégorie", Unit.FromCentimeter(3), operation => GetAccountingCategoryLabel(operation, accountingCategories)));
            tableColumns.Add(new TableColumn("Libellé", Unit.FromCentimeter(4), operation => operation.Label));

            // group operations by account book
            Dictionary<long, Dictionary<PaymentMethod, List<Operation>>> operationsMapsId = new Dictionary<long, Dictionary<PaymentMethod, List<Operation>>>();
            foreach (Operation operation in operations)
            {
                if (!operationsMapsId.TryGetValue(operation.AccountBookId, out Dictionary<PaymentMethod, List<Operation>>? accountBookOperationMaps))
                {
                    accountBookOperationMaps = new Dictionary<PaymentMethod, List<Operation>>();
                    operationsMapsId.Add(operation.AccountBookId, accountBookOperationMaps);
                }

                if (!accountBookOperationMaps.TryGetValue(operation.PaymentMethod, out List<Operation>? paymentMethodOperations))
                {
                    paymentMethodOperations = new List<Operation>();
                    accountBookOperationMaps.Add(operation.PaymentMethod, paymentMethodOperations);
                }
                
                paymentMethodOperations.Add(operation);
            }
            
            List<(AccountBook accountBook, Dictionary<PaymentMethod, List<Operation>>)> accountBooksOperations = operationsMapsId.Select(keyValue => (accountBook: accountBooks[keyValue.Key], keyValue.Value)).Where(item => item.accountBook != null).ToList();
            accountBooksOperations.Sort((left, right) => ItemsByIdComparer(left.accountBook, right.accountBook));

            foreach ((AccountBook accountBook, Dictionary<PaymentMethod, List<Operation>> paymentMethodOperationMap) in accountBooksOperations) {
                var editionGroup = BuildAccountBookOperationsContent(accountBook, paymentMethodOperationMap, accountingEntries, tableColumns);
                Content.Add(editionGroup);
            }
        }

        private static EditionContent BuildPaymentMethodOperationsContent(PaymentMethod paymentMethod, IEnumerable<Operation> operations, Dictionary<long, AccountingEntry> accountingEntries, List<TableColumn> tableColumns)
        {
            var operationsTable = new OperationTable(tableColumns, operations, accountingEntries);
            Amount totalAmount = GetOperationsAmount(operations, accountingEntries);
            return new TitledContentGroup("Moyen de paiement", paymentMethod.GetDisplayString(), 3, operationsTable, totalAmount);
        }

        private static EditionContent BuildAccountBookOperationsContent(AccountBook accountBook, Dictionary<PaymentMethod, List<Operation>> paymentMethodOperationMap, Dictionary<long, AccountingEntry> accountingEntries, List<TableColumn> tableColumns)
        {
            List<EditionContent> accountBookContent = Enum.GetValues<PaymentMethod>()
                .Select(paymentMethod => (paymentMethod, operations: paymentMethodOperationMap.GetValueOrDefault(paymentMethod)))
                .Where(tuple => tuple.operations != null)
                .Select(pair => BuildPaymentMethodOperationsContent(pair.paymentMethod, pair.operations!, accountingEntries, tableColumns))
                .Cast<EditionContent>()
                .ToList();
            Amount totalAmount = GetOperationsAmount(paymentMethodOperationMap.SelectMany(pair => pair.Value), accountingEntries);
            var contentGroup = new TitledContentGroup("Livre comptable", accountBook.Label, 2, accountBookContent, totalAmount, true);
            contentGroup.Border = true;
            return contentGroup;
        }
    }
}
