using System;
using System.Collections;
using UnityEngine;
using Panahon.Data;
using Panahon.Board;

namespace Panahon.Player
{
    public class PlayerController : MonoBehaviour
    {
        [Header("Character Profile")]
        [SerializeField] private PlayerCharacterData characterData;
        [SerializeField] private int playerIndex;

        [Header("Runtime Stats")]
        [SerializeField] private int currentEnergy = 10;
        [SerializeField] private int maxEnergy = 15;
        [SerializeField] private int individualParadox = 0;
        [SerializeField] private int maxParadox = 5;
        [SerializeField] private int score = 0;
        [SerializeField] private int abilityCooldown = 0;
        [SerializeField] private bool skipNextTurn = false;

        [Header("Board Location")]
        [SerializeField] private EraType currentEra;
        [SerializeField] private BoardTile currentTile;

        [Header("Inventory")]
        [SerializeField] private PlayerInventory inventory = new PlayerInventory();

        [Header("Visual Components")]
        [SerializeField] private Renderer tokenRenderer;
        [SerializeField] private Transform tokenModel;

        public PlayerCharacterData CharacterData => characterData;
        public int PlayerIndex => playerIndex;
        public int CurrentEnergy => currentEnergy;
        public int MaxEnergy => maxEnergy;
        public int IndividualParadox => individualParadox;
        public int MaxParadox => maxParadox;
        public int Score => score;
        public int AbilityCooldown => abilityCooldown;
        public bool SkipNextTurn { get => skipNextTurn; set => skipNextTurn = value; }
        public EraType CurrentEra => currentEra;
        public BoardTile CurrentTile => currentTile;
        public PlayerInventory Inventory => inventory;
        public bool IsMoving { get; private set; }

        public event Action<PlayerController> OnStatsChanged;
        public event Action<PlayerController, BoardTile> OnTileLanded;
        public event Action<PlayerController, EraType> OnEraWarped;

        public void Initialize(int index, PlayerCharacterData data, BoardTile startTile)
        {
            playerIndex = index;
            characterData = data;
            if (data != null)
            {
                currentEnergy = data.StartingEnergy;
                maxEnergy = data.MaxEnergy;
                currentEra = data.StartingEra;
            }
            individualParadox = 0;
            score = 0;
            abilityCooldown = 0;
            skipNextTurn = false;

            if (startTile != null)
            {
                PlaceOnTile(startTile);
            }

            ApplyVisualColor();
            OnStatsChanged?.Invoke(this);
        }

        public void PlaceOnTile(BoardTile tile)
        {
            currentTile = tile;
            if (tile != null)
            {
                currentEra = tile.Era;
                transform.position = tile.LandingPosition;
            }
        }

        [SerializeField] private SpriteRenderer tokenSpriteRenderer;

