using System;
using UnityEngine;

namespace MiniHexMap
{
    [Serializable]
    public struct HexCoordinates : IEquatable<HexCoordinates>
    {
        [SerializeField]
        private int x, z;

        public int Y
        {
            get
            {
                return -X - Z;
            }
        }

        public int X
        {
            get
            {
                return x;
            }
        }

        public int Z
        {
            get
            {
                return z;
            }
        }

        public HexCoordinates(int x, int z)
        {
            this.x = x;
            this.z = z;
        }

        public static HexCoordinates FromOffsetCoordinates(int x, int z)
        {
            return new HexCoordinates(x - z / 2, z);
        }

        public static HexCoordinates FromPosition(Vector3 position)
        {
            float x = position.x / (HexMetrics.innerRadius * 2f);
            float y = -x;
            float offset = position.z / (HexMetrics.outerRadius * 3f);
            x -= offset;
            y -= offset;

            int iX = Mathf.RoundToInt(x);
            int iY = Mathf.RoundToInt(y);
            int iZ = Mathf.RoundToInt(-x - y);

            if (iX + iY + iZ != 0)
            {
                float dX = Mathf.Abs(x - iX);
                float dY = Mathf.Abs(y - iY);
                float dZ = Mathf.Abs(-x - y - iZ);

                if (dX > dY && dX > dZ)
                {
                    iX = -iY - iZ;
                }
                else if (dZ > dY)
                {
                    iZ = -iX - iY;
                }
            }
            return new HexCoordinates(iX, iZ);
        }

        public override string ToString()
        {
            return "(" + X + ", " + Y + ", " + Z + ")";
        }

        public string ToStringOnSeparateLines()
        {
            return X + "\n" + Y + "\n" + Z;
        }

        public bool Equals(HexCoordinates other)
        {
            return X == other.X && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(HexCoordinates c1, HexCoordinates c2)
        {
            return c1.Equals(c2);
        }

        public static bool operator !=(HexCoordinates c1, HexCoordinates c2)
        {
            return !c1.Equals(c2);
        }
    }
}