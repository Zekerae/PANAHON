using System.Collections.Generic;
using UnityEngine;
using Panahon.Data;

namespace Panahon.Cards
{
    public class CardDatabase : MonoBehaviour
    {
        public static CardDatabase Instance { get; private set; }

        [Header("Default Historical Decks")]
        [SerializeField] private List<MissionCardData> missionCards = new List<MissionCardData>();
        [SerializeField] private List<ClueCardData> clueCards = new List<ClueCardData>();
        [SerializeField] private List<ArtifactCardData> artifactCards = new List<ArtifactCardData>();
        [SerializeField] private List<EventCardData> eventCards = new List<EventCardData>();
        [SerializeField] private List<DangerCardData> dangerCards = new List<DangerCardData>();

        public IReadOnlyList<MissionCardData> MissionCards => missionCards;
        public IReadOnlyList<ClueCardData> ClueCards => clueCards;
        public IReadOnlyList<ArtifactCardData> ArtifactCards => artifactCards;
        public IReadOnlyList<EventCardData> EventCards => eventCards;
        public IReadOnlyList<DangerCardData> DangerCards => dangerCards;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            // Generate default historical cards if list is empty
            if (missionCards.Count == 0 || clueCards.Count == 0 || artifactCards.Count == 0)
            {
                PopulateDefaultDatabase();
            }
        }

