using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public int CurrentPlayer = 0;
    public static GameManager Instance;

    public Player[] _players;

    void Awake()
    {
        GameManager.Instance = this;
    }

    void Start()
    {
        GeneratePlayers();
    }

    public void NextTurn()
    {
        Debug.Log("Next Turn");
        CurrentPlayer = CurrentPlayer >= _players.Length - 1 ? 0 : ++CurrentPlayer;
        StartTurn();
        UIManager.Instance.ActivateUI();
    }

    void StartTurn()
    {
        GetActivePlayer().StartTurn();
    }

    public Player GetActivePlayer()
    {
        return _players[CurrentPlayer];
    }

    void GeneratePlayers()
    {
        //_players = new List<Player>();

        //_players.Add(new HumanPlayer());
        //_players.Add(new AIPlayer());

        StartTurn();
    }
}
