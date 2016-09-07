using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class IntegratorEuler : IIntegrator
    {
        private double halfDt;
        private Physics physics;
        public IntegratorEuler()
        {
        }

        public void Init(Physics Physics)
        {
            this.physics = Physics;
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
}
