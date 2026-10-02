using System;
using UnityEngine;

namespace Panahon.Networking
{
    public enum FusionMode
    {
        Shared,            // Client-authoritative with Photon cloud relay
        Host,              // First player hosts simulation
        Server,            // Dedicated server
        OfflineSimulation  // Local multiplayer / Hotseat / Offline testing
    }

    [CreateAssetMenu(fileName = "PhotonFusionConfig", menuName = "PANAHON/Networking/Photon Fusion Config")]
    public class PhotonFusionConfig : ScriptableObject
    {
        private const string APP_ID_PREF_KEY = "Panahon_Photon_AppId";
        private const string REGION_PREF_KEY = "Panahon_Photon_Region";

        [Header("Photon Fusion Server Credentials")]
        [Tooltip("Paste your Photon Fusion App ID from your Photon Dashboard here.")]
        [SerializeField] private string fusionAppId = "";

        [Tooltip("Target Photon Cloud Region (e.g., asia, us, eu, jp, kr).")]
        [SerializeField] private string region = "asia";

        [Tooltip("Application Game Version for matchmaking compatibility.")]
        [SerializeField] private string gameVersion = "1.0.0";

        [Header("Room & Session Settings")]
        [SerializeField] private FusionMode fusionMode = FusionMode.Shared;
        [SerializeField] private int maxPlayers = 4;
        [SerializeField] private string defaultRoomPrefix = "PANAHON-";
        [SerializeField] private int connectionTimeoutSeconds = 15;

        public string FusionAppId
        {
            get
            {
                // Prioritize runtime playerprefs override if entered via in-game settings
                string saved = PlayerPrefs.GetString(APP_ID_PREF_KEY, string.Empty);
                return !string.IsNullOrEmpty(saved) ? saved : fusionAppId;
            }
            set
            {
                fusionAppId = value;
                PlayerPrefs.SetString(APP_ID_PREF_KEY, value);
                PlayerPrefs.Save();
            }
        }

        public string Region
        {
            get => PlayerPrefs.GetString(REGION_PREF_KEY, region);
            set
            {
                region = value;
                PlayerPrefs.SetString(REGION_PREF_KEY, value);
                PlayerPrefs.Save();
            }
        }

        public string GameVersion => gameVersion;
        public FusionMode FusionMode => fusionMode;
        public int MaxPlayers => maxPlayers;
        public string DefaultRoomPrefix => defaultRoomPrefix;
        public int ConnectionTimeoutSeconds => connectionTimeoutSeconds;

        public bool HasValidAppId => !string.IsNullOrWhiteSpace(FusionAppId) && FusionAppId.Length >= 8;

        public void ResetAppIdOverride()
        {
            PlayerPrefs.DeleteKey(APP_ID_PREF_KEY);
            PlayerPrefs.Save();
        }
    }
}
