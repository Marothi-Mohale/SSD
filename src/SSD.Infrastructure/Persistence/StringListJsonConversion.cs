using System.Text.Json;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace SSD.Infrastructure.Persistence;

internal static class StringListJsonConversion
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public static PropertyBuilder<List<string>> HasJsonbStringListConversion(this PropertyBuilder<List<string>> propertyBuilder)
    {
        var converter = new ValueConverter<List<string>, string>(
            value => Serialize(value),
            value => Deserialize(value));

        var comparer = new ValueComparer<List<string>>(
            (left, right) => Serialize(left) == Serialize(right),
            value => Serialize(value).GetHashCode(StringComparison.Ordinal),
            value => Deserialize(Serialize(value)));

        propertyBuilder.HasConversion(converter);
        propertyBuilder.Metadata.SetValueComparer(comparer);
        propertyBuilder.HasColumnType("jsonb");

        return propertyBuilder;
    }

    private static string Serialize(List<string>? value)
    {
        return JsonSerializer.Serialize(value ?? [], SerializerOptions);
    }

    private static List<string> Deserialize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return [];
        }

        return JsonSerializer.Deserialize<List<string>>(value, SerializerOptions) ?? [];
    }
}
