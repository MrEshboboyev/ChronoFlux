using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace PointOfSales.Api.Core;

public static class ETagExtensions
{
    public static int ToExpectedVersion(string? eTag)
    {
        ArgumentNullException.ThrowIfNull(eTag);

        var value = EntityTagHeaderValue.Parse(eTag).Tag.Value;

        return value is null 
            ? throw new ArgumentNullException(nameof(eTag)) 
            : int.Parse(value[1..^1]);
    }
}

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class FromIfMatchHeaderAttribute: FromHeaderAttribute
{
    public FromIfMatchHeaderAttribute()
    {
        Name = "If-Match";
    }
}
