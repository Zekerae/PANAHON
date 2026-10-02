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
        [SerializeField] private bool is2DPlane = true;
        [SerializeField] private SpriteRenderer spriteRenderer;

        public EraType Era => era;
        public TileType Type => tileType;
        public int TileIndex => tileIndex;
        public string TileName => string.IsNullOrEmpty(tileName) ? $"{era} - #{tileIndex} ({tileType})" : tileName;
        public BoardTile NextTile => nextTile;
        public IReadOnlyList<BoardTile> TimeGateConnections => timeGateConnections;
        public Vector3 LandingPosition => is2DPlane 
            ? transform.position + new Vector3(0, 0, -0.35f) 
            : transform.position + Vector3.up * 0.35f;

        public void Setup(EraType eraType, TileType type, int index, string customName = "", bool use2DPlane = true)
        {
            era = eraType;
            tileType = type;
            tileIndex = index;
            tileName = string.IsNullOrEmpty(customName) ? $"{era} - {type} #{index}" : customName;
            is2DPlane = use2DPlane;
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

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }

            if (spriteRenderer != null)
            {
                spriteRenderer.color = c;
                return;
            }

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

        private void OnDrawGizmos()
        {
            Gizmos.color = tileType.GetTileColor();
            Gizmos.DrawWireCube(transform.position, new Vector3(1.4f, 0.2f, 1.4f));

            if (nextTile != null)
            {
                Gizmos.color = Color.white * 0.7f;
                Gizmos.DrawLine(LandingPosition, nextTile.LandingPosition);
            }

            if (timeGateConnections != null && timeGateConnections.Count > 0)
            {
                Gizmos.color = new Color(0.8f, 0.3f, 1f, 0.8f);
                foreach (var gateDest in timeGateConnections)
                {
                    if (gateDest != null)
                    {
                        Gizmos.DrawLine(LandingPosition + Vector3.up * 0.2f, gateDest.LandingPosition + Vector3.up * 0.2f);
                    }
                }
            }
        }
    }
}
