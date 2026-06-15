using TMPro;
using Unity.Netcode;
using UnityEngine;

public class ServerUI : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField inputField;

    private void Start()
    {
        if (!NetworkManager.Singleton.IsServer)
        {
            gameObject.SetActive(false);
        }
    }

    public void ApplyMaxPlayers()
    {
        if (int.TryParse(inputField.text, out int value))
        {
            if (value > 0)
            {
                TeamManager.Instance.maxPlayersPerTeam = value;
            }
        }
    }
}