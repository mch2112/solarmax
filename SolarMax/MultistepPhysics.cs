using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class MultistepPhysics : Physics
    {
        private const int HISTORY_SIZE = 8;
        private int historyPointer = 0;

        public MultistepPhysics(bool AllowLoadSnapshot, StartupDoneDelegate StartupDone)
            : base(AllowLoadSnapshot, StartupDone)
        {
        }
        protected override void Init()
        {
            foreach (var o in AllOrbiters)
            {
                o.Acceleration = GetAccelleration(o);
                o.InitHistory(HISTORY_SIZE);
            }
        }
        protected override void MoveOrbiters()
        {
            int tNext = (historyPointer + 1) % HISTORY_SIZE;
            int t0 = historyPointer;
            int t1 = (historyPointer + (HISTORY_SIZE - 1)) % HISTORY_SIZE;
            int t2 = (historyPointer + (HISTORY_SIZE - 2)) % HISTORY_SIZE;
            int t3 = (historyPointer + (HISTORY_SIZE - 3)) % HISTORY_SIZE;
            int t4 = (historyPointer + (HISTORY_SIZE - 4)) % HISTORY_SIZE;
            int t5 = (historyPointer + (HISTORY_SIZE - 5)) % HISTORY_SIZE;
            int t6 = (historyPointer + (HISTORY_SIZE - 6)) % HISTORY_SIZE;
            int t7 = (historyPointer + (HISTORY_SIZE - 7)) % HISTORY_SIZE;
       //     int t8 = (historyPointer + (HISTORY_SIZE - 8)) % HISTORY_SIZE;

            double dt2 = dt * dt;

            foreach (var o in AllOrbiters)
            {
                Vector v = new Vector();
                Vector temp = o.AccellerationHistory[tNext] = GetAccelleration(o);

                v += (22081.0 / 15120.0) * temp;
                temp = o.AccellerationHistory[t1] + o.AccellerationHistory[t6];
                v += (-7337.0 / 15120.0) * temp;
                temp = o.AccellerationHistory[t2] + o.AccellerationHistory[t5];
                v += (-45765.0 / 15120.0) * temp;
                temp = o.AccellerationHistory[t3] + o.AccellerationHistory[t4];
                v += (-29.0 / 15120.0) * temp;
                v.Multiply(dt2);
                v += o.VelocityHistory[t7];

                o.Position = o.PositionHistory[tNext] = o.Position + v;
                o.Velocity = o.VelocityHistory[tNext] = v;
            }
            historyPointer = tNext;

            /*
            foreach (var o in AllOrbiters)
            {
                o.Acceleration = GetAccelleration(o);
                o.AccellerationHistory[historyPointer] = o.Acceleration;
            }
            foreach (var o in AllOrbiters)
            {
                // QT: p4 = -1(p-4) + 2(p3+p-3) - 2(p2+p-2) + 1(p1+p-1) + dt*dt* (17671(a3+a-3) - 23622(a2+a-2) + 61449(a1+a-1) - 50516a0 )/15120 
                
                var pa = (( 17671.0 * (o.AccellerationHistory[t0] + o.AccellerationHistory[t6])) +
                          (-23622.0 * (o.AccellerationHistory[t1] + o.AccellerationHistory[t5])) +
                          ( 61499.0 * (o.AccellerationHistory[t2] + o.AccellerationHistory[t4])) +
                          (-50516.0 * (o.AccellerationHistory[t3]))) / 15120.0;

                var p = (-1.0 * o.PositionHistory[t8]) +
                        ( 2.0 * (o.PositionHistory[t1] + o.PositionHistory[t7])) +
                        (-2.0 * (o.PositionHistory[t2] + o.PositionHistory[t6])) +
                        ( 1.0 * (o.PositionHistory[t3] + o.PositionHistory[t5]));

                o.Position =
                o.PositionHistory[t0] = p + pa * dt2;
            }
            historyPointer = tNext;*/
        }
    }
}
