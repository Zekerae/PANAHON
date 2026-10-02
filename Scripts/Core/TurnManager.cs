using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Panahon.Player;
using Panahon.Board;
using Panahon.Data;
using Panahon.Cards;

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

        public IReadOnlyList<PlayerController> Players => players;
        public PlayerController ActivePlayer => (players != null && players.Count > activePlayerIndex) ? players[activePlayerIndex] : null;
        public TurnPhase CurrentPhase => currentPhase;
        public int CurrentRoundNumber => currentRoundNumber;
        public int LastDiceRoll => lastDiceRoll;

        public event Action<TurnPhase> OnPhaseChanged;
        public event Action<PlayerController> OnTurnStarted;
        public event Action<int> OnDiceRolled;
        public event Action<PlayerController, BoardTile> OnTileLanded;
        public event Action<string> OnGameLog;

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

            activePlayerIndex = startingPlayerIdx;
            OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} got the highest roll ({highestRoll}) and takes the first turn!");
            yield return new WaitForSeconds(0.5f);

            onComplete?.Invoke(ActivePlayer, rollResults);
        }

        public void StartTurn()
        {
            if (ActivePlayer == null) return;

            SetPhase(TurnPhase.TurnStart);
            ActivePlayer.TickCooldown();

            if (ActivePlayer.SkipNextTurn)
            {
                ActivePlayer.SkipNextTurn = false;
                OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName}'s turn was skipped due to temporal disruption!");
                EndTurn();
                return;
            }

            OnTurnStarted?.Invoke(ActivePlayer);
            OnGameLog?.Invoke($"Turn {currentRoundNumber}: It is now {ActivePlayer.CharacterData?.CharacterName}'s turn.");
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
                default:
                    OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} rested at {tile.TileName}.");
                    EndTurn();
                    break;
            }
        }

        private void ResolveClueTile()
        {
            ClueCardData clue = DeckManager.Instance != null ? DeckManager.Instance.DrawClue(ActivePlayer.CurrentEra) : null;
            if (clue != null)
            {
                ActivePlayer.Inventory.AddClue(clue);
                ActivePlayer.AddScore(5);
                OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} investigated a Clue: [{clue.Title}] - {clue.ClueDetails}");
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
                        OnGameLog?.Invoke($"SUCCESS! {ActivePlayer.CharacterData?.CharacterName} returned [{artifactToReturn.Title}] to its correct era ({currentEra})! (+{artifactToReturn.PointsReward} pts)");
                        if (TimelineManager.Instance != null) TimelineManager.Instance.StabilizeTimeline(2, "Artifact correctly restored");
                    }
                    else
                    {
                        OnGameLog?.Invoke($"PARADOX! [{artifactToReturn.Title}] was placed into the WRONG timeline ({currentEra})!");
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
            if (availableArtifact != null && !inv.HeldArtifacts.Contains(availableArtifact) && !inv.ReturnedArtifacts.Contains(availableArtifact))
            {
                if (inv.CanAcquireArtifact(availableArtifact))
                {
                    inv.AcquireArtifact(availableArtifact);
                    ActivePlayer.AddScore(15);
                    OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} acquired [{availableArtifact.Title}] using gathered clues!");
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
                if (evt.EnergyChange != 0)
                {
                    if (evt.EnergyChange > 0) ActivePlayer.RestoreEnergy(evt.EnergyChange);
                    else ActivePlayer.SpendEnergy(-evt.EnergyChange);
                }
                if (evt.TimelineStabilityChange != 0 && TimelineManager.Instance != null)
                {
                    if (evt.TimelineStabilityChange > 0) TimelineManager.Instance.IncreaseDestabilization(evt.TimelineStabilityChange, evt.Title);
                    else TimelineManager.Instance.StabilizeTimeline(-evt.TimelineStabilityChange, evt.Title);
                }
                if (evt.IndividualParadoxChange != 0)
                {
                    if (evt.IndividualParadoxChange > 0) ParadoxManager.Instance?.ApplyParadox(ActivePlayer, evt.IndividualParadoxChange, evt.Title);
                    else ActivePlayer.ReduceParadox(-evt.IndividualParadoxChange);
                }
                if (evt.PointsBonus > 0) ActivePlayer.AddScore(evt.PointsBonus);
            }
            EndTurn();
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
                }
                if (danger.ForcedRelocationToStart)
                {
                    BoardTile startTile = BoardManager.Instance?.GetStartTile(ActivePlayer.CurrentEra);
                    if (startTile != null) ActivePlayer.PlaceOnTile(startTile);
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

            if (ActivePlayer.CurrentEnergy < cost)
            {
                OnGameLog?.Invoke($"Not enough Energy to jump through Time Gate! (Needs {cost} Energy, has {ActivePlayer.CurrentEnergy})");
                EndTurn();
                return;
            }

            BoardPath path = BoardManager.Instance?.GetEraPath(targetEra);
            if (path != null && path.GetStartTile() != null)
            {
                ActivePlayer.WarpToTile(path.GetStartTile(), cost);
                OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} jumped through Time Gate into {targetEra.GetDisplayName()}! (-{cost} Energy)");
            }
            EndTurn();
        }

        public void PassTimeGate()
        {
            OnGameLog?.Invoke($"{ActivePlayer.CharacterData?.CharacterName} chose to stay in {ActivePlayer.CurrentEra}.");
            EndTurn();
        }

        public void EndTurn()
        {
            SetPhase(TurnPhase.TurnEnd);
            CheckAllMissionsComplete();

            if (currentPhase == TurnPhase.GameOver) return;

            // Move to next player in sequence
            activePlayerIndex = (activePlayerIndex + 1) % players.Count;
            if (activePlayerIndex == 0)
            {
                currentRoundNumber++;
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
                OnGameLog?.Invoke("MISSION SUCCESS! All 4 Temporal Castaways have restored their designated artifacts to the timeline!");
                GameManager.Instance?.TriggerVictory();
            }
        }
    }
}
