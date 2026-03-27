using UnityEngine;
using UnityEngine.SceneManagement;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Settings : MonoBehaviour
{
    public GameObject container;
    public Menu settings;

    bool paused;

    void Awake()
    {
        if (!settings) settings = FindFirstObjectByType<Menu>();
        if (container) container.SetActive(false);
        paused = false;
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k != null && k.escapeKey.wasPressedThisFrame)
#else
        if (Input.GetKeyDown(KeyCode.Escape))
#endif
        {
            if (settings && settings.IsShowing())
            {
                settings.Hide();
                if (container) container.SetActive(true);
                return;
            }

            if (paused) ResumeButton();
            else Pause();
        }
    }

    void Pause()
    {
        paused = true;
        if (container) container.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeButton()
    {
        paused = false;
        if (container) container.SetActive(false);
        if (settings) settings.Hide();
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void MainMenuButton()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("Title");
    }

    public void SettingButton()
    {
        if (container) container.SetActive(false);
        if (settings) settings.Show();
    }

    public void QuitButton()
    {
        Application.Quit();
    }
}
