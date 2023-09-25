// ReSharper disable UnusedMember.Global

using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using FluentAssertions;
using FluentAssertions.Execution;
using FluentAssertions.Primitives;

namespace HamedStack.FluentAssertions;

/// <summary>
/// Provides fluent assertion methods for asserting JSON documents.
/// </summary>
public class JsonDocumentAssertion : ReferenceTypeAssertions<JsonDocument, JsonDocumentAssertion>
{
    private readonly JsonDocument _actualJDoc;
    /// <summary>
    /// Initializes a new instance of the <see cref="JsonDocumentAssertion"/> class.
    /// </summary>
    /// <param name="actual">The actual <see cref="JsonDocument"/> to perform assertions on.</param>
    public JsonDocumentAssertion(JsonDocument actual) : base(actual)
    {
        _actualJDoc = actual;
    }

    /// <summary>
    /// Gets the identifier for the JSON document type.
    /// </summary>
    protected override string Identifier => nameof(JsonDocument);

    /// <summary>
    /// Removes unknown keys from the differences between actual and expected keys.
    /// </summary>
    /// <param name="actual">The actual keys.</param>
    /// <param name="expected">The expected keys.</param>
    /// <returns>
    /// A tuple containing two collections of keys: actual keys with unknown keys removed and
    /// expected keys with unknown keys removed.
    /// </returns>
    private (IEnumerable<string>, IEnumerable<string>) RemoveUnknownFromDifferences(IEnumerable<string> actual, IEnumerable<string> expected)
    {
        var actualResult = new List<string>();
        var expectedResult = new List<string>();
        var enumerable = actual.ToList();
        var actualUnknownPaths = enumerable
            .Where(x => x.EndsWith("-undefined") || x.EndsWith("-null"))
            .Select(x => x.Substring(0, x.LastIndexOfAny(new[] { '-' })));

        var items = expected.ToList();
        var expectedUnknownPaths = items
            .Where(x => x.EndsWith("-undefined") || x.EndsWith("-null"))
            .Select(x => x.Substring(0, x.LastIndexOfAny(new[] { '-' })));

        var allUnknownKeys = actualUnknownPaths.Concat(expectedUnknownPaths).Distinct();
        var unknownKeys = allUnknownKeys.ToList();
        foreach (var item in enumerable)
        {
            var lastDash = item.LastIndexOfAny(new[] { '-' });
            var path = item.Substring(0, lastDash);
            if (unknownKeys.Any(x => x == path) && items.Any(x => x.StartsWith($"{path}-")))
            {
                actualResult.Add(path);
            }
            else
            {
                actualResult.Add(item);
            }
        }

        foreach (var item in items)
        {
            var lastDash = item.LastIndexOfAny(new[] { '-' });
            var path = item.Substring(0, lastDash);
            if (unknownKeys.Any(x => x == path) && enumerable.Any(x => x.StartsWith($"{path}-")))
            {
                expectedResult.Add(path);
            }
            else
            {
                expectedResult.Add(item);
            }
        }
        return (actualResult, expectedResult);
    }
    /// <summary>
    /// Asserts that the JSON document has the same schema as the expected document.
    /// </summary>
    /// <param name="expected">The expected <see cref="JsonDocument"/> to compare against.</param>
    /// <param name="because">A reason why this assertion is needed.</param>
    /// <param name="becauseArgs">Arguments for the because message formatting.</param>
    /// <returns>An <see cref="AndConstraint{TAssertion}"/> for fluent assertion chaining.</returns>
    public AndConstraint<JsonDocumentAssertion> HaveSameSchemaAs(JsonDocument expected, string because = "", params object[] becauseArgs)
    {
        var actualKeys = _actualJDoc.RootElement.GetKeys();
        var expectedKeys = expected.RootElement.GetKeys();

        var (actualResult, expectedResult) = RemoveUnknownFromDifferences(actualKeys, expectedKeys);

        var enumerable = actualResult.ToList();
        var result = expectedResult.ToList();
        var existsInActual = enumerable.Except(result).ToList();
        var existsInExpected = result.Except(enumerable).ToList();

        var status = !existsInActual.Any() && !existsInExpected.Any();

        var sb = new StringBuilder();
        if (existsInActual.Any())
        {
            sb.AppendLine("The inputs do not match, the differences are as follows:");
            sb.AppendLine();
            sb.AppendLine("Actual:");

            foreach (var item in existsInActual)
            {
                var lastDash = item.LastIndexOfAny(new[] { '-' });
                if (!string.IsNullOrEmpty(item))
                {
                    var path = item.Substring(0, lastDash);
                    var type = item.Substring(lastDash + 1);
                    sb.AppendLine($"Path: {path}, Type:{type}");
                }
            }
        }
        if (existsInExpected.Any())
        {
            sb.AppendLine();
            sb.AppendLine("Expected:");

            foreach (var item in existsInExpected)
            {
                var lastDash = item.LastIndexOfAny(new[] { '-' });
                if (!string.IsNullOrEmpty(item))
                {
                    var path = item.Substring(0, lastDash);
                    var type = item.Substring(lastDash + 1);
                    sb.AppendLine($"Path: {path}, Type:{type}");
                }
            }
        }

        var message = sb.ToString();

        Execute.Assertion
            .ForCondition(status)
            .BecauseOf(because, becauseArgs)
            .FailWith(message);

        return new AndConstraint<JsonDocumentAssertion>(this);
    }
    
