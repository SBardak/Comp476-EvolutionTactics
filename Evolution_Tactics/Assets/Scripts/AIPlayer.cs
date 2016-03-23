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
    [SerializeField]
    Squad[] _squads;
    int _selectedSquad = 0;

    bool _isPlaying;

    void Awake()
    { 
        // Move this somewhere else probably
        foreach (var s in _squads)
            s.MovementComplete += MovedSquad;

        foreach (var s in _squads)
            s.SetControllingPlayer(this);
    }

    void Start()
    {
        foreach (var s in _squads)
            PositionCharacter(new List<Character>(s.GetComponentsInChildren<Character>()));
    }

    /// <summary>
    /// Start the AIs turn
    /// </summary>
    public override void StartTurn()
    {
        Debug.Log("Ai Start turn");
        // This shouldn't happen
        if (_squads.Length == 0)
        {
            EndTurn();
            return;
        }

        _isPlaying = true;

        // Reactivate all units
        foreach (var s in _squads)
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
        var s = _squads[_selectedSquad];

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
        return _selectedSquad >= _squads.Length;
    }

    //IEnumerator ExecuteTurn()
    //{
    //    yield return null;

    //    //GameManager.Instance.NextTurn();
    //}

    public bool IsPlaying
    {
        get
        {
            return _isPlaying;
        }
    }
}
