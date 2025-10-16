using UnityEngine;

/// <summary>
/// Simple enemy container. For now it's a single dummy enemy with configurable HP and speed.
/// </summary>
[CreateAssetMenu(fileName = "NewEnemyDummy", menuName = "Game/EnemyDummy")]
public class EnemyDummy : ScriptableObject
{
    public string enemyName = "Dummy";
    public int maxHP = 100;
    public int speed = 100;
}
