#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Panahon.Editor
{
    public class PanahonSetupWindow : EditorWindow
    {
        [MenuItem("Window/P.A.N.A.H.O.N./Setup & Control Panel")]
        [MenuItem("Tools/P.A.N.A.H.O.N./Setup Control Panel", false, 0)]
        public static void OpenWindow()
        {
            var window = GetWindow<PanahonSetupWindow>("P.A.N.A.H.O.N. Hub");
            window.minSize = new Vector2(400, 480);
            window.Show();
        }

        private void OnGUI()
        {
            GUILayout.Space(10);
            GUIStyle headerStyle = new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 18,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.LabelField("P.A.N.A.H.O.N.", headerStyle);
            EditorGUILayout.LabelField("2.5D Cooperative & Competitive Tabletop Board Game", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(15);

            EditorGUILayout.HelpBox(
                "Welcome to P.A.N.A.H.O.N. for Unity (2.5D Tabletop)!\n\n" +
                "• 2.5D Orthographic Tabletop Board Layout\n" +
                "• 4 Players: Temporal Castaways with unique skills\n" +
                "• 4 Timelines: Spanish, American, Japanese, Myth & Legend\n" +
                "• Shared Timeline Stability Meter (Cooperative Loss on Collapse)\n" +
                "• Individual Paradox & Energy management\n" +
                "• Forward-only movement & strategic Time Gate jumping",
                MessageType.Info);

            GUILayout.Space(15);

            GUI.backgroundColor = new Color(0.2f, 0.7f, 0.35f);
            if (GUILayout.Button("GENERATE 2.5D PLAYABLE TABLETOP SCENE", GUILayout.Height(45)))
            {
                PanahonSceneBuilder.GenerateCompleteScene();
            }
            GUI.backgroundColor = Color.white;

            GUILayout.Space(10);
            EditorGUILayout.LabelField("Quick Utilities", EditorStyles.boldLabel);

            if (GUILayout.Button("Repopulate Historical Card Database"))
            {
                var db = FindFirstObjectByType<Cards.CardDatabase>();
                if (db != null)
                {
                    db.PopulateDefaultDatabase();
                    EditorUtility.SetDirty(db);
                    Debug.Log("[PANAHON] Card database repopulated with Spanish, American, Japanese, and Myth Era cards!");
                }
                else
                {
                    Debug.LogWarning("[PANAHON] No CardDatabase found in current scene. Click 'GENERATE COMPLETE PLAYABLE SCENE' first.");
                }
            }

            if (GUILayout.Button("Focus Main Camera on Game Board"))
            {
                Camera cam = Camera.main;
                if (cam != null)
                {
                    cam.transform.position = new Vector3(0, 32f, -24f);
                    cam.transform.rotation = Quaternion.Euler(56f, 0, 0);
                    Selection.activeGameObject = cam.gameObject;
                }
            }

            GUILayout.FlexibleSpace();
            EditorGUILayout.LabelField("Base Game Framework v1.0.0 - Ready to import into Unity", EditorStyles.centeredGreyMiniLabel);
            GUILayout.Space(5);
        }
    }
}
#endif
