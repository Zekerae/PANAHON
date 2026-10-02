#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using Panahon.Networking;

namespace Panahon.Editor
{
    public class PanahonSetupWindow : EditorWindow
    {
        [MenuItem("Window/P.A.N.A.H.O.N./Setup & Control Panel")]
        [MenuItem("Tools/P.A.N.A.H.O.N./Setup Control Panel", false, 10)]
        public static void OpenWindow()
        {
            var window = GetWindow<PanahonSetupWindow>("P.A.N.A.H.O.N. Hub");
            window.minSize = new Vector2(460, 680);
            window.Show();
        }

        private Vector2 scrollPos;
        private string inputAppId = "";
        private int selectedRegionIndex = 0;
        private readonly string[] regionOptions = { "asia", "us", "eu", "jp", "kr" };
        private FusionMode selectedMode = FusionMode.Shared;

        private void OnEnable()
        {
            var config = Resources.Load<PhotonFusionConfig>("PhotonFusionConfig");
            if (config != null)
            {
                inputAppId = config.FusionAppId;
                for (int i = 0; i < regionOptions.Length; i++)
                {
                    if (string.Equals(config.Region, regionOptions[i], System.StringComparison.OrdinalIgnoreCase))
                    {
                        selectedRegionIndex = i;
                        break;
                    }
                }
            }
        }

        private void OnGUI()
        {
            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
            GUILayout.Space(10);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 20,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField("P.A.N.A.H.O.N.", headerStyle);
            EditorGUILayout.LabelField("2.5D Multiplayer Tabletop Time-Travel Game", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(10);

            // ==========================================
            // Main Generation Button
            // ==========================================
            GUI.backgroundColor = new Color(0.2f, 0.75f, 0.4f);
            if (GUILayout.Button("GENERATE COMPLETE 2.5D MULTIPLAYER SCENE\n(Main Menu + Lobby + Settings + Board Game)", GUILayout.Height(50)))
            {
                SavePhotonSettings();
                PanahonSceneBuilder.GenerateCompleteScene();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(15);

            // ==========================================
            // Photon Fusion Server Configuration
            // ==========================================
            EditorGUILayout.LabelField("Photon Fusion Cloud Server", EditorStyles.boldLabel);
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            EditorGUILayout.LabelField("Connect your Photon Fusion App for 4-player online multiplayer:", EditorStyles.wordWrappedMiniLabel);
            GUILayout.Space(5);

            inputAppId = EditorGUILayout.TextField("Fusion App ID", inputAppId);
            selectedRegionIndex = EditorGUILayout.Popup("Photon Region", selectedRegionIndex, regionOptions);
            selectedMode = (FusionMode)EditorGUILayout.EnumPopup("Fusion Network Mode", selectedMode);

            GUILayout.Space(5);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Save Photon Config Asset"))
            {
                SavePhotonSettings();
            }
            if (GUILayout.Button("Open Photon Dashboard", GUILayout.Width(160)))
            {
                Application.OpenURL("https://dashboard.photonengine.com/en-US/PublicCloud");
            }
            EditorGUILayout.EndHorizontal();

            if (!string.IsNullOrEmpty(inputAppId))
            {
                EditorGUILayout.HelpBox($"Ready! Fusion App ID: {(inputAppId.Length > 8 ? inputAppId.Substring(0, 8) + "..." : inputAppId)} on region [{regionOptions[selectedRegionIndex]}]", MessageType.Info);
            }
            else
            {
                EditorGUILayout.HelpBox("No App ID specified yet. Game will automatically run in Offline / Hotseat Tabletop mode.", MessageType.Warning);
            }

            EditorGUILayout.EndVertical();

            GUILayout.Space(15);

            // ==========================================
            // Quick Scene & Asset Utilities
            // ==========================================
            EditorGUILayout.LabelField("Quick Utilities", EditorStyles.boldLabel);

            if (GUILayout.Button("Repopulate Historical Card Database"))
            {
                var db = FindAnyObjectByType<Cards.CardDatabase>();
                if (db != null)
                {
                    db.PopulateDefaultDatabase();
                    EditorUtility.SetDirty(db);
                    Debug.Log("[PANAHON] Card database repopulated with Spanish, American, Japanese, and Myth Era cards!");
                }
                else
                {
                    Debug.LogWarning("[PANAHON] No CardDatabase found in current scene. Click 'GENERATE' first.");
                }
            }

            if (GUILayout.Button("Focus Main Camera on Game Board"))
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    cam.orthographic = true;
                    cam.orthographicSize = 13.5f;
                    cam.transform.position = new Vector3(0, 22f, -14f);
                    cam.transform.rotation = Quaternion.Euler(58f, 0, 0);
                    Selection.activeGameObject = cam.gameObject;
                    SceneView.lastActiveSceneView?.FrameSelected();
                }
            }

            if (GUILayout.Button("Create / Refresh PanahonGameConfig Asset"))
            {
                var config = ScriptableObject.CreateInstance<Data.GameConfig>();
                string path = "Assets/PANAHON/Resources/PanahonGameConfig.asset";
                System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(System.IO.Path.Combine(Application.dataPath, "../", path)));
                AssetDatabase.CreateAsset(config, path);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
                EditorGUIUtility.PingObject(config);
                Debug.Log("[PANAHON] GameConfig asset created at " + path);
            }

            GUILayout.Space(10);
            EditorGUILayout.LabelField("4 Historical Eras & Lore", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "• Spanish Era (1521-1898): Galleons, Intramuros, Colonial relics\n" +
                "• American Era (1898-1946): Commonwealth, Modern telegraphs, Industry\n" +
                "• Japanese Era (1942-1945): Guerrilla resistance, Wartime intelligence\n" +
                "• Myth & Legend Era: Pre-Colonial ancient lore, Bathala, Bakunawa\n\n" +
                "Win Condition: All 4 players complete missions before time expires.\n" +
                "Lose Condition: Shared Timeline Meter reaches maximum (10/10 Collapse).",
                MessageType.None);

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("P.A.N.A.H.O.N. 2.5D Multiplayer Tabletop v2.1.0", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(5);
            EditorGUILayout.EndScrollView();
        }

        private void SavePhotonSettings()
        {
            string resourcesDir = "Assets/PANAHON/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesDir))
            {
                AssetDatabase.CreateFolder("Assets/PANAHON", "Resources");
            }

            string assetPath = $"{resourcesDir}/PhotonFusionConfig.asset";
            var config = AssetDatabase.LoadAssetAtPath<PhotonFusionConfig>(assetPath);
            if (config == null)
            {
                config = ScriptableObject.CreateInstance<PhotonFusionConfig>();
                AssetDatabase.CreateAsset(config, assetPath);
            }

            config.FusionAppId = inputAppId.Trim();
            config.Region = regionOptions[selectedRegionIndex];

            EditorUtility.SetDirty(config);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log($"[PANAHON] Saved Photon Fusion Config: AppID={(string.IsNullOrEmpty(config.FusionAppId) ? "(None/Offline)" : config.FusionAppId.Substring(0, Mathf.Min(8, config.FusionAppId.Length)) + "...")} on Region {config.Region}");
        }
    }
}
#endif
