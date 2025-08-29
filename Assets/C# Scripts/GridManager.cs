using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


namespace FirePixel.PathFinding
{
    [BurstCompile]
    public class GridManager : MonoBehaviour
    {
#pragma warning disable UDR0001
        public static GridManager Instance;
#pragma warning restore UDR0001
        private void Awake()
        {
            Instance = this;
        }




        public NativeArray<Node> nodes;

        [SerializeField] private LayerMask gridUsedLayer;

        [SerializeField] private float3 gridSize;
        [SerializeField] private float3 gridPosition;

        [Range(0.1f, 5)]
        public float nodeSize;

        private int gridSizeX, gridSizeY, gridSizeZ;
        public int totalGridSize;


        private void Start()
        {
            CreateGrid();
        }
        public void CreateGrid()
        {
            gridSizeX = Mathf.RoundToInt(gridSize.x / nodeSize);
            gridSizeY = Mathf.RoundToInt(gridSize.y / nodeSize);
            gridSizeZ = Mathf.RoundToInt(gridSize.z / nodeSize);

            totalGridSize = gridSizeX * gridSizeY * gridSizeZ;

            nodes = new NativeArray<Node>(totalGridSize, Allocator.Persistent);

            float3 worldBottomLeft = gridPosition - 0.5f * gridSize.x * MathLogic.Float3Right - 0.5f * gridSize.z * MathLogic.Float3Forward;

            SphereCollider[] sphereColliders = new SphereCollider[1];

            for (int gridId = 0; gridId < totalGridSize; gridId++)
            {
                int3 gridPos = GridPosToGridId(gridId, gridSizeX, gridSizeY);
                float3 worldPos = worldBottomLeft + MathLogic.Float3Right * (gridPos.x * nodeSize + nodeSize * 0.5f) + MathLogic.Float3Forward * (gridPos.z * nodeSize + nodeSize * 0.5f);

                // On hit (detected obstacle) > not walkable
                if (Physics.OverlapSphereNonAlloc(worldPos, nodeSize * 0.75f, sphereColliders, gridUsedLayer) != 0 && sphereColliders[0].GetComponent<SurfaceTypeIdentifier>)
                {
                    nodes[gridId] = new Node(gridId, false, 0);
                }
                else
                {
                    nodes[gridId] = new Node(gridId, true, 0);
                }
            }
        }
        public Node NodeFromWorldPoint(float3 worldPosition)
        {
            // Get percent along each axis
            float percentX = (worldPosition.x + gridSize.x / 2f) / gridSize.x;
            float percentY = worldPosition.y / gridSize.y;               // Y goes 0 -> gridSize.y
            float percentZ = (worldPosition.z + gridSize.z / 2f) / gridSize.z;

            percentX = math.clamp(percentX, 0f, 1f);
            percentY = math.clamp(percentY, 0f, 1f);
            percentZ = math.clamp(percentZ, 0f, 1f);

            // Convert to integer grid coordinates
            int x = (int)math.round((gridSizeX - 1) * percentX);
            int y = (int)math.round((gridSizeY - 1) * percentY);
            int z = (int)math.round((gridSizeZ - 1) * percentZ);

            // Convert 3D coords to linear gridId
            int gridId = x + y * gridSizeX + z * gridSizeX * gridSizeY;

            return nodes[gridId];
        }

        public Node NodeFromId(int gridId)
        {
            return nodes[gridId];
        }


        private int3 GridPosToGridId(int gridId, int gridSizeX, int gridSizeY)
        {
            int x = gridId % gridSizeX;
            int y = (gridId / gridSizeX) % gridSizeY;
            int z = gridId / (gridSizeX * gridSizeY);
            return new int3(x, y, z);
        }
        private int GridIdToGridPos(int3 gridPos, int gridSizeX, int gridSizeY)
        {
            return gridPos.x + gridPos.y * gridSizeX + gridPos.z * gridSizeX * gridSizeY;
        }



#if UNITY_EDITOR

        [SerializeField] private bool drawNodeColorGizmos = false;
        [SerializeField] private Color[] nodeLayerColors;

        [Header("REALLY EXPENSIVE")]
        [SerializeField] private bool recalculateGridEveryFrame;

        private void OnDrawGizmos()
        {
            if (Application.isPlaying)
            {
                if (recalculateGridEveryFrame)
                {
                    CreateGrid();
                }

                if (drawNodeColorGizmos == true)
                {
                    float3 nodeToCenterOffset = new float3((gridSize.x - nodeSize) * 0.5f, 0 - nodeSize * 0.5f, (gridSize.z - nodeSize) * 0.5f);

                    for (int gridId = 0; gridId < totalGridSize; gridId++)
                    {
                        Node node = nodes[gridId];

                        Gizmos.color = nodeLayerColors[0];
                        if (node.walkable == 0)
                        {
                            Gizmos.color = nodeLayerColors[1];
                        }

                        // Draw cube
                        Gizmos.DrawCube(node.GetWorldPos(gridSizeX, gridSizeY, nodeSize, nodeToCenterOffset), Vector3.one * nodeSize * 0.9f);
                    }
                }
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(gridPosition + MathLogic.Float3Up * gridSize.y * 0.5f, new Vector3(gridSize.x, gridSize.y, gridSize.z));
        }

#endif
    }
}