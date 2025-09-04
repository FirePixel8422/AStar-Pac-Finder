using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;



[System.Serializable]
public struct GhostSenses
{
    [Header("Line in front of the ghost that it detects pacman in")]
    [SerializeField] private int viewDistance;

    [Header("Square around the ghost that ghost detects pac man in")]
    [SerializeField] private int detectRadius;

    [Header("If true, ghost can sense pacman through walls")]
    [SerializeField] private bool xrayVision;

    [Header("If true, ghost will ALWAYS know pacman's location")]
    [SerializeField] private bool alwaysKnowPacmanLocation;


    public void CalculateTileDetectionOffsets(out NativeArray<int3> tileDetectionOffsets)
    {
        // Capacity is detectRadius Srd (square box around ghost) + viewDistance - detectRadius (line in front of ghost). if detectRadius is 2, 2 tiles of the viewDistance re already accounted for
        int outerViewTileCount = math.clamp(viewDistance - detectRadius, 0, int.MaxValue);
        int capacity = detectRadius * detectRadius + outerViewTileCount;

        tileDetectionOffsets = new NativeArray<int3>(capacity, Allocator.Persistent);


        for (int x = 0; x < detectRadius; x++)
        {
            for (int z = 0; z < detectRadius; z++)
            {
                int3 tileDirectionOffset = new int3(x, 0, z);

                tileDetectionOffsets[x * detectRadius + z] = tileDirectionOffset;
            }
        }

        int startIndex = detectRadius * detectRadius;
        for (int i = 0; i < outerViewTileCount; i++)
        {
            int3 tileDirectionOffset = new int3(0, 0, detectRadius + i);
            tileDetectionOffsets[startIndex + i] = tileDirectionOffset;
        }
    }
}