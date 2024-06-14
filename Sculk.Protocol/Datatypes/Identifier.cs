using System.Text.RegularExpressions;

namespace Sculk.Protocol.Datatypes;

/// <remarks>
/// Might be valid. Might be invalid.
/// Make sure to check <see cref="Identifier.IsValid" />.
/// </remarks>
public readonly partial record struct Identifier(string Value)
{
    public static Identifier WithDefaultNamespace(string path)
        => new Identifier($"minecraft:{path}");

    public static Identifier WithNamespace(string @namespace, string path)
        => new Identifier($"{@namespace}:{path}");

    public static bool IsValid(string identifier)
    {
        var parts = identifier.Split(':');
        return parts.Length == 2 && NamespaceRegex().IsMatch(parts[0]) && PathRegex().IsMatch(parts[1]);
    }

    [GeneratedRegex(@"^[a-z0-9.\-_]$")]
    private static partial Regex NamespaceRegex();

    [GeneratedRegex(@"^[a-z0-9.\-_/]$")]
    private static partial Regex PathRegex();

    public static implicit operator string(Identifier identifier)
        => identifier.Value;
}