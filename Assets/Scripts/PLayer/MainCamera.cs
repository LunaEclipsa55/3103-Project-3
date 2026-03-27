using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class MainCamera : MonoBehaviour
{
    [Header("Look")]
    public float mouseSensitivity = 1.6f;
    public float keyLookSensitivity = 240f;
    public float maxMouseStep = 0f;
    public float maxKeyStep = 0f;
    public float maxPitch = 85f;
    public float minPitch = -85f;

    [Header("Movement")]
    public bool enableMovement = true;
    public float moveSpeed = 6f;
    public float sprintMultiplier = 1.6f;

    [Header("Jump / Gravity")]
    public bool allowJump = false;
    public float jumpForce = 5f;
    public float gravity = -9.81f;
    public KeyCode jumpKey = KeyCode.Space;

    [Header("Inventory Integration")]
    public bool pauseWhenInventoryOpen = true;

    [Header("Control Scheme")]
    public int scheme = 1;

    [Header("Scheme 1")]
    public KeyCode forwardKey = KeyCode.W;
    public KeyCode backwardKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;
    public KeyCode sprintKey = KeyCode.LeftShift;

    [Header("Scheme 2")]
    public KeyCode forwardKey2 = KeyCode.T;
    public KeyCode backwardKey2 = KeyCode.G;
    public KeyCode leftKey2 = KeyCode.F;
    public KeyCode rightKey2 = KeyCode.H;
    public KeyCode lookLeftKey = KeyCode.LeftArrow;
    public KeyCode lookRightKey = KeyCode.RightArrow;
    public KeyCode lookUpKey = KeyCode.UpArrow;
    public KeyCode lookDownKey = KeyCode.DownArrow;
    public KeyCode sprintKey2 = KeyCode.LeftControl;

    string keyToChange = "null";
    bool keyChanging = false;

    bool showControls;
    Rect panelRect = new Rect(40, 90, 820, 420);

    float yaw;
    float pitch;
    CharacterController cc;
    float verticalVel;

    void Start()
    {
        cc = GetComponent<CharacterController>();

        Vector3 e = transform.eulerAngles;
        yaw = e.y;
        pitch = e.x;

        showControls = false;
        LockCursor(true);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) scheme = 1;
        if (Input.GetKeyDown(KeyCode.Alpha2)) scheme = 2;

        bool invOpen = pauseWhenInventoryOpen
                       && Inventory.Instance != null
                       && Inventory.Instance.showInventory;

        if (invOpen)
        {
            LockCursor(false);
            return;
        }
        else
        {
            LockCursor(true);
        }

        Vector2 look = LookUpdate();
        yaw += look.x;
        pitch -= look.y;
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
        transform.rotation = Quaternion.Euler(pitch, yaw, 0f);

        if (enableMovement)
            MoveUpdate();

        ChangeKeyCode();
    }

    void LockCursor(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    Vector2 LookUpdate()
    {
        if (scheme == 1)
        {
            float mx = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
            float my = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

            if (maxMouseStep > 0f)
            {
                mx = Mathf.Clamp(mx, -maxMouseStep, maxMouseStep);
                my = Mathf.Clamp(my, -maxMouseStep, maxMouseStep);
            }

            return new Vector2(mx, my);
        }
        else
        {
            float yawIn = (Input.GetKey(lookRightKey) ? 1f : 0f) - (Input.GetKey(lookLeftKey) ? 1f : 0f);
            float pitchIn = (Input.GetKey(lookUpKey) ? 1f : 0f) - (Input.GetKey(lookDownKey) ? 1f : 0f);

            float dx = yawIn * keyLookSensitivity * Time.deltaTime;
            float dy = pitchIn * keyLookSensitivity * Time.deltaTime;

            if (maxKeyStep > 0f)
            {
                dx = Mathf.Clamp(dx, -maxKeyStep, maxKeyStep);
                dy = Mathf.Clamp(dy, -maxKeyStep, maxKeyStep);
            }

            return new Vector2(dx, dy);
        }
    }

    void MoveUpdate()
    {
        KeyCode f = (scheme == 1) ? forwardKey : forwardKey2;
        KeyCode b = (scheme == 1) ? backwardKey : backwardKey2;
        KeyCode l = (scheme == 1) ? leftKey : leftKey2;
        KeyCode r = (scheme == 1) ? rightKey : rightKey2;
        KeyCode sprint = (scheme == 1) ? sprintKey : sprintKey2;

        float ix = (Input.GetKey(r) ? 1f : 0f) - (Input.GetKey(l) ? 1f : 0f);
        float iz = (Input.GetKey(f) ? 1f : 0f) - (Input.GetKey(b) ? 1f : 0f);

        Vector3 input = new Vector3(ix, 0f, iz);
        if (input.sqrMagnitude > 1f) input.Normalize();

        float speed = moveSpeed * (Input.GetKey(sprint) ? sprintMultiplier : 1f);
        Vector3 moveWorld = transform.TransformDirection(input) * speed;

        if (cc)
        {
            if (cc.isGrounded && verticalVel < 0f)
                verticalVel = -2f;

            verticalVel += gravity * Time.deltaTime;

            if (allowJump && cc.isGrounded && Input.GetKeyDown(jumpKey))
                verticalVel = jumpForce;

            moveWorld.y = verticalVel;
            cc.Move(moveWorld * Time.deltaTime);
        }
        else
        {
            transform.position += moveWorld * Time.deltaTime;
        }
    }

    void ChangeKeyCode()
    {
        if (!keyChanging) return;

        if (Input.anyKeyDown)
        {
            foreach (KeyCode kcode in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (kcode == KeyCode.None) continue;

                if (Input.GetKeyDown(kcode))
                {
                    ApplyNewKey(kcode);
                    keyChanging = false;
                    break;
                }
            }
        }
    }

    void ApplyNewKey(KeyCode newKey)
    {
        switch (keyToChange)
        {
            case "Forward": if (scheme == 1) forwardKey = newKey; else forwardKey2 = newKey; break;
            case "Backward": if (scheme == 1) backwardKey = newKey; else backwardKey2 = newKey; break;
            case "Left": if (scheme == 1) leftKey = newKey; else leftKey2 = newKey; break;
            case "Right": if (scheme == 1) rightKey = newKey; else rightKey2 = newKey; break;
            case "TurnLeft": lookLeftKey = newKey; break;
            case "TurnRight": lookRightKey = newKey; break;
            case "LookUp": lookUpKey = newKey; break;
            case "LookDown": lookDownKey = newKey; break;
        }

        keyToChange = "null";
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(16, 64, 160, 22), "View Controls"))
            showControls = !showControls;

        if (!showControls) return;

        GUI.BeginGroup(panelRect, GUI.skin.box);
        float w = panelRect.width;
        float h = panelRect.height;

        var header = new GUIStyle(GUI.skin.label)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };

        GUI.Label(new Rect(8, 6, w - 16, 22), "Control Groups", header);

        if (GUI.Button(new Rect(16, 40, 260, 26), "(LS 1) WASD + mouse")) scheme = 1;
        if (GUI.Button(new Rect(292, 40, 260, 26), "(LS 2) TGFH -><-")) scheme = 2;

        float x = 40f;
        float y = 100f;
        float row = 28f;
        float bw = panelRect.width - 2f * x - 40f;

        DrawRow(ref y, bw, "Forward", "Forward");
        DrawRow(ref y, bw, "Backward", "Backward");
        DrawRow(ref y, bw, "Left", "Left");
        DrawRow(ref y, bw, "Right", "Right");
        DrawRow(ref y, bw, "Rot L", "TurnLeft");
        DrawRow(ref y, bw, "Rot R", "TurnRight");
        DrawRow(ref y, bw, "Look Up", "LookUp");
        DrawRow(ref y, bw, "Look Down", "LookDown");

        if (keyChanging)
            GUI.Label(new Rect(x, h - 58, 240, 20), "Press any key...");

        GUI.EndGroup();
    }

    void DrawRow(ref float y, float bw, string label, string token)
    {
        KeyCode current = token switch
        {
            "Forward" => (scheme == 1 ? forwardKey : forwardKey2),
            "Backward" => (scheme == 1 ? backwardKey : backwardKey2),
            "Left" => (scheme == 1 ? leftKey : leftKey2),
            "Right" => (scheme == 1 ? rightKey : rightKey2),
            "TurnLeft" => lookLeftKey,
            "TurnRight" => lookRightKey,
            "LookUp" => lookUpKey,
            "LookDown" => lookDownKey,
            _ => KeyCode.None
        };

        if (GUI.Button(new Rect(40, y, bw, 24), $"{label}: {current}"))
        {
            keyToChange = token;
            keyChanging = true;
        }

        y += row + 6f;
    }
}
