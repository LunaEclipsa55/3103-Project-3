using System.IO;
using System.Xml.Serialization;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Menu : MonoBehaviour
{
    [System.Serializable]
    [XmlRoot("settings")]
    public class Settings
    {
        [XmlElement("width")] public int width = 1920;
        [XmlElement("height")] public int height = 1080;
        [XmlElement("fullscreen")] public bool fullscreen = true;
        [XmlElement("fov")] public float fov = 60f;
        [XmlElement("shadows")] public int shadows = 2;
        [XmlElement("vSync")] public int vSync = 0;
        [XmlElement("volMaster")] public float volMaster = 1f;
        [XmlElement("volBG")] public float volBG = 0.6f;
        [XmlElement("volSFX")] public float volSFX = 0.8f;
        [XmlElement("muteAll")] public bool muteAll = false;
    }

    public Camera targetCamera;
    public KeyCode toggleKey = KeyCode.F2;

    Rect win = new Rect(20, 80, 560, 420);
    bool show;

    float fov = 60f;
    int resIndex = 0;
    bool fullscreen = true;
    int shadowMode = 2;
    int vSync = 0;
    int quality = 2;

    float volMaster = 1f;
    float volBG = 0.6f;
    float volSFX = 0.8f;
    bool muteAll = false;

    Vector2 scroll;

    GUIStyle _title;
    GUIStyle _section;
    GUIStyle _valueTag;
    GUIStyle _thinLine;
    GUIStyle _padBox;

    public Settings data = new();

    static readonly Vector2Int[] ResOpts = new[]
    {
        new Vector2Int(1920,1080),
        new Vector2Int(1600,900),
        new Vector2Int(1366,768),
        new Vector2Int(1280,720),
        new Vector2Int(640,400),
    };

    static readonly string[] ResLabels = { "1920×1080", "1600×900", "1366×768", "1280×720", "640×400" };
    string[] _qualityLabels = { "Low", "Medium", "High" };
    string[] _shadowLabels = { "None", "Hard", "Soft" };
    string[] _vsLabels = { "Off", "On" };

    string FilePath => XMLPath.SettingsXml;

    void Awake()
    {
        if (!targetCamera) targetCamera = Camera.main;
        if (targetCamera) fov = targetCamera.fieldOfView;

        fullscreen = Screen.fullScreen;
        vSync = QualitySettings.vSyncCount > 0 ? 1 : 0;

        var cur = new Vector2Int(Screen.currentResolution.width, Screen.currentResolution.height);
        int best = 0;
        int bestDelta = int.MaxValue;
        for (int i = 0; i < ResOpts.Length; i++)
        {
            int d = Mathf.Abs(ResOpts[i].x - cur.x) + Mathf.Abs(ResOpts[i].y - cur.y);
            if (d < bestDelta)
            {
                best = i;
                bestDelta = d;
            }
        }
        resIndex = best;

        shadowMode = 0;
        foreach (var light in FindObjectsOfType<Light>())
        {
            if (light.shadows == LightShadows.Soft) { shadowMode = 2; break; }
            if (light.shadows == LightShadows.Hard) shadowMode = Mathf.Max(shadowMode, 1);
        }

        SetupStyles();
    }

    void Start()
    {
        LoadFromFile();
        ApplyAll();
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k != null && k.f2Key.wasPressedThisFrame)
            show = !show;
#else
        if (Input.GetKeyDown(toggleKey))
            show = !show;
#endif
    }

    public bool IsShowing() => show;
    public void Show() => show = true;
    public void Hide() => show = false;

    public bool CanLoad() => File.Exists(FilePath);

    void SyncDataFromUI()
    {
        var r = ResOpts[Mathf.Clamp(resIndex, 0, ResOpts.Length - 1)];
        data.width = r.x;
        data.height = r.y;
        data.fullscreen = fullscreen;
        data.fov = fov;
        data.shadows = shadowMode;
        data.vSync = vSync;
        data.volMaster = volMaster;
        data.volBG = volBG;
        data.volSFX = volSFX;
        data.muteAll = muteAll;
    }

    void SyncUIFromData()
    {
        int best = 0;
        int bestDelta = int.MaxValue;
        for (int i = 0; i < ResOpts.Length; i++)
        {
            int d = Mathf.Abs(ResOpts[i].x - data.width) + Mathf.Abs(ResOpts[i].y - data.height);
            if (d < bestDelta)
            {
                best = i;
                bestDelta = d;
            }
        }

        resIndex = best;
        fullscreen = data.fullscreen;
        fov = data.fov;
        shadowMode = data.shadows;
        vSync = data.vSync;
        volMaster = data.volMaster;
        volBG = data.volBG;
        volSFX = data.volSFX;
        muteAll = data.muteAll;
    }

    public void ApplyDefaultsAndSave()
    {
        data = new Settings();
        SyncUIFromData();
        ApplyAll();
        SaveToFile();
    }

    public void SaveToFile()
    {
        try
        {
            XMLPath.EnsureDir();
            SyncDataFromUI();

            var xs = new XmlSerializer(typeof(Settings));
            using var fs = new FileStream(FilePath, FileMode.Create, FileAccess.Write, FileShare.None);
            xs.Serialize(fs, data);

            Debug.Log($"[Settings] Saved -> {FilePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Settings] Save failed: {ex}");
        }
    }

    public void LoadFromFile()
    {
        try
        {
            if (!File.Exists(FilePath))
            {
                ApplyDefaultsAndSave();
                return;
            }

            var xs = new XmlSerializer(typeof(Settings));
            using var fs = new FileStream(FilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            data = (Settings)xs.Deserialize(fs);

            SyncUIFromData();
            ApplyAll();
            Debug.Log($"[Settings] Loaded <- {FilePath}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Settings] Load failed: {ex.Message}");
            ApplyDefaultsAndSave();
        }
    }

    public void ApplyAll()
    {
        Screen.SetResolution(data.width, data.height, data.fullscreen);
        QualitySettings.vSyncCount = data.vSync;

        var cam = targetCamera ? targetCamera : Camera.main;
        if (cam) cam.fieldOfView = Mathf.Clamp(data.fov, 30f, 120f);

        var lights = GameObject.FindObjectsByType<Light>(FindObjectsSortMode.None);
        foreach (var l in lights)
        {
            l.shadows = data.shadows switch
            {
                0 => LightShadows.None,
                1 => LightShadows.Hard,
                _ => LightShadows.Soft
            };
        }

        SetAllAudio(data.volMaster, data.volBG, data.volSFX, data.muteAll);
    }

    void SetAllAudio(float master, float bg, float sfx, bool mute)
    {
        float m = Mathf.Clamp01(master);
        AudioSource[] audios = GameObject.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (var src in audios)
        {
            if (!src) continue;

            if (mute)
            {
                src.mute = true;
                continue;
            }

            src.mute = false;

            string n = src.name.ToLowerInvariant();

            if (n.Contains("bg"))
                src.volume = m * Mathf.Clamp01(bg);
            else if (n.Contains("sfx"))
                src.volume = m * Mathf.Clamp01(sfx);
            else
                src.volume = m;
        }
    }

    void SetupStyles()
    {
        _title = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            padding = new RectOffset(0, 0, 2, 6),
            richText = true
        };

        _section = new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            fontSize = 13,
            padding = new RectOffset(2, 0, 8, 4)
        };

        _valueTag = new GUIStyle(GUI.skin.box)
        {
            alignment = TextAnchor.MiddleCenter,
            fixedHeight = 20
        };

        _thinLine = new GUIStyle(GUI.skin.box);
        _padBox = new GUIStyle(GUI.skin.box)
        {
            padding = new RectOffset(12, 12, 10, 10)
        };
    }

    void Separator()
    {
        GUILayout.Space(6);
        GUILayout.Box(GUIContent.none, GUILayout.ExpandWidth(true), GUILayout.Height(1));
        GUILayout.Space(6);
    }

    GUIStyle EditorLabelBold()
    {
        return new GUIStyle(GUI.skin.label)
        {
            fontStyle = FontStyle.Bold,
            padding = new RectOffset(0, 0, 6, 4)
        };
    }

    void OnGUI()
    {
        if (!show) return;
        if (_title == null) SetupStyles();

        float targetW = Mathf.Clamp(Screen.width * 0.65f, 560f, Screen.width - 40f);
        float targetH = Mathf.Clamp(Screen.height * 0.70f, 420f, Screen.height - 80f);
        win.width = targetW;
        win.height = targetH;
        win.x = (Screen.width - win.width) * 0.5f;
        win.y = (Screen.height - win.height) * 0.5f;

        win = GUI.Window(42, win, DrawWindow, "Settings");
    }

    void DrawWindow(int id)
    {
        bool requestClose = false;

        GUILayout.BeginVertical(GUI.skin.box);

        GUILayout.Label("<b>Settings</b>", _title);

        scroll = GUILayout.BeginScrollView(scroll, false, true, GUILayout.Height(win.height - 80));

        GUILayout.Label("Quality", EditorLabelBold());
        int newQuality = GUILayout.Toolbar(quality, _qualityLabels);
        if (newQuality != quality)
        {
            quality = newQuality;
            int qIndex = (quality == 0) ? 0 : (quality == 1 ? 2 : 4);
            QualitySettings.SetQualityLevel(qIndex, true);
        }
        Separator();

        GUILayout.Label("Field of View", EditorLabelBold());
        GUILayout.BeginHorizontal();
        GUILayout.Label("40", GUILayout.Width(30));
        float newFov = GUILayout.HorizontalSlider(fov, 40f, 110f);
        GUILayout.Label("110", GUILayout.Width(30));
        GUILayout.Label(Mathf.RoundToInt(newFov).ToString(), GUI.skin.box, GUILayout.Width(42));
        GUILayout.EndHorizontal();
        if (!Mathf.Approximately(newFov, fov))
        {
            fov = newFov;
            if (targetCamera) targetCamera.fieldOfView = fov;
        }
        Separator();

        GUILayout.Label("Display", EditorLabelBold());
        GUILayout.BeginHorizontal();
        GUILayout.Label("Resolution", GUILayout.Width(90));
        int newRes = GUILayout.SelectionGrid(resIndex, ResLabels, Mathf.Max(1, ResLabels.Length), GUILayout.MaxWidth(360));
        bool newFS = GUILayout.Toggle(fullscreen, " Fullscreen", GUILayout.Width(110));
        GUILayout.EndHorizontal();
        if (newRes != resIndex || newFS != fullscreen)
        {
            resIndex = newRes;
            fullscreen = newFS;
            var r = ResOpts[Mathf.Clamp(resIndex, 0, ResOpts.Length - 1)];
            Screen.SetResolution(r.x, r.y, fullscreen);
        }
        Separator();

        GUILayout.Label("Shadows", EditorLabelBold());
        int newShadow = GUILayout.Toolbar(shadowMode, _shadowLabels);
        if (newShadow != shadowMode)
        {
            shadowMode = newShadow;
            foreach (var l in FindObjectsOfType<Light>())
                l.shadows = shadowMode == 0 ? LightShadows.None :
                            shadowMode == 1 ? LightShadows.Hard :
                                              LightShadows.Soft;
        }
        Separator();

        GUILayout.Label("VSync", EditorLabelBold());
        int newVs = GUILayout.Toolbar(vSync, _vsLabels, GUILayout.MaxWidth(160));
        if (newVs != vSync)
        {
            vSync = newVs;
            QualitySettings.vSyncCount = (vSync == 0) ? 0 : 1;
        }
        Separator();

        GUILayout.Label("Audio", EditorLabelBold());

        GUILayout.BeginHorizontal();
        GUILayout.Label("Master", GUILayout.Width(90));
        float newMaster = GUILayout.HorizontalSlider(volMaster, 0f, 1f);
        GUILayout.Label(Mathf.RoundToInt(newMaster * 100).ToString(), GUI.skin.box, GUILayout.Width(42));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Background Music", GUILayout.Width(120));
        float newBG = GUILayout.HorizontalSlider(volBG, 0f, 1f);
        GUILayout.Label(Mathf.RoundToInt(newBG * 100).ToString(), GUI.skin.box, GUILayout.Width(42));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("Sound Effects", GUILayout.Width(120));
        float newSFX = GUILayout.HorizontalSlider(volSFX, 0f, 1f);
        GUILayout.Label(Mathf.RoundToInt(newSFX * 100).ToString(), GUI.skin.box, GUILayout.Width(42));
        GUILayout.EndHorizontal();

        bool newMute = GUILayout.Toggle(muteAll, " Mute all");
        if (!Mathf.Approximately(newMaster, volMaster) ||
            !Mathf.Approximately(newBG, volBG) ||
            !Mathf.Approximately(newSFX, volSFX) ||
            newMute != muteAll)
        {
            volMaster = newMaster;
            volBG = newBG;
            volSFX = newSFX;
            muteAll = newMute;
            SetAllAudio(volMaster, volBG, volSFX, muteAll);
        }

        GUILayout.EndScrollView();

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Defaults", GUILayout.Height(24))) ApplyDefaultsAndSave();
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Apply & Save", GUILayout.Height(24))) SaveToFile();
        if (GUILayout.Button("Close", GUILayout.Width(80), GUILayout.Height(24))) requestClose = true;
        GUILayout.EndHorizontal();

        GUILayout.EndVertical();

        GUI.DragWindow(new Rect(0, 0, 10000, 24));
        if (requestClose) show = false;
    }

    void OnApplicationQuit()
    {
        try { SaveToFile(); } catch { }
    }
}
