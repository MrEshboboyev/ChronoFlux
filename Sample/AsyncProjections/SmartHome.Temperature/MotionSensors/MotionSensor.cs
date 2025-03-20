using Core.Aggregates;
using SmartHome.Temperature.MotionSensors.InstallingMotionSensor;

namespace SmartHome.Temperature.MotionSensors;

public class MotionSensor: Aggregate
{
    #region Properties

    public DateTime InstalledAt { get; private set; }

    #endregion

    #region Constructors

    // For serialization
    public MotionSensor() { }

    private MotionSensor(Guid motionSensorId, in DateTime installedAt)
    {
        var @event = MotionSensorInstalled.Create(motionSensorId, installedAt);

        Enqueue(@event);
        Apply(@event);
    }

    #endregion

    #region Factory methods

    public static MotionSensor Install(Guid motionSensorId) =>
        new(motionSensorId, DateTime.UtcNow);

    #endregion

    #region Public methods

    public void Apply(MotionSensorInstalled @event)
    {
        Id = @event.MotionSensorId;
        InstalledAt = @event.InstalledAt;
    }

    #endregion
}
