using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    [Header("Basic Info")]
    [Tooltip("The character's display name.")]
    public string characterName;

    [Tooltip("A short biography or description of the character.")]
    [TextArea]
    public string characterBio;

    [Header("Stats")]
    [Tooltip("The character's attack power.")]
    public int attack;

    [Tooltip("The character's total health.")]
    public int health;

    [Tooltip("The character's movement or action speed.")]
    public int speed;

    [Header("Category")]
    [Tooltip("The character's category or class type.")]
    public CharacterCategory category;

    [Header("Moves")]
    [Tooltip("Exactly four moves this character can use.")]
    public MoveData[] moves = new MoveData[4];
}

public enum CharacterCategory
{
    Warrior,
    Mage,
    Healer,
    Tank,
    Assassin,
    Support,
    Ranger
}
