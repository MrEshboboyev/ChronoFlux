namespace Tickets.Maintenance.Commands;

public record RebuildProjection(
    string ViewName
)
{
    public static RebuildProjection Create(string? viewName)
    {
        ArgumentNullException.ThrowIfNull(viewName);

        return new RebuildProjection(viewName);
    }
}
