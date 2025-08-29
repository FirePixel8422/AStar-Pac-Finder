using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "AstarObstacleData", menuName = "ScriptableObjects/AstarObstacleDataSO", order = 1)]
public class AstarObstacleDataSO : ScriptableObject
{
    public int movementPenalty = 0;
}