        public void ApplyVisualColor()
        {
            if (characterData == null) return;

            if (tokenSpriteRenderer == null)
            {
                tokenSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (tokenSpriteRenderer != null)
            {
                tokenSpriteRenderer.color = characterData.TokenColor;
                return;
            }

            if (tokenRenderer == null)
            {
                tokenRenderer = GetComponentInChildren<Renderer>();
            }

            if (tokenRenderer != null)
            {
                MaterialPropertyBlock block = new MaterialPropertyBlock();
                tokenRenderer.GetPropertyBlock(block);
                block.SetColor("_Color", characterData.TokenColor);
                block.SetColor("_BaseColor", characterData.TokenColor);
                tokenRenderer.SetPropertyBlock(block);
            }
        }

        public void SpendEnergy(int amount)
        {
            currentEnergy = Mathf.Clamp(currentEnergy - amount, 0, maxEnergy);
            OnStatsChanged?.Invoke(this);
        }

        public void RestoreEnergy(int amount)
        {
            currentEnergy = Mathf.Clamp(currentEnergy + amount, 0, maxEnergy);
            OnStatsChanged?.Invoke(this);
        }

        public void AddParadox(int amount)
        {
            individualParadox = Mathf.Clamp(individualParadox + amount, 0, maxParadox);
            OnStatsChanged?.Invoke(this);
        }

        public void ReduceParadox(int amount)
        {
            individualParadox = Mathf.Clamp(individualParadox - amount, 0, maxParadox);
            OnStatsChanged?.Invoke(this);
        }

        public void AddScore(int points)
        {
            score = Mathf.Max(0, score + points);
            OnStatsChanged?.Invoke(this);
        }

        public void TickCooldown()
        {
            if (abilityCooldown > 0)
            {
                abilityCooldown--;
                OnStatsChanged?.Invoke(this);
            }
        }

        public bool CanUseAbility()
        {
            return abilityCooldown <= 0;
        }

        public bool TriggerAbility(out string effectSummary)
        {
            effectSummary = "";
            if (!CanUseAbility())
            {
                effectSummary = "Ability is still cooling down!";
                return false;
            }

            switch (playerIndex)
            {
                case 0: // Dr. Mateo Cruz - Chrono-Archivist
                    effectSummary = $"{characterData.CharacterName} unlocked Temporal Archive insight! (+3 Energy, -1 Paradox)";
                    RestoreEnergy(3);
                    ReduceParadox(1);
                    break;
                case 1: // Tala Dimaculangan - Rift-Walker
                    effectSummary = $"{characterData.CharacterName} harmonized rift fields! (+4 Energy for time travel)";
                    RestoreEnergy(4);
                    break;
                case 2: // Capt. Andres Valiente - Paradox Dampener
                    effectSummary = $"{characterData.CharacterName} deployed Temporal Anchor! (-2 Personal Paradox)";
                    ReduceParadox(2);
                    break;
                case 3: // Maya Santos - Quantum Scavenger
                    effectSummary = $"{characterData.CharacterName} scavenged chrono-particles! (+20 Points, +2 Energy)";
                    AddScore(20);
                    RestoreEnergy(2);
                    break;
                default:
                    effectSummary = "Ability activated.";
                    break;
            }

            abilityCooldown = characterData != null ? characterData.AbilityCooldownTurns : 2;
            OnStatsChanged?.Invoke(this);
            return true;
        }

        /// <summary>
        /// Moves forward along the current era's track step-by-step.
        /// </summary>
        public IEnumerator MoveSpacesRoutine(int spaces, Action onComplete)
        {
            IsMoving = true;

            for (int s = 0; s < spaces; s++)
            {
                if (currentTile == null || currentTile.NextTile == null) break;

                BoardTile next = currentTile.NextTile;
                Vector3 startPos = transform.position;
                Vector3 targetPos = next.LandingPosition;

                float elapsed = 0f;
                float duration = 0.28f;

                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = Mathf.Clamp01(elapsed / duration);
                    // 2.5D Tabletop hop curve
                    float hop = Mathf.Sin(t * Mathf.PI) * 0.45f;
                    Vector3 hopOffset = new Vector3(0, hop * 0.4f, -hop * 0.6f);
                    transform.position = Vector3.Lerp(startPos, targetPos, t) + hopOffset;
                    yield return null;
                }

                transform.position = targetPos;
                currentTile = next;
                currentEra = next.Era;
            }

            IsMoving = false;
            OnTileLanded?.Invoke(this, currentTile);
            onComplete?.Invoke();
        }

        /// <summary>
        /// Warps player through a Time Gate to another era.
        /// </summary>
        public void WarpToTile(BoardTile destinationTile, int energyCost = 2)
        {
            if (destinationTile == null) return;

            // Rift-Walker special perk: reduced energy cost
            if (playerIndex == 1) // Tala
            {
                energyCost = Mathf.Max(1, energyCost - 1);
            }

            SpendEnergy(energyCost);
            PlaceOnTile(destinationTile);
            OnEraWarped?.Invoke(this, destinationTile.Era);
            OnTileLanded?.Invoke(this, currentTile);
        }
    }
}
