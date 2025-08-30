using UnityEngine;



public class AstarEnvironmentObject : MonoBehaviour
{
    [SerializeField] private AstarEnvironmentObjectSO data;

    public int LayerId => data.layerId;
    public int MovementPenalty => data.movementPenalty;
    public bool Walkable => data.walkable;
}