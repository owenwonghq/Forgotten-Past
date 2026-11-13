using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlacePortalOnFloor : MonoBehaviour
{
    [Tooltip("Drag AR_Portal_Root here")]
    public Transform portalRoot;

    private ARRaycastManager raycastManager;
    private static readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
    }

    void Update()
    {
        if (portalRoot == null || raycastManager == null || Pointer.current == null) return;

        if (Pointer.current.press.wasPressedThisFrame)
        {
            Vector2 screenPos = Pointer.current.position.ReadValue();

            if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
            {
                portalRoot.position = hits[0].pose.position;

                // Keep the portal upright and facing toward the camera
                Vector3 lookTarget = Camera.main.transform.position;
                lookTarget.y = portalRoot.position.y;
                portalRoot.LookAt(lookTarget);
            }
        }
    }
}
