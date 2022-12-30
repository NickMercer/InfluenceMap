namespace Natick
{
    public static class FloatExtensions
    {
        public static float Normalize(this float value, float min, float max) 
        {
            return (value - min) / (max - min);
        }
    }
}