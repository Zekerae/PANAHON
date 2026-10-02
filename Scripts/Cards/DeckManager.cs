using System.Collections.Generic;
using UnityEngine;
using Panahon.Data;

namespace Panahon.Cards
{
    public class DeckManager : MonoBehaviour
    {
        public static DeckManager Instance { get; private set; }

        private Queue<MissionCardData> missionDeck = new Queue<MissionCardData>();
        private List<ClueCardData> clueDeck = new List<ClueCardData>();
        private List<ArtifactCardData> artifactDeck = new List<ArtifactCardData>();
        private List<EventCardData> eventDeck = new List<EventCardData>();
        private List<DangerCardData> dangerDeck = new List<DangerCardData>();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        public void InitializeDecks()
        {
            CardDatabase db = CardDatabase.Instance;
            if (db == null) return;

            // Setup Missions
            List<MissionCardData> missions = new List<MissionCardData>(db.MissionCards);
            ShuffleList(missions);
            missionDeck = new Queue<MissionCardData>(missions);

            // Setup Clues
            clueDeck = new List<ClueCardData>(db.ClueCards);
            ShuffleList(clueDeck);

            // Setup Artifacts
            artifactDeck = new List<ArtifactCardData>(db.ArtifactCards);
            ShuffleList(artifactDeck);

            // Setup Events
            eventDeck = new List<EventCardData>(db.EventCards);
            ShuffleList(eventDeck);

            // Setup Dangers
            dangerDeck = new List<DangerCardData>(db.DangerCards);
            ShuffleList(dangerDeck);
        }

        public MissionCardData DrawMission()
        {
            if (missionDeck.Count > 0)
            {
                return missionDeck.Dequeue();
            }
            return null;
        }

        public ClueCardData DrawClue(EraType? matchingEra = null)
        {
            if (clueDeck.Count == 0 && CardDatabase.Instance != null)
            {
                clueDeck = new List<ClueCardData>(CardDatabase.Instance.ClueCards);
                ShuffleList(clueDeck);
            }

            if (matchingEra.HasValue)
            {
                int matchIdx = clueDeck.FindIndex(c => c.Era == matchingEra.Value);
                if (matchIdx >= 0)
                {
                    var card = clueDeck[matchIdx];
                    clueDeck.RemoveAt(matchIdx);
                    return card;
                }
            }

            if (clueDeck.Count > 0)
            {
                var card = clueDeck[0];
                clueDeck.RemoveAt(0);
                return card;
            }

            return null;
        }

        public ArtifactCardData GetArtifactForEra(EraType era)
        {
            return artifactDeck.Find(a => a.DisplacedFoundEra == era);
        }

        public ArtifactCardData GetArtifactById(string id)
        {
            return artifactDeck.Find(a => a.CardId == id);
        }

        public EventCardData DrawEvent(EraType? matchingEra = null)
        {
            if (eventDeck.Count == 0 && CardDatabase.Instance != null)
            {
                eventDeck = new List<EventCardData>(CardDatabase.Instance.EventCards);
                ShuffleList(eventDeck);
            }

            if (matchingEra.HasValue)
            {
                int matchIdx = eventDeck.FindIndex(e => e.Era == matchingEra.Value);
                if (matchIdx >= 0)
                {
                    var card = eventDeck[matchIdx];
                    eventDeck.RemoveAt(matchIdx);
                    return card;
                }
            }

            if (eventDeck.Count > 0)
            {
                var card = eventDeck[0];
                eventDeck.RemoveAt(0);
                return card;
            }

            return null;
        }

        public DangerCardData DrawDanger(EraType? matchingEra = null)
        {
            if (dangerDeck.Count == 0 && CardDatabase.Instance != null)
            {
                dangerDeck = new List<DangerCardData>(CardDatabase.Instance.DangerCards);
                ShuffleList(dangerDeck);
            }

            if (matchingEra.HasValue)
            {
                int matchIdx = dangerDeck.FindIndex(d => d.Era == matchingEra.Value);
                if (matchIdx >= 0)
                {
                    var card = dangerDeck[matchIdx];
                    dangerDeck.RemoveAt(matchIdx);
                    return card;
                }
            }

            if (dangerDeck.Count > 0)
            {
                var card = dangerDeck[0];
                dangerDeck.RemoveAt(0);
                return card;
            }

            return null;
        }

        private void ShuffleList<T>(List<T> list)
        {
            for (int i = 0; i < list.Count; i++)
            {
                int rnd = Random.Range(i, list.Count);
                T temp = list[i];
                list[i] = list[rnd];
                list[rnd] = temp;
            }
        }
    }
}
