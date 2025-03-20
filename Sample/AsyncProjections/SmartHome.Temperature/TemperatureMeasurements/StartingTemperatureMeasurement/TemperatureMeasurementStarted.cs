using Newtonsoft.Json;

namespace SmartHome.Temperature.TemperatureMeasurements.StartingTemperatureMeasurement;

public class TemperatureMeasurementStarted
{
    #region Properties

    public Guid MeasurementId { get; }
    public DateTimeOffset StartedAt { get; }

    #endregion

    #region Constructors

    [JsonConstructor]
    private TemperatureMeasurementStarted(Guid measurementId, DateTimeOffset startedAt)
    {
        MeasurementId = measurementId;
        StartedAt = startedAt;
    }

    #endregion

    #region Factory Methods

    public static TemperatureMeasurementStarted Create(Guid measurementId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(measurementId, Guid.Empty);

        return new TemperatureMeasurementStarted(measurementId, DateTimeOffset.UtcNow);
    }

    #endregion
}
