using Core.Commands;
using Core.Marten.Repository;

namespace SmartHome.Temperature.TemperatureMeasurements.RecordingTemperature;

public record RecordTemperature(
    Guid MeasurementId,
    decimal Temperature
)
{
    public static RecordTemperature Create(Guid measurementId, decimal temperature)
    {
        ArgumentOutOfRangeException.ThrowIfEqual(measurementId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfLessThan(temperature, -273);

        return new RecordTemperature(measurementId, temperature);
    }
}

public class HandleRecordTemperature(IMartenRepository<TemperatureMeasurement> repository):
    ICommandHandler<RecordTemperature>
{
    public Task HandleAsync(RecordTemperature command, CancellationToken ct)
    {
        var (measurementId, temperature) = command;

        return repository.GetAndUpdate(
            measurementId,
            reservation => reservation.Record(temperature),
            ct: ct
        );
    }
}
