using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class IntegratorRK4Simple : IIntegrator
    {
        private double halfDt;
        private Physics physics;
        public IntegratorRK4Simple()
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
                o.Acceleration = physics.GetAccelleration(o);

                o.Pos2 = o.Position + o.Velocity * halfDt;
                o.Acc2 = physics.GetAccelleration(o, o.Pos2);
                o.Vel2 = o.Velocity + o.Acc2 * halfDt;

                o.Pos3 = o.Position + o.Vel2 * halfDt;
                o.Acc3 = physics.GetAccelleration(o, o.Pos3);
                o.Vel3 = o.Velocity + o.Acc3 * halfDt;

                o.Pos4 = o.Position + o.Vel3 * dt;
                o.Acc4 = physics.GetAccelleration(o, o.Pos4);
                o.Vel4 = o.Velocity + o.Acc4 * dt;

                o.Position += (o.Velocity + 2.0 * (o.Vel2 + o.Vel3) + o.Vel4) * (dt / 6.0);
                o.Velocity += (o.Acceleration + 2.0 * (o.Acc2 + o.Acc3) + o.Acc4) * (dt / 6.0);
            }
        }
    }
}
