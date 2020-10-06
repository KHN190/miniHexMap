using UnityEngine;
using UnityEngine.UI;
using EasyButtons;

public class HexGrids : HexGridBase
{
    public Text cellLabelPrefab;

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
    #endregion



    #region Generate

    protected override HexCell CreateCell(int x, int z, int i, int elevation = 0)
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
        SetNeighbors(cell, x, z, i);

        // set canvas
        SetCanvas(cell);

        return cell;
    }

    private void SetNeighbors(HexCell cell, int x, int z, int i)
    {
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
    }

    private void SetCanvas(HexCell cell)
    {
        Canvas canvas = Instantiate(gridCanvasPrefab);
        canvas.transform.SetParent(cell.transform, false);

        Text label = Instantiate(cellLabelPrefab);
        label.rectTransform.SetParent(canvas.transform, false);
        label.rectTransform.anchoredPosition = Vector2.zero;
        if (!noText)
            label.text = cell.coordinates.ToStringOnSeparateLines();

        cell.uiRect = label.rectTransform;
    }
    #endregion



    #region Save/Load

    [Button("Save", ButtonSpacing.Before)]
    public void Save()
    {
        Debug.Log("save file");
    }

    [Button("Load", ButtonSpacing.After)]
    public void Load()
    {
        Debug.Log("load file");
    }
    #endregion
}
