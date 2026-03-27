using UnityEngine;
using System.IO;

public static class XMLPath
{
    public static string Dir => Application.persistentDataPath;
    public static string SettingsXml => Path.Combine(Dir, "settings.xml");
    public static string InventoryXml => Path.Combine(Dir, "inventory.xml");

    public static void EnsureDir()
    {
        if (!Directory.Exists(Dir))
            Directory.CreateDirectory(Dir);
    }
}

