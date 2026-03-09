#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Shogun.Features.Characters
{
    public class CharacterDatabaseEditorWindow : EditorWindow
    {
        private string _characterId = "";
        private string _givenName = "";
        private string _surname = "";
        private Vector2 _scrollPosition;
        private string _lastValidationReport = "";

        [MenuItem("Tools/Shogun/Character Database")]
        public static void ShowWindow()
        {
            GetWindow<CharacterDatabaseEditorWindow>("Character Database");
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Character Database", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox("Use this window to scaffold character folders/assets, rebuild the runtime catalog, validate production asset placement, and enforce crisp sprite import quality.", MessageType.Info);

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("New Character Scaffold", EditorStyles.boldLabel);
            _characterId = EditorGUILayout.TextField("Character ID", _characterId);
            _givenName = EditorGUILayout.TextField("Given Name", _givenName);
            _surname = EditorGUILayout.TextField("Surname", _surname);

            if (GUILayout.Button("Create Character Scaffold"))
            {
                CharacterDefinition definition = CharacterCatalogEditorUtility.CreateCharacterScaffold(_characterId, _givenName, _surname);
                if (definition != null)
                {
                    Selection.activeObject = definition;
                    EditorGUIUtility.PingObject(definition);
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Catalog", EditorStyles.boldLabel);
            if (GUILayout.Button("Rebuild Character Catalog"))
            {
                CharacterCatalogEditorUtility.RebuildCatalog();
            }

            if (GUILayout.Button("Validate Character Database"))
            {
                CharacterCatalogEditorUtility.ValidationReport report = CharacterCatalogEditorUtility.ValidateDatabase();
                _lastValidationReport = string.Join("\n", report.Messages);
                if (string.IsNullOrWhiteSpace(_lastValidationReport))
                    _lastValidationReport = "No validation issues found.";
            }

            if (GUILayout.Button("Fix Production Sprite Import Settings"))
            {
                CharacterCatalogEditorUtility.NormalizeProductionSpriteImportSettings();
            }

            if (!string.IsNullOrWhiteSpace(_lastValidationReport))
            {
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Validation Report", EditorStyles.boldLabel);
                _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition, GUILayout.Height(240));
                EditorGUILayout.TextArea(_lastValidationReport, GUILayout.ExpandHeight(true));
                EditorGUILayout.EndScrollView();
            }
        }
    }
}
#endif
