using MigraDocCore.DocumentObjectModel;

namespace Malte2.Model.Accounting.Edition
{
    public class OperationsByAccountBookAndCategoryEdition : OperationEdition
    {
        public OperationsByAccountBookAndCategoryEdition(string title, IEnumerable<Operation> operations, Dictionary<long, AccountBook> accountBooks, Dictionary<long, AccountingEntry> accountingEntries, Dictionary<long, AccountingCategory> accountingCategories)
        {
            Title = title;
            TableColumns.Add(new TableColumn("Date", Unit.FromCentimeter(2), operation => operation.OperationDateTime.ToShortDateString()));
            TableColumns.Add(new TableColumn("Recettes", Unit.FromCentimeter(2), operation => GetOperationAmount(operation, AccountingEntryType.Revenue, accountingEntries)?.ToCultureString(System.Globalization.CultureInfo.CurrentCulture)));
            TableColumns.Add(new TableColumn("Dépenses", Unit.FromCentimeter(2), operation => GetOperationAmount(operation, AccountingEntryType.Expense, accountingEntries)?.ToCultureString(System.Globalization.CultureInfo.CurrentCulture)));
            TableColumns.Add(new TableColumn("Type", Unit.FromCentimeter(1.6), operation => operation.PaymentMethod.GetDisplayString()));
            TableColumns.Add(new TableColumn("Libellé", Unit.FromCentimeter(4), operation => operation.Label));

            NonCategorizedTableColumns.Add(new TableColumn("Date", Unit.FromCentimeter(2), operation => operation.OperationDateTime.ToShortDateString()));
            NonCategorizedTableColumns.Add(new TableColumn("Recettes", Unit.FromCentimeter(2), operation => GetOperationAmount(operation, AccountingEntryType.Revenue, accountingEntries)?.ToCultureString(System.Globalization.CultureInfo.CurrentCulture)));
            NonCategorizedTableColumns.Add(new TableColumn("Dépenses", Unit.FromCentimeter(2), operation => GetOperationAmount(operation, AccountingEntryType.Expense, accountingEntries)?.ToCultureString(System.Globalization.CultureInfo.CurrentCulture)));
            NonCategorizedTableColumns.Add(new TableColumn("Type", Unit.FromCentimeter(1.6), operation => operation.PaymentMethod.GetDisplayString()));
            NonCategorizedTableColumns.Add(new TableColumn("Imputation", Unit.FromCentimeter(2.5), operation => accountingEntries[operation.AccountingEntryId].Label));
            NonCategorizedTableColumns.Add(new TableColumn("Libellé", Unit.FromCentimeter(4), operation => operation.Label));

            // group operations by account book
            Dictionary<long, Dictionary<long, List<Operation>>> operationsMapsId = new Dictionary<long, Dictionary<long, List<Operation>>>();
            foreach (Operation operation in operations)
            {
                if (!operationsMapsId.TryGetValue(operation.AccountBookId, out Dictionary<long, List<Operation>>? accountBookOperations))
                {
                    accountBookOperations = new Dictionary<long, List<Operation>>();
                    operationsMapsId.Add(operation.AccountBookId, accountBookOperations);
                }

                long accountingCategoryKey = operation.AccountingCategoryId.GetValueOrDefault(NULL_OBJECT_ID_KEY);

                if (!accountBookOperations.TryGetValue(accountingCategoryKey, out List<Operation>? accountingCategoryOperations))
                {
                    accountingCategoryOperations = new List<Operation>();
                    accountBookOperations.Add(accountingCategoryKey, accountingCategoryOperations);
                }
                
                accountingCategoryOperations.Add(operation);
            }
            
            List<(AccountBook accountBook, Dictionary<long, List<Operation>> operationsMap)> accountBooksOperations = operationsMapsId.Select(keyValue => (accountBook: accountBooks[keyValue.Key], operationsMap: keyValue.Value)).Where(item => item.accountBook != null).ToList();
            accountBooksOperations.Sort((left, right) => ItemsByIdComparer(left.accountBook, right.accountBook));

            foreach ((AccountBook accountBook, Dictionary<long, List<Operation>> categoryOperationsMap) in accountBooksOperations) {
                var editionGroup = BuildAccountBookOperationsContent(accountBook, categoryOperationsMap, accountingEntries, accountingCategories);
                Content.Add(editionGroup);
            }
        }

        public List<TableColumn> TableColumns = new List<TableColumn>();

        public List<TableColumn> NonCategorizedTableColumns = new List<TableColumn>();

        private EditionContent BuildAccountBookOperationsContent(AccountBook accountBook, Dictionary<long, List<Operation>> accountBookOperationsMap, Dictionary<long, AccountingEntry> accountingEntries, Dictionary<long, AccountingCategory> accountingCategories)
        {
            var accountBookContent = accountBookOperationsMap
                .Select(keyValuePair => (category: keyValuePair.Key != NULL_OBJECT_ID_KEY ? accountingCategories[keyValuePair.Key] : null, operationsMap: keyValuePair.Value))
                .ToList();
            accountBookContent.Sort((left, right) => ItemsByIdComparer(left.category, right.category));
            var contentSections = accountBookContent
                .Select(keyValuePair => BuildAccountingCategoryOperationsContent(keyValuePair.category, keyValuePair.operationsMap, accountingEntries))
                .Cast<EditionContent>()
                .ToList();
            Amount totalAmount = GetOperationsAmount(accountBookOperationsMap.SelectMany(pair => pair.Value), accountingEntries);
            var contentGroup = new TitledContentGroup("Livre comptable", accountBook.Label, 2, contentSections, totalAmount, true);
            contentGroup.Border = true;
            return contentGroup;
        }

        private EditionContent BuildAccountingCategoryOperationsContent(AccountingCategory? accountingCategory, IEnumerable<Operation> operations, Dictionary<long, AccountingEntry> accountingEntries)
        {
            Amount totalAmount = GetOperationsAmount(operations, accountingEntries);
            if (accountingCategory != null) {
                var operationsTable = new OperationTable(TableColumns, operations, accountingEntries);
                var contentGroup = new TitledContentGroup("", accountingCategory.Label, 4, operationsTable, totalAmount);
                if (accountingCategory.AccountingEntryId != null)
                {
                    var accountingEntryParagraph = new Paragraph();
                    contentGroup.Details.Add(accountingEntryParagraph);
                    accountingEntryParagraph.AddText("Imputation comptable ");
                    accountingEntryParagraph.AddFormattedText(accountingEntries[accountingCategory.AccountingEntryId.Value].Label, TextFormat.Bold);
                }
                return contentGroup;
            }
            else {
                var operationsTable = new OperationTable(NonCategorizedTableColumns, operations, accountingEntries);
                return new TitledContentGroup("", "Non catégorisées", 4, operationsTable, totalAmount);
            }
        }
    }
}
