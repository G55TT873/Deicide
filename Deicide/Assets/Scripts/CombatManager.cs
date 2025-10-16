using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CombatManager : MonoBehaviour
{
    [Header("Runtime")]
    private Queue<string> logHistory = new Queue<string>();
    private const int maxLogLines = 3;

    public EnemyDummy enemyAsset;

    [Header("UI")]
    public Text turnOrderText;
    public Transform movesPanel;
    public GameObject moveButtonPrefab; 
    public Transform targetsPanel;
    public GameObject targetButtonPrefab;
    public Text enemyInfoText;
    public Text logText;

    // runtime lists
    private List<CharacterInstance> playerInstances = new List<CharacterInstance>();
    private EnemyInstance enemyInstance;
    private List<TurnEntity> turnEntities = new List<TurnEntity>();
    private int currentTurnIndex = 0;

    // input control
    private bool waitingForPlayerInput = false;
    private CharacterInstance currentPlayer = null;

    // move/target selection
    private MoveData pendingMove = null;
    private int pendingTargetsNeeded = 0;
    private List<TargetSelection> pendingSelectedTargets = new List<TargetSelection>();

    private void Start()
    {
        SetupFromRuntimeSelection();
        SetupEnemy();
        BuildTurnOrder();
        RenderTurnOrder();
        StartCoroutine(ProcessTurns());
    }

    private void SetupFromRuntimeSelection()
    {
        var runtime = GameRuntimeData.Instance;
        if (runtime == null)
        {
            Debug.LogError("No GameRuntimeData found. Did you launch CombatScene directly?");
            return;
        }

        for (int i = 0; i < runtime.SelectedCharacters.Length; i++)
        {
            var c = runtime.SelectedCharacters[i];
            if (c != null)
                playerInstances.Add(new CharacterInstance(c));
        }
    }

    private void SetupEnemy()
    {
        enemyInstance = new EnemyInstance(enemyAsset);
        UpdateEnemyUI();
    }

    private void BuildTurnOrder()
    {
        turnEntities.Clear();

        // Add enemy
        turnEntities.Add(new TurnEntity() { isEnemy = true, enemy = enemyInstance, speed = enemyAsset.speed });

        // Add players
        foreach (var p in playerInstances)
            turnEntities.Add(new TurnEntity() { isEnemy = false, character = p, speed = p.data.speed });

        // Sort by speed descending
        turnEntities = turnEntities.OrderByDescending(t => t.speed).ToList();
    }

    private void RenderTurnOrder()
    {
        if (turnOrderText == null) return;
        turnOrderText.text = "Turn Order:\n";
        foreach (var t in turnEntities)
        {
            if (t.isEnemy) turnOrderText.text += $"{t.enemy.data.enemyName} (E) - Speed {t.speed}\n";
            else turnOrderText.text += $"{t.character.data.characterName} - Speed {t.speed}\n";
        }
    }

    private IEnumerator ProcessTurns()
    {
        while (true)
        {
            if (CheckBattleEnd()) yield break;

            var entity = turnEntities[currentTurnIndex];

            // remove buffs applied by this character last round
            if (!entity.isEnemy)
                RemoveBuffsAppliedBy(entity.character);

            if (entity.isEnemy)
            {
                yield return StartCoroutine(EnemyTurn());
            }
            else
            {
                yield return StartCoroutine(PlayerTurn(entity.character));
            }

            // advance to next turn
            currentTurnIndex = (currentTurnIndex + 1) % turnEntities.Count;
            yield return null;
        }
    }

    private bool CheckBattleEnd()
    {
        if (enemyInstance.currentHP <= 0)
        {
            AppendLog("Enemy defeated! Victory!");
            return true;
        }

        if (playerInstances.All(p => p.IsDead()))
        {
            AppendLog("All players defeated! Game Over.");
            return true;
        }

        return false;
    }

    private IEnumerator EnemyTurn()
    {
        AppendLog($"Enemy {enemyInstance.data.enemyName} takes its turn.");

        var target = playerInstances.FirstOrDefault(p => !p.IsDead());
        if (target != null)
        {
            int dmg = 10;
            dmg = Mathf.CeilToInt(dmg * (1f + enemyInstance.attackBuffPercent / 100f));
            AppendLog($"Enemy attacks {target.data.characterName} for {dmg} damage.");
            target.ApplyDamage(dmg);
            UpdateEnemyUI();
            UpdatePlayersUI();
        }

        yield return new WaitForSeconds(0.75f);
    }

    private IEnumerator PlayerTurn(CharacterInstance player)
    {
        if (player.IsDead())
        {
            AppendLog($"{player.data.characterName} is dead; skipping turn.");
            yield break;
        }

        AppendLog($"It's {player.data.characterName}'s turn.");

        waitingForPlayerInput = true;
        currentPlayer = player;

        ShowMovesForPlayer(player);

        // Wait for player to select and resolve their move
        yield return new WaitUntil(() => waitingForPlayerInput == false);

        yield return new WaitForSeconds(0.25f);
    }

    private void ShowMovesForPlayer(CharacterInstance player)
    {
        ClearPanel(movesPanel);
        pendingMove = null;
        pendingTargetsNeeded = 0;
        pendingSelectedTargets.Clear();

        for (int i = 0; i < player.data.moves.Length; i++)
        {
            var move = player.data.moves[i];
            if (move == null) continue;

            var go = Instantiate(moveButtonPrefab, movesPanel);
            var btn = go.GetComponent<Button>();
            var label = go.GetComponentInChildren<Text>();
            label.text = move.moveName;

            int idx = i;
            btn.onClick.AddListener(() => OnMoveButtonPressed(player, player.data.moves[idx]));
        }
    }

    private void OnMoveButtonPressed(CharacterInstance actor, MoveData move)
    {
        pendingMove = move;
        pendingSelectedTargets.Clear();
        ClearPanel(targetsPanel);

        switch (move)
        {
            case AttackBuffMove ab:
                pendingTargetsNeeded = ab.numberOfTargets;
                ShowTargetsForAllies(actor, ab.numberOfTargets);
                break;

            case DefenseBuffMove db:
                pendingTargetsNeeded = db.numberOfTargets;
                ShowTargetsForAllies(actor, db.numberOfTargets);
                break;

            case HealMove hm:
                pendingTargetsNeeded = hm.numberOfTargets;
                ShowTargetsForAllies(actor, hm.numberOfTargets);
                break;

            case AttackMove am:
                pendingTargetsNeeded = am.numberOfTargets;
                ShowTargetsForEnemies(am.numberOfTargets, actor);
                break;

            case DefendMove dm:
                actor.isDefending = true;
                AppendLog($"{actor.data.characterName} used Defend and will block incoming damage until next turn.");
                ClearPanel(movesPanel);
                EndPlayerInput(); // <— ends turn immediately
                UpdatePlayersUI();
                break;

            default:
                Debug.LogWarning("Unknown move type selected.");
                break;
        }
    }

    #region Target Selection
    private void ShowTargetsForAllies(CharacterInstance actor, int needed)
    {
        ClearPanel(targetsPanel);
        foreach (var p in playerInstances)
        {
            var go = Instantiate(targetButtonPrefab, targetsPanel);
            var btn = go.GetComponent<Button>();
            var label = go.GetComponentInChildren<Text>();
            label.text = $"{p.data.characterName} (HP: {p.currentHP}/{p.data.health})";
            btn.onClick.AddListener(() => OnAllyTargetClicked(actor, p, needed, go));
        }
    }

    private void ShowTargetsForEnemies(int needed, CharacterInstance actor)
    {
        ClearPanel(targetsPanel);
        var go = Instantiate(targetButtonPrefab, targetsPanel);
        var btn = go.GetComponent<Button>();
        var label = go.GetComponentInChildren<Text>();
        label.text = $"{enemyInstance.data.enemyName} (HP: {enemyInstance.currentHP}/{enemyInstance.data.maxHP})";
        btn.onClick.AddListener(() => OnEnemyTargetClicked(actor, enemyInstance, go));
    }

    private void OnAllyTargetClicked(CharacterInstance actor, CharacterInstance target, int needed, GameObject buttonGO)
    {
        if (pendingSelectedTargets.Any(t => !t.isEnemy && t.character == target)) return;

        pendingSelectedTargets.Add(new TargetSelection() { isEnemy = false, character = target });
        MarkButtonSelectedVisual(buttonGO);

        if (pendingSelectedTargets.Count >= needed)
            StartCoroutine(ApplyPendingMove(actor));
    }

    private void OnEnemyTargetClicked(CharacterInstance actor, EnemyInstance target, GameObject buttonGO)
    {
        if (pendingSelectedTargets.Any(t => t.isEnemy && t.enemy == target)) return;

        pendingSelectedTargets.Add(new TargetSelection() { isEnemy = true, enemy = target });
        MarkButtonSelectedVisual(buttonGO);

        if (pendingSelectedTargets.Count >= pendingTargetsNeeded)
            StartCoroutine(ApplyPendingMove(actor));
    }

    private void MarkButtonSelectedVisual(GameObject go)
    {
        var img = go.GetComponent<Image>();
        if (img) img.color = new Color(0.6f, 1f, 0.6f, 1f);
    }
    #endregion

    private IEnumerator ApplyPendingMove(CharacterInstance actor)
    {
        if (pendingMove == null) yield break;

        AppendLog($"{actor.data.characterName} used {pendingMove.moveName}.");

        if (pendingMove is AttackBuffMove ab)
        {
            foreach (var sel in pendingSelectedTargets)
            {
                if (sel.isEnemy) continue;
                var target = sel.character;
                target.totalAttackBuffPercent += ab.attackBuffPercent;
                target.activeBuffs.Add(new ActiveBuff(actor, BuffType.AttackPercent, ab.attackBuffPercent));
                AppendLog($"{target.data.characterName} gains +{ab.attackBuffPercent}% attack until {actor.data.characterName}'s next turn.");
            }
        }
        else if (pendingMove is DefenseBuffMove db)
        {
            foreach (var sel in pendingSelectedTargets)
            {
                if (sel.isEnemy) continue;
                var target = sel.character;
                target.totalDamageReductionPercent += db.damageReductionPercent;
                target.activeBuffs.Add(new ActiveBuff(actor, BuffType.DamageReductionPercent, db.damageReductionPercent));
                AppendLog($"{target.data.characterName} gains {db.damageReductionPercent}% damage reduction until {actor.data.characterName}'s next turn.");
            }
        }
        else if (pendingMove is HealMove hm)
        {
            foreach (var sel in pendingSelectedTargets)
            {
                if (sel.isEnemy) continue;
                var target = sel.character;
                if (target.IsDead())
                {
                    AppendLog($"{target.data.characterName} is dead and cannot be healed.");
                    continue;
                }

                target.ApplyHealPercent(hm.healPercent);
                AppendLog($"{target.data.characterName} healed {hm.healPercent}% of max HP.");
            }

        }
        else if (pendingMove is AttackMove am)
        {
            foreach (var sel in pendingSelectedTargets)
            {
                if (!sel.isEnemy) continue;
                var target = sel.enemy;
                int baseDamage = am.attackPoints;
                int totalBuff = actor.totalAttackBuffPercent;
                int damage = Mathf.CeilToInt(baseDamage * (1f + totalBuff / 100f));
                int finalDamage = Mathf.CeilToInt(damage * (1f - target.totalDamageReductionPercent / 100f));

                if (target.isDefending)
                {
                    AppendLog($"{target.data.enemyName} is defending and blocked the attack!");
                    target.isDefending = false;
                }
                else
                {
                    target.currentHP -= Mathf.Max(0, finalDamage);
                    AppendLog($"{actor.data.characterName} dealt {finalDamage} damage to {target.data.enemyName}.");
                }
            }
        }

        // cleanup
        pendingMove = null;
        pendingTargetsNeeded = 0;
        pendingSelectedTargets.Clear();
        ClearPanel(movesPanel);
        ClearPanel(targetsPanel);
        UpdateEnemyUI();
        UpdatePlayersUI();

        EndPlayerInput();

        yield return null;
    }

    private void EndPlayerInput()
    {
        waitingForPlayerInput = false;
        currentPlayer = null;
    }

    private void RemoveBuffsAppliedBy(CharacterInstance source)
    {
        foreach (var p in playerInstances)
        {
            var buffsToRemove = p.activeBuffs.FindAll(b => b.source == source);
            foreach (var b in buffsToRemove)
            {
                if (b.type == BuffType.AttackPercent)
                {
                    p.totalAttackBuffPercent -= b.value;
                    AppendLog($"{p.data.characterName}'s +{b.value}% attack buff from {source.data.characterName} expired.");
                }
                else if (b.type == BuffType.DamageReductionPercent)
                {
                    p.totalDamageReductionPercent -= b.value;
                    AppendLog($"{p.data.characterName}'s {b.value}% damage reduction buff from {source.data.characterName} expired.");
                }
                p.activeBuffs.Remove(b);
            }
        }

        var enemyBuffs = enemyInstance.activeBuffs.FindAll(b => b.source == source);
        foreach (var b in enemyBuffs)
        {
            if (b.type == BuffType.AttackPercent) enemyInstance.attackBuffPercent -= b.value;
            else if (b.type == BuffType.DamageReductionPercent) enemyInstance.totalDamageReductionPercent -= b.value;
            enemyInstance.activeBuffs.Remove(b);
        }
    }

    private void UpdateEnemyUI()
    {
        if (enemyInfoText)
            enemyInfoText.text = $"{enemyInstance.data.enemyName} HP: {enemyInstance.currentHP}/{enemyInstance.data.maxHP}";
    }

    private void UpdatePlayersUI()
    {
        string s = "Players:\n";
        foreach (var p in playerInstances)
        {
            s += $"{p.data.characterName} HP:{p.currentHP}/{p.data.health} | ATK_BUFF:{p.totalAttackBuffPercent}% | DEF_BUFF:{p.totalDamageReductionPercent}%\n";
        }
        AppendLog(s, appendToUI: false);
    }

    private void ClearPanel(Transform panel)
    {
        foreach (Transform t in panel)
            Destroy(t.gameObject);
    }

    private void AppendLog(string message, bool appendToUI = true)
{
    Debug.Log(message);
    if (appendToUI && logText)
    {
        logHistory.Enqueue(message);
        if (logHistory.Count > maxLogLines)
            logHistory.Dequeue();

        logText.text = string.Join("\n", logHistory);
    }
    }

    #region Helper structs
    private class TurnEntity
    {
        public bool isEnemy;
        public EnemyInstance enemy;
        public CharacterInstance character;
        public int speed;
    }

    private class TargetSelection
    {
        public bool isEnemy;
        public CharacterInstance character;
        public EnemyInstance enemy;
    }
    #endregion
}
