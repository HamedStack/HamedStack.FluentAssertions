// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedType.Global

using System.Text.Json;

namespace HamedStack.FluentAssertions;

/// <summary>
/// Provides extension methods for FluentAssertions to assert JSON documents.
/// </summary>
public static class FluentAssertionsExtensions
{
    /// <summary>
    /// Creates a <see cref="JsonDocumentAssertion"/> instance to perform assertions on a JSON document.
    /// </summary>
    /// <param name="jsonDocument">The <see cref="JsonDocument"/> to perform assertions on.</param>
    /// <returns>A <see cref="JsonDocumentAssertion"/> instance for fluent assertion chaining.</returns>
    /// <remarks>
    /// This extension method allows you to use FluentAssertions to perform various assertions
    /// on a JSON document, such as checking its structure, content, and more.
    /// </remarks>
    public static JsonDocumentAssertion Should(this JsonDocument jsonDocument) => new(jsonDocument);
}