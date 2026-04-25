using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;
using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Inventory : MonoBehaviour
{
    public class Entry
    {
        public string name;
        public int amount;
        public Entry(string n, int a) { name = n; amount = a; }
    }

    public static Inventory Instance { get; private set; }

    public PlayerStats playerStats;
    public Gun playerGun;

    public KeyCode toggleInventoryKey = KeyCode.I;
    public KeyCode hubKey = KeyCode.F1;

    public int maxUnique = 12;

    public bool showInventory;
    public bool showhud;
    public Rect invRect = new Rect(20, 20, 680, 560);
    public Rect quickRect = new Rect(20, Screen.height - 100, 520, 80);

    string popupMsg = "";
    float popupTimer;
    Vector2 scrollPos;

    public LinkedList<Entry> items = new LinkedList<Entry>();
    public List<string> quickbar = new List<string> { "Empty", "Empty", "Empty", "Empty" };

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    void Start()
    {
        showInventory = false;
        showhud = true;
    }

    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        var k = Keyboard.current;
        if (k != null)
        {
            if (k.iKey.wasPressedThisFrame) showInventory = !showInventory;
            if (k.f1Key.wasPressedThisFrame) showhud = !showhud;

            if (k.digit1Key.wasPressedThisFrame) Use(0);
            if (k.digit2Key.wasPressedThisFrame) Use(1);
            if (k.digit3Key.wasPressedThisFrame) Use(2);
            if (k.digit4Key.wasPressedThisFrame) Use(3);

            if (k.f5Key.wasPressedThisFrame) SaveToFile();
            if (k.f9Key.wasPressedThisFrame) LoadFromFile();
        }
#else
        if (Input.GetKeyDown(toggleInventoryKey)) showInventory = !showInventory;
        if (Input.GetKeyDown(hubKey)) showhud = !showhud;
        if (Input.GetKeyDown(KeyCode.Alpha1)) Use(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) Use(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) Use(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) Use(3);
        if (Input.GetKeyDown(KeyCode.F5)) SaveToFile();
        if (Input.GetKeyDown(KeyCode.F9)) LoadFromFile();
#endif

        if (popupTimer > 0) popupTimer -= Time.deltaTime;
    }

    LinkedListNode<Entry> FindNodeByName(string itemName)
    {
        for (var n = items.First; n != null; n = n.Next)
            if (n.Value.name == itemName) return n;
        return null;
    }

    public int UniqueCount
    {
        get
        {
            int c = 0;
            for (var n = items.First; n != null; n = n.Next) c++;
            return c;
        }
    }

    public Entry GetAt(int index)
    {
        int i = 0;
        for (var n = items.First; n != null; n = n.Next, i++)
            if (i == index) return n.Value;
        return null;
    }

    public bool AddToInventory(int amount, string item)
    {
        if (amount <= 0 || string.IsNullOrEmpty(item)) return false;

        var node = FindNodeByName(item);
        if (node != null)
        {
            node.Value.amount += amount;
            SaveToFile();
            return true;
        }

        if (UniqueCount >= maxUnique)
        {
            ShowPopup("Storage is full! Cannot take more different items.");
            return false;
        }

        items.AddLast(new Entry(item, amount));
        SaveToFile();
        return true;
    }

    public void RemoveFromInventory(int amount, string itemName)
    {
        if (amount <= 0 || string.IsNullOrEmpty(itemName)) return;

        var node = FindNodeByName(itemName);
        if (node == null) return;

        node.Value.amount -= amount;
        if (node.Value.amount <= 0)
        {
            items.Remove(node);

            for (int i = 0; i < quickbar.Count; i++)
                if (quickbar[i] == itemName) quickbar[i] = "Empty";
        }

        SaveToFile();
    }

    public int AmountInInventory(string itemName)
    {
        var node = FindNodeByName(itemName);
        return (node != null) ? node.Value.amount : 0;
    }

    void SetQuickItem(int slot, string itemName)
    {
        if (slot < 0 || slot >= 4) return;
        if (string.IsNullOrEmpty(itemName)) return;
        if (FindNodeByName(itemName) == null) return;
        quickbar[slot] = itemName;
        SaveToFile();
    }

    void Use(int spot)
    {
        if (spot < 0 || spot >= 4) return;
        string n = quickbar[spot];
        if (n == "Empty") return;
        UseItem(n, 1);
    }

    void UseItem(string itemName, int num)
    {
        if (num <= 0) return;

        bool consumed = ApplyItemEffect(itemName);
        if (!consumed) return;

        RemoveFromInventory(num, itemName);
    }

    void ShowPopup(string msg)
    {
        popupMsg = msg;
        popupTimer = 3f;
    }

    bool ApplyItemEffect(string itemName)
    {
        if (playerStats == null) playerStats = FindFirstObjectByType<PlayerStats>();
        if (playerGun == null) playerGun = FindFirstObjectByType<Gun>();

        if (itemName == "Health10" || itemName == "Health20" || itemName == "Health30")
        {
            int amt = 0;
            if (itemName == "Health10") amt = 10;
            else if (itemName == "Health20") amt = 20;
            else if (itemName == "Health30") amt = 30;

            if (HealIfPossible(amt))
            {
                ShowPopup($"Healed {amt} HP");
                return true;
            }

            ShowPopup("Health is full");
            return false;
        }

        if (itemName == "AmmoRed" || itemName == "AmmoGreen" || itemName == "AmmoYellow")
        {
            if (playerGun)
            {
                playerGun.Equip(itemName);
                ShowPopup($"Equipped {itemName}");
            }
            return false;
        }

        return false;
    }

    bool HealIfPossible(int amount)
    {
        if (!playerStats) return false;
        if (playerStats.health >= playerStats.healthMax) return false;
        playerStats.Heal(amount);
        return true;
    }

    public bool Loadable() => File.Exists(XMLPath.InventoryXml);

    public void ClearToNewGame()
    {
        items.Clear();
        if (quickbar == null || quickbar.Count != 4)
            quickbar = new List<string> { "Empty", "Empty", "Empty", "Empty" };
        else
            for (int i = 0; i < 4; i++) quickbar[i] = "Empty";
    }

    public void SaveToFile()
    {
        try
        {
            XMLPath.EnsureDir();

            var root = new XElement("inventory");

            var xItems = new XElement("items");
            for (var n = items.First; n != null; n = n.Next)
            {
                var e = n.Value;
                if (!string.IsNullOrEmpty(e.name) && e.amount > 0)
                {
                    xItems.Add(new XElement("item",
                        new XAttribute("name", e.name),
                        new XAttribute("amount", e.amount)));
                }
            }
            root.Add(xItems);

            var xQuick = new XElement("quickbar");
            for (int i = 0; i < 4; i++)
            {
                string q = (i < quickbar.Count) ? quickbar[i] : "Empty";
                bool present = !string.IsNullOrEmpty(q) && FindNodeByName(q) != null;
                xQuick.Add(new XElement("slot",
                    new XAttribute("index", i),
                    present ? q : "Empty"));
            }
            root.Add(xQuick);

            new XDocument(root).Save(XMLPath.InventoryXml);
            Debug.Log($"[Inventory] Saved: {XMLPath.InventoryXml}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Inventory] Save failed: {ex}");
        }
    }

    public void LoadFromFile()
    {
        try
        {
            if (!File.Exists(XMLPath.InventoryXml))
            {
                Debug.Log($"[Inventory] No save file at {XMLPath.InventoryXml}");
                ClearToNewGame();
                return;
            }

            var doc = XDocument.Load(XMLPath.InventoryXml);
            var root = doc.Element("inventory");
            if (root == null) throw new System.Exception("Missing <inventory> root.");

            items.Clear();
            var itemsElem = root.Element("items");
            if (itemsElem != null)
            {
                foreach (var xe in itemsElem.Elements("item"))
                {
                    string nm = (string)xe.Attribute("name") ?? "";
                    int amt = (int?)xe.Attribute("amount") ?? 0;
                    if (!string.IsNullOrEmpty(nm) && amt > 0)
                        items.AddLast(new Entry(nm, amt));
                }
            }

            if (quickbar == null || quickbar.Count != 4)
                quickbar = new List<string> { "Empty", "Empty", "Empty", "Empty" };

            for (int i = 0; i < 4; i++) quickbar[i] = "Empty";

            var quickElem = root.Element("quickbar");
            if (quickElem != null)
            {
                foreach (var slot in quickElem.Elements("slot"))
                {
                    int idx = (int?)slot.Attribute("index") ?? -1;
                    string val = (slot.Value ?? "Empty").Trim();
                    if (idx >= 0 && idx < 4 && FindNodeByName(val) != null)
                        quickbar[idx] = val;
                }
            }

            Debug.Log($"[Inventory] Loaded: {XMLPath.InventoryXml}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[Inventory] Load failed: {ex}");
            ClearToNewGame();
        }
    }

    void OnApplicationQuit()
    {
        try { SaveToFile(); } catch { }
    }

    void OnGUI()
    {
        if (showhud)
        {
            GUI.Box(new Rect(16, 16, 320, 28), "Player Health: " + playerStats.health + "/" + playerStats.healthMax);

            if (playerGun)
            {
                string eq = playerGun.EquippedName;
                int cnt = !string.IsNullOrEmpty(eq) ? AmountInInventory(eq) : 0;
                GUI.Box(new Rect(16, 48, 240, 22), $"Equipped: {(string.IsNullOrEmpty(eq) ? "(none)" : eq)} x{cnt}");
            }

            if (popupTimer > 0)
            {
                var style = GUI.skin.box;
                style.wordWrap = true;

                float w = Mathf.Min(640f, Screen.width * 0.9f);
                float h = style.CalcHeight(new GUIContent(popupMsg), w);

                var rect = new Rect((Screen.width - w) * 0.5f, Screen.height - (h + 120f), w, h + 8f);
                GUI.Box(rect, popupMsg, style);
            }
        }

        DrawQuickbar();

        if (showInventory)
            invRect = GUI.Window(100, invRect, InventoryGUI, "Inventory (Linked List)");
    }

    void DrawQuickbar()
    {
        Rect quickRect = new Rect(20, Screen.height - 100, 520, 80);
        GUI.Box(quickRect, "Quick Items (1-4)");

        GUILayout.BeginArea(new Rect(quickRect.x + 8, quickRect.y + 20, quickRect.width - 16, quickRect.height - 28));
        GUILayout.BeginHorizontal();

        for (int i = 0; i < 4; i++)
        {
            GUILayout.BeginVertical(GUILayout.Width(120));

            string n = (i < quickbar.Count) ? quickbar[i] : "Empty";
            int amt = (n == "Empty") ? 0 : AmountInInventory(n);

            if (GUILayout.Button($"{i + 1}. {(n == "Empty" ? "(Empty)" : $"{n} x{amt}")}", GUILayout.Height(40)))
            {
                if (n != "Empty") UseItem(n, 1);
            }

            GUILayout.EndVertical();
        }

        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }

    void InventoryGUI(int windowID)
    {
        int unique = UniqueCount;
        int minRows = 6;

        GUILayout.Space(6);
        GUILayout.Label($"Unique Items: {unique} / {maxUnique}");

        GUILayout.BeginHorizontal();
        if (GUILayout.Button("Save Inventory")) SaveToFile();
        if (GUILayout.Button("Load Inventory")) LoadFromFile();
        GUILayout.EndHorizontal();

        float scrollHeight = invRect.height - 90f;
        scrollPos = GUILayout.BeginScrollView(scrollPos, GUILayout.Height(scrollHeight));

        for (var n = items.First; n != null; n = n.Next)
        {
            var e = n.Value;
            GUILayout.BeginHorizontal("box", GUILayout.Height(32));

            GUILayout.Label(e.name, GUILayout.Width(180));
            GUILayout.Label($"x{e.amount}", GUILayout.Width(60));

            if (GUILayout.Button("Use", GUILayout.Width(50)))
                UseItem(e.name, 1);

            GUILayout.Label("Assign:", GUILayout.Width(55));
            for (int s = 0; s < 4; s++)
            {
                if (GUILayout.Button($"{s + 1}", GUILayout.Width(30)))
                    SetQuickItem(s, e.name);
            }

            GUILayout.EndHorizontal();
        }

        int rowsToShow = Mathf.Min(Mathf.Max(unique, minRows), maxUnique);
        for (int i = unique; i < rowsToShow; i++)
        {
            GUILayout.BeginHorizontal("box", GUILayout.Height(32));
            GUILayout.Label("(Empty)", GUILayout.Width(240));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        GUILayout.EndScrollView();

        GUI.DragWindow(new Rect(0, 0, 10000, 20));
    }
}
