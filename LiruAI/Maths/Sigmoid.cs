using System;

namespace LiruAI.Maths
{
    public class Sigmoid : IActivationFunction
    {
        /// <summary> The sigmoid function. </summary>
        /// <param name="value"> The value to sigmoid. </param>
        public float Activation(float value) => 1.0f / (1.0f + (float)Math.Exp(-value));

        /// <summary> The derivative of the sigmoid function. </summary>
        /// <param name="value"> The value to find the derivative of. </param>
        /// <returns> The slope of the sigmoid function at the given <paramref name="value"/>. </returns>
        public float ActivationDerivative(float value) => value * (1.0f - value);
    }
}
