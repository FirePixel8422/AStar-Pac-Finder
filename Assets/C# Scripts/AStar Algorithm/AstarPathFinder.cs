using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using Unity.VisualScripting;
using UnityEngine;



namespace FirePixel.PathFinding
{
    [BurstCompile]
    public static class AStarPathfinder
    {
#pragma warning disable UDR0001
        private static GridManager gridManager;
        public static bool allowDiagonalMovement = true;
#pragma warning restore UDR0001

        public static void Init(GridManager gridManager, bool allowDiagonalMovement)
        {
            AStarPathfinder.gridManager = gridManager;
            AStarPathfinder.allowDiagonalMovement = allowDiagonalMovement;
        }

        public static bool TryGetPathToTarget(Vector3 startPos, Vector3 targetPos, int visibleTileCost, List<Vector3> path, bool forceAvoidVisibleTiles)
        {
            Node startNode = gridManager.NodeFromWorldPoint(startPos);
            Node targetNode = gridManager.NodeFromWorldPoint(targetPos);

            int arrayMaxCapacity = gridManager.TotalGridSize;

            NodeHeap openNodes = new NodeHeap(arrayMaxCapacity);
            NativeHashSet<Node> closedNodes = new NativeHashSet<Node>(arrayMaxCapacity, Allocator.Temp);


            openNodes.Add(startNode);
            while (openNodes.Count > 0)
            {
                Node currentNode = openNodes.RemoveFirst();
                closedNodes.Add(currentNode);

                if (currentNode == targetNode)
                {
                    bool pathSucces = RetracePath(startNode, targetNode, path, forceAvoidVisibleTiles);

                    return pathSucces;
                }


                foreach (Node neigbour in gridManager.GetNeigbours(currentNode))
                {
                    if (neigbour.walkable == false || closedNodes.Contains(neigbour))
                    {
                        continue;
                    }

                    int2 currentNodeGridId = currentNode.gridId;

                    int neigbourDist = GetDistance(currentNodeGridId, neigbour.gridId);
                    int newMovementCostToNeigbour = currentNode.gCost + neigbourDist;


                    if (newMovementCostToNeigbour < neigbour.gCost || !openNodes.Contains(neigbour))
                    {
                        neigbour.gCost = newMovementCostToNeigbour;

                        neigbour.hCost = GetDistance(neigbour.gridId, targetNode.gridId);
                        neigbour.parentGridId = currentNodeGridId;

                        if (!openNodes.Contains(neigbour))
                        {
                            openNodes.Add(neigbour);
                        }
                    }
                }
            }

            return false;
        }

        private static bool RetracePath(Node startNode, Node endNode, List<Vector3> path, bool forceAvoidVisibleTiles)
        {
            path.Clear();

            if (endNode == startNode)
            {
                Debug.LogWarning("Target Already Reached");
                return false;
            }

            Node currentNode = endNode;

            while (currentNode != startNode)
            {
                path.Add(currentNode.worldPos);

                currentNode = gridManager.NodeFromGridId(currentNode.parentGridId);
            }

            // Reverse when done
            path.Reverse();

            return true;
        }


        private static int GetDistance(int2 gridIdA, int2 gridIdB)
        {
            int distX = math.abs(gridIdA.x - gridIdB.x);
            int distZ = math.abs(gridIdA.y - gridIdB.y);

            if (distX > distZ)
            {
                return 14 * distZ + 10 * (distX - distZ);
            }
            else
            {
                return 14 * distX + 10 * (distZ - distX);
            }
        }
    }
}