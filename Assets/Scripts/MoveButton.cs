using Unity.Netcode;
using UnityEngine;

public class MoveButton : MonoBehaviour
{
    public void MoveToCenter()
    {
        if (NetworkManager.Singleton.LocalClient?.PlayerObject != null)
        {
            PlayerNetwork player =
                NetworkManager.Singleton.LocalClient.PlayerObject.GetComponent<PlayerNetwork>();

            player.MoveToCenter();
        }
    }
}