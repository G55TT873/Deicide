using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackBuffMove", menuName = "Game/Moves/Attack Buff Move")]
public class AttackBuffMove : MoveData
{
    [Header("Attack Buff Settings")]
    [Tooltip("Percentage increase in attack power (e.g., 20 for +20%).")]
    public int attackBuffPercent;
}
