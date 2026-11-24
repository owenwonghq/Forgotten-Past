using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacePortalOnFloor : MonoBehaviour
{
    [Tooltip("Drag AR_Portal_Root here")]
    public Transform portalRoot;

    [Header("Wall Placement Settings")]
    [Tooltip("Check this if the door spawns facing into the wall instead of out into the room")]
    public bool flipFacingDirection = false;

    [Tooltip("Aligns the bottom of the doorway with the detected floor height so the mammoth doesn't float")]
    public bool snapToDetectedFloor = true;

    [Tooltip("Manual vertical adjustment (in meters) if needed")]
    public float verticalOffset = 0f;

    private ARRaycastManager raycastManager;
    private ARAnchorManager anchorManager;
    private ARPlaneManager planeManager;
    private ARAnchor currentAnchor;
    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        anchorManager = GetComponent<ARAnchorManager>();
        planeManager = GetComponent<ARPlaneManager>();
    }

    async void Update()
    {
        if (portalRoot == null || raycastManager == null || Pointer.current == null) return;

        if (Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();

            if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            {
                // Filter specifically for a vertical wall hit
                ARRaycastHit? wallHit = null;
                foreach (var hit in hits)
                {
                    if (hit.trackable is ARPlane plane && plane.alignment == PlaneAlignment.Vertical)
                    {
                        wallHit = hit;
                        break;
                    }
                }

                if (!wallHit.HasValue) return;

                Pose hitPose = wallHit.Value.pose;

                // Clean up the previous anchor if repositioning the portal
                if (currentAnchor != null)
                {
                    portalRoot.SetParent(null);
                    Destroy(currentAnchor.gameObject);
                    currentAnchor = null;
                }

                // Create an AR Anchor at the tapped spot on the wall
                if (anchorManager != null)
                {
                    var result = await anchorManager.TryAddAnchorAsync(hitPose);
                    if (result.status.IsSuccess())
                    {
                        currentAnchor = result.value;
                        portalRoot.SetParent(currentAnchor.transform);
                    }
                }

                // Snap the doorway's base to the floor height while keeping it flush against the wall
                float yOffset = verticalOffset;
                if (snapToDetectedFloor && TryGetFloorY(out float floorY))
                {
                    yOffset += (floorY - hitPose.position.y);
                }

                if (currentAnchor != null)
                    portalRoot.localPosition = new Vector3(0f, yOffset, 0f);
                else
                    portalRoot.position = hitPose.position + new Vector3(0f, yOffset, 0f);

                // In AR Foundation, hitPose.up is the surface normal pointing outward from the wall
                Vector3 wallNormal = hitPose.up;
                wallNormal.y = 0f; // Keep the doorway strictly upright

                if (wallNormal.sqrMagnitude > 0.001f)
                {
                    Vector3 forwardDir = flipFacingDirection ? -wallNormal.normalized : wallNormal.normalized;
                    portalRoot.rotation = Quaternion.LookRotation(forwardDir, Vector3.up);
                }
            }
        }
    }

    private bool TryGetFloorY(out float lowestFloorY)
    {
        lowestFloorY = float.MaxValue;
        bool foundFloor = false;

        if (planeManager == null || Camera.main == null) return false;

        float cameraY = Camera.main.transform.position.y;

        foreach (ARPlane plane in planeManager.trackables)
        {
            if (plane.alignment == PlaneAlignment.HorizontalUp && plane.transform.position.y < cameraY)
            {
                if (plane.transform.position.y < lowestFloorY)
                {
                    lowestFloorY = plane.transform.position.y;
                    foundFloor = true;
                }
            }
        }

        return foundFloor;
    }
}