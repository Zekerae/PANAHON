using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Panahon.Player;
using Panahon.Board;
using Panahon.Data;
using Panahon.Cards;
using Panahon.UI;

namespace Panahon.Core
{
    public enum TurnPhase
    {
        RollForFirstTurn,
        TurnStart,
        RollingDice,
        Moving,
        TileInteraction,
        TimeGateDecision,
        BonusMovement,
        AbilityUsage,
        TurnEnd,
        GameOver
    }

    public class TurnManager : MonoBehaviour
    {
        public static TurnManager Instance { get; private set; }

        [Header("Players")]
        [SerializeField] private List<PlayerController> players = new List<PlayerController>();
        [SerializeField] private int activePlayerIndex = 0;
        [SerializeField] private TurnPhase currentPhase = TurnPhase.RollForFirstTurn;
        [SerializeField] private int currentRoundNumber = 1;
        [SerializeField] private int lastDiceRoll = 1;

        [Header("Turn Pacing")]
        [SerializeField] private float turnTransitionDelay = 0.8f;
        [SerializeField] private bool autoAdvanceTurns = true;

        public IReadOnlyList<PlayerController> Players => players;
        public PlayerController ActivePlayer => (players != null && players.Count > activePlayerIndex && activePlayerIndex >= 0) ? players[activePlayerIndex] : null;
        public int ActivePlayerIndex => activePlayerIndex;
        public TurnPhase CurrentPhase => currentPhase;
        public int CurrentRoundNumber => currentRoundNumber;
        public int LastDiceRoll => lastDiceRoll;

        public event Action<TurnPhase> OnPhaseChanged;
        public event Action<PlayerController> OnTurnStarted;
        public event Action<PlayerController> OnTurnEnded;
        public event Action<int> OnDiceRolled;
        public event Action<PlayerController, BoardTile> OnTileLanded;
        public event Action<string> OnGameLog;
        public event Action<int> OnRoundChanged;

        // Pending bonus movement from Event cards
        private int pendingBonusMovement = 0;
        // Pending free warp from Event cards
        private bool pendingFreeWarp = false;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void RegisterPlayers(List<PlayerController> playerList)
        {
            players = new List<PlayerController>(playerList);
        }

        public void SetPhase(TurnPhase phase)
        {
            currentPhase = phase;
            OnPhaseChanged?.Invoke(phase);
        }

        /// <summary>
        /// Rolls 1d6 for all 4 players to decide who takes the first turn.
        /// Handles ties by re-rolling tied players.
        /// </summary>
        public IEnumerator DetermineFirstTurnRoutine(Action<PlayerController, Dictionary<int, int>> onComplete)
        {
            SetPhase(TurnPhase.RollForFirstTurn);
            Dictionary<int, int> rollResults = new Dictionary<int, int>();
            int highestRoll = -1;
            int startingPlayerIdx = 0;

            OnGameLog?.Invoke("Rolling dice to determine first turn order...");

            for (int i = 0; i < players.Count; i++)
            {
                int roll = UnityEngine.Random.Range(1, 7);
                rollResults[i] = roll;
                OnGameLog?.Invoke($"{players[i].CharacterData?.CharacterName ?? $"Player {i+1}"} rolled a {roll}.");
                if (roll > highestRoll)
                {
                    highestRoll = roll;
                    startingPlayerIdx = i;
                }
                yield return new WaitForSeconds(0.35f);
            }

            // Check for ties at the highest roll
            List<int> tiedPlayers = new List<int>();
            foreach (var kvp in rollResults)
            {
                if (kvp.Value == highestRoll) tiedPlayers.Add(kvp.Key);
            }

            // Re-roll ties
            while (tiedPlayers.Count > 1)
            {
                OnGameLog?.Invoke($"Tie detected! Re-rolling for {tiedPlayers.Count} players...");
                yield return new WaitForSeconds(0.3f);

                highestRoll = -1;
                Dictionary<int, int> tieBreakers = new Dictionary<int, int>();
                foreach (int idx in tiedPlayers)
                {
                    int roll = UnityEngine.Random.Range(1, 7);
                    tieBreakers[idx] = roll;
                    OnGameLog?.Invoke($"{players[idx].CharacterData?.CharacterName ?? $"Player {idx+1}"} re-rolled a {roll}.");
                    if (roll > highestRoll)
                    {
                        highestRoll = roll;
                        startingPlayerIdx = idx;
                    }
                    yield return new WaitForSeconds(0.25f);
                }

                tiedPlayers = tieBreakers.Where(kvp => kvp.Value == highestRoll).Select(kvp => kvp.Key).ToList();
            }

            activePlayerIndex = startingPlayerIdx;
            OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} got the highest roll ({highestRoll}) and takes the first turn!");
            yield return new WaitForSeconds(0.5f);

