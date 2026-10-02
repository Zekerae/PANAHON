using System;
using System.Collections.Generic;
using UnityEngine;
using Panahon.Data;

namespace Panahon.Board
{
    [ExecuteAlways]
    public class BoardTile : MonoBehaviour
    {
        [Header("Tile Identification")]
        [SerializeField] private EraType era;
        [SerializeField] private TileType tileType;
        [SerializeField] private int tileIndex;
        [SerializeField] private string tileName;

        [Header("Path Connections")]
        [SerializeField] private BoardTile nextTile;
        [SerializeField] private List<BoardTile> timeGateConnections = new List<BoardTile>();

        [Header("Visual Components")]
        [SerializeField] private Renderer tileRenderer;
        [SerializeField] private Light tileLight;

        [Header("2.5D Configuration")]
        [SerializeField] private bool is2DMode = true;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [Tooltip("Sorting order for 2.5D depth layering")]
        [SerializeField] private int baseSortingOrder = 0;

        [Header("Tile Label")]
        [SerializeField] private TextMesh tileLabel;

        public EraType Era => era;
        public TileType Type => tileType;
        public int TileIndex => tileIndex;
        public string TileName => string.IsNullOrEmpty(tileName) ? $"{era.GetDisplayName()} - #{tileIndex} ({tileType.GetTileName()})" : tileName;
        public BoardTile NextTile => nextTile;
        public IReadOnlyList<BoardTile> TimeGateConnections => timeGateConnections;

        /// <summary>
        /// Landing position in 2.5D: player stands slightly in front of the tile (negative Z offset)
        /// so the token overlaps the tile visually without being directly on top.
        /// In 3D mode, offset upward on Y instead.
        /// </summary>
        public Vector3 LandingPosition => is2DMode 
            ? transform.position + new Vector3(0, 0.05f, -0.35f) 
            : transform.position + Vector3.up * 0.35f;

        public void Setup(EraType eraType, TileType type, int index, string customName = "", bool use2DMode = true)
        {
            era = eraType;
            tileType = type;
            tileIndex = index;
            tileName = string.IsNullOrEmpty(customName) ? $"{era.GetDisplayName()} - {type.GetTileName()} #{index}" : customName;
            is2DMode = use2DMode;
            UpdateVisuals();
        }

        public void SetNextTile(BoardTile next)
        {
            nextTile = next;
        }

        public void AddTimeGateConnection(BoardTile destinationTile)
        {
            if (destinationTile != null && !timeGateConnections.Contains(destinationTile))
            {
                timeGateConnections.Add(destinationTile);
            }
        }

        public void UpdateVisuals()
        {
            Color c = tileType.GetTileColor();
            if (tileType == TileType.TimeGate)
            {
                c = new Color(0.6f, 0.2f, 0.95f);
            }

            // 2.5D SpriteRenderer path
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = c;
                // Set sorting order based on Y position for proper 2.5D depth
                spriteRenderer.sortingOrder = baseSortingOrder - Mathf.RoundToInt(transform.position.z * 10);
                return;
            }

            // 3D Renderer fallback
            if (tileRenderer == null)
            {
                tileRenderer = GetComponentInChildren<Renderer>();
            }

            if (tileRenderer != null)
            {
                MaterialPropertyBlock propBlock = new MaterialPropertyBlock();
                tileRenderer.GetPropertyBlock(propBlock);
                propBlock.SetColor("_Color", c);
                propBlock.SetColor("_BaseColor", c);
                propBlock.SetColor("_EmissionColor", c * 0.6f);
                tileRenderer.SetPropertyBlock(propBlock);
            }
        }

        /// <summary>
        /// Creates a procedural tile icon label (used by scene builder).
        /// </summary>
        public void CreateTileLabel(string iconSymbol)
        {
            if (tileLabel != null) return;

            GameObject labelObj = new GameObject("TileLabel");
            labelObj.transform.SetParent(transform);
            labelObj.transform.localPosition = new Vector3(0, 0.01f, 0);
            labelObj.transform.localRotation = Quaternion.Euler(90, 0, 0);

            tileLabel = labelObj.AddComponent<TextMesh>();
            tileLabel.text = iconSymbol;
            tileLabel.characterSize = 0.25f;
            tileLabel.fontSize = 32;
            tileLabel.alignment = TextAlignment.Center;
            tileLabel.anchor = TextAnchor.MiddleCenter;
            tileLabel.color = Color.white;
        }

        /// <summary>
        /// Returns a Unicode symbol representing this tile type for 2.5D labels.
        /// </summary>
        public string GetTileSymbol()
        {
            switch (tileType)
            {
                case TileType.Start:    return "★";
                case TileType.Clue:     return "🔎";
                case TileType.Event:    return "⚡";
                case TileType.Artifact: return "◆";
                case TileType.Danger:   return "⚠";
                case TileType.TimeGate: return "◎";
                default: return "•";
            }
        }

        private void OnDrawGizmos()
        {
            // 2.5D flat tile gizmo
            Gizmos.color = tileType.GetTileColor();
            if (is2DMode)
            {
                Gizmos.DrawWireCube(transform.position, new Vector3(1.4f, 1.4f, 0.1f));
            }
            else
            {
                Gizmos.DrawWireCube(transform.position, new Vector3(1.4f, 0.2f, 1.4f));
            }

            // Draw path connection
            if (nextTile != null)
            {
                Gizmos.color = Color.white * 0.7f;
                Gizmos.DrawLine(LandingPosition, nextTile.LandingPosition);

                // Draw arrow head
                Vector3 dir = (nextTile.LandingPosition - LandingPosition).normalized;
                Vector3 mid = Vector3.Lerp(LandingPosition, nextTile.LandingPosition, 0.7f);
                Gizmos.DrawRay(mid, Quaternion.Euler(0, 0, 30) * -dir * 0.4f);
                Gizmos.DrawRay(mid, Quaternion.Euler(0, 0, -30) * -dir * 0.4f);
            }

            // Draw Time Gate connections
            if (timeGateConnections != null && timeGateConnections.Count > 0)
            {
                Gizmos.color = new Color(0.8f, 0.3f, 1f, 0.8f);
                foreach (var gateDest in timeGateConnections)
                {
                    if (gateDest != null)
                    {
                        Vector3 offset = is2DMode ? Vector3.up * 0.2f : Vector3.up * 0.2f;
                        Gizmos.DrawLine(LandingPosition + offset, gateDest.LandingPosition + offset);
                    }
                }
            }
        }
    }
}
