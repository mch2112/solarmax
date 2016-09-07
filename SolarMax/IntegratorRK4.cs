﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal sealed class IntegratorRK4 : IIntegrator
    {
        private double halfDt;
        private Physics physics;
        public IntegratorRK4()
        {
        }
        public void Init(Physics Physics)
        {
            this.physics = Physics;
        }
        public void MoveOrbiters(double dt)
        {
            halfDt = dt / 2.0;

            physics.UpdateAcceleration();

            foreach (var o in physics.AllOrbiters)
            {
                o.Pos2 = o.Position + o.Velocity * halfDt;
            }
            
            UpdateAccelleration2();
            
            foreach (var o in physics.AllOrbiters)
            {
                o.Vel2 = o.Velocity + o.Acc2 * halfDt;
                o.Pos3 = o.Position + o.Vel2 * halfDt;
            }

            UpdateAccelleration3();

            foreach (var o in physics.AllOrbiters)
            {
                o.Vel3 = o.Velocity + o.Acc3 * halfDt;
                o.Pos4 = o.Position + o.Vel3 * dt;
            }

            UpdateAccelleration4();

            foreach (var o in physics.AllOrbiters)
            {
                o.Vel4 = o.Velocity + o.Acc4 * dt;
                o.Position += (o.Velocity + 2.0 * (o.Vel2 + o.Vel3) + o.Vel4) * (dt / 6.0);
                o.Velocity += (o.Acceleration + 2.0 * (o.Acc2 + o.Acc3) + o.Acc4) * (dt / 6.0);
            }
        }
        private void UpdateAccelleration2()
        {
            foreach (var o in physics.AllOrbiters)
                o.Acc2 = new Vector();

            foreach (var gi in physics.GravitationalInfluences)
                gi.Item1.Acc2 = gi.Item1.Acc2 + gi.Item1.Pos2.DifferenceDirection(gi.Item2.Pos2) * (gi.Item2.MG / gi.Item1.Pos2.DistanceToSquared(gi.Item2.Pos2));
        }
        private void UpdateAccelleration3()
        {
            foreach (var o in physics.AllOrbiters)
                o.Acc3 = new Vector();

            foreach (var gi in physics.GravitationalInfluences)
                gi.Item1.Acc3 = gi.Item1.Acc3 + gi.Item1.Pos3.DifferenceDirection(gi.Item2.Pos3) * (gi.Item2.MG / gi.Item1.Pos3.DistanceToSquared(gi.Item2.Pos3));
        }
        private void UpdateAccelleration4()
        {
            foreach (var o in physics.AllOrbiters)
                o.Acc4 = new Vector();

            foreach (var gi in physics.GravitationalInfluences)
                gi.Item1.Acc4 = gi.Item1.Acc4 + gi.Item1.Pos4.DifferenceDirection(gi.Item2.Pos4) * (gi.Item2.MG / gi.Item1.Pos4.DistanceToSquared(gi.Item2.Pos4));
        }
    }
}
