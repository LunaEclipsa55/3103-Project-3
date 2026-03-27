using UnityEngine;

public class PlayerBullet : MonoBehaviour
{
    public int damage = 10;
    public float lifetime = 5f;

    Rigidbody rb;
    bool hit;


    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        if(rb)
        {
            rb.useGravity = false;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Destroy(gameObject, lifetime);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if(hit) return;
        hit = true;

        if(collision.CompareTag("Enemy")) 
        {
            EnemyHealth enemy = collision.GetComponent<EnemyHealth>();
            if(!enemy)
            {
                enemy = collision.GetComponentInParent<EnemyHealth>();
            }

            if(enemy)
            {
                enemy.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if(!collision.CompareTag("PLayer"))
        {
            Destroy(gameObject);
        }
    }

    public void Launch(Vector3 direction)
    {
        if(!rb) rb = GetComponent<Rigidbody>();
        if(rb) rb.linearVelocity = direction;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
