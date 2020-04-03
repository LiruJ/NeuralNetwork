namespace LiruAI.Maths
{
    public interface IActivationFunction
    {
        float Activation(float value);
        float ActivationDerivative(float value);
    }
}