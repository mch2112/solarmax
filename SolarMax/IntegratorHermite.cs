using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class IntegratorHermite : IIntegrator
    {
        private double dt2;
        private double dt3Over6;

        private double dt2Over2;
        private Physics physics;
        private bool variableAccellerationUpdate;

        public IntegratorHermite(bool VariableAccellerationUpdate)
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

            // All wrong:

            dt2 = dt * dt;
            dt2Over2 = dt * dt / 2.0;
            dt3Over6 = dt2Over2 * dt / 3.0;

            double dt2Over12 = dt * dt / 12.0;
            double halfDt = 0.5 * dt;

            foreach (var body in physics.AllOrbiters)
            {
                body.Pos2 = body.Position + body.Velocity * dt + body.Acceleration * dt2Over2;
                body.Vel2 = body.Velocity + body.Acceleration * dt + body.Jerk * dt2Over2;
                body.Acc2 = body.Acceleration;
                body.Jerk2 = body.Jerk;
            }

            physics.UpdateAcceleration();

            foreach (var body in physics.AllOrbiters)
            {

                body.Velocity += halfDt * (body.Acceleration + body.Acc2) + dt2Over12 * (body.Jerk + body.Jerk2);
                body.Position += halfDt * (body.Velocity + body.Vel2) + dt2Over12 * (body.Acceleration + body.Acc2);
            }

            if (variableAccellerationUpdate)
                physics.UpdateAccellerationVariable();
            else
                physics.UpdateAcceleration();

            foreach (var body in physics.AllOrbiters)
                body.Velocity += body.Acceleration * dt2Over2;
        }
    }
}
