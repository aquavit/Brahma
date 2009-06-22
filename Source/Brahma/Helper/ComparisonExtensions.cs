using System;

namespace Brahma.Helper
{
    public static class ComparisonExtensions
    {
        private const float epsilon = 0.000001f;

        public static bool IsCloseTo(this float value1, float value)
        {
            return Math.Abs(value1 - value) <= epsilon;
        }

        public static bool IsCloseTo(this Vector2 actual, Vector2 expected)
        {
            return ((expected.x.IsCloseTo(actual.x)) && (expected.y.IsCloseTo(actual.y)));
        }

        public static bool IsCloseTo(this Vector3 actual, Vector3 expected)
        {
            return ((expected.x.IsCloseTo(actual.x)) && (expected.y.IsCloseTo(actual.y)) && (expected.z.IsCloseTo(actual.z)));
        }

        public static bool IsCloseTo(this Vector4 actual, Vector4 expected)
        {
            return ((expected.x.IsCloseTo(actual.x)) && (expected.y.IsCloseTo(actual.y)) && (expected.z.IsCloseTo(actual.z)) && (expected.w.IsCloseTo(actual.w)));
        }
    }
}