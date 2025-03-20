using Core.Commands;
using Core.Marten.Repository;

namespace SmartHome.Temperature.MotionSensors.InstallingMotionSensor;

public record InstallMotionSensor(
    Guid MotionSensorId
)
{
    public static InstallMotionSensor Create(
        Guid motionSensorId
    )
    {
        ArgumentOutOfRangeException.ThrowIfEqual(motionSensorId, Guid.Empty);

        return new InstallMotionSensor(motionSensorId);
    }
}

public class HandleInstallMotionSensor(IMartenRepository<MotionSensor> repository):
    ICommandHandler<InstallMotionSensor>
{
    public Task HandleAsync(InstallMotionSensor command, CancellationToken ct) =>
        repository.Add(
            command.MotionSensorId,
            MotionSensor.Install(command.MotionSensorId),
            ct
        );
}
