using System;

namespace LiruAI.Maths
{
    public static class ArrayHelpers
    {
        public static int GetIndexOfHighestValue(this float[] array)
        {
            int highestIndex = -1;
            float highestConfidence = 0;
            for (int g = 0; g < array.Length; g++)
                if (array[g] > highestConfidence)
                {
                    highestIndex = g;
                    highestConfidence = array[g];
                }

            return highestIndex;
        }

        public static void ApplyVectorisedFunction(this int[] array, Func<int, int> function)
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = function(i);
        }

        public static void ApplyVectorisedFunction(this float[] array, Func<int, float> function)
        {
            for (int i = 0; i < array.Length; i++)
                array[i] = function(i);
        }
    }
}
