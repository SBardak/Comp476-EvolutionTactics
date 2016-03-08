using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour {
    public int CurrentPlayer = 0;
    public static GameManager Instance;

    public List<Player> _players;

    void Awake()
    {
        GameManager.Instance = this;
    }

	void Start () {
        GeneratePlayers();
	}
	
    public void NextTurn()
    {
        CurrentPlayer = CurrentPlayer >= _players.Count - 1 ? 0 : ++CurrentPlayer;
        StartTurn();
    }

    void StartTurn()
    {
        _players[CurrentPlayer].StartTurn();
    }

    void GeneratePlayers()
    {
        _players = new List<Player>();
        StartTurn();
    }
}
