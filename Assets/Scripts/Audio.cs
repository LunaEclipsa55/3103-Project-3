using UnityEngine;

public class Audio : MonoBehaviour
{
    [Header("Player refs")]
    public CharacterController cc;
    public Rigidbody rb;

    [Header("Audio")]
    [SerializeField] public AudioSource BackgroundMusic;
    [SerializeField] public AudioClip Walk;
    [SerializeField] public AudioClip Run;

    [Header("Footstep Settings")]
    public float speedThreshold = 0.15f;
    public KeyCode runKey = KeyCode.LeftShift;
    public float footstepVolume = 0.9f;
    public bool footsteps3D = true;

    AudioSource footstepSource;

    public void Awake()
    {
        if (!cc) cc = GetComponent<CharacterController>();
        if (!rb) rb = GetComponent<Rigidbody>();

        if (!BackgroundMusic)
        {
            BackgroundMusic = gameObject.AddComponent<AudioSource>();
            BackgroundMusic.playOnAwake = false;
            BackgroundMusic.loop = true;
            BackgroundMusic.spatialBlend = 0f;
        }

        footstepSource = gameObject.AddComponent<AudioSource>();
        footstepSource.playOnAwake = false;
        footstepSource.loop = true;
        footstepSource.volume = footstepVolume;
        footstepSource.spatialBlend = footsteps3D ? 1f : 0f;
        footstepSource.rolloffMode = AudioRolloffMode.Linear;
        footstepSource.minDistance = 2f;
        footstepSource.maxDistance = 20f;

        if (!Walk) Walk = Resources.Load<AudioClip>("Audio/Footsteps/Walking");
        if (!Run) Run = Resources.Load<AudioClip>("Audio/Footsteps/Running");
    }

    void Start()
    {
        if (BackgroundMusic.clip && !BackgroundMusic.isPlaying)
            BackgroundMusic.Play();
    }

    void Update()
    {
        Vector3 v = Vector3.zero;
        bool grounded = false;

        if (cc)
        {
            v = cc.velocity;
            grounded = cc.isGrounded;
        }
        else if (rb)
        {
            v = rb.linearVelocity;
            grounded = IsGroundedRB();
        }

        v.y = 0f;
        float speed = v.magnitude;
        bool isMoving = grounded && speed > speedThreshold;
        bool isRun = isMoving &&
#if ENABLE_INPUT_SYSTEM
                     UnityEngine.InputSystem.Keyboard.current != null &&
                     UnityEngine.InputSystem.Keyboard.current.leftShiftKey.isPressed;
#else
                     Input.GetKey(runKey);
#endif

        AudioClip desired = null;
        if (isMoving)
            desired = isRun && Run ? Run : Walk;

        if (desired != null)
        {
            if (footstepSource.clip != desired || !footstepSource.isPlaying)
            {
                footstepSource.clip = desired;
                footstepSource.volume = footstepVolume;
                footstepSource.Play();
            }
        }
        else
        {
            if (footstepSource.isPlaying)
                footstepSource.Stop();
        }
    }

    bool IsGroundedRB()
    {
        Vector3 origin = transform.position + Vector3.up * 0.1f;
        float radius = 0.25f;
        float maxDist = 0.2f;
        return Physics.SphereCast(origin, radius, Vector3.down, out _, maxDist, ~0, QueryTriggerInteraction.Ignore);
    }
}
