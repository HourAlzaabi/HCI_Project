using UnityEngine;
using Photon.Pun;

public class PhotonAnimatorSync : MonoBehaviourPun
{
    private SpriteRenderer sr;
    private bool lastFlipX = false;

    void Update()
    {
        if (!photonView.IsMine) return;

        // Get sr every frame as fallback if null
        if (sr == null)
        {
            sr = GetComponent<SpriteRenderer>();
            if (sr == null)
            {
                Debug.LogError("SpriteRenderer still null!");
                return;
            }
        }

        if (sr.flipX != lastFlipX)
        {
            lastFlipX = sr.flipX;
            Debug.Log("Sending flip: " + sr.flipX);
            photonView.RPC("RPC_SyncFlip",
                RpcTarget.Others, sr.flipX);
        }
    }

    [PunRPC]
    void RPC_SyncFlip(bool flipX)
    {
        if (photonView.IsMine) return;

        if (sr == null)
            sr = GetComponent<SpriteRenderer>();

        if (sr == null)
        {
            Debug.LogError("sr NULL on remote when receiving flip!");
            return;
        }

        sr.flipX = flipX;
        Debug.Log("Flip applied: " + flipX);
    }
}