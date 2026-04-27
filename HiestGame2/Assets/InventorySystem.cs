using System;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class InventorySystem : MonoBehaviourPun
{
    public static InventorySystem Instance;
    public List<string> items = new List<string>();
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void AddItem(string itemName)
    {
        if (PhotonNetwork.IsConnected && photonView != null)
            photonView.RPC("RPC_AddItem", RpcTarget.All, itemName);
        else
            RPC_AddItem(itemName);
    }

    [PunRPC]
    void RPC_AddItem(string itemName)
    {
        if (!items.Contains(itemName))
        {
            items.Add(itemName);
            Debug.Log("Added item: " + itemName);
            OnInventoryChanged?.Invoke();
        }
    }

    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }

    public void RemoveItem(string itemName)
    {
        if (PhotonNetwork.IsConnected && photonView != null)
            photonView.RPC("RPC_RemoveItem", RpcTarget.All, itemName);
        else
            RPC_RemoveItem(itemName);
    }

    [PunRPC]
    void RPC_RemoveItem(string itemName)
    {
        if (items.Contains(itemName))
        {
            items.Remove(itemName);
            OnInventoryChanged?.Invoke();
        }
    }
}