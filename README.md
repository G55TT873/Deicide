# Implemented Features
1. CombatManager Overhaul

Fully restructured the combat flow using a single coroutine-driven turn system.

Added player and enemy turn sequencing based on speed.

Implemented a clean turn order display for debugging and clarity.

Introduced a centralized logging system (AppendLog) that:

Displays up to 3 recent messages.

Clears the previous message before appending a new one.

Outputs all logs to both the console and UI text field.

2. Enemy Integration

Added a DummyEnemy asset to act as a test target.

Implemented the EnemyInstance class for runtime data tracking.

The enemy:

Automatically attacks a random living player.

Applies a fixed 10 damage adjusted by buffs/debuffs.

Is defeated when HP reaches 0, ending the battle.

3. Player System

Player instances (CharacterInstance) now:

Track runtime HP, buffs, and defense states.

Skip their turn automatically if dead.

Integrated buff management and defense tracking directly into the player flow.

4. Move Execution System

Implemented dynamic handling for MoveData ScriptableObjects:

AttackMove – Calculates and applies damage with buff/debuff modifiers.

HealMove – Restores a percentage of HP, skipping dead characters.

AttackBuffMove – Adds temporary attack percentage buffs (now lasting 2 turns).

DefenseBuffMove – Adds temporary damage reduction percentage buffs (also 2 turns).

DefendMove – Enables a defend state that blocks incoming damage once.

Target selection UI now dynamically updates based on move type:

Heals and buffs target allies.

Attacks target enemies.

Multiple targets are supported.

5. Buff Duration System

Buffs now persist for 2 full turns instead of 1.

Implemented a system to track and remove expired buffs automatically after duration completion.

Removed redundant buff stacking and ensured proper percentage recalculation after expiration.

6. Turn System & Battle Loop

Created a continuous loop coroutine (ProcessTurns) to manage battle flow.

Each entity (enemy or player) acts in order of descending speed.

Automatically checks win/lose conditions each cycle:

Victory – Enemy HP reaches 0.

Defeat – All players are dead.

7. Input and UI Improvements

Added UI panels for Moves and Targets, generated dynamically each turn.

Ensured that only valid targets (alive allies or enemies) can be selected.

Disabled invalid selections to prevent runtime errors.

Introduced button highlight feedback on selected targets.

Defend action immediately ends the player's turn after use.

8. Healing Restriction

Implemented logic to prevent healing of dead allies.

Log message reflects skipped healing attempts accurately.

9. Log and UI Enhancements

Replaced append-style logs with a queue system that displays the latest 3 messages.

Automatically clears older messages.

Added developer-friendly logging to the Unity console for traceability.

# Technical Summary

| Component                       | Description                                                     |
| ------------------------------- | --------------------------------------------------------------- |
| **CombatManager.cs**            | Core battle logic, turn sequencing, and input handling.         |
| **CharacterInstance.cs**        | Tracks runtime player data (HP, buffs, defense).                |
| **EnemyInstance.cs**            | Runtime representation of the enemy with HP and buffs.          |
| **MoveData.cs & Derived Types** | ScriptableObject definitions for all move types.                |
| **UI Prefabs**                  | Move and target buttons instantiated dynamically during combat. |
| **EnemyDummy Asset**            | Simple ScriptableObject-based enemy used for combat testing.    |

# Current Status

| Category                               | Status                              |
| -------------------------------------- | ----------------------------------- |
| **Core Turn System**                   | ✅ Fully functional                  |
| **Player & Enemy Logic**               | ✅ Working as intended               |
| **Buff & Debuff Duration**             | ✅ Extended to 2 turns               |
| **Healing Restriction**                | ✅ Dead characters cannot be healed  |
| **Combat UI**                          | ✅ Fully responsive                  |
| **Logging System**                     | ✅ Implemented with limited history  |
| **Win/Loss Conditions**                | ✅ Fully operational                 |
| **Data Passing (Character Selection)** | ✅ Integrated with `GameRuntimeData` |




