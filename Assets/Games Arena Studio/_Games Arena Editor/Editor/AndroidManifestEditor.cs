using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

public static class AndroidManifestEditor
{
    private static string ManifestPath => Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");

    /// <summary>
    /// Check if permission already exists
    /// </summary>
    public static bool HasPermission(string permission)
    {
        if (!File.Exists(ManifestPath))
        {
            Debug.LogWarning("AndroidManifest.xml not found.");
            return false;
        }

        XmlDocument doc = new XmlDocument();
        doc.Load(ManifestPath);

        XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
        nsMgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

        string xpath = $"/manifest/uses-permission[@android:name='{permission}']";
        XmlNode node = doc.SelectSingleNode(xpath, nsMgr);

        return node != null;
    }

    /// <summary>
    /// Add permission if not already present
    /// </summary>
    public static bool AddPermission(string permission)
    {
        if (!File.Exists(ManifestPath))
        {
            Debug.LogError("AndroidManifest.xml not found. Cannot add permission.");
            return false;
        }

        XmlDocument doc = new XmlDocument();
        doc.Load(ManifestPath);

        XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
        nsMgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

        string xpath = $"/manifest/uses-permission[@android:name='{permission}']";
        XmlNode existingNode = doc.SelectSingleNode(xpath, nsMgr);

        if (existingNode != null)
        {
            Debug.Log($"Permission already exists: {permission}");
            return false;
        }

        XmlNode manifestNode = doc.SelectSingleNode("/manifest");

        XmlElement permElement = doc.CreateElement("uses-permission");
        permElement.SetAttribute("name", "http://schemas.android.com/apk/res/android", permission);

        manifestNode.AppendChild(permElement);

        doc.Save(ManifestPath);
        AssetDatabase.Refresh();

        Debug.Log($"Permission added: {permission}");
        return true;
    }

    public static bool RemovePermission(string permission)
    {
        if (!File.Exists(ManifestPath))
        {
            Debug.LogError("AndroidManifest.xml not found. Cannot remove permission.");
            return false;
        }

        XmlDocument doc = new XmlDocument();
        doc.Load(ManifestPath);

        XmlNamespaceManager nsMgr = new XmlNamespaceManager(doc.NameTable);
        nsMgr.AddNamespace("android", "http://schemas.android.com/apk/res/android");

        string xpath = $"/manifest/uses-permission[@android:name='{permission}']";
        XmlNodeList nodes = doc.SelectNodes(xpath, nsMgr);

        if (nodes == null || nodes.Count == 0)
        {
            Debug.Log($"Permission not found: {permission}");
            return false;
        }

        foreach (XmlNode node in nodes)
        {
            node.ParentNode.RemoveChild(node);
        }

        doc.Save(ManifestPath);
        AssetDatabase.Refresh();

        Debug.Log($"Removed permission: {permission}");
        return true;
    }
}