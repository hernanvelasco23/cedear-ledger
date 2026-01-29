using System.Text.Json;
using CedearLedger.Api.Serialization;
using Xunit;

namespace CedearLedger.Tests.Serialization;

public sealed class DateOnlyJsonConverterTests
{
    [Fact]
    public void Serialize_DateOnly_As_IsoString()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new DateOnlyJsonConverter());

        var value = new DateOnly(2025, 1, 28);

        var json = JsonSerializer.Serialize(value, options);

        Assert.Equal("\"2025-01-28\"", json);
    }

    [Fact]
    public void Deserialize_IsoString_To_DateOnly()
    {
        var options = new JsonSerializerOptions();
        options.Converters.Add(new DateOnlyJsonConverter());

        var json = "\"2025-01-28\"";

        var value = JsonSerializer.Deserialize<DateOnly>(json, options);

        Assert.Equal(new DateOnly(2025, 1, 28), value);
    }
}
