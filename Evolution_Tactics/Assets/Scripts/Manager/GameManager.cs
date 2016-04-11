using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class GameManager : Singleton<GameManager>
{
    #region Fields

    public int CurrentTurn = 1;
    public int CurrentPlayer = 0;

    public HumanPlayer[] _players;
    public AIPlayer[] _AI;
    List<Player> _allPlayers;

    [SerializeField]
    bool _generateAISquad = false;

    [SerializeField]
    AvailableCharactersList[] AvailableCharacters;

    private List<string> _selectedCharacters;

    public bool isPlaying = false;

    #endregion Fields

    #region Methonds

    protected GameManager()
    {
    }

    #region Unity

    void Awake()
    {
        DontDestroyOnLoad(gameObject);
        //GameManager.Instance = this;
    }

    void OnLevelWasLoaded(int level)
    {
        if (level != 0)
        {
            if (GameManager.Instance.isPlaying)
                GeneratePlayers();
        }
    }

    #endregion Unity

    #region Turn logic

    /// <summary>
    /// Switches active player and start its turn
    /// </summary>
    public void NextTurn()
    {
        Debug.Log("Next Turn");
        ChangePlayer();
        StartTurn();
        UIManager.Instance.ActivateUI();
    }

    /// <summary>
    /// Switch to the next player's turn. If a whole rotation has occured, turn count + 1
    /// </summary>
    void ChangePlayer()
    {
        ++CurrentPlayer;
        // New turn
        if (CurrentPlayer >= _allPlayers.Count)
        {
            ++CurrentTurn;
            CurrentPlayer = 0;
        }
    }

    /// <summary>
    /// Starts then active player's turn
    /// </summary>
    void StartTurn()
    {
        GetActivePlayer().StartTurn();
    }

    public Player GetActivePlayer()
    {
        if (CurrentPlayer >= _allPlayers.Count)
            return null;
        return _allPlayers[CurrentPlayer];
    }

    #endregion Turn logic

    #region Player generation

    public void SetPlayers(List<string> players)
    {
        _selectedCharacters = new List<string>(players);
    }

    /// <summary>
    /// Generates the players and their teams
    /// </summary>
    void GeneratePlayers()
    {
        _allPlayers = new List<Player>();
        foreach (var p in _players)
            _allPlayers.Add(p);
        foreach (var p in _AI)
            _allPlayers.Add(p);

        foreach (var p in _players)
        {
            // Remove everything
            for (int i = p.transform.childCount - 1; i >= 0; i--)
                DestroyImmediate(p.transform.GetChild(i).gameObject);

            GeneratePlayerSquad(p);
        }

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

    /// <summary>
    /// Coroutine to wait a frame so that everything loads properly
    /// </summary>
    /// <returns></returns>
    IEnumerator WaitForCharacterLoad()
    {
        yield return new WaitForEndOfFrame();
        StartTurn();
    }

    /// <summary>
    /// This generates a random team for the player, but the same
    /// kind of logic applies when selecting their own team
    /// </summary>
    /// <param name="p"></param>
    void GeneratePlayerSquad(HumanPlayer p)
    {
        System.Array values = System.Enum.GetValues(typeof(TileStats.type));
        TileStats.type type;
        List<Character> characters = new List<Character>();

        // Add x units
        for (int j = 0; j < 6; j++)
        {
            do
            {
                System.Random random = new System.Random();
                type = (TileStats.type)values.GetValue(Random.Range(0, values.Length));
            } while (type == TileStats.type.Obstacle);

            var chars = AvailableCharacters
                .Where(ac => ac.Type == type)
                .Select(ac => ac.Characters).First();

            var instance = (GameObject)Instantiate(chars[0], Vector3.zero, Quaternion.identity);
            instance.transform.parent = p.transform;
            instance.tag = "Human";

            instance.AddComponent<Experience>();

            characters.Add(instance.GetComponent<Character>());
        }

        p.PrepareCharacters(characters);
    }

    /// <summary>
    /// Generates a random team for the AI based on its specs
    /// </summary>
    /// <param name="ai"></param>
    void GenerateAI(AIPlayer ai)
    {
        int mapLevel = 0;
        if (_players.Length == 0)
            mapLevel = 1;
        else
        {
            foreach (var p in _players)
                mapLevel += p.AverageLevel;
            mapLevel = Mathf.Max(mapLevel / _players.Length, 1);
        }

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
                if (j == ai.MaxUnitCountPerSquad - 1 && Random.Range(0, 1f) < 0.5f)
                    selected = 1;
                var instance = (GameObject)Instantiate(chars[selected], Vector3.zero, Quaternion.identity);
                instance.transform.parent = squad.transform;        
                instance.tag = "AI";

                instance.GetComponent<PokemonStats>().SetLevel(mapLevel + selected);
            }

            squads[i].PrepareSquad();
        }

        ai.SetSquads(squads);

        // Set ai as parent
        squadContainer.transform.parent = ai.transform;
        TileGenerator.Instance.SetNewCollectibleTile();
    }

    #endregion Player generation

    #endregion Methonds
}