using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// Attach to a GameObject in Selection Scene.
/// Assign 'availableCharacters' with CharacterData assets you want selectable.
/// Assign 'contentParent' to a UI panel/content where buttons will be spawned.
/// Assign 'buttonPrefab' to a simple UI Button prefab (expects a Text child for label).
/// Assign 'startBattleButton' to start the battle (will be enabled when >0 selection).
/// Scene to load for combat should be set to "CombatScene" or change string accordingly.
/// </summary>
public class CharacterSelectionUI : MonoBehaviour
{
    [Header("Available Characters")]
    public List<CharacterData> availableCharacters = new List<CharacterData>();

    [Header("UI References")]
    public Transform contentParent;
    public GameObject buttonPrefab;
    public Button startBattleButton;
    public Text selectionInfoText;

    private List<bool> selectedFlags;
    private const int MaxSelection = 3;

    private void Start()
    {
        if (availableCharacters == null) availableCharacters = new List<CharacterData>();
        selectedFlags = new List<bool>(new bool[availableCharacters.Count]);
        PopulateList();
        UpdateStartButton();
    }

    private void PopulateList()
    {
        foreach (Transform t in contentParent) Destroy(t.gameObject);

        for (int i = 0; i < availableCharacters.Count; i++)
        {
            var charData = availableCharacters[i];
            var go = Instantiate(buttonPrefab, contentParent);
            var btn = go.GetComponent<Button>();
            var label = go.GetComponentInChildren<Text>();
            label.text = charData.characterName;
            int idx = i;
            btn.onClick.AddListener(() => ToggleSelection(idx, go));
            UpdateButtonVisual(go, false);
        }
    }

    private void ToggleSelection(int idx, GameObject go)
    {
        int currentSelected = CountSelected();

        if (selectedFlags[idx])
        {
            selectedFlags[idx] = false;
            UpdateButtonVisual(go, false);
        }
        else
        {
            if (currentSelected >= MaxSelection) return; // do nothing when full
            selectedFlags[idx] = true;
            UpdateButtonVisual(go, true);
        }

        UpdateStartButton();
    }

    private int CountSelected()
    {
        int c = 0;
        foreach (var f in selectedFlags) if (f) c++;
        return c;
    }

    private void UpdateButtonVisual(GameObject go, bool selected)
    {
        // simple visual: change alpha of the image
        var img = go.GetComponent<Image>();
        if (img) img.color = selected ? new Color(0.6f, 1f, 0.6f, 1f) : Color.white;
    }

    private void UpdateStartButton()
    {
        int count = CountSelected();
        startBattleButton.interactable = count > 0; // allow 1-3
        if (selectionInfoText) selectionInfoText.text = $"Selected: {count}/{MaxSelection}";
    }

    public void OnStartBattlePressed()
    {
        // Fill runtime selection (first chosen fill slots; empty slots stay null)
        var runtime = FindOrCreateRuntime();
        int slot = 0;
        for (int i = 0; i < availableCharacters.Count && slot < 3; i++)
        {
            if (selectedFlags[i])
            {
                runtime.SelectedCharacters[slot] = availableCharacters[i];
                slot++;
            }
        }
        // Clear remaining slots
        for (int s = slot; s < 3; s++) runtime.SelectedCharacters[s] = null;

        // Load combat scene (make sure CombatScene is added to build settings OR set name accordingly)
        SceneManager.LoadScene("CombatScene");
    }

    private GameRuntimeData FindOrCreateRuntime()
    {
        var existing = FindObjectOfType<GameRuntimeData>();
        if (existing) return existing;

        var go = new GameObject("GameRuntimeData");
        return go.AddComponent<GameRuntimeData>();
    }
}
