using UnityEngine;


public class PacmanController : MonoBehaviour
{
#pragma warning disable UDR0001
    public static PacmanController Instance;
#pragma warning restore UDR0001
    private void Awake() => Instance = this;
}