    /// <summary>
    /// Asserts that the JSON document contains the schema of the expected document.
    /// </summary>
    /// <param name="expected">The expected <see cref="JsonDocument"/> schema to compare against.</param>
    /// <param name="ignoreAdditionalProps">A flag indicating whether to ignore additional properties in the schema.</param>
    /// <param name="because">A reason why this assertion is needed.</param>
    /// <param name="becauseArgs">Arguments for the because message formatting.</param>
    /// <returns>An <see cref="AndConstraint{TAssertion}"/> for fluent assertion chaining.</returns>
    public AndConstraint<JsonDocumentAssertion> ContainSchemaOf(JsonDocument expected, bool ignoreAdditionalProps = false, string because = "", params object[] becauseArgs)
    {
        var actualKeys = _actualJDoc.RootElement
                .GetKeys()
                .WhereIf(ignoreAdditionalProps, x => !x.Contains("additionalProp"))
            ;
        var expectedKeys = expected.RootElement
                .GetKeys()
                .WhereIf(ignoreAdditionalProps, x => !x.Contains("additionalProp"))
            ;

        var (actualResult, expectedResult) = RemoveUnknownFromDifferences(actualKeys, expectedKeys);

        actualResult = actualResult.Select(x => Regex.Replace(x, @"\[[0-9]+\]", ".[item]")).Distinct();
        expectedResult = expectedResult.Select(x => Regex.Replace(x, @"\[[0-9]+\]", ".[item]")).Distinct();

        var enumerable = expectedResult.ToList();
        var result = actualResult.ToList();
        var existsInActual = result.Except(enumerable).ToList();
        var existsInExpected = enumerable.Except(result).ToList();

        var status = !existsInActual.Any() && !existsInExpected.Any();

        var sb = new StringBuilder();
        if (existsInActual.Any())
        {
            sb.AppendLine("The inputs do not match, the differences are as follows:");
            sb.AppendLine();
            sb.AppendLine("Actual:");

            foreach (var item in existsInActual)
            {
                var lastDash = item.LastIndexOfAny(new[] { '-' });
                if (!string.IsNullOrEmpty(item))
                {
                    var path = item.Substring(0, lastDash);
                    var type = item.Substring(lastDash + 1);
                    sb.AppendLine($"Path: {path}, Type:{type}");
                }
            }
        }
        if (existsInExpected.Any())
        {
            sb.AppendLine();
            sb.AppendLine("Expected:");

            foreach (var item in existsInExpected)
            {
                var lastDash = item.LastIndexOfAny(new[] { '-' });
                if (!string.IsNullOrEmpty(item))
                {
                    var path = item.Substring(0, lastDash);
                    var type = item.Substring(lastDash + 1);
                    sb.AppendLine($"Path: {path}, Type:{type}");
                }
            }
        }

        var message = sb.ToString();

        Execute.Assertion
            .ForCondition(status)
            .BecauseOf(because, becauseArgs)
            .FailWith(message);

        return new AndConstraint<JsonDocumentAssertion>(this);

    }
}