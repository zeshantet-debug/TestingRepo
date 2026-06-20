#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace GamesArena.Essentials.Symbols
{
    [InitializeOnLoad]
    public class SymbolManagerWindow : EditorWindow
    {
        private static List<SymbolData> symbolMappings;
        private static string jsonFilePath;

        static SymbolManagerWindow()
        {
            EditorApplication.update += ShowOnFirstProjectOpen;

            // Register custom build handler
            BuildPlayerWindow.RegisterBuildPlayerHandler(BuildPlayerWithSymbolManagerCheck);
        }

        [MenuItem("Games Arena/Symbol Manager")]
        public static void ShowWindow()
        {
            GetWindow<SymbolManagerWindow>("Symbol Manager");
        }

        private static void ShowOnFirstProjectOpen()
        {
            if (!SessionState.GetBool("SymbolManagerWindowOpened", false))
            {
                ShowWindow();
                SessionState.SetBool("SymbolManagerWindowOpened", true);
                EditorApplication.update -= ShowOnFirstProjectOpen;
            }
        }

        private static void BuildPlayerWithSymbolManagerCheck(BuildPlayerOptions options)
        {
            // Show the Symbol Manager window before building
            ShowWindow();

            // Wait for user input
            bool proceed = EditorUtility.DisplayDialog(
                "Symbol Manager",
                "Ensure all required symbols are set before proceeding with the build.",
                "Proceed with Build",
                "Cancel Build"
            );

            if (proceed)
            {
                // Continue with the build
                BuildPipeline.BuildPlayer(options);
            }
            else
            {
                Debug.Log("Build cancelled by the user.");
            }
        }

        private void OnEnable()
        {
            LoadSymbolMappings();
        }

        private void OnGUI()
        {
            EditorGUILayout.LabelField("Symbol Manager", EditorStyles.boldLabel);

            if (string.IsNullOrEmpty(jsonFilePath))
            {
                EditorGUILayout.HelpBox("dependencies.json file not found!", MessageType.Error);
            }
            else
            {
                EditorGUILayout.LabelField("Dependencies File Path", jsonFilePath, EditorStyles.wordWrappedLabel);
            }

            EditorGUILayout.Space();

            if (symbolMappings != null)
            {
                foreach (var symbol in symbolMappings)
                {
                    bool isDefined = IsSymbolDefined(symbol.symbolName);
                    bool newIsDefined = EditorGUILayout.Toggle(symbol.displayName, isDefined);
                    if (newIsDefined != isDefined)
                    {
                        SetSymbol(symbol.symbolName, newIsDefined);
                    }
                }
            }
            else
            {
                EditorGUILayout.HelpBox("No symbols loaded. Ensure dependencies.json is correct.", MessageType.Warning);
            }
        }

        private void LoadSymbolMappings()
        {
            MonoScript script = MonoScript.FromScriptableObject(this);
            string scriptPath = AssetDatabase.GetAssetPath(script);
            string directoryPath = Path.GetDirectoryName(scriptPath);
            jsonFilePath = Path.Combine(directoryPath, "dependencies.json");

            if (File.Exists(jsonFilePath))
            {
                string jsonContent = File.ReadAllText(jsonFilePath);
                var data = JsonUtility.FromJson<SymbolList>(jsonContent);
                symbolMappings = data.symbols;
            }
            else
            {
                Debug.LogError($"dependencies.json file not found at {jsonFilePath}");
                symbolMappings = new List<SymbolData>();
            }
        }

        public static bool IsSymbolDefined(string symbol)
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            return currentSymbols.Contains(symbol);
        }

        public static void SetSymbol(string symbol, bool add)
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string currentSymbols = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            if (add && !currentSymbols.Contains(symbol))
            {
                currentSymbols = string.IsNullOrEmpty(currentSymbols) ? symbol : $"{currentSymbols};{symbol}";
            }
            else if (!add && currentSymbols.Contains(symbol))
            {
                currentSymbols = currentSymbols.Replace(symbol, "").Replace(";;", ";").Trim(';');
            }

            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, currentSymbols);
            Debug.Log($"{(add ? "Added" : "Removed")} define symbol: {symbol}");
        }
    }

    [System.Serializable]
    public class SymbolData
    {
        public string displayName;
        public string symbolName;
    }

    [System.Serializable]
    public class SymbolList
    {
        public List<SymbolData> symbols;
    }
}
#endif
