using UnityEngine;

public class PortalMammothWalk : MonoBehaviour
{
    [Header("Portal Reference")]
    [Tooltip("Drag your Portal Window object here")]
    public Transform portalWindow;

    [Header("Movement Settings")]
    [Tooltip("Walking speed in meters per second")]
    public float walkSpeed = 0.8f;

    [Tooltip("How far past the doorway (in meters) the mammoth stops")]
    public float stopDistance = 1.5f;

    [Header("Animation Clip Names")]
    public string walkClipName = "Mammoth|Mammoth-Move-Fwd";
    public string idleClipName = "Mammoth|A - Mammoth-Idle";

    private Animation anim;
    private float initialSide;
    private bool hasCrossed = false;
    private bool hasStopped = false;

    void Start()
    {
        anim = GetComponent<Animation>();

        if (anim != null)
        {
            if (anim[walkClipName] != null)
            {
                anim[walkClipName].wrapMode = WrapMode.Loop;
                anim.Play(walkClipName);
            }

            if (anim[idleClipName] != null)
            {
                anim[idleClipName].wrapMode = WrapMode.Loop;
            }
        }

        // Record which side of the doorway the mammoth starts on
        if (portalWindow != null)
        {
            float startZ = portalWindow.InverseTransformPoint(transform.position).z;
            initialSide = startZ >= 0f ? 1f : -1f;
        }
    }

    void Update()
    {
        if (hasStopped || portalWindow == null) return;

        // Move forward along the mammoth's local Z-axis
        transform.Translate(Vector3.forward * walkSpeed * Time.deltaTime);

        float currentZ = portalWindow.InverseTransformPoint(transform.position).z;
        float currentSide = currentZ >= 0f ? 1f : -1f;

        // 1. Switch layer to Default the moment it crosses the Portal Window plane
        if (!hasCrossed && currentSide != initialSide)
        {
            SetLayerRecursively(gameObject, LayerMask.NameToLayer("Default"));
            hasCrossed = true;
        }

        // 2. Stop walking and transition to Idle once inside the real room
        if (hasCrossed && Mathf.Abs(currentZ) >= stopDistance)
        {
            hasStopped = true;
            if (anim != null && anim[idleClipName] != null)
            {
                anim.CrossFade(idleClipName, 0.3f);
            }
        }
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}