using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

public static class PackageCache
{
    private static HashSet<string> cachedPackages = new HashSet<string>();
    private static bool isRefreshing = false;

    public static void RefreshCache(System.Action onComplete = null)
    {
        ListRequest listRequest = Client.List(true);
        EditorApplication.update += Poll;

        void Poll()
        {
            if (listRequest.IsCompleted)
            {
                if (listRequest.Status == StatusCode.Success)
                {
                    // ❗ SAFETY CHECK
                    if (listRequest.Result == null || listRequest.Result.Count() == 0)
                    {
                        Debug.LogWarning("[PackageCache] Empty result, skipping update to avoid clearing cache.");
                    }
                    else
                    {
                        if (cachedPackages == null)
                            cachedPackages = new HashSet<string>();

                        HashSet<string> latestPackages = new HashSet<string>();

                        foreach (var pkg in listRequest.Result)
                        {
                            latestPackages.Add(pkg.name);

                            if (!cachedPackages.Contains(pkg.name))
                                cachedPackages.Add(pkg.name);
                        }

                        // ❌ Only remove if we got valid data
                        cachedPackages.RemoveWhere(pkg => !latestPackages.Contains(pkg));
                    }
                }
                else
                {
                    Debug.LogError("Failed to list packages: " + listRequest.Error.message);
                }

                EditorApplication.update -= Poll;
                onComplete?.Invoke();
            }
        }
    }

    public static bool IsInstalled(string packageName)
    {
        if (cachedPackages == null)
        {
            Debug.LogWarning("Package cache not initialized! Call RefreshCache() first.");
            return false;
        }

        return cachedPackages.Contains(packageName);
    }

}