using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class PackageRemover
{
    static Queue<string> packagesQueue;
    static RemoveRequest currentRequest;
    static string currentPackage;
    static double startTime;

    public static void RemoveMultiplePackages(string[] packages)
    {
        foreach (var pkg in packages)
        {
            if (!PackageCache.IsInstalled(pkg))
            {
                Debug.Log($"⏭ Skipping (not installed): {pkg}");
                continue;
            }

            Debug.Log($"🗑 Removing: {pkg}");

            var removeRequest = UnityEditor.PackageManager.Client.Remove(pkg);

            double startTime = EditorApplication.timeSinceStartup;
            const double TIMEOUT = 30; // safety

            // ✅ SAFE WHILE LOOP (does NOT freeze editor)
            while (!removeRequest.IsCompleted)
            {
                // Pump Unity editor updates
                EditorApplication.QueuePlayerLoopUpdate();

                // Optional progress bar
                float elapsed = (float)(EditorApplication.timeSinceStartup - startTime);
                float progress = Mathf.Clamp01(elapsed / 5f);

                EditorUtility.DisplayProgressBar(
                    "Removing Packages",
                    $"Removing {pkg}...",
                    progress
                );

                // ❗ Timeout safety (VERY IMPORTANT)
                if (elapsed > TIMEOUT)
                {
                    Debug.LogWarning($"⏱ Timeout removing: {pkg}");
                    break;
                }
            }

            EditorUtility.ClearProgressBar();

            // ✅ Result handling
            if (removeRequest.Status == StatusCode.Success)
            {
                Debug.Log($"✅ Removed: {pkg}");
            }
            else if (removeRequest.Status >= StatusCode.Failure)
            {
                Debug.LogError($"❌ Failed: {pkg} → {removeRequest.Error?.message}");
            }
        }

        Debug.Log("✅ All remove operations completed");

        // Refresh cache once at end
        PackageCache.RefreshCache(() => { Debug.Log("Packages Loaded"); });
    }
}