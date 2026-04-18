using UnityEngine;
using System.Collections.Generic;

public class Gun : MonoBehaviour
{
    [Header("Ammo Prefabs")]
    public PlayerBullet red;
    public PlayerBullet yellow;
    public PlayerBullet green;

    [Header("Firing")]
    public Transform firePoint;
    public float bulletSpeed = 20f;
    public float fireCool = 0.2f;
    public bool enableMouse = true;

    float fireTimer;
    static int lastShotFrame = -1;

    Dictionary<string, PlayerBullet> ammoMap;
    string equippedName = null;
    PlayerBullet equippedPrefab = null;

    public string EquippedName => equippedName;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        ammoMap = new Dictionary<string, PlayerBullet>
        {
            { "AmmoRed", red },
            { "AmmoYellow", yellow },
            { "AmmoGreen", green }
        };
    }

    public void Equip(string ammoName)
    {
        if(ammoMap.TryGetValue(ammoName, out var pb) && pb != null)
        {
            equippedName = ammoName;
            equippedPrefab = pb;
            Debug.Log($"Equipped {EquippedName}");
        }
        else
        {
            Debug.LogError($"Equip failed: no prefab for {ammoName}");
        }
    }

    public bool TryFire()
    {
        if(!equippedPrefab || string.IsNullOrEmpty(equippedName)) return false;
        if(!firePoint) return false;
        if (Time.time < fireTimer) return false;
        if(Time.frameCount == lastShotFrame) return false;

        var inv = Inventory.Instance;
        if(!inv || inv.AmountInInventory(equippedName) <= 0)
        {
            Debug.Log("Out of ammo or no inventory!");
            return false;
        }

        var bullet = Instantiate(equippedPrefab, firePoint.position, firePoint.rotation);

        var bCol = bullet.GetComponent<Collider>();
        // if(bCol)
        // {
        //     foreach (var c in GetComponentInChildren<Collider>())
        //     {
        //         Physics.IgnoreCollision(bCol, c, true);
        //     }
        // }

        bullet.Launch(firePoint.forward * bulletSpeed);

        inv.RemoveFromInventory(1, equippedName);

        lastShotFrame = Time.frameCount;
        fireTimer = Time.time + fireCool;
        return true;
    }

    // Update is called once per frame
    void Update()
    {
#if ENABLE_INPUT_SYSTEM
        // var m = Mouse.current;
        // if(enableMouse && m != null && m.leftButton.wasPressedThisFrame)
        // {
        //     TryFire();
        // }
#else
        if(enableMouse && Input.GetMouseButtonDown(0))
        {
            TryFire();
        }
#endif
    }
}
