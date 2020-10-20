using UnityEngine;

namespace MiniHexMap
{
    public static class HexMetrics
    {
        public const float outerRadius = 10f;
        public const float innerRadius = outerRadius * 0.866025404f;
        public const float elevationStep = 5f;
        public const float solidFactor = 0.85f;
        public const float edgeFactor = 1f - solidFactor;

        public static Vector3[] corners = {
            new Vector3(0f, 0f, outerRadius),
            new Vector3(innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(0f, 0f, -outerRadius),
            new Vector3(-innerRadius, 0f, -0.5f * outerRadius),
            new Vector3(-innerRadius, 0f, 0.5f * outerRadius),
            new Vector3(0f, 0f, outerRadius)
        };

        public static Vector3[] edges = {
            new Vector3(innerRadius * -.5f, 0f, innerRadius), // NE
            new Vector3(innerRadius * .5f, 0f, innerRadius), // E
            new Vector3(innerRadius, 0f, 0f), // SE
            new Vector3(innerRadius * .5f, 0f, -innerRadius), // SW
            new Vector3(innerRadius * -.5f, 0f, -innerRadius), // W
            new Vector3(-innerRadius, 0f, 0f), // NW
        };
        
        public static Vector3 GetBridge(HexDirection direction)
        {
            return (corners[(int)direction] + corners[(int)direction + 1]) * 0.5f * edgeFactor;
        }
    }
}