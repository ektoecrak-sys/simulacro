using Unity.Netcode;
using UnityEngine;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField]
    private float moveSpeed = 5f;

    private Renderer meshRenderer;

    private Team currentTeam = Team.None;

    private bool canMove = true;

    private void Awake()
    {
        meshRenderer = GetComponent<Renderer>();
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            SpawnAtRandomPosition();
        }
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (!canMove)
            return;

        HandleMovement();
        if(!IsServer) return;
        {
            CheckTeam();
        }
        if (Input.GetKeyDown(KeyCode.M))
        {
            MoveToCenterServerRpc();
        }
    }

    private void HandleMovement()
    {
        float horizontal = 0;
        float vertical = 0;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            horizontal = -1;

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            horizontal = 1;

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
            vertical = 1;

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
            vertical = -1;

        Vector3 movement = new Vector3(horizontal, 0, vertical);

        transform.position += movement.normalized * moveSpeed * Time.deltaTime;

        if (IsOwner)
        {
            CheckTeamServerRpc(transform.position.x);
        }
    }

    private void SpawnAtRandomPosition()
    {
        transform.position = new Vector3(
            Random.Range(-4f, 4f),
            1f,
            Random.Range(-4f, 4f)
        );

        SetColor(Color.white);
    }

    [ServerRpc]
private void CheckTeamServerRpc(float xPosition)
{
    Debug.Log($"Posición X: {xPosition}");

    Team newTeam;

    if (xPosition < -5f)
        newTeam = Team.Team1;
    else if (xPosition > 5f)
        newTeam = Team.Team2;
    else
        newTeam = Team.None;

    if (newTeam != currentTeam)
    {
        Debug.Log($"Cambio de {currentTeam} a {newTeam}");

        Team oldTeam = currentTeam;
        currentTeam = newTeam;

        TeamManager.Instance.ChangeTeam(this, oldTeam, newTeam);
    }
    
}
private void CheckTeam()
{
    float x = transform.position.x;

    Team newTeam;

    if (x < -5f)
        newTeam = Team.Team1;
    else if (x > 5f)
        newTeam = Team.Team2;
    else
        newTeam = Team.None;

    if (newTeam != currentTeam)
    {
        Debug.Log($"Cambio real detectado en SERVER: {currentTeam} → {newTeam}");

        Team oldTeam = currentTeam;
        currentTeam = newTeam;

        TeamManager.Instance.ChangeTeam(this, oldTeam, newTeam);
    }
}

    [ServerRpc]
    private void MoveToCenterServerRpc()
    {
        PlayerNetwork[] players =
            FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);

        foreach (PlayerNetwork player in players)
        {
            player.transform.position = new Vector3(
                1,
                1,
                1
            );

            Team oldTeam = player.currentTeam;

            player.currentTeam = Team.None;

            TeamManager.Instance.ChangeTeam(
                player,
                oldTeam,
                Team.None
            );
        }
    }

    public void SetColor(Color color)
    {
        SetColorClientRpc(color.r, color.g, color.b);
    }

[ClientRpc]
private void SetColorClientRpc(float r, float g, float b)
{
    Debug.Log($"Color recibido: {r}, {g}, {b}");

    if (meshRenderer != null)
    {
        meshRenderer.material.color = new Color(r, g, b);
    }
}

    [ClientRpc]
    public void UpdateMovementClientRpc(
        bool team1Full,
        bool team2Full)
    {
        if (currentTeam == Team.Team1)
        {
            canMove = team1Full;
        }
        else if (currentTeam == Team.Team2)
        {
            canMove = team2Full;
        }
        else
        {
            canMove = !(team1Full || team2Full);
        }
    }

    public void MoveToCenter()
    {
        if (IsOwner)
        {
            MoveToCenterServerRpc();
        }
    }
}