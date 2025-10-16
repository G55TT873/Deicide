using UnityEngine;

public abstract class MoveData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("Name of the move.")]
    public string moveName;

    [Tooltip("Number of targets affected by the move.")]
    public int numberOfTargets;
}
