namespace HamedStack.FluentAssertions;

/// <summary>
/// Contains a collection of extension methods for various types.
/// </summary>
internal static class Extensions
{
    /// <summary>
    /// Filters elements of an <see cref="IEnumerable{TSource}"/> based on a condition
    /// if the specified condition is true, or returns the original source otherwise.
    /// </summary>
    /// <typeparam name="TSource">The type of elements in the source sequence.</typeparam>
    /// <param name="source">The source sequence to filter.</param>
    /// <param name="condition">
    /// A Boolean value indicating whether to apply the filter specified by the <paramref name="predicate"/>.
    /// </param>
    /// <param name="predicate">A function to test each element for a condition.</param>
    /// <returns>
    /// An <see cref="IEnumerable{TSource}"/> that contains elements from the input sequence
    /// that satisfy the condition specified by the <paramref name="predicate"/> if the <paramref name="condition"/> is true;
    /// otherwise, returns the original source.
    /// </returns>
    /// <remarks>
    /// This extension method is designed to be used in LINQ queries to conditionally apply a filter.
    /// If <paramref name="condition"/> is true, it will filter the elements using the provided <paramref name="predicate"/>,
    /// and if <paramref name="condition"/> is false, it will return the original source unchanged.
    /// </remarks>
    internal static IEnumerable<TSource> WhereIf<TSource>(this IEnumerable<TSource> source, bool condition, Func<TSource, bool> predicate)
    {
        if (condition)
            return source.Where(predicate);
        return source;
    }
}