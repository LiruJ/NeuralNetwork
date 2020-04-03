namespace LiruAI.Maths
{
    interface IErrorFunction
    {
        float Error(float actual, float target);

        float ErrorDerivative(float actual, float target);
    }
}
