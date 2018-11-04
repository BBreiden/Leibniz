using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Leibniz.Test.Output
{
    class SimpleTests
    {
        public double D_Const(double x)
        {
            return 0;
        }

        public double D_Add(double x)
        {
            return 1 + 4;
        }

        public double D_Subtract(double x)
        {
            return 1 - (4 - 1);
        }

        public double D_Multiply(double x)
        {
            return 4 + ((2 * x + 2 * x));
        }

        public double D_Divide(double x)
        {
            return 3 / (4 * x) - (3 * x) * 4 / ((4 * x) * (4 * x));
        }

        public double D_CallMath(double x)
        {
            return 3 * Math.Cos(x) * Math.Cos(x) + 3 * Math.Sin(x) * -Math.Sin(x);
        }

        public double D_CallExp(double x)
        {
            return 4 * Math.Exp(-x * x) * (-1 * x + -x);
        }

        public double D_NestedCalls(double x)
        {
            return Math.Exp(Math.Sin(2 * x)) * Math.Cos(2 * x) * 2;
        }

        public (double, double) T_Const(double x)
        {
            return (1, 0);
        }
    }
}
