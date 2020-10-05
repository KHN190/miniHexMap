using UnityEngine;
using System.Collections.Generic;
using EasyButtons;
using UnityEditor;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class HexMeshGen : MonoBehaviour
{
    public HexCell hexCellPrefab;
    public Material hexCellMaterail;

    public bool generateEdge = true;
    public bool generateBottom = true;

    [Range(0, 3)]
    public int elevation;

    Mesh hexMesh;
    readonly List<Vector3> vertices = new List<Vector3>();
    readonly List<int> triangles = new List<int>();

    void Initialize()
    {
        GetComponent<MeshFilter>().mesh = hexMesh = new Mesh();
        hexMesh.name = "HexCell";
    }

    [Button]
    public void Generate()
    {
        if (hexCellPrefab == null)
        {
            Debug.Log("Hex cell prefab not assigned. Cannot generate mesh.");
            return;
        }
        Initialize();

        HexCell[] cells = { Instantiate(hexCellPrefab) };
        cells[0].SetElevation(elevation);
        Triangulate(cells);

        DestroyImmediate(cells[0].gameObject);
    }

    [Button]
    public void SaveAsset()
    {
        if (hexMesh == null)
        {
            Debug.Log("Not generated, cannot save.");
            return;
        }
        string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
        path = FileUtil.GetProjectRelativePath(path);
        Debug.Log("Saving mesh to: " + path);

        AssetDatabase.CreateAsset(hexMesh, path);
        AssetDatabase.SaveAssets();
    }

    public void Triangulate(HexCell[] cells)
    {
        Initialize();

        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();
        for (int i = 0; i < cells.Length; i++)
        {
            Triangulate(cells[i]);
        }
        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();
    }

    public void RefreshSingleCell(HexCell cell)
    {
        Initialize();

        hexMesh.Clear();
        vertices.Clear();
        triangles.Clear();

        Triangulate(cell);

        hexMesh.vertices = vertices.ToArray();
        hexMesh.triangles = triangles.ToArray();
        hexMesh.RecalculateNormals();

        cell.GetComponent<MeshFilter>().mesh = hexMesh;
    }

    void Triangulate(HexCell cell)
    {
        for (HexDirection d = HexDirection.NE; d <= HexDirection.NW; d++)
        {
            Triangulate(d, cell);
        }
    }

    void Triangulate(HexDirection direction, HexCell cell)
    {
        Vector3 center = cell.transform.localPosition;

        if (generateEdge)
        {
            Vector3 v1 = center + HexMetrics.corners[(int)direction] * HexMetrics.solidFactor;
            Vector3 v2 = center + HexMetrics.corners[(int)direction + 1] * HexMetrics.solidFactor;

            AddTriangle(center, v1, v2);

            TriangulateEdge(direction, cell, v1, v2);
        }
        else
        {
            Vector3 v1 = center + HexMetrics.corners[(int)direction];
            Vector3 v2 = center + HexMetrics.corners[(int)direction + 1];

            AddTriangle(center, v1, v2);

            TriangulateElevation(direction, cell, v1, v2);
        }

        if (generateBottom)
            TriangulateBottom(direction, cell);
    }

    void TriangulateEdge(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
    {
        Vector3 bridge = HexMetrics.GetBridge(direction);
        Vector3 v3 = v1 + bridge;
        Vector3 v4 = v2 + bridge;

        float elev = .1f * HexMetrics.elevationStep;
        v3.y -= elev;
        v4.y -= elev;

        Vector3 v5 = v2 + HexMetrics.GetBridge(direction.Next());
        v5.y -= elev;

        // generate edge
        AddQuad(v1, v2, v3, v4);
        // connect edges
        AddTriangle(v2, v4, v5);
        // generate side
        TriangulateElevation(direction, cell, v3, v4);
    }

    void TriangulateElevation(HexDirection direction, HexCell cell, Vector3 v1, Vector3 v2)
    {
        if (cell.Elevation > 0)
        {
            Vector3 v3 = v1;
            Vector3 v4 = v2;

            float elev = generateEdge ? (cell.Elevation - .1f) : cell.Elevation;
            elev  *= HexMetrics.elevationStep;

            v3.y -= elev;
            v4.y -= elev;

            AddQuad(v1, v2, v3, v4);

            if (generateEdge)
            {
                Vector3 v5 = v1 - HexMetrics.GetBridge(direction.Next());
                v5.y -= elev;
                Vector3 v6 = v5 + v1 - v3;

                AddQuad(v1, v3, v6, v5);
            }
        }
    }

    void TriangulateBottom(HexDirection direction, HexCell cell)
    {
        if (cell.Elevation == 0 && !generateEdge)
            return;

        Vector3 center = cell.transform.localPosition;
        
        if (generateEdge && cell.Elevation == 0)
            center.y -= (cell.Elevation + .1f) * HexMetrics.elevationStep;
        else
            center.y -= cell.Elevation * HexMetrics.elevationStep;

        Vector3 v1 = center + HexMetrics.corners[(int)direction];
        Vector3 v2 = center + HexMetrics.corners[(int)direction + 1];

        AddTriangle(center, v2, v1);
    }

    void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
    }

    void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
    {
        int vertexIndex = vertices.Count;
        vertices.Add(v1);
        vertices.Add(v2);
        vertices.Add(v3);
        vertices.Add(v4);
        triangles.Add(vertexIndex);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 1);
        triangles.Add(vertexIndex + 2);
        triangles.Add(vertexIndex + 3);
    }
}