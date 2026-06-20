using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEngine.Networking;

public class SDKDownloadWindow : EditorWindow
{
    private string sdkName;
    private string downloadUrl;
    private string savePath;
    private bool isDownloading = false;
    private float downloadProgress = 0f;
    private UnityWebRequest currentRequest;
    private static SDKDownloadWindow window;
    private GUIStyle titleStyle;

    public static void OpenWindow(string sdkName, string downloadUrl)
    {
        window = GetWindow<SDKDownloadWindow>($"{sdkName} Download");
        window.sdkName = sdkName;
        window.downloadUrl = downloadUrl;

        string downloadsFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments);
        string saveFolder = Path.Combine(downloadsFolder, "UnityDownloads");

        if (!Directory.Exists(saveFolder))
        {
            Directory.CreateDirectory(saveFolder);
        }
        Debug.Log(downloadUrl);
        window.savePath = Path.Combine(saveFolder, Path.GetFileName(downloadUrl));

        window.minSize = new Vector2(200, 150);
        window.Show();

        window.CheckAndStartDownload();
    }
    private void OnEnable()
    {
        titleStyle = new GUIStyle(EditorStyles.boldLabel);
        titleStyle.fontSize = 20;
        titleStyle.normal.textColor = Color.white;
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.margin = new RectOffset(0, 0, 20, 20);
    }
    private void OnGUI()
    {
        GUILayout.Label($"{sdkName} - Download and Import", titleStyle);


        if (isDownloading)
        {
            EditorGUILayout.LabelField("Downloading...");
            Rect rect = GUILayoutUtility.GetRect(50, 20);
            EditorGUI.ProgressBar(rect, downloadProgress, $"{(downloadProgress * 100).ToString("0.00")}%");
            Repaint(); 
        }
        else
        {
            Repaint();
        }
    }

    private void CheckAndStartDownload()
    {
        if (File.Exists(savePath))
        {
            Debug.Log($"File already downloaded: {savePath}");
            ImportAndCloseWindow();
        }
        else
        {
            StartDownload();
        }
    }

    private void StartDownload()
    {
        isDownloading = true;
        currentRequest = new UnityWebRequest(downloadUrl, UnityWebRequest.kHttpVerbGET)
        {
            downloadHandler = new DownloadHandlerFile(savePath)
        };
        currentRequest.SendWebRequest();
        EditorApplication.update += UpdateDownloadProgress;
    }

    private void UpdateDownloadProgress()
    {
        if (currentRequest.isDone)
        {
            isDownloading = false;
            EditorApplication.update -= UpdateDownloadProgress;

            if (currentRequest.result == UnityWebRequest.Result.Success)
            {
                Debug.Log($"Download complete: {savePath}");
                ImportAndCloseWindow();
            }
            else
            {
                Debug.LogError($"Download failed: {currentRequest.error}");
                EditorUtility.DisplayDialog("Download Failed", "An error occurred during the download. Check the console for details.", "OK");
            }
        }
        else
        {
            downloadProgress = currentRequest.downloadProgress;
            Repaint();
        }
    }

    private void ImportAndCloseWindow()
    {
        AssetDatabase.ImportPackage(savePath, true);
        window.Close();
    }
}