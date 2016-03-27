using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : MonoBehaviour
{
    public int CurrentPlayer = 0;
    public static GameManager Instance;

    public HumanPlayer[] _players;
    public AIPlayer[] _AI;
    List<Player> _allPlayers;

    [SerializeField]
    bool _generateAISquad = false;

    [SerializeField]
    AvailableCharactersList[] AvailableCharacters;

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
        CurrentPlayer = CurrentPlayer >= _allPlayers.Count - 1 ? 0 : ++CurrentPlayer;
        StartTurn();
        UIManager.Instance.ActivateUI();
    }

    void StartTurn()
    {
        GetActivePlayer().StartTurn();
    }

    public Player GetActivePlayer()
    {
        return _allPlayers[CurrentPlayer];
    }

    void GeneratePlayers()
    {
        _allPlayers = new List<Player>();
        foreach (var p in _players)
            _allPlayers.Add(p);
        foreach (var p in _AI)
            _allPlayers.Add(p);

        if (_generateAISquad)
        {
            foreach (var p in _AI)
            {
                // Remove everything
                for (int i = p.transform.childCount - 1; i >= 0; i--)
                    DestroyImmediate(p.transform.GetChild(i).gameObject);

                GenerateAI(p);
            }
        }

        StartCoroutine(WaitForCharacterLoad());
    }

    IEnumerator WaitForCharacterLoad()
    {
        yield return new WaitForEndOfFrame();
        StartTurn();
    }

    void GenerateAI(AIPlayer ai)
    {
        List<Squad> squads = new List<Squad>();
        System.Array values = System.Enum.GetValues(typeof(TileStats.type));
        TileStats.type type;

        // Create the initial squad container
        GameObject squadContainer = new GameObject();
        squadContainer.name = "Squads";

        // Create x squads
        for (int i = 0; i < ai.SquadCount; i++)
        {
            GameObject squad = new GameObject();
            squads.Add(squad.AddComponent<Squad>());
            squad.transform.parent = squadContainer.transform;
            squad.name = "Squad" + i;

            do
            {
                System.Random random = new System.Random();
                type = (TileStats.type)values.GetValue(Random.Range(0, values.Length));
            } while (type == TileStats.type.Obstacle);

            var chars = AvailableCharacters
                .Where(ac => ac.Type == type)
                .Select(ac => ac.Characters).First();

            // Add x units
            int selected = 0;
            for (int j = 0; j < ai.MaxUnitCountPerSquad; j++)
            {
                if (j == ai.MaxUnitCountPerSquad -1 && Random.Range(0, 1f) < 0.5f)
                    selected = 1;
                var instance = (GameObject)Instantiate(chars[selected], Vector3.zero, Quaternion.identity);
                instance.transform.parent = squad.transform;
            }

            squads[i].PrepareSquad();
        }

        ai.SetSquads(squads);

        // Set ai as parent
        squadContainer.transform.parent = ai.transform;
    }
}
