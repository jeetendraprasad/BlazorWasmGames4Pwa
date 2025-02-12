namespace BlazorWasmGames4Pwa.Code
{
    internal static class ExtensionMethods
    {
        public static Stack<T> Clone <T>(this Stack<T> original)
        {
            T[] arr = new T[original.Count];
            original.CopyTo(arr, 0);
            Array.Reverse(arr);
            return new Stack<T>(arr);
        }
    }

    internal static class ListExtension
    {
        /// <summary>
        /// Creates a single flat list from a list of lists input. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="inListOfLists"></param>
        /// <returns>A single flatten list from a list of lists input</returns>
        public static List<T> Flatten<T>(this List<List<T>> inListOfLists)
        {
            List<T> outList = [];
            foreach (List<T> list in inListOfLists)
            {
                outList.AddRange(list);
            }
            return outList;
        }
    }
}
