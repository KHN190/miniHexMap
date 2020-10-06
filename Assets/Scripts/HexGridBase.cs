using EasyButtons;
using UnityEngine;

public abstract class HexGridBase : MonoBehaviour
{
    [Header("Setup")]

    [Range(1, 100)]
    public int width = 6;
    [Range(1, 100)]
    public int height = 6;
    [Range(1, 4)]
    public int layers = 1;

    public bool noText = true;

    [Header("Prefabs")]

    public HexCell[] cellsPrefab;
    public Canvas gridCanvasPrefab;

    protected HexCell[] cells;

    void Awake()
    {
        RegenerateCells();
    }

    #region Generate

    [Button("Regenerate")]
    public void RegenerateCells()
    {
        ClearCells();
        CreateCells();
    }

    [Button("Clear")]
    public void ClearCells()
    {
        if (cells == null)
            return;

        foreach (HexCell c in cells)
        {
            if (c != null)
                DestroyImmediate(c.gameObject);
        }
        cells = new HexCell[0];
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
    #endregion

}
