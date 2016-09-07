using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    class IntegratorVVJerk : IIntegrator
    {
        private Physics physics;
        private bool variableAccellerationUpdate;

        public IntegratorVVJerk(bool VariableAccellerationUpdate)
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

            if (variableAccellerationUpdate)
                physics.UpdateAccellerationVariable();
            else
                physics.UpdateAcceleration();

            foreach (var body in physics.AllOrbiters)
            {
                body.Jerk = (body.Acceleration - body.Acc2) / dt;
                body.Velocity += body.Acceleration * halfDt + body.Jerk * quarterDt2;
            }
        }
    }
}
