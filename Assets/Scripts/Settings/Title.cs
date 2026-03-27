using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class GameSession
{
    public enum Mode { NewGame, Resume }
    public static Mode StartMode = Mode.NewGame;
}

public class Title : MonoBehaviour
{
    [SerializeField] string gameSceneName = "Main Game";

    bool canContinue;

    void Awake()
    {
        XMLPath.EnsureDir();
        canContinue = File.Exists(XMLPath.SettingsXml) || File.Exists(XMLPath.InventoryXml);
    }

    void StartNewGame()
    {
        GameSession.StartMode = GameSession.Mode.NewGame;
        SceneManager.LoadScene(gameSceneName);
    }

    void ContinueGame()
    {
        GameSession.StartMode = GameSession.Mode.Resume;
        SceneManager.LoadScene(gameSceneName);
    }

    void OnGUI()
    {
        const float w = 300f, h = 44f, gap = 14f;
        float x = (Screen.width - w) * 0.5f;
        float y = (Screen.height - (h * 3 + gap * 2)) * 0.5f;

        GUI.Box(new Rect(x - 20, y - 40, w + 40, h * 3 + gap * 2 + 80), "");
        GUI.Label(
            new Rect(x, y - 32, w, 24),
            "<b>My Game</b>",
            new GUIStyle(GUI.skin.label)
            {
                alignment = TextAnchor.MiddleCenter,
                richText = true
            });

        if (GUI.Button(new Rect(x, y, w, h), "New Game"))
            StartNewGame();

        GUI.enabled = canContinue;
        if (GUI.Button(new Rect(x, y + h + gap, w, h), "Continue"))
            ContinueGame();
        GUI.enabled = true;

        if (GUI.Button(new Rect(x, y + (h + gap) * 2, w, h), "Quit"))
            Application.Quit();
    }
}