            onComplete?.Invoke(ActivePlayer, rollResults);
        }

        public void StartTurn()
        {
            if (ActivePlayer == null) return;
            if (currentPhase == TurnPhase.GameOver) return;

            SetPhase(TurnPhase.TurnStart);
            ActivePlayer.TickCooldown();
            pendingBonusMovement = 0;
            pendingFreeWarp = false;

            if (ActivePlayer.SkipNextTurn)
            {
                ActivePlayer.SkipNextTurn = false;
                OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName}'s turn was skipped due to temporal disruption!");
                StartCoroutine(DelayedEndTurn());
                return;
            }

            OnTurnStarted?.Invoke(ActivePlayer);
            OnGameLog?.Invoke($"Turn {currentRoundNumber}: It is now {ActivePlayer.CharacterData?.CharacterName}'s turn. ({ActivePlayer.CurrentEra.GetDisplayName()})");
        }

        public void RollMovementDice()
        {
            if (currentPhase != TurnPhase.TurnStart && currentPhase != TurnPhase.RollingDice) return;

            SetPhase(TurnPhase.RollingDice);
            lastDiceRoll = UnityEngine.Random.Range(1, 7);
            OnDiceRolled?.Invoke(lastDiceRoll);
            OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} rolled a {lastDiceRoll} for movement.");

            // Start movement coroutine
            StartCoroutine(PerformPlayerMovement(lastDiceRoll));
        }

        private IEnumerator PerformPlayerMovement(int spaces)
        {
            SetPhase(TurnPhase.Moving);
            bool movementFinished = false;

            StartCoroutine(ActivePlayer.MoveSpacesRoutine(spaces, () => {
                movementFinished = true;
            }));

            yield return new WaitUntil(() => movementFinished);

            BoardTile landedTile = ActivePlayer.CurrentTile;
            OnTileLanded?.Invoke(ActivePlayer, landedTile);
            SetPhase(TurnPhase.TileInteraction);

            ResolveTileInteraction(landedTile);
        }

        public void ResolveTileInteraction(BoardTile tile)
        {
            if (tile == null)
            {
                EndTurn();
                return;
            }

            switch (tile.Type)
            {
                case TileType.Clue:
                    ResolveClueTile();
                    break;
                case TileType.Artifact:
                    ResolveArtifactTile();
                    break;
                case TileType.Event:
                    ResolveEventTile();
                    break;
                case TileType.Danger:
                    ResolveDangerTile();
                    break;
                case TileType.TimeGate:
                    ResolveTimeGateTile();
                    break;
                case TileType.Start:
                    ResolveStartTile();
                    break;
                default:
                    OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} rested at {tile.TileName}.");
                    EndTurn();
                    break;
            }
        }

        private void ResolveStartTile()
        {
            // Landing on Start restores a small amount of energy
            ActivePlayer.RestoreEnergy(1);
            OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} returned to the Timeline Nexus. (+1 Energy recovered)");
            EndTurn();
        }

        private void ResolveClueTile()
        {
            ClueCardData clue = DeckManager.Instance != null ? DeckManager.Instance.DrawClue(ActivePlayer.CurrentEra) : null;
            if (clue != null)
            {
                ActivePlayer.Inventory.AddClue(clue);
                ActivePlayer.AddScore(5);
                OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} investigated a Clue: [{clue.Title}] - {clue.ClueDetails}");

                // Show card modal if available
                if (CardModalUI.Instance != null)
                {
                    CardModalUI.Instance.ShowCard(
                        "CLUE", clue.Title, clue.Era.GetDisplayName(),
                        clue.ClueDetails, $"Category: {clue.Category} | Linked Artifact: {clue.TargetArtifactId}",
                        TileType.Clue.GetTileColor());
                }
            }
            else
            {
                OnGameLog?.Invoke("No more clues available in this timeline.");
            }
            EndTurn();
        }

        private void ResolveArtifactTile()
        {
            var inv = ActivePlayer.Inventory;
            EraType currentEra = ActivePlayer.CurrentEra;

            // 1. Check if holding an artifact to return
            if (inv.HeldArtifacts.Count > 0)
            {
                ArtifactCardData artifactToReturn = inv.HeldArtifacts[0];
                bool returned = inv.ReturnArtifact(artifactToReturn, currentEra, out bool isCorrectEra);
                if (returned)
                {
                    if (isCorrectEra)
                    {
                        ActivePlayer.AddScore(artifactToReturn.PointsReward);
                        OnGameLog?.Invoke($"SUCCESS! {ActivePlayer.CharacterData?.CharacterName} returned [{artifactToReturn.Title}] to its correct era ({currentEra.GetDisplayName()})! (+{artifactToReturn.PointsReward} pts)");
                        if (TimelineManager.Instance != null) TimelineManager.Instance.StabilizeTimeline(2, "Artifact correctly restored");
                    }
                    else
                    {
                        OnGameLog?.Invoke($"PARADOX! [{artifactToReturn.Title}] was placed into the WRONG timeline ({currentEra.GetDisplayName()})!");
                        if (ParadoxManager.Instance != null) ParadoxManager.Instance.ApplyParadox(ActivePlayer, 2, "Artifact returned to wrong timeline");
                    }

                    // Check endgame victory condition
                    CheckAllMissionsComplete();
                    EndTurn();
                    return;
                }
            }

            // 2. Check if can acquire artifact in this era
            ArtifactCardData availableArtifact = DeckManager.Instance != null ? DeckManager.Instance.GetArtifactForEra(currentEra) : null;
            if (availableArtifact != null && !inv.HasArtifact(availableArtifact) && !inv.HasReturnedArtifact(availableArtifact))
            {
                if (inv.CanAcquireArtifact(availableArtifact))
                {
                    inv.AcquireArtifact(availableArtifact);
                    ActivePlayer.AddScore(15);
                    OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} acquired [{availableArtifact.Title}] using gathered clues!");

                    if (CardModalUI.Instance != null)
                    {
                        CardModalUI.Instance.ShowCard(
                            "ARTIFACT", availableArtifact.Title, availableArtifact.Era.GetDisplayName(),
                            availableArtifact.Description,
                            $"Origin: {availableArtifact.CorrectOriginEra.GetDisplayName()} | Reward: {availableArtifact.PointsReward} pts",
                            TileType.Artifact.GetTileColor());
                    }
                }
                else
                {
                    int cluesFound = inv.GetClueCountForArtifact(availableArtifact.CardId);
                    int required = inv.ActiveMission != null ? inv.ActiveMission.RequiredCluesCount : 1;
                    OnGameLog?.Invoke($"Artifact [{availableArtifact.Title}] located, but you need more clues to decipher its extraction! ({cluesFound}/{required} clues)");
                }
            }
            else
            {
                OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} surveyed the artifact vault, but found no displaced relics here.");
            }

            EndTurn();
        }

        private void ResolveEventTile()
        {
            EventCardData evt = DeckManager.Instance != null ? DeckManager.Instance.DrawEvent(ActivePlayer.CurrentEra) : null;
            if (evt != null)
            {
                OnGameLog?.Invoke($"EVENT TRIGGERED: [{evt.Title}] - {evt.Description}");

                // Energy change
                if (evt.EnergyChange != 0)
                {
                    if (evt.EnergyChange > 0) ActivePlayer.RestoreEnergy(evt.EnergyChange);
                    else ActivePlayer.SpendEnergy(-evt.EnergyChange);
                }

                // Timeline stability change
                if (evt.TimelineStabilityChange != 0 && TimelineManager.Instance != null)
                {
                    if (evt.TimelineStabilityChange > 0) TimelineManager.Instance.IncreaseDestabilization(evt.TimelineStabilityChange, evt.Title);
                    else TimelineManager.Instance.StabilizeTimeline(-evt.TimelineStabilityChange, evt.Title);
                }

                // Individual paradox change
                if (evt.IndividualParadoxChange != 0)
                {
                    if (evt.IndividualParadoxChange > 0) ParadoxManager.Instance?.ApplyParadox(ActivePlayer, evt.IndividualParadoxChange, evt.Title);
                    else ActivePlayer.ReduceParadox(-evt.IndividualParadoxChange);
                }

                // Points bonus
                if (evt.PointsBonus > 0) ActivePlayer.AddScore(evt.PointsBonus);

                // Bonus movement (extra spaces forward)
                if (evt.BonusMovement > 0)
                {
                    pendingBonusMovement = evt.BonusMovement;
                    OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} gains +{evt.BonusMovement} bonus movement spaces!");
                }

                // Free era warp
                if (evt.AllowsFreeEraWarp)
                {
                    pendingFreeWarp = true;
                    OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} received a FREE Time Gate warp blessing!");
                }

                // Show card modal
                if (CardModalUI.Instance != null)
                {
                    CardModalUI.Instance.ShowCard(
                        "EVENT", evt.Title, evt.Era.GetDisplayName(),
                        evt.Description,
                        BuildEventEffectsSummary(evt),
                        TileType.Event.GetTileColor());
                }

                // Execute pending bonus actions
                if (pendingBonusMovement > 0)
                {
                    StartCoroutine(ExecuteBonusMovement());
                    return; // Don't end turn yet
                }

                if (pendingFreeWarp)
                {
                    SetPhase(TurnPhase.TimeGateDecision);
                    OnGameLog?.Invoke($"Choose a destination era for your free warp!");
                    return; // TimeGateDialog will handle
                }
            }
            EndTurn();
        }

        private string BuildEventEffectsSummary(EventCardData evt)
        {
            List<string> effects = new List<string>();
            if (evt.EnergyChange != 0) effects.Add($"Energy: {(evt.EnergyChange > 0 ? "+" : "")}{evt.EnergyChange}");
            if (evt.TimelineStabilityChange != 0) effects.Add($"Timeline: {(evt.TimelineStabilityChange > 0 ? "+" : "")}{evt.TimelineStabilityChange}");
            if (evt.IndividualParadoxChange != 0) effects.Add($"Paradox: {(evt.IndividualParadoxChange > 0 ? "+" : "")}{evt.IndividualParadoxChange}");
            if (evt.BonusMovement > 0) effects.Add($"Bonus Move: +{evt.BonusMovement}");
            if (evt.PointsBonus > 0) effects.Add($"Points: +{evt.PointsBonus}");
            if (evt.AllowsFreeEraWarp) effects.Add("Free Era Warp!");
            return string.Join(" | ", effects);
        }

        private IEnumerator ExecuteBonusMovement()
        {
            SetPhase(TurnPhase.BonusMovement);
            int bonusSpaces = pendingBonusMovement;
            pendingBonusMovement = 0;

            OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} moves {bonusSpaces} bonus spaces!");
            yield return new WaitForSeconds(0.3f);

            bool movementFinished = false;
            StartCoroutine(ActivePlayer.MoveSpacesRoutine(bonusSpaces, () => {
                movementFinished = true;
            }));

            yield return new WaitUntil(() => movementFinished);

            BoardTile landedTile = ActivePlayer.CurrentTile;
            OnTileLanded?.Invoke(ActivePlayer, landedTile);
            OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} landed on {landedTile?.TileName ?? "unknown"} after bonus movement.");

            // Resolve tile interaction after bonus movement (but no recursive bonus)
            if (landedTile != null && landedTile.Type != TileType.Event)
            {
                SetPhase(TurnPhase.TileInteraction);
                ResolveTileInteraction(landedTile);
            }
            else
            {
                EndTurn();
            }
        }

        private void ResolveDangerTile()
        {
            DangerCardData danger = DeckManager.Instance != null ? DeckManager.Instance.DrawDanger(ActivePlayer.CurrentEra) : null;
            if (danger != null)
            {
                OnGameLog?.Invoke($"WARNING - DANGER ENCOUNTERED: [{danger.Title}] - {danger.Description}");
                ActivePlayer.SpendEnergy(danger.EnergyLoss);
                ParadoxManager.Instance?.ApplyParadox(ActivePlayer, danger.IndividualParadoxIncrease, danger.Title);

                if (danger.TimelineDestabilization > 0 && TimelineManager.Instance != null)
                {
                    TimelineManager.Instance.IncreaseDestabilization(danger.TimelineDestabilization, danger.Title);
                }
                if (danger.LoseNextTurn)
                {
                    ActivePlayer.SkipNextTurn = true;
                    OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} will SKIP their next turn!");
                }
                if (danger.ForcedRelocationToStart)
                {
                    BoardTile startTile = BoardManager.Instance?.GetStartTile(ActivePlayer.CurrentEra);
                    if (startTile != null)
                    {
                        ActivePlayer.PlaceOnTile(startTile);
                        OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} was forcibly relocated to {startTile.TileName}!");
                    }
                }

                // Show card modal
                if (CardModalUI.Instance != null)
                {
                    string effects = $"Energy: -{danger.EnergyLoss} | Paradox: +{danger.IndividualParadoxIncrease} | Timeline: +{danger.TimelineDestabilization}";
                    if (danger.LoseNextTurn) effects += " | SKIP NEXT TURN";
                    if (danger.ForcedRelocationToStart) effects += " | RELOCATED TO START";

                    CardModalUI.Instance.ShowCard(
                        "DANGER", danger.Title, danger.Era.GetDisplayName(),
                        danger.Description, effects,
                        TileType.Danger.GetTileColor());
                }
            }
            EndTurn();
        }

        private void ResolveTimeGateTile()
        {
            SetPhase(TurnPhase.TimeGateDecision);
            OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} reached a Time Gate! Inter-era travel is available.");
            // UI will open TimeGate Dialog, allowing player to choose destination or pass.
        }

        public void TravelThroughTimeGate(EraType targetEra)
        {
            int cost = 2;
            if (ActivePlayer.PlayerIndex == 1) cost = 1; // Rift-Walker Tala

            // Free warp from events costs nothing
            if (pendingFreeWarp)
            {
                cost = 0;
                pendingFreeWarp = false;
                OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} used their FREE Time Gate warp!");
            }

            if (cost > 0 && ActivePlayer.CurrentEnergy < cost)
            {
                OnGameLog?.Invoke($"Not enough Energy to jump through Time Gate! (Needs {cost} Energy, has {ActivePlayer.CurrentEnergy})");
                EndTurn();
                return;
            }

            BoardPath path = BoardManager.Instance?.GetEraPath(targetEra);
            if (path != null && path.GetStartTile() != null)
            {
                ActivePlayer.WarpToTile(path.GetStartTile(), cost);
                OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} jumped through Time Gate into {targetEra.GetDisplayName()}! {(cost > 0 ? $"(-{cost} Energy)" : "(FREE!)")}");
            }
            EndTurn();
        }

        public void PassTimeGate()
        {
            pendingFreeWarp = false;
            OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} chose to stay in {ActivePlayer.CurrentEra.GetDisplayName()}.");
            EndTurn();
        }

        /// <summary>
        /// Allows the active player to use their unique ability during their turn.
        /// </summary>
        public void UsePlayerAbility()
        {
            if (ActivePlayer == null) return;
            if (currentPhase != TurnPhase.TurnStart) return;

            if (ActivePlayer.TriggerAbility(out string effectSummary))
            {
                OnGameLog?.Invoke($"ABILITY ACTIVATED: {effectSummary}");
            }
            else
            {
                OnGameLog?.Invoke($"Cannot use ability: {effectSummary}");
            }
        }

        public void EndTurn()
        {
            if (currentPhase == TurnPhase.GameOver) return;

            SetPhase(TurnPhase.TurnEnd);
            OnTurnEnded?.Invoke(ActivePlayer);
            CheckAllMissionsComplete();

            if (currentPhase == TurnPhase.GameOver) return;

            if (autoAdvanceTurns)
            {
                StartCoroutine(DelayedNextTurn());
            }
        }

        private IEnumerator DelayedEndTurn()
        {
            yield return new WaitForSeconds(turnTransitionDelay);
            EndTurn();
        }

        private IEnumerator DelayedNextTurn()
        {
            yield return new WaitForSeconds(turnTransitionDelay);

            // Move to next player in sequence
            activePlayerIndex = (activePlayerIndex + 1) % players.Count;
            if (activePlayerIndex == 0)
            {
                currentRoundNumber++;
                OnRoundChanged?.Invoke(currentRoundNumber);
                OnGameLog?.Invoke($"=== ROUND {currentRoundNumber} BEGINS ===");
            }

            StartTurn();
        }

        public void CheckAllMissionsComplete()
        {
            if (players == null || players.Count == 0) return;

            bool allComplete = true;
            foreach (var p in players)
            {
                if (!p.Inventory.IsMissionComplete())
                {
                    allComplete = false;
                    break;
                }
            }

            if (allComplete)
            {
                SetPhase(TurnPhase.GameOver);
                OnGameLog?.Invoke("MISSION SUCCESS! All 4 Temporal Castaways have restored their designated artifacts to the timeline!");
                GameManager.Instance?.TriggerVictory();
            }
        }
    }
}
