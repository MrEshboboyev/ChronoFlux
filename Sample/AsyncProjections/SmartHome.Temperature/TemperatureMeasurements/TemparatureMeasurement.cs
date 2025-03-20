using Core.Aggregates;
using SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;
using SmartHome.Temperature.TemperatureMeasurements.StartingTemperatureMeasurement;

namespace SmartHome.Temperature.TemperatureMeasurements;

public class TemperatureMeasurement: Aggregate
{
    #region Properties

    public DateTimeOffset Started { get; set; }
    public DateTimeOffset? LastRecorded { get; set; }

    public List<decimal> Measurements { get; set; } = default!;

    #endregion

    #region Constructors

    // For serialization
    public TemperatureMeasurement() { }

    private TemperatureMeasurement(Guid measurementId)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(measurementId, Guid.Empty);

        var @event = TemperatureMeasurementStarted.Create(
            measurementId
        );

        Enqueue(@event);
        Apply(@event);
    }

    #endregion

    #region Starting methods

    public static TemperatureMeasurement Start(Guid measurementId) =>new(measurementId);

    #endregion

    #region Public methods

    public void Record(decimal temperature)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(temperature, -273);

        var @event = TemperatureRecorded.Create(
            Id,
            temperature
        );

        Enqueue(@event);
        Apply(@event);
    }

    public void Apply(TemperatureMeasurementStarted @event)
    {
        Id = @event.MeasurementId;
        Started = @event.StartedAt;
        Measurements = [];
    }


    public void Apply(TemperatureRecorded @event)
    {
        Measurements.Add(@event.Temperature);
        LastRecorded = @event.MeasuredAt;
    }

    #endregion
}
