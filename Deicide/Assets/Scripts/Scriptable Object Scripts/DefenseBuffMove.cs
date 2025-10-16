using UnityEngine;

[CreateAssetMenu(fileName = "NewDefenseBuffMove", menuName = "Game/Moves/Defense Buff Move")]
public class DefenseBuffMove : MoveData
{
    [Header("Defense Buff Settings")]
    [Tooltip("Percentage of damage reduction (e.g., 15 for 15% less damage).")]
    public int damageReductionPercent;
}
