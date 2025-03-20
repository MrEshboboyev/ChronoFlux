namespace SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;

public record TemperatureRecorded(
    Guid MeasurementId,
    decimal Temperature,
    DateTimeOffset MeasuredAt
)
{
    public static TemperatureRecorded Create(Guid measurementId, decimal temperature)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(measurementId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfLessThan(temperature, -273);

        return new TemperatureRecorded(measurementId, temperature, DateTimeOffset.UtcNow);
    }
}
