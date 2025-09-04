using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;



namespace FirePixel.PathFinding
{
    [BurstCompile]
    public struct AstarPathFindJob : IJob
    {
        [NoAlias][ReadOnly] public NativeArray<Node> nodes;

        [NoAlias][ReadOnly] public NativeArray<int3> neighbourOffsets;
        [NoAlias] public NativeArray<Node> neighbours;

        [NoAlias][ReadOnly] public float3 gridSize;
        [NoAlias][ReadOnly] public float3 gridPosition;
        [NoAlias][ReadOnly] public int3 gridLength;
        [NoAlias][ReadOnly] public float nodeSize;

        [NoAlias][ReadOnly] public bool allowDiagonalMovement;

        [NoAlias] public NodeHeap openNodes;
        [NoAlias] public NativeHashSet<Node> closedNodes;

        [NoAlias][ReadOnly] public float3 startWorldPos;
        [NoAlias][ReadOnly] public float3 targetWorldPos;

        [NoAlias] public NativeArray<float3> path;


        public void SetupPathfinderData(NativeArray<Node> nodes, NodeHeap openNodes, NativeHashSet<Node> closedNodes, float3 startWorldPos, float3 targetWorldPos, NativeArray<float3> path)
        {
            this.nodes = nodes;
            this.openNodes = openNodes;
            this.closedNodes = closedNodes;
            this.startWorldPos = startWorldPos;
            this.targetWorldPos = targetWorldPos;
            this.path = path;
        }


        public void Execute()
        {
            Node startNode = GetNodeFromWorldPos(startWorldPos);
            Node targetNode = GetNodeFromWorldPos(targetWorldPos);

            openNodes.Add(startNode);
            while (openNodes.Count > 0)
            {
                Node currentNode = openNodes.RemoveFirstSwapBack();
                closedNodes.Add(currentNode);

                if (currentNode == targetNode)
                {
                    RetracePath(startNode, targetNode, path);
                    return;
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

                        nodes[neighbour.gridId] = neighbour;

                        if (!openNodes.Contains(neighbour))
                        {
                            openNodes.Add(neighbour);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Get valid neighbour nodes of targetNode
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void GetNeighbours(Node targetNode, NativeArray<Node> neighbours, out int neighbourCount)
        {
            neighbourCount = 0;

            int maxNeighbours = neighbourOffsets.Length;
            int3 pos = GridIdToGridPos(targetNode.gridId);

            for (int i = 0; i < maxNeighbours; i++)
            {
                int3 neighbourPos = pos + neighbourOffsets[i];

                // Check bounds
                if (neighbourPos.x >= 0 && neighbourPos.x < gridLength.x &&
                    neighbourPos.y >= 0 && neighbourPos.y < gridLength.y &&
                    neighbourPos.z >= 0 && neighbourPos.z < gridLength.z)
                {
                    neighbours[neighbourCount] = nodes[GridPosToGridId(neighbourPos)];
                    neighbourCount += 1;
                }
            }
        }

        /// <summary>
        /// Get int distance cost between 2 nodes A and B
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
        /// Calculate path by retracing from endNode to startNode
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void RetracePath(Node startNode, Node endNode, NativeArray<float3> path)
        {
            if (endNode == startNode)
            {
                DebugLogger.LogWarning("Target Already Reached");
                return;
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
        }

        /// <summary>
        /// Reverse Array
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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


        #region Utility Methods (Id to Pos and reversed, Get Node from World Pos)

        /// <summary>
        /// Get Node from nodes array from world position input
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Node GetNodeFromWorldPos(float3 worldPos)
        {
            // Offset by half the grid size if your grid is centered at (0,0,0)
            float3 localPos = worldPos - gridPosition + 0.5f * gridSize.x * MathLogic.Float3Right + 0.5f * gridSize.z * MathLogic.Float3Forward;

            // Divide by node size to get grid coordinates
            int x = (int)math.floor(localPos.x / nodeSize);
            int y = (int)math.floor(localPos.y / nodeSize);
            int z = (int)math.floor(localPos.z / nodeSize);

            // Clamp to grid bounds
            x = math.clamp(x, 0, gridLength.x - 1);
            y = math.clamp(y, 0, gridLength.y - 1);
            z = math.clamp(z, 0, gridLength.z - 1);

            // Flatten 3D coordinates to 1D index
            int gridId = x + y * gridLength.x + z * gridLength.x * gridLength.y;

            return nodes[gridId];
        }

        /// <summary>
        /// GridId to Node GridPos
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int3 GridIdToGridPos(int gridId)
        {
            int x = gridId % gridLength.x;
            int y = (gridId / gridLength.x) % gridLength.y;
            int z = gridId / (gridLength.x * gridLength.y);
            return new int3(x, y, z);
        }

        /// <summary>
        /// GridPos to Grid Id
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private int GridPosToGridId(int3 gridPos)
        {
            return gridPos.x + gridPos.y * gridLength.x + gridPos.z * gridLength.x * gridLength.y;
        }

        #endregion
    }
}