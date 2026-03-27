using UnityEngine;

public class WaypointSnapper : MonoBehaviour
{
    public LayerMask groundMask = ~0;
    public float rayHeight = 100f;
    public float yOffset = 0f;

    [ContextMenu("Snap All Children To Ground")]
    public void SnapAllChildrenToGround()
    {
        foreach (Transform child in transform)
        {
            Vector3 start = child.position + Vector3.up * rayHeight;
            if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, rayHeight * 2f, groundMask, QueryTriggerInteraction.Ignore))
            {
                child.position = hit.point + Vector3.up * yOffset;
            }
        }
    }
}
