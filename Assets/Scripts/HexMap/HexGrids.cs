using UnityEngine;
using UnityEngine.UI;
using EasyButtons;

public class HexGrids : HexGridBase
{
    public Text cellLabelPrefab;
    public GameObject waterPrefab;

    [Header("Proc Gen")]
    public bool generateGrass = true;
    [Range(0, 1)]
    public float grassDensity = .7f;

    [Header("Perlin Noise")]
    [Range(.1f, 5f)]
    public float noiseScale = 4f;

    private HexCell touchedCell;
    private HexMaterial lastTouchedMaterial;
    
    private Transform cellsTransform;
    private Transform watersTransform;

    private Transform waters;

    private void Awake()
    {
        cellsTransform = transform.GetChild(0).transform;
        watersTransform = transform.GetChild(1).transform;
    }

    private void Start()
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
        touchedCell.SetColor(lastTouchedMaterial);
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
            lastTouchedMaterial = cell.material;
            cell.SetColor(HexMaterial.Magenta);
            touchedCell = cell;
        }
    }
    #endregion



    #region Generate

    [Button("Regenerate")]
    public override void RegenerateCells()
    {
        Clear();

        if (pool == null)
            pool = new RandomNumberPool();

        cells = new HexCell[height * width];
        noises = RandomNumberPool.Perlin(width, height, noiseScale, pool.Next() * 1000);

        for (int z = 0, i = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {   
                int elevation = (int) (noises[i] * 9) % 9 - 2;

                CreateCell(x, z, i, elevation);
                SetHexCellColor(cells[i]);
                SetGrass(cells[i]);

                i++;
            }
        }
        SetWaterSurface();
    }

    [Button("Clear")]
    public override void Clear()
    {
        base.Clear();

        if (waters != null)
        {
            DestroyImmediate(waters.gameObject);
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
        int prefabIndex = elevation <= 0 ? 1 : elevation;
        //Debug.Log("Load prefab: " + prefabIndex);

        HexCell cell = cells[i] = Instantiate(GetCellPrefab(prefabIndex));
        cell.transform.SetParent(cellsTransform, false);
        cell.transform.localPosition = position;
        cell.coordinates = HexCoordinates.FromOffsetCoordinates(x, z);

        // set material
        cell.SetColor(HexMaterial.White);

        // set grid
        cell.grid = this;
        cell.gridIndex = i;

        // set elevation;
        cell.Elevation = elevation;

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
        position.y = transform.position.y + 0.1f - 12.5f;

        GameObject water = Instantiate(waterPrefab);
        water.transform.position = position;
        water.transform.SetParent(watersTransform);

        water.transform.localScale = new Vector3(width * 18.75f, 25, height * 16f);

        waters = water.transform;
    }

    private void SetGrass(HexCell cell)
    {
        if (!generateGrass)
            return;

        if (cell.material == HexMaterial.Green || cell.material == HexMaterial.Emerald)
        {
            float rnd = pool.Next();
            if (rnd < 1 - grassDensity)
                return;

            GameObject grassPrefab = GetGrassPrefab();
            if (grassPrefab == null)
            {
                Debug.LogWarning("Grass prefab not set, cannot instantiate grass.");
                return;
            }
            float scale = 10 * rnd % 3 + 5;

            GameObject grass = Instantiate(grassPrefab);
            grass.transform.SetParent(cell.transform, false);
            grass.transform.localScale = new Vector3(scale, scale, scale);

            Vector3 position = cell.transform.position;
            position.y = cell.Elevation * HexMetrics.elevationStep;
            grass.transform.position = position;
        }
    }

    private void SetHexCellColor(HexCell cell)
    {
        float rnd = noises[cell.gridIndex];
        int index = (int) (rnd * 10 % 10);

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
