namespace SqlKata
{
    public static class Qx
    {
        public static QLazyList<T> ToLazyQList<T>(this ICollection<T> source,
            string separator, Func<T, Q?> selector)
        {
            return new QLazyList<T>(separator, source, selector);
        }
    }
}