using GamesArena.Essentials.Symbols;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEditor.VersionControl;
using UnityEngine;
using UnityEngine.WSA;
using static GamesArenaWindow;
using static UnityEditor.Progress;

public class GamesArenaWindow : EditorWindow
{
    private static List<IntegrationName> allIntegrationItems;
    private static List<ItemData> connectionPackageList;
    private static List<ItemData> monetizationSDKList;
    private static List<ItemData> monetizationPackageList;
    private static List<ItemData> firebaseSDKList;
    private static List<ItemData> firebasePackageList;
    private static List<ItemData> mmpSDKList;
    private static List<ItemData> googleServicesSDKList;
    private static List<ItemData> unityServicesSDKList;
    private static List<ItemData> mmpPackageList;
    private static List<ItemData> googleServicesPackageList;
    private static List<ItemData> unityServicesPackageList;
    private static List<ItemData> revenueCatPackageList;
    private static List<ItemData> revenueCatSDKList;
    private static List<ItemData> essentialsPackageList;
    private static List<ItemData> oneSignalPackageList;
    private static List<ItemData> oneSignalSDKList;
    private static List<ItemData> debugConsoleList;
    private static List<ItemData> debugConsoleSDKList;
    private static List<PermissionData> permissionsList;
    private static string jsonFilePath = "Assets/Games Arena Studio/_Games Arena Editor/dependencies.json";
    private static GamesArenaWindow window;
    private Vector2 scrollPosition;
    private GUIStyle titleStyle;
    private int selectedSdkIndex = 0;
    private Texture2D logoTexture;

    [MenuItem("Games Arena/Main %g")]
    public static void Main()
    {
        window = GetWindow<GamesArenaWindow>("Games Arena");
        window.minSize = new Vector2(750, 860);
        window.maxSize = new Vector2(750, 1000);
        window.Show();
    }

    private void OnEnable()
    {
        LoadDependencies();

        Font customFont = AssetDatabase.LoadAssetAtPath<Font>("Assets/Games Arena Studio/_Games Arena Editor/Fonts/Poppins-SemiBold.ttf");

        titleStyle = new GUIStyle();
        titleStyle.font = customFont;
        titleStyle.fontSize = 20;
        titleStyle.normal.textColor = new Color(37f / 255f, 150f / 255f, 190f / 255f); // #Cyan in RGB
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.margin = new RectOffset(0, 0, 20, 20);
        logoTexture = AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Games Arena Studio/_Games Arena Editor/Textures/1.jpg");

        PackageCache.RefreshCache(() => { Debug.Log("Packages Loaded"); });
    }

    public void OnGUI()
    {

        if (!File.Exists(jsonFilePath))
        {
            EditorGUILayout.HelpBox("Dependencies file is missing. Please ensure the JSON file exists.", MessageType.Error);
            return;
        }

        ImprotLogo();

        //  RenderSdkDropdown();

        EditorGUILayout.BeginHorizontal();

        // ✅ LEFT PANEL (Sidebar)
        DrawLeftPanel();

        // ✅ RIGHT PANEL (Content)
        DrawRightPanel();

        EditorGUILayout.EndHorizontal();
    }

    void DrawSidebarButton(string label, int index)
    {
        Color originalColor = GUI.backgroundColor;

        // Highlight if selected
        if (selectedSdkIndex == index)
            GUI.backgroundColor = new Color(0.3f, 0.7f, 1f); // blue highlight
        else
            GUI.backgroundColor = Color.white;

        if (GUILayout.Button(label, GUILayout.Height(40)))
        {
            selectedSdkIndex = index;
        }

        GUI.backgroundColor = originalColor; // reset
    }

    void DrawLeftPanel()
    {
        EditorGUILayout.BeginVertical(GUILayout.Width(200));

        GUILayout.Space(10);

        DrawSidebarButton("Connection Detector", -1);
        DrawSidebarButton("Monetization", 0);
        DrawSidebarButton("Firebase", 1);
        DrawSidebarButton("MMP", 2);
        DrawSidebarButton("Google Services", 3);
        DrawSidebarButton("Unity Services", 4);
        DrawSidebarButton("Revenue Cat", 5);
        DrawSidebarButton("One Signal", 6);
        DrawSidebarButton("Debug Console", 7);
        DrawSidebarButton("Permissions", 8);

        GUILayout.FlexibleSpace();

        EditorGUILayout.EndVertical();
    }

