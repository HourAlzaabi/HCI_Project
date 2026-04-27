using Photon.Pun;
using Platformer.Mechanics;
using Platformer.Model;
using Platformer.Core;
using UnityEngine;
using System.Collections;
using System.Linq;

public class PhotonPlayerSetup : MonoBehaviourPun
{
    private SpriteRenderer sr;
    private PlayerController pc;
    private KinematicObject ko;
    private Animator anim;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        pc = GetComponent<PlayerController>();
        ko = GetComponent<KinematicObject>();
        anim = GetComponent<Animator>();
    }

    void Start()
    {
        // Basic visual enable
        if (sr != null)
        {
            sr.enabled = true;
            sr.color = Color.white;
        }

        // Remove old duplicates for same owner (helps when old objects survive scene reload)

        if (photonView.IsMine)
        {
            ApplyLocalVisuals();
            StartCoroutine(RegisterWithDelay());

            // Register this player's movement scripts with LevelManager
            StartCoroutine(RegisterMovementScripts());
        }
        else
        {
            // Remote player: disable local-only scripts, read owner props (if present)
            ApplyRemoteSetup();
            Debug.Log("[PhotonPlayerSetup] Remote player started. OwnerActor: " + photonView.OwnerActorNr);
        }
        
    }
    IEnumerator RegisterMovementScripts()
    {
        yield return null; // wait one frame for LevelManager to exist

        if (LevelManager.Instance != null && pc != null && ko != null)
        {
            LevelManager.Instance.RegisterPlayerScripts(pc, ko);
            Debug.Log("[PhotonPlayerSetup] Registered movement scripts with LevelManager.");
        }
        else
        {
            Debug.LogError("[PhotonPlayerSetup] Could not register scripts - LevelManager.Instance: "
                + LevelManager.Instance + " pc: " + pc + " ko: " + ko);
        }
    }
    void ApplyLocalVisuals()
    {
        if (sr != null)
        {
            sr.color = Color.white; // sprite is on the prefab, just ensure no tint
        }

        if (pc != null)
        {
            pc.enabled = true;
            pc.controlEnabled = true;
        }

        if (ko != null) ko.enabled = true;
    }

    void ApplyRemoteSetup()
    {
        // No color tint anymore — visual identity comes from the prefab itself
        if (sr != null) sr.color = Color.white;

        if (pc != null)
        {
            pc.enabled = false;
            pc.controlEnabled = false;
            Debug.Log("[PhotonPlayerSetup] PlayerController disabled on remote player");
        }

        if (ko != null)
        {
            ko.enabled = false;
            Debug.Log("[PhotonPlayerSetup] KinematicObject disabled on remote player");
        }

        if (anim != null)
        {
            anim.SetBool("grounded", true);
            anim.SetFloat("velocityX", 0f);
        }
    }

    IEnumerator RegisterWithDelay()
    {
        // local only
        if (!photonView.IsMine) yield break;

        // Wait a frame so other scene setup runs first
        yield return null;

        var model = Simulation.GetModel<PlatformerModel>();

        if (model == null)
        {
            Debug.LogError("[PhotonPlayerSetup] PlatformerModel NULL - is GameController in scene?");
            yield break;
        }

        if (pc == null)
        {
            Debug.LogError("[PhotonPlayerSetup] PlayerController NULL on prefab!");
            yield break;
        }

        model.player = pc;
        pc.controlEnabled = true;
        pc.enabled = true;

        if (model.virtualCamera != null)
        {
            model.virtualCamera.Follow = transform;
            model.virtualCamera.LookAt = transform;
            Debug.Log("[PhotonPlayerSetup] Camera following my player!");
        }
        else
        {
            Debug.LogWarning("[PhotonPlayerSetup] virtualCamera null - assign in GameController!");
        }

        Debug.Log("[PhotonPlayerSetup] Local player registered! controlEnabled: " + pc.controlEnabled);
    }

    // Utility: remove duplicates for same ActorNumber
   
}