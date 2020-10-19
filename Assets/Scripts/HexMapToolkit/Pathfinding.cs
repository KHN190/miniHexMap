/*
 * Reference:
 *  https://www.redblobgames.com/pathfinding/a-star/introduction.html
 *  https://github.com/BlueRaja/High-Speed-Priority-Queue-for-C-Sharp
 */

using System.Collections.Generic;
using MiniHexMap;
using Priority_Queue;

namespace HexMapToolkit
{
    static class PathfindingBase
    {
        public static HexCell[] ConstructPath(HexCell from, HexCell to, Dictionary<HexCell, HexCell> visited)
        {
            List<HexCell> path = new List<HexCell>();
            HexCell curr = to;

            while (curr != from)
            {
                path.Add(curr);
                curr = visited[curr];
            }
            if (curr == from)
            {
                path.Add(curr);
            }

            return path.ToArray();
        }
    }

    public static class BFS
    {
        public static HexCell[] Path(int from, int to, HexCell[] cells)
        {
            Queue<HexCell> frontier = new Queue<HexCell>();
            Dictionary<HexCell, HexCell> visited = new Dictionary<HexCell, HexCell>();

            HexCell start = cells[from];
            HexCell goal = cells[to];
            VisitCell(start, null, ref frontier, ref visited);

            while (frontier.Count > 0)
            {
                HexCell curr = frontier.Dequeue();

                if (curr == goal) break;

                foreach (HexCell cell in curr.GetAllNeighbors())
                {
                    if (!visited.ContainsKey(cell))
                    {
                        VisitCell(cell, curr, ref frontier, ref visited);
                    }
                }
            }
            return PathfindingBase.ConstructPath(start, goal, visited);
        }

        static void VisitCell(HexCell cell, HexCell from,
            ref Queue<HexCell> frontier,
            ref Dictionary<HexCell, HexCell> visited)
        {
            frontier.Enqueue(cell);
            visited[cell] = from;
        }
    }

    public static class AStar
    {
        public static HexCell[] Path(int from, int to, HexCell[] cells)
        {
            SimplePriorityQueue<HexCell> frontier = new SimplePriorityQueue<HexCell>();
            Dictionary<HexCell, HexCell> visited = new Dictionary<HexCell, HexCell>();
            Dictionary<HexCell, float> costAcc = new Dictionary<HexCell, float>();

            HexCell start = cells[from];
            HexCell goal = cells[to];

            VisitCell(start, null, 0, ref frontier, ref visited);
            costAcc[start] = 0;

            while (frontier.Count > 0)
            {
                HexCell curr = frontier.Dequeue();

                if (curr == goal) break;

                foreach (HexCell cell in curr.GetAllNeighbors())
                {
                    float nextCost = costAcc[curr] + cell.MoveCost;

                    if (!visited.ContainsKey(cell) || nextCost < costAcc[cell])
                    {
                        costAcc[cell] = nextCost + HeuristicCost(cell, goal);

                        VisitCell(cell, curr, nextCost, ref frontier, ref visited);
                    }
                }
            }
            return PathfindingBase.ConstructPath(start, goal, visited);
        }

        static float HeuristicCost(HexCell cell, HexCell goal)
        {
            return Abs(cell.coordinates.X - goal.coordinates.X) + Abs(cell.coordinates.Z - goal.coordinates.Z);
        }

        static float Abs(float a)
        {
            return a < 0 ? -a : a;
        }

        static void VisitCell(HexCell cell, HexCell from, float priority,
            ref SimplePriorityQueue<HexCell> frontier,
            ref Dictionary<HexCell, HexCell> visited)
        {
            frontier.Enqueue(cell, priority);
            visited[cell] = from;
        }
    }
}