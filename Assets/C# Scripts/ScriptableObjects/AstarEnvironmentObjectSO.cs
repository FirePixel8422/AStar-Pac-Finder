using UnityEngine;


[CreateAssetMenu(fileName = "AstarObstacleData", menuName = "ScriptableObjects/AstarObstacleDataSO", order = 1)]
public class AstarEnvironmentObjectSO : ScriptableObject
{
    public int layerId = 0;
    public int movementPenalty = 0;
    public bool walkable = true;
}