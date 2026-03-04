using System;

namespace AnimationService
{
    public static class PositionModifiers
    {
        public static float Floating(double time, float speed, float height)
        {
            return (float)(Math.Sin(time * speed) * height);
        }

        public static float Bounce(double time, float speed, float amplitude)
        {
            return (float)(Math.Abs(Math.Sin(time * speed)) * amplitude);
        }

        public static float Sway(double time, float speed, float angleMax)
        {
            return (float)(Math.Sin(time * speed) * angleMax);
        }

        public static float PushOffset(float value, float midExpected, float absMax)
        {
            if (value <= 0f) return 0f;
            if (absMax <= 0f) return 0f;
            if (midExpected <= 0f) midExpected = 0.0001f;

            float outVal = absMax * (value / (value + midExpected));

            return MathF.Min(outVal, absMax);
        }

        public static float Lerp(float a, float b, float t)
        {
            t = Math.Clamp(t, 0f, 1f);
            return a + (b - a) * t;
        }
    }
}