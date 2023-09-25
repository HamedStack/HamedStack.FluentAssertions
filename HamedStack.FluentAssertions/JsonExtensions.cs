using System.Text.Json;

namespace HamedStack.FluentAssertions;

/// <summary>
/// Provides extension methods for working with JSON.
/// </summary>
internal static class JsonExtensions
{
    /// <summary>
    /// Retrieves all unique keys present in a JSON element, including objects, arrays, and values.
    /// </summary>
    /// <param name="doc">The <see cref="JsonElement"/> to extract keys from.</param>
    /// <returns>
    /// An <see cref="IEnumerable{T}"/> of strings representing the keys found in the JSON element.
    /// The keys are structured based on the JSON hierarchy, and their format depends on the JSON value type.
    /// </returns>
    /// <remarks>
    /// This method recursively processes the JSON element and generates keys for objects, arrays, and values.
    /// The format of keys for different JSON value types is as follows:
    /// - Object: "$.property-object" where "property" is the object's property name.
    /// - Array: "$[index]-array" where "index" is the array index.
    /// - String: "property-string" or "property-date" for date strings.
    /// - Number: "property-number".
    /// - Undefined: "property-undefined".
    /// - Null: "property-null".
    /// - Boolean: "property-boolean".
    /// </remarks>
    internal static IEnumerable<string> GetKeys(this JsonElement doc)
    {
        var queue = new Queue<(string ParentPath, JsonElement element)>();
        queue.Enqueue(("", doc));
        while (queue.Any())
        {
            var (parentPath, element) = queue.Dequeue();
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    parentPath = parentPath == ""
                        ? "$."
                        : $"{parentPath}.";
                    foreach (var nextEl in element.EnumerateObject())
                    {
                        queue.Enqueue(($"{parentPath}{nextEl.Name}", nextEl.Value));
                    }
                    yield return $"{parentPath.Trim('.')}-object";
                    break;
                case JsonValueKind.Array:
                    foreach (var (nextEl, i) in element.EnumerateArray().Select((jsonElement, i) => (jsonElement, i)))
                    {
                        if (string.IsNullOrEmpty(parentPath))
                            parentPath = "$.";
                        queue.Enqueue(($"{parentPath}[{i}]", nextEl));
                    }
                    yield return $"{parentPath.Trim('.')}-array";
                    break;
                case JsonValueKind.String:
                    var isDate = DateTime.TryParse(element.ToString(), out _);
                    var type = isDate ? "-date" : "-string";
                    yield return parentPath.Trim('.') + type;
                    break;
                case JsonValueKind.Number:
                    yield return $"{parentPath.Trim('.')}-number"; 
                    break;
                case JsonValueKind.Undefined:
                    yield return $"{parentPath.Trim('.')}-undefined";
                    break;
                case JsonValueKind.Null:
                    yield return $"{parentPath.Trim('.')}-null";
                    break;
                case JsonValueKind.True:
                case JsonValueKind.False:
                    yield return $"{parentPath.Trim('.')}-boolean"; 
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
    }
}