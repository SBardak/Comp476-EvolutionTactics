using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// See AIPlayer script for main description of how this works
/// </summary>
public class Squad : MonoBehaviour
{
    #region Events

    public delegate void SquadEventHandler(Squad s);

    public event SquadEventHandler MovementComplete;
    public event SquadEventHandler OnDeath;

    #endregion Events
    //=========================================================================
    #region Fields

    [SerializeField]
    List<Unit> _units;
    [SerializeField]
    SquadState _state;

    public Player _controlling;

    int _selectedUnit = 0;
    float _unitEndTurnWaitTime = 1f;

    #endregion Fields
    //=========================================================================
    #region Properties

    public List<Unit> Units
    {
        get
        {
            return _units;
        }
    }

    #endregion Properties
    //=========================================================================
    #region Methods
    //-------------------------------------------------------------------------
    #region Prep/Initilization

    public void SetControllingPlayer(Player p)
    {
        _controlling = p;
        foreach (var u in _units)
            u.SetControllingPlayer(p);
    }

    void Awake()
    {
        _state = new SquadState(this);

        PrepareSquad();
    }

    /// <summary>
    /// Prepares/Initializes the squad to be used
    /// </summary>
    public void PrepareSquad()
    {
        var characters = GetComponentsInChildren<Character>();
        _units = new List<Unit>();
        for (int i = 0; i < characters.Length; i++)
            _units.Add(characters[i].gameObject.AddComponent<Unit>());

        if (_controlling != null)
            SetControllingPlayer(_controlling);

        // Move this somewhere else probably
        foreach (var u in _units)
        {
            u.MovementComplete += MoveComplete;
            u.Death += UnitDeath;
        }
    }

    #endregion Prep/Initilization
    //-------------------------------------------------------------------------
    #region Helpers

    /// <summary>
    /// Returns the average squad position in the world
    /// </summary>
    /// <returns></returns>
    public Vector3 GetAveragePosition()
    {
        var pos = Vector3.zero;

        if (_units.Count == 0)
            return pos;

        foreach (var item in _units)
            pos += item.transform.position;

        return pos / _units.Count;
    }

    /// <summary>
    /// Returns the smallest unit movement range
    /// </summary>
    /// <returns></returns>
    public int GetSmallestMovement()
    {
        int smallest = int.MaxValue;
        foreach (var u in _units)
            smallest = Mathf.Min(smallest, u.GetComponent<PokemonStats>().MovementRange);
        return smallest;
    }

    /// <summary>
    /// Returns all reachable tiles by the squad
    /// </summary>
    /// <returns></returns>
    public Dictionary<Tile, int> GetReachableTiles()
    {
        Dictionary<Tile, int> tiles = new Dictionary<Tile, int>();
        foreach (var c in GetComponentsInChildren<Character>())
        {
            var t = c._currentTile.GetTiles();
            if (t != null)
            {
                tiles = tiles.Concat(
                    t.Where(kvp => !tiles.ContainsKey(kvp.Key)
                    )).ToDictionary(x => x.Key, x => x.Value);
            }
        }
        return tiles;
    }

    #endregion Helpers
    //-------------------------------------------------------------------------
    #region Squad actions

    /// <summary>
    /// Removes a dead unit from the list
    /// </summary>
    /// <param name="u"></param>
    private void UnitDeath(Unit u)
    {
        _units.Remove(u);
        if (_units.Count == 0 && OnDeath != null)
            OnDeath(this);
    }

    /// <summary>
    /// Activates all units in the squad
    /// </summary>
    public void ReactivateSquad()
    {
        foreach (var u in _units)
        {
            // Reactive
            u.Activate();
        }
    }

    /// <summary>
    /// Moves the squad
    /// </summary>
    public void MoveSquad()
    {
        // This shouldn't happen
        if (_units.Count == 0)
        {
            NotifyMovementComplete();
            return;
        }

        // Reset unit selection
        _selectedUnit = 0;

        // Prepare the active state
        _state.Prepare();

        // Start moving units
        ProcessUnit();

        StartCoroutine(CenterOnUnit());
    }

    public int GetUnitCount()
    {
        return _units.Count;
    }

    void ProcessUnit()
    {
        _state.ExecuteAction();
    }

    /// <summary>
    /// Moves the current selected unit
    /// </summary>
    void MoveUnit(Tile t)
    {
        var u = GetCurrentUnit();

        u.Move(t);
    }

    void MoveUnit(Character c)
    {
        var u = GetCurrentUnit();

        u.Move(c);
    }

    /// <summary>
    /// Unit finished its move
    /// </summary>
    /// <param name="c"></param>
    void MoveComplete(Unit c)
    {
        //SelectNextUnit();
        StartCoroutine(EndUnitTurn());
    }

    /// <summary>
    /// Waits a 'few' seconds between unit moves
    /// </summary>
    /// <returns></returns>
    IEnumerator EndUnitTurn()
    {
        yield return new WaitForSeconds(_unitEndTurnWaitTime);
        SelectNextUnit();
    }

    /// <summary>
    /// Select next unit.
    /// </summary>
    void SelectNextUnit()
    {
        ++_selectedUnit;
        if (AllUnitsMoved())
            NotifyMovementComplete();
        else
            ProcessUnit();
    }

    #endregion Squad actions
    //-------------------------------------------------------------------------
    #region State methods

    public void Wander(Tile t)
    {
        Debug.Log("Wander " + t);
        MoveUnit(t);
    }

    public void Attack(Character target)
    {
        MoveUnit(target);
    }

    public void Flee(Tile t)
    {
        Debug.Log("Flee " + t);
        MoveUnit(t);
    }

    public void Idle()
    {
        MoveComplete(GetCurrentUnit());
    }

    #endregion State methods
    //-------------------------------------------------------------------------
    /// <summary>
    /// Returns current unit or null if over
    /// </summary>
    /// <returns></returns>
    Unit GetCurrentUnit()
    {
        if (AllUnitsMoved())
            return null;
        return _units[_selectedUnit];
    }

    /// <summary>
    /// Notifies any listeners that the movement is complete
    /// </summary>
    void NotifyMovementComplete()
    {
        StopCoroutine(CenterOnUnit());

        Debug.Log("Squad finished movement");
        if (MovementComplete != null)
            MovementComplete(this);
    }

    /// <summary>
    /// Returns whether all units finished their turn
    /// </summary>
    /// <returns></returns>
    bool AllUnitsMoved()
    {
        return _selectedUnit >= GetUnitCount();
    }

    IEnumerator CenterOnUnit()
    {
        var u = GetCurrentUnit();
        while ((u = GetCurrentUnit()) != null)
        {
            Map_Movement.Instance.CenterOn(u.gameObject);
            yield return null;
        }
    }

    #endregion Methods
}
