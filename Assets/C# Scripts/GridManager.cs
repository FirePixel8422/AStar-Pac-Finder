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
        public float3 GridSize => gridSize;

        [SerializeField] private float3 gridPosition;
        public float3 GridPosition => gridPosition;

        [Range(0.1f, 5)]
        [SerializeField] private float nodeSize;
        public float NodeSize => nodeSize;

        [SerializeField] private int3[] pathfinderNeighbourOffsets;


        private NativeArray<Node> nodes;
        public ref NativeArray<Node> GetNodeGrid() => ref nodes;


        private NativeArray<int3> neighbourOffsets;
        private NativeArray<Node> neighboursStorage;
        public void GetNeighbourData(out NativeArray<int3> neighbourOffsets, out NativeArray<Node> neighboursStorage)
        {
            neighbourOffsets = this.neighbourOffsets;
            neighboursStorage = this.neighboursStorage;
        }

        public int3 GridLength { get; private set; }
        public int TotalGridLength { get; private set; }


        private void Start()
        {
            CreateGrid();

            neighbourOffsets = new NativeArray<int3>(pathfinderNeighbourOffsets, Allocator.Persistent);
            neighboursStorage = new NativeArray<Node>(neighbourOffsets.Length, Allocator.Persistent);

            GhostManager.Instance.Init();
        }

        public void CreateGrid()
        {
            GridLength = new int3(
                Mathf.RoundToInt(gridSize.x / nodeSize),
                Mathf.RoundToInt(gridSize.y / nodeSize),
                Mathf.RoundToInt(gridSize.z / nodeSize));

            TotalGridLength = GridLength.x * GridLength.y * GridLength.z;

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

        private int3 GridIdToGridPos(int gridId)
        {
            int x = gridId % GridLength.x;
            int y = (gridId / GridLength.x) % GridLength.y;
            int z = gridId / (GridLength.x * GridLength.y);
            return new int3(x, y, z);
        }
        private int GridPosToGridId(int3 gridPos)
        {
            return gridPos.x + gridPos.y * GridLength.x + gridPos.z * GridLength.x * GridLength.y;
        }


        private void OnDestroy()
        {
            if (nodes.IsCreated)
                nodes.Dispose();

            if (neighbourOffsets.IsCreated)
                neighbourOffsets.Dispose();

            if (neighboursStorage.IsCreated)
                neighboursStorage.Dispose();
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