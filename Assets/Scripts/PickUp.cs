using UnityEngine;

public class Pickup : MonoBehaviour
{
    public string itemName = "Health10";
    public int amount = 1;
    public bool destroyOnPickup = true;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        var inv = Inventory.Instance ?? FindFirstObjectByType<Inventory>();
        if (!inv) return;

        bool added = inv.AddToInventory(amount, itemName);
        if (added && destroyOnPickup)
            Destroy(gameObject);
    }
}
