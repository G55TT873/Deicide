using System.Collections.Generic;

/// <summary>
/// Runtime enemy instance container used by CombatManager.
/// Make sure this file is saved as "EnemyInstance.cs" and is NOT inside an Editor folder.
/// </summary>
public class EnemyInstance
{
    public EnemyDummy data;
    public int currentHP;
    public bool isDefending = false;
    public int totalDamageReductionPercent = 0;
    public int attackBuffPercent = 0;

    // Re-uses ActiveBuff defined in CharacterInstance.cs
    public List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    public EnemyInstance(EnemyDummy d)
    {
        data = d;
        currentHP = d != null ? d.maxHP : 0;
    }
}
