using UnityEngine;

[CreateAssetMenu(fileName = "NewAttackMove", menuName = "Game/Moves/Attack Move")]
public class AttackMove : MoveData
{
    [Header("Attack Settings")]
    [Tooltip("Flat attack points this move deals.")]
    public int attackPoints;
}
