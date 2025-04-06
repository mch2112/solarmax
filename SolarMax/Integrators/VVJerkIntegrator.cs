namespace SolarMax.Integrators;

class VVJerkIntegrator(bool VariableAccelerationUpdate) : IIntegrator
{
    private Physics physics;
    private readonly bool variableAccelerationUpdate = VariableAccelerationUpdate;

    public void Init(Physics Physics)
    {
        physics = Physics;
        physics.UpdateAcceleration();
    }
    public void MoveOrbiters(double dt)
    {
        double halfDt = dt * 0.5;
        double halfDt2 = dt * dt * 0.5;
        double quarterDt2 = dt * dt * 0.25;
        double sixthDt3 = dt * dt * dt * (1.0 / 6.0);

        foreach (var body in physics.AllOrbiters)
        {
            body.Position += body.Velocity * dt + body.Acceleration * halfDt2 + body.Jerk * sixthDt3;
            body.Velocity += body.Acceleration * halfDt + body.Jerk * quarterDt2;
            body.Acc2 = body.Acceleration;
        }

        if (variableAccelerationUpdate)
            physics.UpdateAccelerationVariable();
        else
            physics.UpdateAcceleration();

        foreach (var body in physics.AllOrbiters)
        {
            body.Jerk = (body.Acceleration - body.Acc2) / dt;
            body.Velocity += body.Acceleration * halfDt + body.Jerk * quarterDt2;
        }
    }
}
