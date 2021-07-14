using System;
using osu.Framework.Extensions;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Sentakki.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Sentakki
{
    public static class SentakkiExtensions
    {
        public static int NormalizePath(this int path)
        {
            while (path < 0) path += 8;
            path %= 8;
            return path;
        }

        public static float GetDeltaAngle(float a, float b)
        {
            float x = b;
            float y = a;

            if (a > b)
            {
                x = a;
                y = b;
            }

            if (x - y < 180)
                x -= y;
            else
                x = 360 - x + y;

            return x;
        }

        public static float GetRotationForLane(this int lane)
        {
            while (lane < 0) lane += 8;
            lane %= 8;
            return SentakkiPlayfield.LANEANGLES[lane];
        }
        public static Vector2 GetPositionAlongLane(float distance, int lane) => GetCircularPosition(distance, lane.GetRotationForLane());

        public static Vector2 GetCircularPosition(float distance, float angle)
        {
            return new Vector2(-(distance * (float)Math.Cos((angle + 90) * (float)(Math.PI / 180))), -(distance * (float)Math.Sin((angle + 90) * (float)(Math.PI / 180))));
        }

        public static float GetDegreesFromPosition(this Vector2 a, Vector2 b)
        {
            Vector2 direction = b - a;
            float angle = MathHelper.RadiansToDegrees(MathF.Atan2(direction.Y, direction.X));
            if (angle < 0f) angle += 360f;
            return angle + 90;
        }

        public static Color4 GetColorForSentakkiResult(this HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                    return Color4.Orange;
                case HitResult.Good:
                    return Color4.DeepPink;
                case HitResult.Ok:
                    return Color4.Green;
                default:
                    return Color4.LightGray;
            }
        }

        public static string GetDisplayNameForSentakkiResult(this HitResult result)
        {
            switch (result)
            {
                case HitResult.Great:
                    return "Perfect";
                case HitResult.Good:
                    return "Great";
                case HitResult.Ok:
                    return "Good";
                default:
                    return result.GetDescription();
            }
        }

        public static int GetNoteLaneFromDegrees(this float degrees)
        {
            if (degrees < 0) degrees += 360;
            if (degrees >= 360) degrees %= 360;
            int result = 0;

            for (int i = 0; i < SentakkiPlayfield.LANEANGLES.Length; ++i)
            {
                if (SentakkiPlayfield.LANEANGLES[i] - degrees >= -22.5f && SentakkiPlayfield.LANEANGLES[i] - degrees <= 22.5f)
                    result = i;
            }
            return result;
        }
    }
}
