using System;
using System.Collections.Generic;
using System.Text;

namespace Leibniz.Test.Inputs
{
    class SimpleTests
    {
        public double Const(double x)
        {
            return 1.0;
        }

        public double Add(double x)
        {
            return x + 4 * x;
        }

        public double Subtract(double x)
        {
            return x - (4 - x);
        }

        public double Multiply(double x)
        {
            return 4 * x + (2 * x * x);
        }

        public double Divide(double x)
        {
            return (3 * x) / (4 * x);
        }

        public double CallMath(double x)
        {
            return 3 * Math.Sin(x) * Math.Cos(x);
        }

        public double CallExp(double x)
        {
            return 4 * Math.Exp(-x * x);
        }

        public double NestedCalls(double x)
        {
            return Math.Exp(Math.Sin(2 * x));
        }

        public double Composition(double x)
        {
            return Multiply(x) * Add(x);
        }
    }
}
