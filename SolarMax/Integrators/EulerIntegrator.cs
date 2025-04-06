namespace SolarMax.Integrators;

internal sealed class EulerIntegrator : IIntegrator
{
    private double halfDt;
    private Physics physics;
    public EulerIntegrator()
    {
    }

    public void Init(Physics Physics)
    {
        physics = Physics;
    }
    public void MoveOrbiters(double dt)
    {
        halfDt = dt / 2.0;

        foreach (var o in physics.AllOrbiters)
        {
            o.Velocity += o.Acceleration * dt;
            o.Position += (o.Velocity + o.Acceleration * halfDt) * dt;
        }
        physics.UpdateAcceleration();
    }
}
