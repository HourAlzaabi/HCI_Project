using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkGameManager : MonoBehaviourPunCallbacks
{
    [Header("Roles Data")]
    public PlayerRolesData rolesData;

    public Transform spawnPointP1;
    public Transform spawnPointP2;

    private static bool hasSpawnedThisLoad = false;

    void Awake()
    {
        hasSpawnedThisLoad = false;
    }

    void Start()
    {
        if (!PhotonNetwork.IsConnected)
        {
            Debug.LogError("[NGM] Not connected!");
            return;
        }

        if (hasSpawnedThisLoad)
        {
            Debug.LogWarning("[NGM] Already spawned this load - skipping.");
            return;
        }
        hasSpawnedThisLoad = true;

        PhotonNetwork.SerializationRate = 20;
        PhotonNetwork.SendRate = 30;

        // Destroy ALL player prefab instances before spawning fresh
        foreach (var go in GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None))
        {
            if (go.name.Contains("PlayerPrefab") ||
                go.name.Contains("Player1Prefab") ||
                go.name.Contains("Player2Prefab"))
            {
                var pv = go.GetComponent<PhotonView>();
                if (pv != null && pv.IsMine)
                {
                    Debug.Log("[NGM] Destroying my stale player before respawn.");
                    PhotonNetwork.Destroy(go);
                }
            }
        }

        SpawnMyPlayer();
    }

    void SpawnMyPlayer()
    {
        Transform spawnPoint = PhotonNetwork.IsMasterClient
            ? spawnPointP1 : spawnPointP2;

        if (spawnPoint == null)
        {
            Debug.LogError("[NGM] Spawn point null!");
            return;
        }

        string prefabName = (rolesData != null)
            ? rolesData.GetPrefabName(PhotonNetwork.IsMasterClient)
            : "PlayerPrefab";

        Debug.Log("[NGM] Spawning " + prefabName + " at: " + spawnPoint.name
            + " | Actor: " + PhotonNetwork.LocalPlayer.ActorNumber
            + " | IsMaster: " + PhotonNetwork.IsMasterClient);

        PhotonNetwork.Instantiate(prefabName, spawnPoint.position, Quaternion.identity);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.Log("[NGM] Player left: " + otherPlayer.NickName);
    }
}