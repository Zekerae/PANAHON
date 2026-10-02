namespace Panahon.Data
{
    public enum EraType
    {
        SpanishEra,     // 1521-1898: Galleons, Intramuros, Colonial relics
        AmericanEra,    // 1898-1946: Commonwealth, Telegraphs, Modern industry
        JapaneseEra,    // 1942-1945: Guerrilla resistance, Wartime intelligence
        MythLegendEra   // Ancient Pre-colonial: Bathala, Bakunawa, Mythic spirits
    }

    public static class EraExtensions
    {
        public static string GetDisplayName(this EraType era)
        {
            switch (era)
            {
                case EraType.SpanishEra: return "Spanish Era (1521-1898)";
                case EraType.AmericanEra: return "American Era (1898-1946)";
                case EraType.JapaneseEra: return "Japanese Era (1942-1945)";
                case EraType.MythLegendEra: return "Myth & Legend Era (Pre-Colonial)";
                default: return era.ToString();
            }
        }

        public static UnityEngine.Color GetEraColor(this EraType era)
        {
            switch (era)
            {
                case EraType.SpanishEra: return new UnityEngine.Color(0.85f, 0.45f, 0.2f);      // Terra cotta / Ochre
                case EraType.AmericanEra: return new UnityEngine.Color(0.2f, 0.5f, 0.85f);     // Vintage Navy
                case EraType.JapaneseEra: return new UnityEngine.Color(0.8f, 0.2f, 0.25f);     // Crimson / Bamboo red
                case EraType.MythLegendEra: return new UnityEngine.Color(0.6f, 0.25f, 0.8f);   // Mystical Amethyst
                default: return UnityEngine.Color.white;
            }
        }
    }
}
