using FirePixel.PathFinding;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using UnityEngine;


public class GhostManager : MonoBehaviour
{
#pragma warning disable UDR0001
    public static GhostManager Instance;
#pragma warning restore UDR0001
    private void Awake() => Instance = this;


    [SerializeField] private GameObject ghostPrefab;
    [SerializeField] private Transform ghostSpawnPoint;
    [SerializeField] private int ghostCount = 4;

    [SerializeField] private float updatePathInterval = 1f;
    [SerializeField] private bool updatePathAsync = true;
    [SerializeField] private float updateGhostInterval = 0.1f;

    [SerializeField] private int maxPathLength = 15;

    [Header("Ghosts Cores > Data driven")]
    [SerializeField] private GhostData[] ghostData;
    [SerializeField] private GhostSenses[] ghostSenses;


    private GridManager gridManager;

    private NativeArray<NativeArray<int3>> tileDetectionOffsets;

    private NativeArray<NativeArray<Node>> nodesSetsArray;
    private NativeArray<NativeArray<Node>> openNodesSetsArray;
    private NativeArray<NativeHashSet<int>> closedNodeIdSetsArray;

    private AstarPathFindJob pathFindJob;
    private JobHandle pathFindJobHandle;

    private float timeSincePathUpdate;
    private float timeSinceGhostUpdate;


    private void OnEnable() => UpdateScheduler.RegisterUpdate(OnUpdate);
    private void OnDisable() => UpdateScheduler.UnregisterUpdate(OnUpdate);


    public void Init()
    {
        gridManager = GridManager.Instance;

        tileDetectionOffsets = new NativeArray<NativeArray<int3>>(ghostCount, Allocator.Persistent);

        nodesSetsArray = new NativeArray<NativeArray<Node>>(ghostCount, Allocator.Persistent);
        openNodesSetsArray = new NativeArray<NativeArray<Node>>(ghostCount, Allocator.Persistent);
        closedNodeIdSetsArray = new NativeArray<NativeHashSet<int>>(ghostCount, Allocator.Persistent);

        for (int i = 0; i < ghostCount; i++)
        {
            GameObject ghost = Instantiate(ghostPrefab, ghostSpawnPoint.position, Quaternion.identity);

            ghostData[i].Init(maxPathLength + 1, ghost.transform);

            ghostSenses[i].CalculateTileDetectionOffsets(out NativeArray<int3> tempTileDetectionOffsets);
            tileDetectionOffsets[i] = tempTileDetectionOffsets;

            int arrayCapacity = gridManager.TotalGridLength;

            nodesSetsArray[i] = new NativeArray<Node>(arrayCapacity, Allocator.Persistent);
            nodesSetsArray[i].CopyFrom(gridManager.GetNodeGrid());

            openNodesSetsArray[i] = new NativeArray<Node>(arrayCapacity, Allocator.Persistent);
            closedNodeIdSetsArray[i] = new NativeHashSet<int>(arrayCapacity, Allocator.Persistent);
        }

        gridManager.GetNeighbourData(out NativeArray<int3> neighbourOffsets, out NativeArray<Node> neighbourStorage);

        pathFindJob = new AstarPathFindJob()
        {
            allowDiagonalMovement = false,
            gridLength = gridManager.GridLength,

            neighbourOffsets = neighbourOffsets,
            neighbours = neighbourStorage,

            gridSize = gridManager.GridSize,
            gridPosition = gridManager.GridPosition,
            nodeSize = gridManager.NodeSize,
            maxPathLength = maxPathLength,
            
        };
    }

    private void OnUpdate()
    {
        timeSinceGhostUpdate += Time.deltaTime;
        if (timeSinceGhostUpdate >= updateGhostInterval)
        {
            UpdateGhosts(timeSinceGhostUpdate);
            timeSinceGhostUpdate = 0f;
        }

        timeSincePathUpdate += Time.deltaTime;
        if (timeSincePathUpdate >= updatePathInterval && (pathFindJobHandle.IsCompleted || updatePathAsync == false))
        {
            pathFindJobHandle.Complete();
            
            UpdatePath();

            timeSincePathUpdate = 0f;
        }
    }


    /// <summary>
    /// Called once per <see cref="updatePathInterval"/> to update the path"/>
    /// </summary>
    private void UpdatePath()
    {
        NativeArray<JobHandle> jobHandles = new NativeArray<JobHandle>(ghostCount, Allocator.Temp);

        for (int i = 0; i < ghostCount; i++)
        {
            ghostData[i].SetNewPath();

            pathFindJob.SetupPathfinderData(nodesSetsArray[i], openNodesSetsArray[i], closedNodeIdSetsArray[i], ghostData[i].currentPathPosition, PacmanController.Instance.transform.position, ghostData[i].nextPath);

            jobHandles[i] = pathFindJob.Schedule();
        }

        pathFindJobHandle = JobHandle.CombineDependencies(jobHandles);
        jobHandles.Dispose();
    }
    
    private void UpdateGhosts(float deltaTime)
    {
        for (int i = 0; i < ghostCount; i++)
        {
            ghostData[i].UpdateGhost(deltaTime);
        }
    }


    private void OnDestroy()
    {
        // Complete any running jobs first
        if (pathFindJobHandle.IsCompleted == false)
            pathFindJobHandle.Complete();

        // Dispose all GhostData NativeArrays
        for (int i = 0; i < ghostCount; i++)
        {
            ghostData[i].DisposeIfCreated();

            tileDetectionOffsets[i].DisposeIfCreated();
            nodesSetsArray[i].DisposeIfCreated();

            openNodesSetsArray[i].DisposeIfCreated();

            closedNodeIdSetsArray[i].DisposeIfCreated();
        }

        tileDetectionOffsets.DisposeIfCreated();
        nodesSetsArray.DisposeIfCreated();
        openNodesSetsArray.DisposeIfCreated();
        closedNodeIdSetsArray.DisposeIfCreated();
    }
}
