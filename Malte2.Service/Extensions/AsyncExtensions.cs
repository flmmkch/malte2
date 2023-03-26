namespace Malte2.Extensions
{
    public static class AsyncExtensions
    {
        public static async Task<List<T>> ToListAsync<T>(this IAsyncEnumerable<T> asyncEnumerable) {
            List<T> list = new List<T>();
            IAsyncEnumerator<T> asyncEnumerator = asyncEnumerable.GetAsyncEnumerator();
            while (await asyncEnumerator.MoveNextAsync()) {
                list.Add(asyncEnumerator.Current);
            }
            return list;
        }

    }
}