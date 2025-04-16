
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;

namespace Helpdesk.Api.Core.Http;

public static class ETagExtensions
{
    public static int ToExpectedVersion(string? eTag)
    {
        ArgumentNullException.ThrowIfNull(eTag);

        var value = EntityTagHeaderValue.Parse(eTag).Tag.Value
            ?? throw new ArgumentNullException(nameof(eTag));
        
        return int.Parse(value[1..^1]);
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
