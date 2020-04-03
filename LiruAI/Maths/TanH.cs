using System;

namespace LiruAI.Maths
{
    public class TanH : IActivationFunction
    {
        public float Activation(float value) => (float)Math.Tanh(value);

        public float ActivationDerivative(float value) => 1 - (float)Math.Pow(Activation(value), 2);
    }
}
