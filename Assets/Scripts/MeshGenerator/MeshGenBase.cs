using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MiniHexMap
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public abstract class MeshGenBase : MonoBehaviour
    {
        protected Mesh mesh;
        protected Material defaultMat;
        protected readonly List<Vector3> vertices = new List<Vector3>();
        protected readonly List<int> triangles = new List<int>();

        public virtual void SaveAsset()
        {
            if (mesh == null)
            {
                Debug.Log("Not generated, cannot save.");
                return;
            }
            string path = EditorUtility.SaveFilePanel("Save Separate Mesh Asset", "Assets/", name, "asset");
            path = FileUtil.GetProjectRelativePath(path);
            Debug.Log("Saving mesh to: " + path);

            AssetDatabase.CreateAsset(mesh, path);
            AssetDatabase.SaveAssets();
        }

        protected void AddTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            int vertexIndex = vertices.Count;
            vertices.Add(v1);
            vertices.Add(v2);
            vertices.Add(v3);
            triangles.Add(vertexIndex);
            triangles.Add(vertexIndex + 1);
            triangles.Add(vertexIndex + 2);
        }

        protected void AddQuad(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4)
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
}