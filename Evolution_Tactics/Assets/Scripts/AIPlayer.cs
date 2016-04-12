using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// To minimize conditions in the update loop while units move and attack,
/// the AI player uses events to move all units. 
/// 
/// In this manner, the AI will first begin its turn when the StartTurn() 
/// method is called and start the movement of the first squad. This squad,
/// once finished, will generate an event that this script listens for. 
/// Once read, it will then move on to the next squad and so on until 
/// all have moved. 
/// 
/// Once completed, it lets the game manager know that it completed its
/// turn.
/// </summary>
public class AIPlayer : Player
{
    #region Fields

    public int SquadCount = 3;
    public int MaxUnitCountPerSquad = 3;

    [SerializeField]
    Squad[] _squads;
    List<Squad> _squadList;
    int _selectedSquad = 0;

    bool _isPlaying;

    #endregion Fields

    #region Properties

    public bool IsPlaying
    {
        get
        {
            return _isPlaying;
        }
    }

    #endregion Properties

    #region Methods

    #region Squad

    void Awake()
    {
        _squadList = new List<Squad>(_squads);
        PrepareSquads();
    }

    public void SetSquads(List<Squad> squads)
    {
        _squadList = squads;
        PrepareSquads();
    }

    void PrepareSquads()
    {
        // Move this somewhere else probably
        foreach (var s in _squadList)
        {
            s.MovementComplete += MovedSquad;
            s.SetControllingPlayer(this);
            PositionCharacter(new List<Character>(s.GetComponentsInChildren<Character>()));
            s.OnDeath += Squad_OnDeath;
        }
    }

    private void Squad_OnDeath(Squad s)
    {
        _squadList.Remove(s);
        if (_squadList.Count == 0)
            GameManager.Instance.DeadAI(this);
    }

    #endregion Squad

    #region Turn

    /// <summary>
    /// Start the AIs turn
    /// </summary>
    public override void StartTurn()
    {
        Debug.Log("Ai Start turn");
        // This shouldn't happen
        if (_squadList.Count == 0)
        {
            EndTurn();
            return;
        }

        _isPlaying = true;

        // Reactivate all units
        foreach (var s in _squadList)
            s.ReactivateSquad();

        // Select first squad
        _selectedSquad = 0;

        // Move unit
        MoveSquad();

        //base.StartTurn();
        //StartCoroutine(ExecuteTurn());
    }

    /// <summary>
    /// Move the active squad
    /// </summary>
    void MoveSquad()
    { 
        var s = _squadList[_selectedSquad];

        s.MoveSquad();
    }

    /// <summary>
    /// Squad movement complete, possibly do something?
    /// </summary>
    /// <param name="s"></param>
    void MovedSquad(Squad s)
    {
        NextSquad();
    }

    /// <summary>
    /// Changes the current active squad
    /// </summary>
    void NextSquad()
    {
        ++_selectedSquad;
        if (AllSquadsMoved())
            EndTurn();
        else
            MoveSquad();
    }

    void EndTurn()
    {
        Debug.Log("AI finished turn");
        _isPlaying = false;
        GameManager.Instance.NextTurn();
    }

    /// <summary>
    /// Returns whether all squads finished their turn
    /// </summary>
    /// <returns></returns>
    bool AllSquadsMoved()
    {
        return _selectedSquad >= _squadList.Count;
    }

    #endregion Turn

    #endregion Methods

}
