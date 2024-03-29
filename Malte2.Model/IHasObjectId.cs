using System.Linq;

namespace Malte2.Model
{

    /// <summary>
    /// Object de la base de donnée
    /// </summary>
    public interface IHasObjectId
    {
        long? Id { get; set; }
    }

    public static class IHasObjectIdHelper
    {
        public const long NULL_OBJECT_ID_KEY = long.MaxValue;

        private static bool AddItemToDictionary<T>(T item, Dictionary<long, T> dictionary) where T: IHasObjectId
        {
            long? id = item.Id;
            if (id.HasValue) {
                dictionary.Add(id.Value, item);
                return true;
            }
            return false;
        }

        public static Dictionary<long, T> BuildDictionaryById<T>(this IEnumerable<T> enumerable) where T: IHasObjectId
        {
            Dictionary<long, T> dictionary = new Dictionary<long, T>();
            foreach (T item in enumerable)
            {
                AddItemToDictionary(item, dictionary);
            }
            return dictionary;
        }

        public static async Task<Dictionary<long, T>> BuildDictionaryById<T>(this IAsyncEnumerable<T> enumerable) where T: IHasObjectId
        {
            Dictionary<long, T> dictionary = new Dictionary<long, T>();
            await foreach (T item in enumerable)
            {
                AddItemToDictionary(item, dictionary);
            }
            return dictionary;
        }

        public static int ItemsByIdComparer<T>(T? left, T? right) where T: IHasObjectId
        {
            long comparison = (left?.Id).GetValueOrDefault(NULL_OBJECT_ID_KEY).CompareTo((right?.Id).GetValueOrDefault(NULL_OBJECT_ID_KEY));
            return (int) comparison;
        }
    }
}