        public void PopulateDefaultDatabase()
        {
            missionCards.Clear();
            clueCards.Clear();
            artifactCards.Clear();
            eventCards.Clear();
            dangerCards.Clear();

            // 1. Artifacts
            var art1 = ScriptableObject.CreateInstance<ArtifactCardData>();
            art1.SetupArtifact("ART_01", "Golden Monstrance of Intramuros", 
                "A consecrated Spanish colonial vessel made of pure embossed gold and encrusted with rubies.",
                EraType.SpanishEra, EraType.AmericanEra, 35, "Spiritual anchor displaced during the Battle of Manila.");
            artifactCards.Add(art1);

            var art2 = ScriptableObject.CreateInstance<ArtifactCardData>();
            art2.SetupArtifact("ART_02", "Corregidor Ham Radio Transceiver", 
                "A vintage shortwave vacuum-tube radio used to broadcast Allied updates across the Pacific.",
                EraType.AmericanEra, EraType.JapaneseEra, 30, "Crucial communications relic displaced into the Japanese wartime lines.");
            artifactCards.Add(art2);

            var art3 = ScriptableObject.CreateInstance<ArtifactCardData>();
            art3.SetupArtifact("ART_03", "Guerrilla Intelligence Cipher Book", 
                "Handwritten micro-codebook containing encrypted spy rings and mountain patrol coordinates.",
                EraType.JapaneseEra, EraType.SpanishEra, 35, "Underground resistance cipher lost into colonial archives.");
            artifactCards.Add(art3);

            var art4 = ScriptableObject.CreateInstance<ArtifactCardData>();
            art4.SetupArtifact("ART_04", "Golden Tara of Agusan", 
                "A pre-colonial 21-karat Buddhist-Hindu gold figurine discovered along the Wawa River.",
                EraType.MythLegendEra, EraType.AmericanEra, 45, "Sacred divine effigy pulled through a temporal vortex into modern times.");
            artifactCards.Add(art4);

            var art5 = ScriptableObject.CreateInstance<ArtifactCardData>();
            art5.SetupArtifact("ART_05", "Spoliarium Charcoal Sketch", 
                "An original preparatory drawing by Juan Luna depicting fallen gladiators.",
                EraType.SpanishEra, EraType.MythLegendEra, 30, "Masterpiece draft scattered into mythical dreamscapes.");
            artifactCards.Add(art5);

            var art6 = ScriptableObject.CreateInstance<ArtifactCardData>();
            art6.SetupArtifact("ART_06", "Bakunawa Lunar Pearl", 
                "A luminous pearl said to have been vomited by the giant dragon Bakunawa during an eclipse.",
                EraType.MythLegendEra, EraType.JapaneseEra, 40, "Ancient celestial essence capable of stabilizing temporal rifts.");
            artifactCards.Add(art6);

            // 2. Missions (Assigned to players 1-4)
            var mis1 = ScriptableObject.CreateInstance<MissionCardData>();
            mis1.SetupMission("MIS_01", "Operation: Monstrance Sanctuary", 
                "Locate the Golden Monstrance displaced in the American Era and return it to the Spanish Era altar in Intramuros.",
                EraType.SpanishEra, "ART_01", "Golden Monstrance of Intramuros", EraType.SpanishEra, 2, 50,
                "Restoring this sacred vessel will seal the ecclesiastical time vortex.");
            missionCards.Add(mis1);

            var mis2 = ScriptableObject.CreateInstance<MissionCardData>();
            mis2.SetupMission("MIS_02", "Operation: Voice of Freedom", 
                "Find the Corregidor Ham Radio Transceiver from wartime Japanese occupation and deliver it to the American Era base.",
                EraType.AmericanEra, "ART_02", "Corregidor Ham Radio Transceiver", EraType.AmericanEra, 2, 50,
                "The airwaves must remain unsevered to protect the timeline sequence.");
            missionCards.Add(mis2);

            var mis3 = ScriptableObject.CreateInstance<MissionCardData>();
            mis3.SetupMission("MIS_03", "Operation: Shadow Cipher", 
                "Retrieve the Guerrilla Intelligence Cipher Book from colonial records and deliver it to the Japanese Era resistance.",
                EraType.JapaneseEra, "ART_03", "Guerrilla Intelligence Cipher Book", EraType.JapaneseEra, 2, 50,
                "Without the cipher, the underground timeline collapses into an endless patrol loop.");
            missionCards.Add(mis3);

            var mis4 = ScriptableObject.CreateInstance<MissionCardData>();
            mis4.SetupMission("MIS_04", "Operation: Agusan Awakening", 
                "Uncover the Golden Tara of Agusan in modern displays and return it to the sacred Myth & Legend Era sanctuary.",
                EraType.MythLegendEra, "ART_04", "Golden Tara of Agusan", EraType.MythLegendEra, 2, 55,
                "Bathala's blessing can only hold if the ancient gold returns to the riverbed.");
            missionCards.Add(mis4);

            // 3. Clues
            CreateClue("CLUE_01", "Gilded Ornaments", "Crafted with ornate baroque filigree with a sunburst halo.", EraType.SpanishEra, "ART_01", ClueCategory.Appearance);
            CreateClue("CLUE_02", "Church Inventory Records", "Archival log mentions an Intramuros monstrance vanished during storm.", EraType.SpanishEra, "ART_01", ClueCategory.Location);
            CreateClue("CLUE_03", "Shortwave Frequency Dial", "Operates on 6.2 Megahertz with military grade crystal resonance.", EraType.AmericanEra, "ART_02", ClueCategory.Purpose);
            CreateClue("CLUE_04", "Corregidor Tunnel Map", "Blueprints marking a hidden telegraph depot near Malinta Tunnel.", EraType.AmericanEra, "ART_02", ClueCategory.Location);
            CreateClue("CLUE_05", "Invisible Ink Pigment", "Formulated from kalamansi juice and tea extract to evade censors.", EraType.JapaneseEra, "ART_03", ClueCategory.Appearance);
            CreateClue("CLUE_06", "Hukbalahap Signal Codes", "Cryptic messages detailing night courier crossings.", EraType.JapaneseEra, "ART_03", ClueCategory.HistoricalConnection);
            CreateClue("CLUE_07", "21-Karat Deity Silhouette", "A seated female figure in meditation adorned with royal ear ornaments.", EraType.MythLegendEra, "ART_04", ClueCategory.Appearance);
            CreateClue("CLUE_08", "Wawa River Prophecy", "Ancient chant foretelling gold that walks among future strangers.", EraType.MythLegendEra, "ART_04", ClueCategory.Purpose);
            CreateClue("CLUE_09", "Pigafetta's Lost Notes", "Fragments from the first circumnavigators describing glowing idols.", EraType.SpanishEra, "ART_04", ClueCategory.HistoricalConnection);
            CreateClue("CLUE_10", "Galleon Bill of Lading", "Manifest listing stolen relics rerouted to Acapulco.", EraType.SpanishEra, "ART_01", ClueCategory.Location);
            CreateClue("CLUE_11", "MacArthur's Dispatch", "Decrypted telegram requesting secure transceiver repair.", EraType.AmericanEra, "ART_02", ClueCategory.HistoricalConnection);
            CreateClue("CLUE_12", "Bamboo Courier Tube", "Cylindrical bamboo case preserving dry micro-documents.", EraType.JapaneseEra, "ART_03", ClueCategory.Appearance);

            // 4. Events
            CreateEvent("EVT_01", "Galleon Trade Fair", "A grand trade convoy arrives at the bay. Gain +3 Energy and +10 Points.", EraType.SpanishEra, 3, 0, 0, 1, 10);
            CreateEvent("EVT_02", "Telegraph Synchrony", "Coordinated temporal signals stabilize the shared timeline (-1 Timeline Destabilization).", EraType.AmericanEra, 1, -1, 0, 0, 15);
            CreateEvent("EVT_03", "Bayanihan Volunteer Spirit", "Fellow temporal travellers share provisions. Gain +2 Energy.", EraType.JapaneseEra, 2, 0, 0, 0, 10);
            CreateEvent("EVT_04", "Bathala's Blessing", "The celestial heavens align! Timeline stabilizes (-2 Timeline Meter) and free Warp granted.", EraType.MythLegendEra, 2, -2, -1, 0, 20, true);
            CreateEvent("EVT_05", "Solar Resonance Shift", "Chrono-energy charges your suit. Gain +4 Energy.", EraType.AmericanEra, 4, 0, 0, 0, 5);
            CreateEvent("EVT_06", "Sanctuary Rest", "Safe haven found inside ancient stone walls. Cleanse -2 Paradox.", EraType.SpanishEra, 1, 0, -2, 0, 10);
            CreateEvent("EVT_07", "Guerrilla Supply Drop", "Secret cache discovered in the jungle. Gain +3 Energy.", EraType.JapaneseEra, 3, 0, 0, 1, 10);
            CreateEvent("EVT_08", "Anito Guidance", "Ancestral spirits illuminate your path. Move forward +2 extra spaces.", EraType.MythLegendEra, 0, 0, 0, 2, 10);

            // 5. Dangers
            CreateDanger("DNG_01", "Inquisition Patrol", "You are detained for carrying strange futuristic technology! Lose 3 Energy and gain +1 Paradox.", EraType.SpanishEra, 3, 1, 1, 10, false, false);
            CreateDanger("DNG_02", "Chronal Rift Rupture", "A violent timeline tear shakes reality! Shared Timeline Meter increases by +1!", EraType.AmericanEra, 2, 1, 1, 5, false, false);
            CreateDanger("DNG_03", "Kempeitai Checkpoint", "Intense inspection forces an emergency retreat! Relocated to Timeline Start tile.", EraType.JapaneseEra, 2, 1, 1, 5, true, false);
            CreateDanger("DNG_04", "Wrath of Bakunawa", "The mythical moon-eater thrashes through the void! Lose 4 Energy and skip your next turn!", EraType.MythLegendEra, 4, 1, 2, 15, false, true);
            CreateDanger("DNG_05", "Temporal Feedback Loop", "Conflicting actions trigger an echo feedback! Gain +2 Paradox.", EraType.SpanishEra, 2, 2, 1, 10, false, false);
            CreateDanger("DNG_06", "Blackout Sabotage", "Grid power failure drains suit batteries. Lose 3 Energy.", EraType.JapaneseEra, 3, 0, 1, 5, false, false);
        }

        private void CreateClue(string id, string title, string desc, EraType era, string artifactId, ClueCategory cat)
        {
            var clue = ScriptableObject.CreateInstance<ClueCardData>();
            clue.SetupClue(id, title, desc, era, artifactId, cat, desc);
            clueCards.Add(clue);
        }

        private void CreateEvent(string id, string title, string desc, EraType era, int energy, int timeline, int paradox, int move, int pts, bool warp = false)
        {
            var evt = ScriptableObject.CreateInstance<EventCardData>();
            evt.SetupEvent(id, title, desc, era, energy, timeline, paradox, move, pts, warp);
            eventCards.Add(evt);
        }

        private void CreateDanger(string id, string title, string desc, EraType era, int energy, int paradox, int timeline, int pts, bool relocate, bool skipTurn)
        {
            var dng = ScriptableObject.CreateInstance<DangerCardData>();
            dng.SetupDanger(id, title, desc, era, energy, paradox, timeline, pts, relocate, skipTurn);
            dangerCards.Add(dng);
        }
    }
}
