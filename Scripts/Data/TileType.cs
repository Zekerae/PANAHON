namespace Panahon.Data
{
    public enum TileType
    {
        Start,          // Timeline starting node / recovery point
        Clue,           // Provides hints regarding mission artifacts
        Event,          // Triggers historical events (positive or twist)
        Artifact,       // Location where artifacts can be discovered or returned
        Danger,         // Temporal hazard (energy loss, paradox increase, collapse risk)
        TimeGate        // Inter-dimensional gateway allowing travel to other eras
    }

    public static class TileTypeExtensions
    {
        public static string GetTileName(this TileType type)
        {
            switch (type)
            {
                case TileType.Start: return "Timeline Nexus (Start)";
                case TileType.Clue: return "Clue Site";
                case TileType.Event: return "Historical Event";
                case TileType.Artifact: return "Artifact Vault";
                case TileType.Danger: return "Temporal Anomaly (Danger)";
                case TileType.TimeGate: return "Time Gate Portal";
                default: return type.ToString();
            }
        }

        public static UnityEngine.Color GetTileColor(this TileType type)
        {
            switch (type)
            {
                case TileType.Start: return new UnityEngine.Color(0.2f, 0.8f, 0.4f);       // Emerald Green
                case TileType.Clue: return new UnityEngine.Color(0.25f, 0.7f, 0.95f);     // Cyan
                case TileType.Event: return new UnityEngine.Color(0.95f, 0.75f, 0.2f);    // Gold / Amber
                case TileType.Artifact: return new UnityEngine.Color(0.95f, 0.9f, 0.3f);  // Luminous Yellow
                case TileType.Danger: return new UnityEngine.Color(0.95f, 0.25f, 0.25f);  // Warning Red
                case TileType.TimeGate: return new UnityEngine.Color(0.45f, 0.3f, 0.95f); // Cosmic Purple
                default: return UnityEngine.Color.gray;
            }
        }
    }
}
