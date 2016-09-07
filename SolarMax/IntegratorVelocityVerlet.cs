using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class IntegratorVelocityVerlet : IIntegrator
    {
        private double halfDt;
        private Physics physics;
        private bool variableAccellerationUpdate;

        public IntegratorVelocityVerlet(bool VariableAccellerationUpdate)
        {
            variableAccellerationUpdate = VariableAccellerationUpdate;
        }
        public void Init(Physics Physics)
        {
            this.physics = Physics;
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

            if (variableAccellerationUpdate)
                physics.UpdateAccellerationVariable();
            else
                physics.UpdateAcceleration();

            foreach (var body in physics.AllOrbiters)
                body.Velocity += body.Acceleration * halfDt;
        }
    }
}
