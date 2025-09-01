using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;



namespace FirePixel.PathFinding
{
    [BurstCompile]
    public struct AstarPathFindJobParallelBatched : IJobParallelForBatch
    {
        [NativeDisableParallelForRestriction]
        [NoAlias][ReadOnly] public NativeArray<Node> nodes;

        [NativeDisableParallelForRestriction]
        [NoAlias][ReadOnly] public NativeArray<int3> neighbourOffsets;

        [NativeDisableParallelForRestriction]
        [NoAlias] public NativeArray<Node> neighbours;

        [NoAlias][ReadOnly] public int gridLengthX, gridLengthY, gridLengthZ;
        [NoAlias][ReadOnly] public bool allowDiagonalMovement;



        public void Execute(int startIndex, int count)
        {
            int cIndex = startIndex;
            for (int i = 0; i < count; i++, cIndex++)
            {

            }
        }

        private bool TryGetPathToTarget(Node startNode, Node targetNode, NativeArray<float3> path)
        {
            int arrayMaxCapacity = gridManager.TotalGridSize;

            NodeHeap openNodes = new NodeHeap(arrayMaxCapacity, Allocator.TempJob);
            NativeHashSet<Node> closedNodes = new NativeHashSet<Node>(arrayMaxCapacity, Allocator.TempJob);


            openNodes.Add(startNode);
            while (openNodes.Count > 0)
            {
                Node currentNode = openNodes.RemoveFirstSwapBack();
                closedNodes.Add(currentNode);

                if (currentNode == targetNode)
                {
                    bool pathSucces = RetracePath(startNode, targetNode, path);

                    return pathSucces;
                }

                GetNeighbours(currentNode, neighbours, out int neighbourCount);

                for (int i = 0; i < neighbourCount; i++)
                {
                    Node neighbour = neighbours[i];

                    if (neighbour.walkable == false || closedNodes.Contains(neighbour))
                    {
                        continue;
                    }

                    int currentNodeGridId = currentNode.gridId;

                    int neigbourDist = GetDistanceCost(currentNodeGridId, neighbour.gridId);
                    int newMovementCostToNeigbour = currentNode.gCost + neigbourDist;


                    if (newMovementCostToNeigbour < neighbour.gCost || !openNodes.Contains(neighbour))
                    {
                        neighbour.UpdateNode(newMovementCostToNeigbour, GetDistanceCost(neighbour.gridId, targetNode.gridId), currentNodeGridId);

                        if (!openNodes.Contains(neighbour))
                        {
                            openNodes.Add(neighbour);
                        }
                    }
                }
            }

            return false;
        }

        private void GetNeighbours(Node targetNode, NativeArray<Node> neighbours, out int neighbourCount)
        {
            neighbourCount = 0;

            int maxNeighbours = neighbourOffsets.Length;
            int3 pos = GridIdToGridPos(targetNode.gridId);

            for (int i = 0; i < maxNeighbours; i++)
            {
                int3 neighbourPos = pos + neighbourOffsets[i];

                // Check bounds
                if (neighbourPos.x >= 0 && neighbourPos.x < gridLengthX &&
                    neighbourPos.y >= 0 && neighbourPos.y < gridLengthY &&
                    neighbourPos.z >= 0 && neighbourPos.z < gridLengthZ)
                {
                    neighbours[neighbourCount] = nodes[GridPosToGridId(neighbourPos)];
                    neighbourCount += 1;
                }
            }
        }

        /// <summary>
        /// Get int distance cost between 2 nodes A and B
        /// </summary>
        private int GetDistanceCost(int gridIdA, int gridIdB)
        {
            int3 gridPosA = GridIdToGridPos(gridIdA);
            int3 gridPosB = GridIdToGridPos(gridIdB);

            int distX = math.abs(gridPosA.x - gridPosB.x);
            int distY = math.abs(gridPosA.y - gridPosB.y);
            int distZ = math.abs(gridPosA.z - gridPosB.z);

            if (allowDiagonalMovement)
            {
                if (distX > distZ)
                {
                    return 14 * distZ + 10 * (distX - distZ) + 50 * distY;
                }
                else
                {
                    return 14 * distX + 10 * (distZ - distX) + 50 * distY;
                }
            }
            else
            {
                return 10 * (distX + distZ) + 50 * distY;
            }
        }
        /// <summary>
        /// GridId to Node GridPos
        /// </summary>
        private int3 GridIdToGridPos(int gridId)
        {
            int x = gridId % gridLengthX;
            int y = (gridId / gridLengthX) % gridLengthY;
            int z = gridId / (gridLengthX * gridLengthY);
            return new int3(x, y, z);
        }
        /// <summary>
        /// GridPos to Grid Id
        /// </summary>
        private int GridPosToGridId(int3 gridPos)
        {
            return gridPos.x + gridPos.y * gridLengthX + gridPos.z * gridLengthX * gridLengthY;
        }


        private bool RetracePath(Node startNode, Node endNode, NativeArray<float3> path)
        {
            if (endNode == startNode)
            {
                DebugLogger.LogWarning("Target Already Reached");
                return false;
            }

            Node currentNode = endNode;
            int pathNodeCount = 0;

            while (currentNode != startNode)
            {
                path[pathNodeCount++] = currentNode.worldPos;

                currentNode = nodes[currentNode.parentNodeId];
            }

            // Reverse path array because its now from end to start instead of from start to end
            ReversePath(path, pathNodeCount);

            return true;
        }

        /// <summary>
        /// Reverse Array
        /// </summary>
        private void ReversePath(NativeArray<float3> path, int pathNodeCount)
        {
            int left = 0;
            int right = pathNodeCount - 1;

            while (left < right)
            {
                (path[right], path[left]) = (path[left], path[right]);
                left++;
                right--;
            }
        }
    }
}