using UnityEngine;
using UnityEngine.UI;
using EasyButtons;

public class HexGrids : MonoBehaviour
{
    [Range(1, 300)]
    public int width = 6;
    [Range(1, 300)]
    public int height = 6;

    public bool noText = true;

    public HexCell[] cellsPrefab;
    public Text cellLabelPrefab;
    public Canvas gridCanvasPrefab;

    private HexCell[] cells;
    private HexCell touchedCell;

    void Awake()
    {
        RegenerateCells();
    }

    #region Track Mouse

    void Update()
    {
        HandleInput();
    }

    void HandleInput()
    {
        Ray inputRay = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(inputRay, out RaycastHit hit))
        {
            TouchCell(hit.point);
        }
    }

    void ResetLastTouch()
    {
        if (touchedCell == null)
            return;
        touchedCell.SetColor(HexMaterial.White);
        touchedCell = null;
    }

    void TouchCell(Vector3 position)
    {
        position = transform.InverseTransformPoint(position);
        HexCoordinates coordinates = HexCoordinates.FromPosition(position);

        HexCell cell = GetCell(coordinates);
        if (cell != null)
        {
            ResetLastTouch();
            cell.SetColor(HexMaterial.Magenta);
            touchedCell = cell;
        }
    }

    public HexCell GetCell(HexCoordinates coordinates)
    {
        int index = coordinates.X + coordinates.Z * width + coordinates.Z / 2;
        if (index < 0 || index >= cells.Length)
            return null;
        return cells[index];
    }
    #endregion



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

    private void RefreshCell(int i)
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
    
    private void CreateCells()
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

    private HexCell CreateCell(int x, int z, int i, int elevation = 0)
    {
        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = 0f;
        position.z = z * 15f;

        // position cells
        HexCell cell = cells[i] = Instantiate(GetCellPrefab(elevation));
        cell.transform.SetParent(transform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        // set material
        cell.SetColor(HexMaterial.White);

        // set grid
        cell.grid = this;
        cell.gridIndex = i;

        // set neightbours
        if (x > 0)
        {
            cell.SetNeighbor(HexDirection.W, cells[i - 1]);
        }
        if (z > 0)
        {
            if ((z & 1) == 0)
            {
                cell.SetNeighbor(HexDirection.SE, cells[i - width]);
                if (x > 0)
                {
                    cell.SetNeighbor(HexDirection.SW, cells[i - width - 1]);
                }
            }
            else
            {
                cell.SetNeighbor(HexDirection.SW, cells[i - width]);
                if (x < width - 1)
                {
                    cell.SetNeighbor(HexDirection.SE, cells[i - width + 1]);
                }
            }
        }

        // set canvas
        Canvas canvas = Instantiate(gridCanvasPrefab);
        canvas.transform.SetParent(cell.transform, false);

        Text label = Instantiate(cellLabelPrefab);
        label.rectTransform.SetParent(canvas.transform, false);
        label.rectTransform.anchoredPosition = Vector2.zero;
        if (!noText)
            label.text = cell.coordinates.ToStringOnSeparateLines();

        cell.uiRect = label.rectTransform;

        return cell;
    }

    HexCell GetCellPrefab(int elevation)
    {
        return cellsPrefab[elevation];
    }
    #endregion



    #region Save/Load

    [Button("Save", ButtonSpacing.Before)]
    public void Save()
    {
        Debug.Log("save file");
    }

    [Button]
    public void Load()
    {
        Debug.Log("load file");
    }
    #endregion
}
