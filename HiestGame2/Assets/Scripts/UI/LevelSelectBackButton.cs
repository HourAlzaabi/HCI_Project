using UnityEngine;
using UnityEngine.SceneManagement;
using Photon.Pun;

public class LevelSelectBackButton : MonoBehaviour
{
    public void OnBackClicked()
    {
        // Only host can navigate (or solo player)
        if (PhotonNetwork.IsConnected && !PhotonNetwork.IsMasterClient)
        {
            Debug.Log("Only host can go back to lobby");
            return;
        }

        if (PhotonNetwork.IsConnected && PhotonNetwork.IsMasterClient)
            PhotonNetwork.LoadLevel("Lobby");
        else
            SceneManager.LoadScene("Lobby");
    }
}