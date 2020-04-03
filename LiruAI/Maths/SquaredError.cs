using System;

namespace LiruAI.Maths
{
    public class SquaredError : IErrorFunction
    {
        public float Error(float actual, float target) => 0.5f * (float)Math.Pow(actual - target, 2);

        public float ErrorDerivative(float actual, float target) => actual - target;
    }
}
