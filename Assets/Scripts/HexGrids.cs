using UnityEngine;
using UnityEngine.UI;
using EasyButtons;

public class HexGrids : HexGridBase
{
    public Text cellLabelPrefab;
    public GameObject waterPrefab;

    private HexCell touchedCell;
    private GameObject waters;

    private Transform cellsTransform;
    private Transform watersTransform;

    private void Awake()
    {
        cellsTransform = transform.GetChild(0).transform;
        watersTransform = transform.GetChild(1).transform;
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

    [Button]
    public void RandomGenerate()
    {
        ClearCells();

        cells = new HexCell[height * width];

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                int elevation = (int) (Random.value * 10) % 6 - 2;

                CreateCell(x, z, i, elevation);
                SetHexCellColor(cells[i]);

                i++;
            }
        }
        SetWaterSurface();
    }

    [Button("Clear")]
    public override void ClearCells()
    {
        base.ClearCells();

        if (waters != null)
        {
            DestroyImmediate(waters);
        }
    }

    protected override HexCell CreateCell(int x, int z, int i, int elevation = 0)
    {
        if (cellsTransform == null)
        {
            cellsTransform = transform.GetChild(0).transform;
        }

        Vector3 position;
        position.x = (x + z * 0.5f - z / 2) * (HexMetrics.innerRadius * 2f);
        position.y = elevation < 0 ? elevation * HexMetrics.elevationStep : 0;
        position.z = z * 15f;

        // position cells
        HexCell cell = cells[i] = Instantiate(GetCellPrefab(Mathf.Abs(elevation)));
        cell.transform.SetParent(cellsTransform, false);
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
        if (noText)
            return;

        Canvas canvas = Instantiate(gridCanvasPrefab);
        canvas.transform.SetParent(cell.transform, false);

        Text label = Instantiate(cellLabelPrefab);
        label.rectTransform.SetParent(canvas.transform, false);
        label.rectTransform.anchoredPosition = Vector2.zero;
        label.text = cell.coordinates.ToStringOnSeparateLines();

        cell.uiRect = label.rectTransform;
    }

    private void SetWaterSurface()
    {
        if (waterPrefab == null)
        {
            Debug.LogWarning("Water prefab not set, cannot instantiate water surface.");
            return;
        }
        if (watersTransform == null)
        {
            watersTransform = transform.GetChild(1).transform;
        }

        Vector3 position = Center();
        position.y = transform.position.y + 0.1f - 15f;

        GameObject water = Instantiate(waterPrefab);
        water.transform.position = position;
        water.transform.SetParent(watersTransform);

        water.transform.localScale = new Vector3(25 * width, 30, height * 20);

        waters = water;
    }

    private void SetHexCellColor(HexCell cell)
    {
        float rnd = Random.value;
        int index = (int) (rnd * 10 % 6);

        cell.SetColor((HexMaterial) index);
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
