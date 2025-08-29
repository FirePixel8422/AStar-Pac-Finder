using System.Collections;
using UnityEngine;



public class AStarObstacle : MonoBehaviour
{
    [SerializeField] private AstarObstacleDataSO data;
    public int MovementPenalty => data.movementPenalty;
}