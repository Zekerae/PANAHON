using System;
using System.Collections.Generic;
using UnityEngine;
using Panahon.Data;

namespace Panahon.Board
{
    public class BoardPath : MonoBehaviour
    {
        [Header("Era Configuration")]
        [SerializeField] private EraType era;
        [SerializeField] private List<BoardTile> tiles = new List<BoardTile>();

        public EraType Era => era;
        public IReadOnlyList<BoardTile> Tiles => tiles;
        public int TileCount => tiles.Count;

        public void InitializePath(EraType eraType, List<BoardTile> pathTiles)
        {
            era = eraType;
            tiles = pathTiles != null ? new List<BoardTile>(pathTiles) : new List<BoardTile>();
            LinkTilesInSequence();
        }

        public void LinkTilesInSequence()
        {
            if (tiles == null || tiles.Count == 0) return;

            for (int i = 0; i < tiles.Count; i++)
            {
                if (tiles[i] == null) continue;

                // Circular loop within era: last tile connects back to start tile (or stays looped)
                BoardTile next = (i + 1 < tiles.Count) ? tiles[i + 1] : tiles[0];
                tiles[i].SetNextTile(next);
            }
        }

        public BoardTile GetTileAtIndex(int index)
        {
            if (tiles == null || tiles.Count == 0) return null;
            int wrappedIndex = Mathf.Clamp(index, 0, tiles.Count - 1);
            return tiles[wrappedIndex];
        }

        public BoardTile GetStartTile()
        {
            if (tiles == null || tiles.Count == 0) return null;
            return tiles[0];
        }

        public List<BoardTile> GetTilesOfType(TileType type)
        {
            List<BoardTile> results = new List<BoardTile>();
            foreach (var tile in tiles)
            {
                if (tile != null && tile.Type == type)
                {
                    results.Add(tile);
                }
            }
            return results;
        }
    }
}
