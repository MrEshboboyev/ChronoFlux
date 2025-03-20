namespace SmartHome.Temperature.MotionSensors.InstallingMotionSensor;

public record MotionSensorInstalled(
    Guid MotionSensorId,
    DateTime InstalledAt
)
{
    public static MotionSensorInstalled Create(
        Guid motionSensorId,
        DateTime installedAt
    )
    {
        ArgumentOutOfRangeException.ThrowIfEqual(motionSensorId, Guid.Empty);
        ArgumentOutOfRangeException.ThrowIfEqual(installedAt, default);

        return new MotionSensorInstalled(motionSensorId, installedAt);
    }
}
