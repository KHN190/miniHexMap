using EasyButtons;
using UnityEngine;

public abstract class HexGridBase : MonoBehaviour
{
    [Header("Setup")]

    [Range(1, 100)]
    public int width = 6;
    [Range(1, 100)]
    public int height = 6;

    public bool noText = true;

    [Header("Prefabs")]

    public HexCell[] cellsPrefab;
    public Canvas gridCanvasPrefab;

    protected HexCell[] cells;
    protected float[] noises;

    #region Generate

    [Button("Regenerate")]
    public virtual void RegenerateCells()
    {
        ClearCells();
        CreateCells();
    }

    [Button("Clear")]
    public virtual void ClearCells()
    {
        if (cells == null)
            return;

        foreach (HexCell c in cells)
        {
            if (c != null)
                DestroyImmediate(c.gameObject);
        }
        cells = null;
        noises = null;
    }

    [Button]
    public void Refresh()
    {
        if (cells == null)
            return;

        for (int i = 0; i < cells.Length; i++)
        {
            RefreshCell(i);
        }
    }

    public void Refresh(HexCell cell)
    {
        if (cell == null || cells == null)
            return;

        RefreshCell(cell.gridIndex);
    }

    protected void RefreshCell(int i)
    {
        HexCell cell = cells[i];

        if (cell != null)
        {
            HexCoordinates coord = cell.coordinates;
            HexCell newCell = CreateCell(coord.X + coord.Z / 2, coord.Z, i, cell.Elevation);
            newCell.SetColor(cell.material);
            newCell.SetElevation(cell.Elevation);

            DestroyImmediate(cell.gameObject);
        }
    }

    protected virtual void CreateCells()
    {
        cells = new HexCell[height * width];

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                CreateCell(x, z, i++);
            }
        }
    }

    protected virtual HexCell CreateCell(int x, int z, int i, int elevation = 0)
    {
        throw new System.NotImplementedException();
    }

    protected HexCell GetCellPrefab(int elevation)
    {
        return cellsPrefab[elevation];
    }

    public HexCell GetCell(HexCoordinates coordinates)
    {
        int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
        if (index < 0 || index >= cells.Length)
            return null;
        return cells[index];
    }

    protected Vector3 Center()
    {
        Vector3 center = Vector3.zero;
        if (cells == null)
            return center;

        foreach (HexCell cell in cells)
        {
            center += cell.transform.position;
        }
        center /= width * height;

        return new Vector3(center.x, transform.position.y, center.z);
    }
    #endregion

}
