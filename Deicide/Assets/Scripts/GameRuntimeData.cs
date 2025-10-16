using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Runtime holder for selected characters. DontDestroyOnLoad so selection survives scene load.
/// Use GameRuntimeData.Instance.SelectedCharacters to access the array (length up to 3).
/// </summary>
public class GameRuntimeData : MonoBehaviour
{
    public static GameRuntimeData Instance { get; private set; }

    // Selected characters (max 3). Fill from CharacterSelectionUI before loading combat.
    public CharacterData[] SelectedCharacters = new CharacterData[3];

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}
