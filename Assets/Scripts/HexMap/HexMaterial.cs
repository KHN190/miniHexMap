using UnityEngine;

namespace MiniHexMap
{
    public enum HexMaterial
    {
        White, Blue, LightBlue, Yellow, Brown, Black, Emerald, Green, Red, DarkRed, Magenta
    }

    public static class HexMaterialExtensions
    {
        public static Material GetMaterial(this HexMaterial hexMat)
        {
            return LoadMaterial((int)hexMat);
        }

        static Material LoadMaterial(int index)
        {
            return Resources.Load<Material>("Materials/HexCell_" + index);
        }
    }
}