    void DrawRightPanel()
    {
        EditorGUILayout.BeginVertical();

        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

        if (selectedSdkIndex == -1)
        {

            ImportItem("Connection Detector", connectionPackageList);
        }
        else if (selectedSdkIndex == 0)
        {
            DownloadItem("MONETIZATION SDKs", monetizationSDKList);
            ImportItem("MONETIZATION PACKAGES", monetizationPackageList);
        }
        else if (selectedSdkIndex == 1)
        {
            DownloadItem("FIREBASE SDKs", firebaseSDKList);
            ImportItem("FIREBASE PACKAGES", firebasePackageList);
        }
        else if (selectedSdkIndex == 2)
        {
            DownloadItem("MMP SDKs", mmpSDKList);
            ImportItem("MMP PACKAGES", mmpPackageList);
        }
        else if (selectedSdkIndex == 3)
        {
            DownloadItem("GOOGLE SERVICES SDKs", googleServicesSDKList);
            ImportItem("GOOGLE SERVICES PACKAGES", googleServicesPackageList);
        }
        else if (selectedSdkIndex == 4)
        {
            DownloadItem("UNITY SERVICES SDKs", unityServicesSDKList);
            ImportItem("UNITY SERVICES PACKAGES", unityServicesPackageList);
        }
        else if (selectedSdkIndex == 5)
        {
            DownloadItem("Revenue Cat SDK", revenueCatSDKList);
            ImportItem("Revenue Cat Package", revenueCatPackageList);
        }
        else if (selectedSdkIndex == 6)
        {
            DownloadItem("One Signal SDK", oneSignalSDKList);
            ImportItem("One Signal Package", oneSignalPackageList);
        }
        else if (selectedSdkIndex == 7)
        {
            DownloadItem("Debug Console SDK", debugConsoleSDKList);
            ImportItem("Debug Console Package", debugConsoleList);
        }
        else if (selectedSdkIndex == 8)
        {
            PermissionItem("Permissions", permissionsList);
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void RenderSdkDropdown()
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.LabelField("INTEGRATION TOOL", titleStyle);
        EditorGUILayout.Space();

        List<string> itemNames = new List<string>();

        foreach (var item in allIntegrationItems)
        {
            itemNames.Add(item.integrationName);
        }

        selectedSdkIndex = EditorGUILayout.Popup("Select Integration", selectedSdkIndex, itemNames.ToArray());

        EditorGUILayout.EndVertical();
    }

    private void PermissionItem(string LabelTitle, List<PermissionData> itemDataList)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(LabelTitle, titleStyle);
        EditorGUILayout.Space();

        //scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));
        if (itemDataList != null)
        {
            foreach (var item in itemDataList)
            {
                EditorGUILayout.BeginHorizontal("box", GUILayout.Height(40));

                GUILayout.Space(10);

                GUILayout.BeginVertical(GUILayout.Width(20));
                GUILayout.FlexibleSpace();


                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();



                // ✅ LEFT SIDE (Label centered vertically)
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField(item.itemName, EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();

                // ✅ Push buttons to right
                GUILayout.FlexibleSpace();

                // ✅ RIGHT SIDE (Buttons centered vertically)
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();


                if (!AndroidManifestEditor.HasPermission(item.permission))
                {
                    if (GUILayout.Button("Import", GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        AndroidManifestEditor.AddPermission(item.permission);
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(1f, 0.65f, 0f);

                    if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        AndroidManifestEditor.RemovePermission(item.permission);
                    }

                    GUI.backgroundColor = Color.white;
                }

                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();

                GUILayout.Space(30);

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No SDK items found. Please check your JSON file.", MessageType.Info);
        }

        //EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }


    #region SDK

    private void DownloadItem(string LabelTitle, List<ItemData> itemDataList)
    {
        RenderItem(LabelTitle, itemDataList);
    }
    private void RenderItem(string LabelTitle, List<ItemData> itemDataList)
    {
        EditorGUILayout.BeginVertical("box");

        EditorGUILayout.Space();
        EditorGUILayout.LabelField(LabelTitle, titleStyle);
        EditorGUILayout.Space();

        //scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));

        if (itemDataList != null)
        {
            foreach (var item in itemDataList)
            {
                EditorGUILayout.BeginHorizontal("box", GUILayout.Height(40));

                GUILayout.Space(10);

                GUILayout.BeginVertical(GUILayout.Width(20));
                GUILayout.FlexibleSpace();

                if (!string.IsNullOrEmpty(item.symbolName))
                {
                    bool isDefined = SymbolManagerWindow.IsSymbolDefined(item.symbolName);
                    bool newIsDefined = EditorGUILayout.Toggle(isDefined);

                    if (newIsDefined != isDefined)
                    {
                        SymbolManagerWindow.SetSymbol(item.symbolName, newIsDefined);
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();



                // ✅ LEFT SIDE (Label centered vertically)
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField(item.itemName, EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();

                // ✅ Push buttons to right
                GUILayout.FlexibleSpace();

                // ✅ RIGHT SIDE (Buttons centered vertically)
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                GUILayout.BeginHorizontal();

                bool isInstalled = IsItemInstalled(item);

                if (!isInstalled)
                {
                    if (GUILayout.Button("Import", GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        ImportSDKItem(item);
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(1f, 0.65f, 0f);

                    if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        RemoveSDKItem(item);
                    }

                    GUI.backgroundColor = Color.white;
                }

                GUILayout.EndHorizontal();

                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();

                GUILayout.Space(30);

                EditorGUILayout.EndHorizontal();
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No SDK items found. Please check your JSON file.", MessageType.Info);
        }

        //EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }
    bool IsItemInstalled(ItemData item)
    {
        switch (item.itemName)
        {
            case "AdMob":
                return CheckFile(item) || CheckFilePluginsFileAdmob();

            case "External Dependency Manager":
                return PackageCache.IsInstalled("com.google.external-dependency-manager");

            case "Firebase App":

                return PackageCache.IsInstalled("com.google.firebase.app");

            case "Analytics":
                return PackageCache.IsInstalled("com.google.firebase.analytics");

            case "Remote Config":
                return PackageCache.IsInstalled("com.google.firebase.remote-config");

            case "Crashlytics":
                return PackageCache.IsInstalled("com.google.firebase.crashlytics");

            case "Messaging":
                return PackageCache.IsInstalled("com.google.firebase.messaging");

            case "Android App Bundle":
                return PackageCache.IsInstalled("com.google.android.appbundle");

            case "Play Common":
                return PackageCache.IsInstalled("com.google.play.common");

            case "Play Core":
                return PackageCache.IsInstalled("com.google.play.core");

            case "Play In App Reviews":
                return PackageCache.IsInstalled("com.google.play.review");

            case "Play In App Update":
                return PackageCache.IsInstalled("com.google.play.appupdate");

            case "Solar Engine":
                return CheckSolarEngine();
            case "In App Purchase":
                return PackageCache.IsInstalled("com.unity.purchasing");

            case "Debug Console":
                return CheckFile(item) && Directory.Exists("Assets/Plugins/IngameDebugConsole");

            case "One Signal":
                return PackageCache.IsInstalled("com.onesignal.unity.android") && PackageCache.IsInstalled("com.onesignal.unity.core") &&
                                     PackageCache.IsInstalled("com.onesignal.unity.ios");

            case "LevelPlay Mediation":
                return CheckLevelPlayMediation();

            default:
                return CheckFile(item);
        }
    }

    void RemoveSDKItem(ItemData item)
    {

        switch (item.itemName)
        {
            case "External Dependency Manager":
                RemovePackage("com.google.external-dependency-manager");
                break;

            case "AdMob":
                RemoveFilePluginsFileAdmob();
                break;

            case "LevelPlay":
                RemovePackage("com.unity.services.levelplay");
                break;
            case "LevelPlay Mediation":
                RemoveLevelPlayMediation();
                break;


            case "Analytics":
                RemovePackage("com.google.firebase.analytics");
                break;
            case "Firebase App":
                RemovePackage("com.google.firebase.app");
                RemoveFirebaseApp();
                break;
            case "Remote Config":
                RemovePackage("com.google.firebase.remote-config");
                RemoveFirebaseAnalytics();
                break;
            case "Crashlytics":
                RemovePackage("com.google.firebase.crashlytics");
                break;
            case "Messaging":
                RemovePackage("com.google.firebase.messaging");
                break;


            case "Android App Bundle":
                RemovePackage("com.google.android.appbundle");
                break;
            case "Play Common":
                RemovePackage("com.google.play.common");
                break;
            case "Play Core":
                RemovePackage("com.google.play.core");
                break;
            case "Play In App Reviews":
                RemovePackage("com.google.play.review");
                break;
            case "Play In App Update":
                RemovePackage("com.google.play.appupdate");
                break;

            case "Solar Engine":
                RemoveSolarEngine();
                break;

            case "In App Purchase":
                RemovePackage("com.unity.purchasing");
                break;

            case "One Signal":

                PackageRemover.RemoveMultiplePackages(
                    new[] { "com.onesignal.unity.android", "com.onesignal.unity.core", "com.onesignal.unity.ios" });

                RemoveOneSignalPluginFile();
                break;

            default:
                RemovePackage(item);
                break;
        }

        RemovePackage(item);
        RemoveItemSymbols(item);
    }
    void ImportSDKItem(ItemData item)
    {
        switch (item.itemName)
        {
            case "External Dependency Manager":
                InstallPackage(item.importUrl);
                break;
            case "AdMob":
                ImportPackage(item);
                break;

            case "LevelPlay":
                InstallPackage("com.unity.services.levelplay");
                break;
            case "Firebase App":
                InstallPackage(item.importUrl);
                break;
            case "Analytics":
                InstallPackage(item.importUrl);
                break;
            case "Crashlytics":
                InstallPackage(item.importUrl);
                break;
            case "Messaging":
                InstallPackage(item.importUrl);
                break;
            case "Remote Config":
                InstallPackage(item.importUrl);
                break;
            case "Android App Bundle":
                InstallPackage(item.importUrl);
                break;
            case "Play Common":
                InstallPackage(item.importUrl);
                break;
            case "Play Core":
                InstallPackage(item.importUrl);
                break;
            case "Play In App Reviews":
                InstallPackage(item.importUrl);
                break;
            case "Play In App Update":
                InstallPackage(item.importUrl);
                break;

            case "In App Purchase":
                InstallPackage("com.unity.purchasing");
                break;



            default:
                ImportPackage(item);
                break;
        }
    }

    private void ImportSDKClick(ItemData item, bool import)
    {
        if (import)
        {
            ImportSDK(item);
        }
        else
        {
            RemoveSDK(item);
        }
    }

    private void ImportSDK(ItemData item)
    {
        string downloadPath = GenerateLink(item.importUrl, item.itemVersion);
        SDKDownloadWindow.OpenWindow(item.itemName, downloadPath);
    }

    private void RemoveSDK(ItemData item)
    {
        string directoryPath = GenerateLink(item.checkDirectory, item.itemVersion);
        if (Directory.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
            Debug.Log($"{item.itemName} removed from {directoryPath}");
        }
        else
        {
            Debug.LogError($"Directory not found: {directoryPath}");
        }
    }

    public static string GenerateLink(string mainString, string replace)
    {
        if (mainString.Contains("{}")) return Regex.Replace(mainString, "{}", replace);
        else return mainString;
    }

    #endregion

    #region Package

    private void ImportItem(string LabelTitle, List<ItemData> itemDataList)
    {
        RenderSDKPackageItem(LabelTitle, itemDataList);
    }

    private void RenderSDKPackageItem(string LabelTitle, List<ItemData> itemDataList)
    {
        EditorGUILayout.BeginVertical("box");
        EditorGUILayout.Space();

        EditorGUILayout.LabelField(LabelTitle, titleStyle);

        EditorGUILayout.Space();

        //scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition, GUILayout.ExpandHeight(true));


        if (itemDataList != null)
        {
            foreach (var item in itemDataList)
            {
                EditorGUILayout.BeginHorizontal("box", GUILayout.Height(40));

                GUILayout.Space(10);

                // ✅ LEFT SIDE (Toggle + Label centered vertically)
                GUILayout.BeginHorizontal();

                GUILayout.BeginVertical(GUILayout.Width(20));
                GUILayout.FlexibleSpace();

                if (!string.IsNullOrEmpty(item.symbolName))
                {
                    bool isDefined = SymbolManagerWindow.IsSymbolDefined(item.symbolName);
                    bool newIsDefined = EditorGUILayout.Toggle(isDefined);

                    if (newIsDefined != isDefined)
                    {
                        SymbolManagerWindow.SetSymbol(item.symbolName, newIsDefined);
                    }
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();




                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                EditorGUILayout.LabelField(item.itemName, EditorStyles.boldLabel);

                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();

                GUILayout.EndHorizontal();

                // ✅ Push right side
                GUILayout.FlexibleSpace();

                // ✅ RIGHT SIDE (Buttons centered vertically)
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();

                bool isInstalled = CheckFile(item);

                if (!isInstalled)
                {
                    if (GUILayout.Button("Import", GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        ImportPackage(item);
                    }
                }
                else
                {
                    GUI.backgroundColor = new Color(1f, 0.65f, 0f);

                    if (GUILayout.Button("Remove", GUILayout.Width(100), GUILayout.Height(30)))
                    {
                        RemovePackage(item);
                        RemoveItemSymbols(item);
                    }

                    GUI.backgroundColor = Color.white;
                }

                GUILayout.FlexibleSpace();

                GUILayout.EndVertical();

                // ✅ RIGHT SIDE (Buttons centered vertically)
                GUILayout.BeginVertical();
                GUILayout.FlexibleSpace();
                AdmobManagerImportButton(item.itemPrefabPath);
                GUILayout.FlexibleSpace();
                GUILayout.EndVertical();

                EditorGUILayout.EndHorizontal();

                // Below row
            }
        }
        else
        {
            EditorGUILayout.HelpBox("No Package items found. Please check your JSON file.", MessageType.Info);
        }

        //EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    private void ImportPackageClick(ItemData item, bool import)
    {
        if (import)
        {
            ImportPackage(item);
        }
        else
        {
            RemovePackage(item);
        }
    }

    private void ImportPackage(ItemData item)
    {
        string packagePath = GenerateLink(item.importUrl, item.itemVersion);
        packagePath = packagePath.Replace("\\", "/");  // Replace backslashes with forward slashes

        if (File.Exists(packagePath))
        {
            AssetDatabase.ImportPackage(packagePath, true);
            Debug.Log($"Package imported from {packagePath}");
        }
        else
        {
            Debug.LogError($"Package not found at {packagePath}");
        }

    }

    private void RemovePackage(ItemData item)
    {
        string directoryPath = GenerateLink(item.checkDirectory, item.itemVersion);

        /* if (Directory.Exists(directoryPath))
         {
             Directory.Delete(directoryPath, true);
             Debug.Log($"Directory {directoryPath} removed.");
         }
         else
         {
             Debug.LogError($"Directory not found at {directoryPath}");
         }*/

        if (AssetDatabase.DeleteAsset(directoryPath))
        {
            Debug.Log($"Directory {directoryPath} removed.");
        }
        else
        {
            Debug.LogWarning($"Failed to delete: {directoryPath}");
        }

        AssetDatabase.Refresh();
    }
    private void RemovePackageFile(ItemData item)
    {
        string directoryPath = GenerateLink(item.checkDirectory, item.itemVersion);

        if (File.Exists(directoryPath))
        {
            Directory.Delete(directoryPath, true);
            Debug.Log($"Directory {directoryPath} removed.");
        }
        else
        {
            Debug.LogError($"Directory not found at {directoryPath}");
        }
    }
    private void RemoveLevelPlayMediation()
    {
        string ISAdMobAdapterDependencies = "Assets/LevelPlay/Editor/ISAdMobAdapterDependencies.xml";
        string ISBigoAdapterDependencies = "Assets/LevelPlay/Editor/ISBigoAdapterDependencies.xml";
        string ISChartboostAdapterDependencies = "Assets/LevelPlay/Editor/ISChartboostAdapterDependencies.xml";
        string ISFacebookAdapterDependencies = "Assets/LevelPlay/Editor/ISFacebookAdapterDependencies.xml";
        string ISFyberAdapterDependencies = "Assets/LevelPlay/Editor/ISFyberAdapterDependencies.xml";
        string ISInMobiAdapterDependencies = "Assets/LevelPlay/Editor/ISInMobiAdapterDependencies.xml";
        string ISMintegralAdapterDependencies = "Assets/LevelPlay/Editor/ISMintegralAdapterDependencies.xml";
        string ISMyTargetAdapterDependencies = "Assets/LevelPlay/Editor/ISMyTargetAdapterDependencies.xml";
        string ISPangleAdapterDependencies = "Assets/LevelPlay/Editor/ISPangleAdapterDependencies.xml";
        string ISSmaatoAdapterDependencies = "Assets/LevelPlay/Editor/ISSmaatoAdapterDependencies.xml";
        string ISVungleAdapterDependencies = "Assets/LevelPlay/Editor/ISVungleAdapterDependencies.xml";
        string ISYandexAdapterDependencies = "Assets/LevelPlay/Editor/ISYandexAdapterDependencies.xml";


        RemoveFile(ISAdMobAdapterDependencies);
        RemoveFile(ISBigoAdapterDependencies);
        RemoveFile(ISChartboostAdapterDependencies);
        RemoveFile(ISFacebookAdapterDependencies);
        RemoveFile(ISFyberAdapterDependencies);
        RemoveFile(ISInMobiAdapterDependencies);
        RemoveFile(ISMintegralAdapterDependencies);
        RemoveFile(ISMyTargetAdapterDependencies);
        RemoveFile(ISPangleAdapterDependencies);
        RemoveFile(ISSmaatoAdapterDependencies);
        RemoveFile(ISVungleAdapterDependencies);
        RemoveFile(ISYandexAdapterDependencies);
    }

    [System.Serializable]
    public class ManifestWrapper
    {
        public List<Dependency> dependencies;
    }

    [System.Serializable]
    public class Dependency
    {
        public string key;
        public string value;
    }

    #endregion

    #region LogoImporter

    //private void ImprotLogo()
    //{
    //    if (logoTexture != null)
    //    {
    //        float width = 200;
    //        float height = 200;
    //        Rect centeredRect = new Rect(
    //            (position.width - width) / 2,
    //            /*(position.height - height) / 2*/0,
    //            width,
    //            height
    //        );
    //        GUI.DrawTexture(centeredRect, logoTexture, ScaleMode.ScaleToFit);
    //        EditorGUILayout.Space(200);
    //    }
    //}

    private void ImprotLogo()
    {
        if (logoTexture != null)
        {
            float maxWidth = 750f;

            // Get original aspect ratio
            float aspect = (float)logoTexture.height / logoTexture.width;

            // Calculate height based on width
            float height = maxWidth * aspect;

            Rect centeredRect = new Rect(
                (position.width - maxWidth) / 2,
                0,
                maxWidth,
                height
            );

            GUI.DrawTexture(centeredRect, logoTexture, ScaleMode.ScaleToFit);

            EditorGUILayout.Space(height);
        }
    }


    #endregion

    /*  private void LoadDependencies()
      {
          if (File.Exists(jsonFilePath))
          {
              string jsonContent = File.ReadAllText(jsonFilePath);

              connectionPackageList = JsonUtility.FromJson<ConnectionPackageList>(jsonContent).Connection_Detector;

              monetizationSDKList = JsonUtility.FromJson<MonetizationSDKList>(jsonContent).MonetizationSDK_List;
              monetizationPackageList = JsonUtility.FromJson<MonetizationPackageList>(jsonContent).MonetizationPackage_List;

              allIntegrationItems = JsonUtility.FromJson<IntegrationList>(jsonContent).Integration_List;

              firebaseSDKList = JsonUtility.FromJson<FirebaseSDKList>(jsonContent).FirebaseSDK_List;
              firebasePackageList = JsonUtility.FromJson<FirebasePackageList>(jsonContent).FirebasePackage_List;

              mmpSDKList = JsonUtility.FromJson<MMP_SDKList>(jsonContent).MMP_SDK_List;
              googleServicesSDKList = JsonUtility.FromJson<GoogleServices_SDKList>(jsonContent).GoogleServices_SDK_List;
              unityServicesSDKList = JsonUtility.FromJson<UnityServices_SDKList>(jsonContent).UnityServices_SDK_List;
              mmpPackageList = JsonUtility.FromJson<MMP_PackageList>(jsonContent).MMP_Package_List;
              googleServicesPackageList = JsonUtility.FromJson<GoogleServices_PackageList>(jsonContent).GoogleServices_Package_List;
              unityServicesPackageList = JsonUtility.FromJson<UnityServices_PackageList>(jsonContent).UnityServices_Package_List;
              revenueCatSDKList = JsonUtility.FromJson<RevenueCat_SDKList>(jsonContent).RevenueCat_SDK_List;
              revenueCatPackageList = JsonUtility.FromJson<RevenueCat_PackageList>(jsonContent).RevenueCat_Package_List;
              essentialsPackageList = JsonUtility.FromJson<Essentials_PackageList>(jsonContent).Essentials_Package_List;
              oneSignalPackageList = JsonUtility.FromJson<OneSignal_PackageList>(jsonContent).OneSignal_Package_List;
              oneSignalSDKList = JsonUtility.FromJson<OneSignal_SDKList>(jsonContent).OneSignal_SDK_List;
              debugConsoleList = JsonUtility.FromJson<Debug_Console_List>(jsonContent).Debug_Manager;
              debugConsoleSDKList = JsonUtility.FromJson<Debug_Console_SDK_List>(jsonContent).Debug_Console;
              permissionsList = JsonUtility.FromJson<PermissonsList>(jsonContent).Permissions;
          }
          else
          {
              monetizationSDKList = new List<ItemData>();
              monetizationPackageList = new List<ItemData>();
              allIntegrationItems = new List<IntegrationName>();
              firebaseSDKList = new List<ItemData>();
              firebasePackageList = new List<ItemData>();
              mmpSDKList = new List<ItemData>();
              googleServicesSDKList = new List<ItemData>();
              unityServicesSDKList = new List<ItemData>();
              googleServicesPackageList = new List<ItemData>();
              unityServicesPackageList = new List<ItemData>();
              mmpPackageList = new List<ItemData>();
              essentialsPackageList = new List<ItemData>();
          }
      }*/

    private void LoadDependencies()
    {
        if (!File.Exists(jsonFilePath))
        {
            Debug.LogWarning("Dependencies JSON missing, initializing empty lists.");
            InitializeEmptyLists();
            return;
        }

        string jsonContent = File.ReadAllText(jsonFilePath);

        DependenciesRoot data = JsonUtility.FromJson<DependenciesRoot>(jsonContent);

        // Assign to your static lists
        allIntegrationItems = data.Integration_List;
        connectionPackageList = data.Connection_Detector;

        monetizationSDKList = data.MonetizationSDK_List;
        monetizationPackageList = data.MonetizationPackage_List;

        firebaseSDKList = data.FirebaseSDK_List;
        firebasePackageList = data.FirebasePackage_List;

        mmpSDKList = data.MMP_SDK_List;
        mmpPackageList = data.MMP_Package_List;

        googleServicesSDKList = data.GoogleServices_SDK_List;
        googleServicesPackageList = data.GoogleServices_Package_List;

        unityServicesSDKList = data.UnityServices_SDK_List;
        unityServicesPackageList = data.UnityServices_Package_List;

        revenueCatSDKList = data.RevenueCat_SDK_List;
        revenueCatPackageList = data.RevenueCat_Package_List;

        oneSignalSDKList = data.OneSignal_SDK_List;
        oneSignalPackageList = data.OneSignal_Package_List;

        debugConsoleSDKList = data.Debug_Console;
        debugConsoleList = data.Debug_Manager;

        permissionsList = data.Permissions;

        essentialsPackageList = new List<ItemData>(); // JSON does not contain Essentials in your example
    }

    private void InitializeEmptyLists()
    {
        allIntegrationItems = new List<IntegrationName>();
        connectionPackageList = new List<ItemData>();
        monetizationSDKList = new List<ItemData>();
        monetizationPackageList = new List<ItemData>();
        firebaseSDKList = new List<ItemData>();
        firebasePackageList = new List<ItemData>();
        mmpSDKList = new List<ItemData>();
        mmpPackageList = new List<ItemData>();
        googleServicesSDKList = new List<ItemData>();
        googleServicesPackageList = new List<ItemData>();
        unityServicesSDKList = new List<ItemData>();
        unityServicesPackageList = new List<ItemData>();
        revenueCatSDKList = new List<ItemData>();
        revenueCatPackageList = new List<ItemData>();
        oneSignalSDKList = new List<ItemData>();
        oneSignalPackageList = new List<ItemData>();
        debugConsoleSDKList = new List<ItemData>();
        debugConsoleList = new List<ItemData>();
        permissionsList = new List<PermissionData>();
        essentialsPackageList = new List<ItemData>();
    }

    public static bool CheckFile(ItemData item)
    {
        string directory = GenerateLink(item.checkDirectory, item.itemVersion);
        return Directory.Exists(directory);
    }
    public static bool CheckFilePluginsFileAdmob()
    {
        string aarFile = "Assets/Plugins/Android/googlemobileads-unity.aar";
        string libFile = "Assets/Plugins/Android/GoogleMobileAdsPlugin.androidlib";
        string nativeTemplates = "Assets/Plugins/iOS/NativeTemplates";
        string GADUAdNetworkExtras = "Assets/Plugins/iOS/GADUAdNetworkExtras.h";
        string unityPluginLibrary = "Assets/Plugins/iOS/unity-plugin-library.a";

        bool isExist = false;

        if (File.Exists(aarFile) || Directory.Exists(libFile) || Directory.Exists(nativeTemplates)
            || File.Exists(GADUAdNetworkExtras) || File.Exists(unityPluginLibrary))
        {
            isExist = true;
        }
        else
        {
            isExist = false;
        }


        return isExist;
    }


    public static bool CheckAnalytics()
    {
        string editorDefaultFolder = "Assets/Editor Default Resources";
        string iOSFolder = "Assets/Plugins/iOS";
        string mainFolder = "Assets/Firebase";
        string firebaseAppFolder = "Assets/Plugins/Android/FirebaseApp.androidlib";
        string analyticsDLL = "Assets/Firebase/Plugins/Firebase.Analytics.dll";

        bool isExist = false;

        if (File.Exists(analyticsDLL))
        {
            isExist = true;
        }
        else
        {
            isExist = false;
        }


        return isExist;
    }
    public static bool CheckFirebaseApp()
    {
        string packageFolder = Path.Combine(UnityEngine.Application.dataPath, "../Packages", "Firebase App (Core)");

        string firebaseAppFolder = "Assets/Plugins/Android/FirebaseApp.androidlib";

        bool isExist = false;

        if (Directory.Exists(packageFolder))
        {
            isExist = true;
        }
        else
        {
            isExist = false;
        }


        return isExist;
    }

    public static bool CheckRemoteConfig()
    {
        string editorDefaultFolder = "Assets/Editor Default Resources";
        string iOSFolder = "Assets/Plugins/iOS";
        string mainFolder = "Assets/Firebase";
        string analyticsDLL = "Assets/Firebase/Plugins/Firebase.RemoteConfig.dll";

        bool isExist = false;

        if (File.Exists(analyticsDLL))
        {
            isExist = true;
        }
        else
        {
            isExist = false;
        }


        return isExist;
    }
    public static bool CheckCrashlytics()
    {
        string editorDefaultFolder = "Assets/Editor Default Resources";
        string iOSFolder = "Assets/Plugins/iOS";
        string mainFolder = "Assets/Firebase";
        string analyticsDLL = "Assets/Firebase/Plugins/Firebase.Crashlytics.dll";

        bool isExist = false;

        if (File.Exists(analyticsDLL))
        {
            isExist = true;
        }
        else
        {
            isExist = false;
        }


        return isExist;
    }

    public static bool CheckMessaging()
    {
        string editorDefaultFolder = "Assets/Editor Default Resources";
        string iOSFolder = "Assets/Plugins/iOS";
        string mainFolder = "Assets/Firebase";
        string analyticsDLL = "Assets/Firebase/Plugins/Firebase.Messaging.dll";

        bool isExist = false;

        if (File.Exists(analyticsDLL))
        {
            isExist = true;
        }
        else
        {
            isExist = false;
        }


        return isExist;
    }

    public static bool CheckSolarEngine()
    {
        string solarEngineNet = "Assets/SolarEngineNet";
        string solarEngineSDK = "Assets/SolarEngineSDK";
        string openHarmony = "Assets/Plugins/OpenHarmony";
        string solarEngine = "Assets/Plugins/SolarEngine";

        bool isExist = false;

        if (Directory.Exists(solarEngineNet) || Directory.Exists(solarEngineSDK) || Directory.Exists(openHarmony) || Directory.Exists(solarEngine))
        {
            isExist = true;
        }
        else
        {
            isExist = false;
        }


        return isExist;
    }

    public static bool CheckLevelPlayMediation()
    {

        string ISAdMobAdapterDependencies = "Assets/LevelPlay/Editor/ISAdMobAdapterDependencies.xml";
        string ISBigoAdapterDependencies = "Assets/LevelPlay/Editor/ISBigoAdapterDependencies.xml";
        string ISChartboostAdapterDependencies = "Assets/LevelPlay/Editor/ISChartboostAdapterDependencies.xml";
        string ISFacebookAdapterDependencies = "Assets/LevelPlay/Editor/ISFacebookAdapterDependencies.xml";
        string ISFyberAdapterDependencies = "Assets/LevelPlay/Editor/ISFyberAdapterDependencies.xml";
        string ISInMobiAdapterDependencies = "Assets/LevelPlay/Editor/ISInMobiAdapterDependencies.xml";
        string ISMintegralAdapterDependencies = "Assets/LevelPlay/Editor/ISMintegralAdapterDependencies.xml";
        string ISMyTargetAdapterDependencies = "Assets/LevelPlay/Editor/ISMyTargetAdapterDependencies.xml";
        string ISPangleAdapterDependencies = "Assets/LevelPlay/Editor/ISPangleAdapterDependencies.xml";
        string ISSmaatoAdapterDependencies = "Assets/LevelPlay/Editor/ISSmaatoAdapterDependencies.xml";
        string ISVungleAdapterDependencies = "Assets/LevelPlay/Editor/ISVungleAdapterDependencies.xml";
        string ISYandexAdapterDependencies = "Assets/LevelPlay/Editor/ISYandexAdapterDependencies.xml";

        bool isExist = false;

        if (File.Exists(ISAdMobAdapterDependencies)
            || File.Exists(ISBigoAdapterDependencies) || File.Exists(ISChartboostAdapterDependencies)
            || File.Exists(ISFacebookAdapterDependencies) || File.Exists(ISFyberAdapterDependencies)
            || File.Exists(ISInMobiAdapterDependencies) || File.Exists(ISMintegralAdapterDependencies)
            || File.Exists(ISMyTargetAdapterDependencies) || File.Exists(ISPangleAdapterDependencies)
            || File.Exists(ISSmaatoAdapterDependencies) || File.Exists(ISVungleAdapterDependencies)
            || File.Exists(ISYandexAdapterDependencies))
        {
            isExist = true;
        }
        else
        {
            isExist = false;
        }


        return isExist;
    }

    public static void RemoveFilePluginsFileAdmob()
    {
        string aarFile = "Assets/Plugins/Android/googlemobileads-unity.aar";
        string libFile = "Assets/Plugins/Android/GoogleMobileAdsPlugin.androidlib";
        string nativeTemplates = "Assets/Plugins/iOS/NativeTemplates";
        string GADUAdNetworkExtras = "Assets/Plugins/iOS/GADUAdNetworkExtras.h";
        string unityPluginLibrary = "Assets/Plugins/iOS/unity-plugin-library.a";

        RemoveFolder(libFile);
        RemoveFile(aarFile);
        RemoveFolder(nativeTemplates);
        RemoveFile(GADUAdNetworkExtras);
        RemoveFile(unityPluginLibrary);
    }
    public static void RemoveOneSignalPluginFile()
    {
        string oneSignalConfig = "Assets/Plugins/Android/OneSignalConfig.androidlib";

        RemoveFolder(oneSignalConfig);
    }
    public static void RemoveFirebaseApp()
    {
        string editorDefaultFolder = "Assets/Editor Default Resources";
        string generatedLocalRepoFolder = "Assets/GeneratedLocalRepo";
        //  string iOSFolder = "Assets/Plugins/iOS";
        //  string mainFolder = "Assets/Firebase";
        string firebaseAppFolder = "Assets/Plugins/Android/FirebaseApp.androidlib";

        //   RemoveFolder(mainFolder);
        RemoveFolder(generatedLocalRepoFolder);
        RemoveFolder(firebaseAppFolder);
        //   RemoveFolder(iOSFolder);
        RemoveFolder(editorDefaultFolder);

    }

    public static void RemoveFirebaseAnalytics()
    {
        string firebaseCrashlyticsFolder = "Assets/Plugins/Android/FirebaseCrashlytics.androidlib";

        RemoveFolder(firebaseCrashlyticsFolder);
    }

    public static void RemoveFirebaseMessaging()
    {
        string editorDefaultFolder = "Assets/Editor Default Resources";
        string generatedLocalRepoFolder = "Assets/GeneratedLocalRepo";

        RemoveFolder(generatedLocalRepoFolder);
        RemoveFolder(editorDefaultFolder);

        SymbolManagerWindow.SetSymbol("GamesArena_Firebase_Messaging", false);
    }
    public static void RemoveFirebaseRemoteConfig()
    {
        string editorDefaultFolder = "Assets/Editor Default Resources";
        string generatedLocalRepoFolder = "Assets/GeneratedLocalRepo";

        RemoveFolder(generatedLocalRepoFolder);
        RemoveFolder(editorDefaultFolder);

        SymbolManagerWindow.SetSymbol("GamesArena_Firebase_RemoteConfig", false);
    }
    public static void RemoveFirebaseCrashlytics()
    {
        string editorDefaultFolder = "Assets/Editor Default Resources";
        string generatedLocalRepoFolder = "Assets/GeneratedLocalRepo";

        RemoveFolder(generatedLocalRepoFolder);
        RemoveFolder(editorDefaultFolder);

        SymbolManagerWindow.SetSymbol("GamesArena_Firebase_Crashlytics", false);
    }

    public static void RemoveSolarEngine()
    {
        string solarEngineNet = "Assets/SolarEngineNet";
        string solarEngineSDK = "Assets/SolarEngineSDK";
        string openHarmony = "Assets/Plugins/OpenHarmony";
        string solarEngine = "Assets/Plugins/SolarEngine";

        RemoveFolder(solarEngineNet);
        RemoveFolder(solarEngineSDK);
        RemoveFolder(openHarmony);
        RemoveFolder(solarEngine);
    }

    public static void RemoveFolder(string path)
    {
        /*if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
            Debug.Log($"Directory {path} removed.");
        }
        else
        {
            Debug.LogError($"Directory not found at {path}");
        }*/

        if (AssetDatabase.DeleteAsset(path))
        {
            Debug.Log($"Deleted: {path}");
        }
        else
        {
            Debug.LogWarning($"Failed to delete: {path}");
        }
    }

    public static void RemoveFile(string path)
    {

        if (File.Exists(path))
        {
            File.Delete(path);
            Debug.Log($"File {path} removed.");
        }
        else
        {
            Debug.Log($"File not found at {path}");
        }

    }

    #region Serializable

    [System.Serializable]
    public class DependenciesRoot
    {
        public List<IntegrationName> Integration_List;
        public List<ItemData> Connection_Detector;
        public List<ItemData> MonetizationSDK_List;
        public List<ItemData> MonetizationPackage_List;
        public List<ItemData> FirebaseSDK_List;
        public List<ItemData> FirebasePackage_List;
        public List<ItemData> MMP_SDK_List;
        public List<ItemData> MMP_Package_List;
        public List<ItemData> GoogleServices_SDK_List;
        public List<ItemData> GoogleServices_Package_List;
        public List<ItemData> UnityServices_SDK_List;
        public List<ItemData> UnityServices_Package_List;
        public List<ItemData> RevenueCat_SDK_List;
        public List<ItemData> RevenueCat_Package_List;
        public List<ItemData> OneSignal_SDK_List;
        public List<ItemData> OneSignal_Package_List;
        public List<ItemData> Debug_Console;
        public List<ItemData> Debug_Manager;
        public List<PermissionData> Permissions;
    }

    [System.Serializable]
    public class ItemData
    {
        public string itemName;
        public string symbolName;
        public string itemPrefabPath;
        public string itemVersion;
        public string checkDirectory;
        public string importUrl;
    }
    [System.Serializable]
    public class PermissionData
    {
        public string itemName;
        public string permission;
    }

    #region Integration

    [System.Serializable]
    public class IntegrationName
    {
        public string integrationName;
    }

    [System.Serializable]
    public class IntegrationList
    {
        public List<IntegrationName> Integration_List;
    }

    #endregion


    [System.Serializable]
    public class PermissonsList
    {
        public List<PermissionData> Permissions;
    }

    [System.Serializable]
    public class ConnectionPackageList
    {
        public List<ItemData> Connection_Detector;
    }

    [System.Serializable]
    public class OneSignal_PackageList
    {
        public List<ItemData> OneSignal_Package_List;
    }
    [System.Serializable]
    public class OneSignal_SDKList
    {
        public List<ItemData> OneSignal_SDK_List;
    }
    public class Debug_Console_List
    {
        public List<ItemData> Debug_Manager;
    }
    public class Debug_Console_SDK_List
    {
        public List<ItemData> Debug_Console;
    }

    #region Monetization

    [System.Serializable]
    public class MonetizationSDKList
    {
        public List<ItemData> MonetizationSDK_List;
    }

    [System.Serializable]
    public class MonetizationPackageList
    {
        public List<ItemData> MonetizationPackage_List;
    }

    #endregion

    #region Firebase

    [System.Serializable]
    public class FirebaseSDKList
    {
        public List<ItemData> FirebaseSDK_List;
    }

    [System.Serializable]
    public class FirebasePackageList
    {
        public List<ItemData> FirebasePackage_List;
    }



    #endregion

    #region MMP

    [System.Serializable]
    public class MMP_SDKList
    {
        public List<ItemData> MMP_SDK_List;
    }

    [System.Serializable]
    public class MMP_PackageList
    {
        public List<ItemData> MMP_Package_List;
    }

    #endregion

    #region Google Services

    [System.Serializable]
    public class GoogleServices_SDKList
    {
        public List<ItemData> GoogleServices_SDK_List;
    }

    [System.Serializable]
    public class GoogleServices_PackageList
    {
        public List<ItemData> GoogleServices_Package_List;
    }



    #endregion

    #region Unity Services

    [System.Serializable]
    public class UnityServices_SDKList
    {
        public List<ItemData> UnityServices_SDK_List;
    }

    [System.Serializable]
    public class UnityServices_PackageList
    {
        public List<ItemData> UnityServices_Package_List;
    }


    #endregion
    public class RevenueCat_PackageList
    {
        public List<ItemData> RevenueCat_Package_List;
    }

    public class RevenueCat_SDKList
    {
        public List<ItemData> RevenueCat_SDK_List;
    }



    #region Essentials

    [System.Serializable]
    public class Essentials_PackageList
    {
        public List<ItemData> Essentials_Package_List;
    }



    #endregion


    #endregion

    public static void RemoveItemSymbols(ItemData item)
    {
        switch (item.itemName)
        {
            case "Connection Detector":
                SymbolManagerWindow.SetSymbol("Connection_Detector", false);
                break;

            case "AdMob":
                SymbolManagerWindow.SetSymbol("Admob_Ads", false);
                break;
            case "LevelPlay":
                SymbolManagerWindow.SetSymbol("Ironsource_Ads", false);
                break;
            case "AppLovin Max":
                SymbolManagerWindow.SetSymbol("Applovin_Ads", false);
                break;

            case "All in 1 Firebase Manager":
                SymbolManagerWindow.SetSymbol("GamesArena_Firebase_Analytics", false);
                SymbolManagerWindow.SetSymbol("GamesArena_Firebase_RemoteConfig", false);
                SymbolManagerWindow.SetSymbol("GamesArena_Firebase_Messaging", false);
                SymbolManagerWindow.SetSymbol("GamesArena_Firebase_Crashlytics", false);
                break;


            case "Firebase App":
                SymbolManagerWindow.SetSymbol("GamesArena_Firebase_App", false);
                break;
            case "Analytics":
                SymbolManagerWindow.SetSymbol("GamesArena_Firebase_Analytics", false);
                break;

            case "Crashlytics":
                SymbolManagerWindow.SetSymbol("GamesArena_Firebase_Crashlytics", false);
                break;

            case "Messaging":
                SymbolManagerWindow.SetSymbol("GamesArena_Firebase_Messaging", false);
                break;

            case "Remote Config":
                SymbolManagerWindow.SetSymbol("GamesArena_Firebase_RemoteConfig", false);
                break;


            case "Play In App Reviews":
                SymbolManagerWindow.SetSymbol("In_App_Review", false);
                break;
            case "Play In App Update":
                SymbolManagerWindow.SetSymbol("In_App_Update", false);
                break;


            case "AppsFlyer":
                SymbolManagerWindow.SetSymbol("AppsFlyer", false);
                break;

            case "Solar Engine":
                SymbolManagerWindow.SetSymbol("Solar_MMP", false);
                break;

            case "In App Purchsing":
                SymbolManagerWindow.SetSymbol("IAPs", false);
                break;

            case "Revenue Cat":
                SymbolManagerWindow.SetSymbol("Revenue_Cat", false);
                break;

            case "One Signal":
                SymbolManagerWindow.SetSymbol("One_Signal", false);
                break;

            default:
                break;
        }
    }

    public static void AdmobManagerImportButton(string path)
    {
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (!CheckPrefabExists(prefab))
        {
            if (GUILayout.Button("Add Prefab", GUILayout.Width(100), GUILayout.Height(30)))
            {
                SpawnPrefab(path);
            }
        }
        else
        {
            GUI.backgroundColor = new Color(1f, 0.65f, 0f);
            if (GUILayout.Button("Remove Prefab", GUILayout.Width(100), GUILayout.Height(30)))
            {
                RemovePrefab(prefab);
            }
            GUI.backgroundColor = Color.white;
        }



        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
    }

    public static void SpawnPrefab(string path)
    {

        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

        if (prefab != null)
        {
            GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab);
            instance.transform.position = Vector3.zero;

            // Register undo (important for editor)
            Undo.RegisterCreatedObjectUndo(instance, "Spawn Prefab");

            Selection.activeGameObject = instance;
        }
        else
        {
            Debug.LogError("Prefab not found at path: " + path);
        }
    }
    public static void RemovePrefab(GameObject prefab)
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(obj);

            if (root == null)
                continue;

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(root);

            if (source == prefab)
            {
                Undo.DestroyObjectImmediate(root);
            }
        }
    }

    public static bool CheckPrefabExists(GameObject prefab)
    {
        GameObject[] allObjects = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            GameObject root = PrefabUtility.GetNearestPrefabInstanceRoot(obj);

            if (root == null)
                continue;

            GameObject source = PrefabUtility.GetCorrespondingObjectFromSource(root);

            if (source == prefab)
            {
                return true;
            }
        }

        return false;
    }

    static AddRequest request;
    static string packageName;
    static double startTime;

    public static void InstallPackage(string pkg)
    {
        packageName = pkg;
        startTime = EditorApplication.timeSinceStartup;

        request = Client.Add(pkg);
        EditorApplication.update += Progress;
    }

    static void Progress()
    {
        if (request == null) return;

        // Fake smooth progress (since Unity doesn't give real %)
        float elapsed = (float)(EditorApplication.timeSinceStartup - startTime);
        float progress = Mathf.Clamp01(elapsed / 5f); // 5 sec fake duration

        EditorUtility.DisplayProgressBar(
            "Installing Package",
            $"Installing {packageName}...\nPlease wait...",
            progress
        );

        if (request.IsCompleted)
        {
            EditorUtility.ClearProgressBar();

            if (request.Status == StatusCode.Success)
            {
                Debug.Log("? Installed: " + request.Result.packageId);

                PackageCache.RefreshCache(() => { Debug.Log("Packages Loaded"); });
            }
            else
            {
                Debug.LogError("? Failed: " + request.Error.message);
            }

            EditorApplication.update -= Progress;
        }
    }


    public static void RemovePackage(string pkg)
    {
        RemoveRequest removeRequest;


        packageName = pkg;
        startTime = EditorApplication.timeSinceStartup;

        removeRequest = Client.Remove(pkg);
        EditorApplication.update += RemoveProgress;

        void RemoveProgress()
        {
            if (removeRequest == null) return;

            // Fake smooth progress
            float elapsed = (float)(EditorApplication.timeSinceStartup - startTime);
            float progress = Mathf.Clamp01(elapsed / 5f);

            EditorUtility.DisplayProgressBar(
                "Removing Package",
                $"Removing {packageName}...\nPlease wait...",
                progress
            );

            if (removeRequest.IsCompleted)
            {
                EditorUtility.ClearProgressBar();

                if (removeRequest.Status == StatusCode.Success)
                {
                    Debug.Log("✅ Removed: " + packageName);

                    PackageCache.RefreshCache(() => { Debug.Log("Packages Loaded"); });
                }
                else
                {
                    Debug.LogError("❌ Failed: " + removeRequest.Error.message);
                }

                EditorApplication.update -= RemoveProgress;
            }
        }
    }
    public static void RemoveMutiplePackage(string pkg)
    {
        RemoveRequest removeRequest;


        packageName = pkg;
        startTime = EditorApplication.timeSinceStartup;

        removeRequest = Client.Remove(pkg);
        EditorApplication.update += RemoveProgress;

        void RemoveProgress()
        {
            if (removeRequest == null) return;

            // Fake smooth progress
            float elapsed = (float)(EditorApplication.timeSinceStartup - startTime);
            float progress = Mathf.Clamp01(elapsed / 5f);

            EditorUtility.DisplayProgressBar(
                "Removing Package",
                $"Removing {packageName}...\nPlease wait...",
                progress
            );

            if (removeRequest.IsCompleted)
            {
                EditorUtility.ClearProgressBar();

                if (removeRequest.Status == StatusCode.Success)
                {
                    Debug.Log("✅ Removed: " + packageName);

                    PackageCache.RefreshCache(() => { Debug.Log("Packages Loaded"); });
                }
                else
                {
                    Debug.LogError("❌ Failed: " + removeRequest.Error.message);
                }

                EditorApplication.update -= RemoveProgress;
            }
        }
    }
}