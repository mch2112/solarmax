﻿namespace SolarMax.Integrators;

internal sealed class BeemanIntegrator : IIntegrator
{
    private Physics physics;
    public BeemanIntegrator()
    {
    }
    public void Init(Physics Physics)
    {
        physics = Physics;
        physics.UpdateAcceleration();
     
        foreach (var o in Physics.AllOrbiters)
            o.Acc2 = o.Acceleration;
    }
    public void MoveOrbiters(double dt)
    {
        foreach (var body in physics.AllOrbiters)
        {
            body.Position += (body.Velocity + (2.0 / 3.0 * body.Acceleration - 1.0 / 6.0 * body.Acc2) * dt) * dt;
            body.Acc3 = body.Acc2;
            body.Acc2 = body.Acceleration;
        }

        physics.UpdateAcceleration();
        
        foreach (var body in physics.AllOrbiters)
            body.Velocity += (1.0 / 3.0 * body.Acceleration + 5.0 / 6.0 * body.Acc2 - 1.0 / 6.0 * body.Acc3) * dt;
    }
}
