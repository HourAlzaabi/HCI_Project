using UnityEngine;

[CreateAssetMenu(fileName = "PlayerRolesData", menuName = "Game/Player Roles Data")]
public class PlayerRolesData : ScriptableObject
{
    [Header("Player 1 (Host / Master Client)")]
    public string player1Tag = "Player 1";
    public Sprite player1Sprite;
    public string player1PrefabName = "Player1Prefab";

    [Header("Player 2 (Joiner)")]
    public string player2Tag = "Player 2";
    public Sprite player2Sprite;
    public string player2PrefabName = "Player2Prefab";

    public string GetTag(bool isMasterClient)
    {
        return isMasterClient ? player1Tag : player2Tag;
    }

    public Sprite GetSprite(bool isMasterClient)
    {
        return isMasterClient ? player1Sprite : player2Sprite;
    }

    public string GetPrefabName(bool isMasterClient)
    {
        return isMasterClient ? player1PrefabName : player2PrefabName;
    }
}