using Unity.Burst;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;


[System.Serializable]
public struct GhostData
{
    [SerializeField] private GhostState state;

    [Header("The more aggressive a ghost is, the faster it starts chasing pacman")]
    [Range(0, 1)]
    [SerializeField] private float aggressiveness;


    #region Teamwork and smart stuff 

    [Header("If true, talks and listens to other ghosts about pacman's location")]
    [SerializeField] private bool workWithTeammates;

    [Header("If true, try to corner/trap pacman -REQUIRES work with teammates")]
    [SerializeField] private bool tryTrapping;

    #endregion


    [Tooltip("Percentage from 0 to 1 that makes the ghost go into wandering state if it becomes 1")]
    [Range(0, 1)]
    [SerializeField] private float cAggressivenessLevel;

    [SerializeField] private float3 lastKnownPacmanLocation;

    public NativeArray<float3> currentPath;
    public int cPathId;
    public NativeArray<float3> nextPath;

    [SerializeField] private Transform ghostTransform;

    public float3 nextPathPosition;
    public float3 currentPathPosition;


    #region States (IsDead, IsWandering, IsSearching, IsScared, IsReturningToSpawn)

    public bool IsDead => state == GhostState.Dead;
    public bool IsWandering => state == GhostState.Wandering;
    public bool IsChasing => state == GhostState.Chasing;
    public bool IsScared => state == GhostState.Scared;
    public bool IsReturningToSpawn => state == GhostState.ReturningToSpawn;

    #endregion


    /// <summary>
    /// Called once per ghost
    /// </summary>
    public void UpdateGhost(float deltaTime)
    {
        if (cPathId == currentPath.Length) return;

        do
        {
            ghostTransform.position = MoveConsumeDelta(ghostTransform.position, currentPath[cPathId], ref deltaTime, out bool targetReached);

            if (targetReached && cPathId < currentPath.Length - 1)
            {
                currentPathPosition = currentPath[cPathId];
                cPathId++;

                if (cPathId != currentPath.Length)
                {
                    nextPathPosition = currentPath[cPathId];
                }
                else
                {
                    // Path is finished
                    break;
                }
            }
        }
        while(deltaTime > 0);
    }

    private float3 MoveConsumeDelta(float3 a, float3 b, ref float deltaTime, out bool targetReached)
    {
        targetReached = false;

        float3 delta = b - a;
        float dist = math.length(delta);

        if (dist < 1e-5f) // already at B
        {
            targetReached = true;
            return b;
        }

        if (deltaTime >= dist)
        {
            // Can fully reach B
            deltaTime -= dist;
            return b;
        }
        else
        {
            // Move partially
            float3 step = delta / dist * deltaTime;
            deltaTime = 0f;
            return a + step;
        }
    }


    /// <summary>
    /// Update the ghost's knowledge of pacman's location when it spots him and update its reaction accordingly
    /// </summary>
    public void OnPacManSpotted(float3 pacManLocation)
    {
        lastKnownPacmanLocation = pacManLocation;

        if (IsScared || IsReturningToSpawn) return;

        // Increase aggressiveness level by "aggressiveness"
        cAggressivenessLevel = math.saturate(cAggressivenessLevel + aggressiveness);

        // If aggressiveness level is maxed out, start chasing pacman
        if (cAggressivenessLevel == 1)
        {
            state = GhostState.Chasing;
        }
    }
}