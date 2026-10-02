using System;
using System.Collections.Generic;
using UnityEngine;
using Panahon.Data;

namespace Panahon.Board
{
    public class BoardManager : MonoBehaviour
    {
        public static BoardManager Instance { get; private set; }

        [Header("Era Tracks")]
        [SerializeField] private BoardPath spanishEraPath;
        [SerializeField] private BoardPath americanEraPath;
        [SerializeField] private BoardPath japaneseEraPath;
        [SerializeField] private BoardPath mythLegendEraPath;

        private Dictionary<EraType, BoardPath> eraPathMap = new Dictionary<EraType, BoardPath>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            RegisterPaths();
        }

        public void RegisterPaths()
        {
            eraPathMap.Clear();
            if (spanishEraPath != null) eraPathMap[EraType.SpanishEra] = spanishEraPath;
            if (americanEraPath != null) eraPathMap[EraType.AmericanEra] = americanEraPath;
            if (japaneseEraPath != null) eraPathMap[EraType.JapaneseEra] = japaneseEraPath;
            if (mythLegendEraPath != null) eraPathMap[EraType.MythLegendEra] = mythLegendEraPath;
        }

        public void SetEraPaths(BoardPath spanish, BoardPath american, BoardPath japanese, BoardPath myth)
        {
            spanishEraPath = spanish;
            americanEraPath = american;
            japaneseEraPath = japanese;
            mythLegendEraPath = myth;
            RegisterPaths();
        }

        public BoardPath GetEraPath(EraType era)
        {
            if (eraPathMap.TryGetValue(era, out BoardPath path))
            {
                return path;
            }
            return null;
        }

        public BoardTile GetStartTile(EraType era)
        {
            BoardPath path = GetEraPath(era);
            return path != null ? path.GetStartTile() : null;
        }

        public BoardTile GetTile(EraType era, int index)
        {
            BoardPath path = GetEraPath(era);
            return path != null ? path.GetTileAtIndex(index) : null;
        }

        /// <summary>
        /// Retrieves the list of destination tiles available when warping via a Time Gate.
        /// Defaults to the Time Gate tile or Start tile of each other era.
        /// </summary>
        public List<BoardTile> GetTimeGateDestinations(EraType currentEra)
        {
            List<BoardTile> destinations = new List<BoardTile>();
            foreach (EraType era in Enum.GetValues(typeof(EraType)))
            {
                if (era == currentEra) continue;

                BoardPath path = GetEraPath(era);
                if (path != null)
                {
                    // Look for a TimeGate tile in the destination era, else default to Start tile
                    var gateTiles = path.GetTilesOfType(TileType.TimeGate);
                    if (gateTiles != null && gateTiles.Count > 0)
                    {
                        destinations.Add(gateTiles[0]);
                    }
                    else if (path.GetStartTile() != null)
                    {
                        destinations.Add(path.GetStartTile());
                    }
                }
            }
            return destinations;
        }
    }
}
