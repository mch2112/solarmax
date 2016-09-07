using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal interface IIntegrator
    {
        void Init(Physics Physics);
        void MoveOrbiters(double dt);
    }
}
