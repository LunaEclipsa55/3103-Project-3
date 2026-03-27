using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStarter : MonoBehaviour
{
    void Awake()
    {
        XMLPath.EnsureDir();

        var settings = FindFirstObjectByType<Menu>();
        if (settings != null)
        {
            if(GameSession.StartMode == GameSession.Mode.Resume)
            {
                settings.LoadFromFile();
            }
            else
            {
                settings.ApplyDefaultsAndSave();
            }
        }

        var inv = FindFirstObjectByType<Inventory>();
        if (inv != null)
        {
            if(GameSession.StartMode == GameSession.Mode.Resume)
            {
                inv.LoadFromFile();
            }
            else
            {
                inv.ClearToNewGame();
            }
        }
    }
}
