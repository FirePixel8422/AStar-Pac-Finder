using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


namespace FirePixel.PathFinding
{
    public class GridManager : MonoBehaviour
    {
#pragma warning disable UDR0001
        public static GridManager Instance;
#pragma warning restore UDR0001
        private void Awake()
        {
            Instance = this;
        }



        [SerializeField] private LayerMask gridUsedLayerMask;
        [SerializeField] private AstarEnvironmentObjectSO defaultNodeData;

        [SerializeField] private float3 gridSize;
        [SerializeField] private float3 gridPosition;

        [Range(0.1f, 5)]
        [SerializeField] private float nodeSize;

        private NativeArray<Node> nodes;

        private int gridLengthX, gridLengthY, gridLengthZ;
        public int TotalGridLength { get; private set; }


        private void Start()
        {
            CreateGrid();
        }
        public void CreateGrid()
        {
            gridLengthX = Mathf.RoundToInt(gridSize.x / nodeSize);
            gridLengthY = Mathf.RoundToInt(gridSize.y / nodeSize);
            gridLengthZ = Mathf.RoundToInt(gridSize.z / nodeSize);

            TotalGridLength = gridLengthX * gridLengthY * gridLengthZ;

            nodes = new NativeArray<Node>(TotalGridLength, Allocator.Persistent);

            float3 worldBottomLeft = gridPosition - 0.5f * gridSize.x * MathLogic.Float3Right - 0.5f * gridSize.z * MathLogic.Float3Forward;

            for (int gridId = 0; gridId < TotalGridLength; gridId++)
            {
                int3 gridPos = GridIdToGridPos(gridId);
                float3 worldPos = worldBottomLeft
                    + MathLogic.Float3Right * (gridPos.x * nodeSize + nodeSize * 0.5f)
                    + MathLogic.Float3Up * (gridPos.y * nodeSize + nodeSize * 0.5f)
                    + MathLogic.Float3Forward * (gridPos.z * nodeSize + nodeSize * 0.5f);

                SphereCollider[] spheres = new SphereCollider[1];

                // On hit get movement penalty from layer
                if (Physics.OverlapSphereNonAlloc(worldPos, nodeSize * 0.1f, spheres, gridUsedLayerMask) != 0 && spheres[0].TryGetComponent(out AstarEnvironmentObject envObj))
                {
                    nodes[gridId] = new Node(gridId, envObj.Walkable, envObj.MovementPenalty, envObj.LayerId, worldPos);
                }
                // if no hit, use default envObjData assigned in
                else
                {
                    nodes[gridId] = new Node(gridId, defaultNodeData.walkable, defaultNodeData.movementPenalty, defaultNodeData.layerId, worldPos);
                }
            }
        }
        public Node NodeFromWorldPoint(float3 worldPosition)
        {
            // Get percent along each axis
            float percentX = (worldPosition.x + gridSize.x / 2f) / gridSize.x;
            float percentY = worldPosition.y / gridSize.y;
            float percentZ = (worldPosition.z + gridSize.z / 2f) / gridSize.z;

            percentX = math.clamp(percentX, 0f, 1f);
            percentY = math.clamp(percentY, 0f, 1f);
            percentZ = math.clamp(percentZ, 0f, 1f);

            // Convert to integer grid coordinates
            int x = (int)math.round((gridLengthX - 1) * percentX);
            int y = (int)math.round((gridLengthY - 1) * percentY);
            int z = (int)math.round((gridLengthZ - 1) * percentZ);

            // Convert 3D coords to linear gridId
            int gridId = x + y * gridLengthX + z * gridLengthX * gridLengthY;

            return nodes[gridId];
        }

        public Node NodeFromGridId(int gridId)
        {
            return nodes[gridId];
        }


        private int3 GridIdToGridPos(int gridId)
        {
            int x = gridId % gridLengthX;
            int y = (gridId / gridLengthX) % gridLengthY;
            int z = gridId / (gridLengthX * gridLengthY);
            return new int3(x, y, z);
        }
        private int GridPosToGridId(int3 gridPos)
        {
            return gridPos.x + gridPos.y * gridLengthX + gridPos.z * gridLengthX * gridLengthY;
        }



#if UNITY_EDITOR

        [Header("DEBUG")]
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
                    for (int gridId = 0; gridId < TotalGridLength; gridId++)
                    {
                        Node node = nodes[gridId];

                        if (node.layerId >= nodeLayerColors.Length)
                        {
                            DebugLogger.LogError($"No color set for layer {node.layerId} but grid is using it. Please add a color to the GridManager script in the inspector.");
                            return;
                        }
                        Gizmos.color = nodeLayerColors[node.layerId];

                        // Draw cube
                        Gizmos.DrawCube(node.worldPos, Vector3.one * nodeSize * 0.9f);
                    }
                }
            }

            Gizmos.color = Color.white;
            Gizmos.DrawWireCube(gridPosition + MathLogic.Float3Up * gridSize.y * 0.5f, new Vector3(gridSize.x, gridSize.y, gridSize.z));
        }

#endif
    }
}