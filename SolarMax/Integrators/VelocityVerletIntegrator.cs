namespace SolarMax.Integrators;

internal sealed class VelocityVerletIntegrator(bool VariableAccelerationUpdate) : IIntegrator
{
    private double halfDt;
    private Physics physics;
    private readonly bool variableAccelerationUpdate = VariableAccelerationUpdate;

    public void Init(Physics Physics)
    {
        physics = Physics;
        physics.UpdateAcceleration();
    }
    public void MoveOrbiters(double dt)
    {
        halfDt = dt * 0.5;

        foreach (var body in physics.AllOrbiters)
        {
            body.Position += (body.Velocity + body.Acceleration * halfDt) * dt;
            body.Velocity += body.Acceleration * halfDt;
        }

        if (variableAccelerationUpdate)
            physics.UpdateAccelerationVariable();
        else
            physics.UpdateAcceleration();

        foreach (var body in physics.AllOrbiters)
            body.Velocity += body.Acceleration * halfDt;
    }
}
