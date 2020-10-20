using System.Collections.Generic;
using EasyButtons;
using UnityEngine;

namespace MiniHexMap
{
    public abstract class HexGridBase : MonoBehaviour
    {
        [Header("Setup")]

        [Range(1, 100)]
        public int width = 6;
        [Range(1, 100)]
        public int height = 6;

        [Header("Prefabs")]
        public Canvas gridCanvasPrefab;

        protected HexCell[] cells;
        protected float[] noises;
        protected Vector3 center;

        protected NoisePool pool;
        protected readonly Dictionary<int, HexCell> cellPrefabCache = new Dictionary<int, HexCell>();

        #region Generate

        [Button("Regenerate")]
        public virtual void RegenerateCells()
        {
            if (pool == null)
                pool = new NoisePool();

            Clear();
            CreateCells();
        }

        [Button("Clear")]
        public virtual void Clear()
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
            center = Vector3.zero;
            cellPrefabCache.Clear();
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
            if (!cellPrefabCache.ContainsKey(elevation))
            {
                HexCell prefab = Resources.Load<HexCell>("Prefabs/Map/HexCell_" + elevation);
                cellPrefabCache[elevation] = prefab;
            }
            return cellPrefabCache[elevation];
        }

        protected GameObject GetWaterPrefab()
        {
            return Resources.Load<GameObject>("Prefabs/Map/Water");
        }

        protected GameObject GetGrassPrefab()
        {
            int index = pool.Next() < .5f ? 0 : 1;
            return Resources.Load<GameObject>("Prefabs/Map/Grass_" + index);
        }

        protected GameObject GetTribePrefab(int index = 0)
        {
            return Resources.Load<GameObject>("Prefabs/Tribe/Tribe_" + index);
        }

        protected GameObject GetFieldPrefab(int index = 0)
        {
            return Resources.Load<GameObject>("Prefabs/Tribe/Field_" + index);
        }

        protected GameObject GetWallPrefab()
        {
            return Resources.Load<GameObject>("Prefabs/Tribe/Wall");
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
            if (center != Vector3.zero)
                return center;

            center = Vector3.zero;
            if (cells == null)
                return center;

            foreach (HexCell cell in cells)
            {
                center += cell.transform.position;
            }
            center /= width * height;

            return new Vector3(center.x, transform.position.y, center.z);
        }

        protected GameObject SetPrefabAtTop(GameObject prefab, HexCell cell, float scale = 1f, bool rotate = true)
        {
            GameObject go = Instantiate(prefab);
            go.transform.SetParent(cell.transform, false);
            go.transform.localScale = new Vector3(scale, scale, scale);

            Vector3 position = cell.transform.position;
            position.y = Mathf.Max(cell.Elevation, 1) * HexMetrics.elevationStep;
            go.transform.position = position;

            if (rotate)
                RandomRotate(go);

            return go;
        }

        protected GameObject SetPrefabAtTopEdge(GameObject prefab, HexCell cell, float scale, HexDirection direction)
        {
            direction = direction.Next();

            GameObject go = Instantiate(prefab);
            Vector3 oldScale = go.transform.localScale;
            go.transform.SetParent(cell.transform, false);
            go.transform.localScale = oldScale * scale;

            Vector3 position = cell.transform.position;
            Vector3 offset = HexMetrics.edges[(int)direction] * .8f;
            position += offset;

            position.y = Mathf.Max(cell.Elevation, 1) * HexMetrics.elevationStep;
            go.transform.position = position;
            go.name += "_" + direction;

            HexRotate(go, direction);

            return go;
        }

        protected void RandomRotate(GameObject go)
        {
            HexDirection face = (HexDirection)(pool.Next() * 10 % 6);
            HexRotate(go, face);
        }

        protected void HexRotate(GameObject go, HexDirection direction)
        {
            Vector3 rotation = new Vector3(0, ((int)direction + 1) % 6 * 60, 0);
            go.transform.Rotate(rotation);
        }
        #endregion

    }
}
