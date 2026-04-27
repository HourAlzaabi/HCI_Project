using ExitGames.Client.Photon;
using Photon.Pun;
using UnityEngine;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour
{
    public Color color;
    public Image playerPreview;

    public void OnColorClicked()
    {
        // Force alpha to 1 when setting color
        Color c = color;
        c.a = 1f;
        GameData.SelectedColor = c;

        Hashtable props = new Hashtable
        { { "color", ColorUtility.ToHtmlStringRGB(c) } };
        PhotonNetwork.LocalPlayer.SetCustomProperties(props);
    }
}