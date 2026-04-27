using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerInteract : MonoBehaviourPun
{
    public float interactDistance = 3f;
    public Camera playerCamera;

    void Update()
    {
        // Only interact on YOUR player
        if (!photonView.IsMine) return;

        var kb = Keyboard.current;
        if (kb == null) return;

        if (kb.eKey.wasPressedThisFrame)
        {
            if (playerCamera == null) return;

            Ray ray = playerCamera.ViewportPointToRay(
                new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, interactDistance))
            {
                PickupItem pickup = hit.collider.GetComponent<PickupItem>();
                if (pickup != null)
                {
                    pickup.PickUp();
                    return;
                }

                ElectricBox box = hit.collider.GetComponent<ElectricBox>();
                if (box != null)
                {
                    box.TryDisable();
                    return;
                }
            }
        }
    }
}