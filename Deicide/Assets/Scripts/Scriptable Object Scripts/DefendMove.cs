using UnityEngine;

[CreateAssetMenu(fileName = "NewDefendMove", menuName = "Game/Moves/Defend Move")]
public class DefendMove : MoveData
{
    [Header("Defend Settings")]
    [Tooltip("A defensive stance move that reduces or blocks incoming damage.")]
    [TextArea]
    public string description = "Takes a defensive stance to reduce incoming damage.";
}
