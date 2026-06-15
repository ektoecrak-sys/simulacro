using System.Collections.Generic;
using UnityEngine;

public class TeamManager : MonoBehaviour
{
    public static TeamManager Instance;

    [Header("Team Settings")]
    public int maxPlayersPerTeam = 2;

    private readonly List<PlayerNetwork> team1Players = new();
    private readonly List<PlayerNetwork> team2Players = new();

    private readonly Color[] team1Colors =
    {
        Color.red,
        new Color(1f,0.5f,0f), // naranja
        Color.magenta // rosa
    };

    private readonly Color[] team2Colors =
    {
        new Color(0f,0f,0.5f), // azul oscuro
        new Color(0.5f,0f,1f), // violeta
        Color.cyan // azul claro
    };

    private readonly Dictionary<PlayerNetwork, Color> assignedColors = new();

    private readonly List<Color> availableTeam1Colors = new();
    private readonly List<Color> availableTeam2Colors = new();

    private void Awake()
    {
        Instance = this;

        availableTeam1Colors.AddRange(team1Colors);
        availableTeam2Colors.AddRange(team2Colors);
    }

    public bool IsTeamFull(Team team)
    {
        switch (team)
        {
            case Team.Team1:
                return team1Players.Count >= maxPlayersPerTeam;

            case Team.Team2:
                return team2Players.Count >= maxPlayersPerTeam;

            default:
                return false;
        }
    }

    public void ChangeTeam(PlayerNetwork player, Team oldTeam, Team newTeam)
    {
        RemovePlayer(player, oldTeam);

        if (newTeam == Team.Team1)
        {
            if (!team1Players.Contains(player))
                team1Players.Add(player);
        }

        if (newTeam == Team.Team2)
        {
            if (!team2Players.Contains(player))
                team2Players.Add(player);
        }

        AssignColor(player, newTeam);

        NotifyMovementState();
    }

    private void RemovePlayer(PlayerNetwork player, Team oldTeam)
    {
        switch (oldTeam)
        {
            case Team.Team1:
                team1Players.Remove(player);

                if (assignedColors.ContainsKey(player))
                {
                    availableTeam1Colors.Add(assignedColors[player]);
                    assignedColors.Remove(player);
                }
                break;

            case Team.Team2:
                team2Players.Remove(player);

                if (assignedColors.ContainsKey(player))
                {
                    availableTeam2Colors.Add(assignedColors[player]);
                    assignedColors.Remove(player);
                }
                break;
        }
    }

    private void AssignColor(PlayerNetwork player, Team team)
    {
        if (team == Team.None)
        {
            player.SetColor(Color.white);
            return;
        }

        Color selectedColor = Color.white;

        if (team == Team.Team1)
        {
            if (availableTeam1Colors.Count > 0)
            {
                int index = Random.Range(0, availableTeam1Colors.Count);
                selectedColor = availableTeam1Colors[index];

                availableTeam1Colors.RemoveAt(index);
            }
        }

        if (team == Team.Team2)
        {
            if (availableTeam2Colors.Count > 0)
            {
                int index = Random.Range(0, availableTeam2Colors.Count);
                selectedColor = availableTeam2Colors[index];

                availableTeam2Colors.RemoveAt(index);
            }
        }

        assignedColors[player] = selectedColor;

        player.SetColor(selectedColor);
    }

    private void NotifyMovementState()
    {
        bool team1Full = IsTeamFull(Team.Team1);
        bool team2Full = IsTeamFull(Team.Team2);

        PlayerNetwork[] players = FindObjectsByType<PlayerNetwork>(FindObjectsSortMode.None);

        foreach (PlayerNetwork player in players)
        {
            player.UpdateMovementClientRpc(team1Full, team2Full);
        }
    }
}