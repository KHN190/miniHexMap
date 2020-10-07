// Base on my previous project:
//  https://github.com/KHN190/UnityGrassShader/blob/master/Assets/GrassMeshGen.cs

using UnityEngine;
using EasyButtons;

public class GrassMeshGen : MeshGenBase
{
    [Header("Mesh Config")]
    [Range(.1f, .5f)]
    public float width = .1f;
    [Range(.5f, 2f)]
    public float maxHeight = 2f;
    [Range(.1f, .5f)]
    public float grassRadius = .1f;
    [Range(1, 30)]
    public int nGrass = 5;

    void Initialize()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Grass";
    }

    [Button]
    public void Generate()
    {
        Triangulate();
    }

    [Button]
    public override void SaveAsset()
    {
        base.SaveAsset();
    }

    public void Triangulate()
    {
        Initialize();

        mesh.Clear();
        vertices.Clear();
        triangles.Clear();

        CreateMesh();

        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
    }

    void CreateMesh()
    {
        for (int n = 0; n < nGrass; ++n)
        {
            float height = Random.Range(.5f, maxHeight);

            Vector3 v1 = new Vector3(0, 0, 0);
            Vector3 v2 = new Vector3(width / 2, height, 0);
            Vector3 v3 = new Vector3(width, 0, 0);

            AddTriangle(v1, v2, v3);

            // rotate and move vertices
            float radius = grassRadius * .5f;
            float rx = Random.value * radius - radius;
            float ry = Random.value * radius - radius;
            Vector3 offset = new Vector3(rx, 0, ry);

            Quaternion rotation = Quaternion.Euler(0, 30, 0);
            Matrix4x4 m = Matrix4x4.Rotate(rotation);

            for (int i = 0; i < vertices.Count; ++i)
            {
                vertices[i] = m.MultiplyPoint3x4(vertices[i]) + offset;
            }
        }
        Recenter();
    }

    void Recenter()
    {
        Vector3 center = Vector3.zero;
        foreach (Vector3 vertex in vertices)
        {
            center += vertex;
        }
        center /= vertices.Count;
        center.y = 0;

        for (int i = 0; i < vertices.Count; i++)
        {
            vertices[i] -= center;
        }
    }
}