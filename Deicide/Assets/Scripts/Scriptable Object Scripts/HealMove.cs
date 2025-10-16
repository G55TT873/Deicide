using UnityEngine;

[CreateAssetMenu(fileName = "NewHealMove", menuName = "Game/Moves/Heal Move")]
public class HealMove : MoveData
{
    [Header("Heal Settings")]
    [Tooltip("Percentage of health restored (e.g., 25 for 25% of max HP).")]
    public int healPercent;
}
