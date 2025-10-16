using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime object representing a character in battle (player controlled).
/// Keeps current HP and temporary status values (buffs/defend).
/// </summary>
public class CharacterInstance
{
    public CharacterData data;
    public int currentHP;
    public bool isDefending = false;

    // Active percent buffs applied to this character (sum of all pending buffs)
    public int totalAttackBuffPercent = 0;
    public int totalDamageReductionPercent = 0;

    // Track which buffs were applied by which source so we can remove them when the source's next turn occurs.
    public List<ActiveBuff> activeBuffs = new List<ActiveBuff>();

    public CharacterInstance(CharacterData d)
    {
        data = d;
        currentHP = d.health;
    }

    public void ApplyHealPercent(int healPercent)
    {
        int amount = Mathf.CeilToInt((data.health * healPercent) / 100f);
        currentHP = Mathf.Min(currentHP + amount, data.health);
    }

    public void ApplyDamage(int amount)
    {
        if (isDefending)
        {
            // Defend blocks all incoming damage and then is cleared.
            isDefending = false;
            return;
        }

        // Apply damage reduction percent
        int reduced = Mathf.CeilToInt(amount * (1f - totalDamageReductionPercent / 100f));
        currentHP -= Mathf.Max(0, reduced);
    }

    public bool IsDead() => currentHP <= 0;
}

/// <summary>
/// A record of a buff applied to a target by a source.
/// We track buff type and values to remove correctly later.
/// </summary>
public class ActiveBuff
{
    public CharacterInstance source; // who applied it
    public BuffType type;
    public int value; // percent or flat depending on buff
    internal int remainingTurns;
    private CharacterInstance actor;
    private BuffType attackPercent;
    private int attackBuffPercent;

    public ActiveBuff(CharacterInstance actor, BuffType attackPercent, int attackBuffPercent)
    {
        this.actor = actor;
        this.attackPercent = attackPercent;
        this.attackBuffPercent = attackBuffPercent;
    }

    public ActiveBuff(CharacterInstance s, BuffType t, int v, int v1)
    {
        source = s; type = t; value = v;
    }
}

public enum BuffType
{
    AttackPercent,
    DamageReductionPercent
}
