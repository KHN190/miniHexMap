using UnityEngine;

public enum HexMaterial
{
    Blue, White, Yellow, Black, Red, Green, Magenta
}

public static class HexMaterialExtensions
{
    public static Material GetMaterial(this HexMaterial hexMat)
    {
        switch (hexMat)
        {
            case HexMaterial.White:
                return LoadMaterial(0);
            case HexMaterial.Blue:
                return LoadMaterial(1);
            case HexMaterial.Yellow:
                return LoadMaterial(2);
            case HexMaterial.Black:
                return LoadMaterial(3);
            case HexMaterial.Green:
                return LoadMaterial(4);
            case HexMaterial.Red:
                return LoadMaterial(5);
            case HexMaterial.Magenta:
                return LoadMaterial(6);
            default:
                return null;
        }
    }

    static Material LoadMaterial(int index)
    {
        return Resources.Load<Material>("Materials/HexCell_" + index);
    }
}
