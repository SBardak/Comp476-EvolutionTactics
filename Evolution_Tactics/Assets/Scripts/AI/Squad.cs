using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// See AIPlayer script for main description of how this works
/// </summary>
public class Squad : MonoBehaviour {
    public delegate void MovementCompleteHandler(Squad s);
    public event MovementCompleteHandler MovementComplete;

    [SerializeField]
    List<Unit> _units;
    int _selectedUnit = 0;

    public Player _controlling;

    [SerializeField]
    SquadState _state;
    Vector3 _squadDirection;
    int _squadDirectionCounter;

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

    private void UnitDeath(Unit u)
    {
        _units.Remove(u);
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
    }

    public Vector3 GetAveragePosition()
    {
        var pos = Vector3.zero;

        if (_units.Count == 0)
            return pos;

        foreach (var item in _units)
            pos += item.transform.position;

        return pos / _units.Count;
    }
    public int GetSmallestMovement()
    {
        int smallest = int.MaxValue;
        foreach (var u in _units)
            smallest = Mathf.Min(smallest, u.GetComponent<PokemonStats>().MovementRange);
        return smallest;
    }
    public Dictionary<Tile, int> GetReachableTiles()
    {
        Dictionary<Tile, int> tiles = new Dictionary<Tile, int>();
        foreach (var c in GetComponentsInChildren<Character>())
            tiles = tiles.Concat(
                c._currentTile.GetTiles().Where(kvp => !tiles.ContainsKey(kvp.Key)
                )).ToDictionary(x => x.Key, x => x.Value);
        return tiles;
    }
    public int GetUnitCount()
    {
        return _units.Count;
    }

    void ProcessUnit()
    {
        _state.ExecuteAction();
    }

    public void Wander(Tile t)
    {
        MoveUnit(t);
    }
    public void Attack(Character target)
    {
        MoveUnit(target);
    }
    public void Flee(Vector3 direction)
    {
        var u = _units[_selectedUnit];

        u.Move();
    }
    public void Idle()
    {
        MoveComplete(_units[_selectedUnit]);
    }

    /// <summary>
    /// Moves the current selected unit
    /// </summary>
    void MoveUnit(Tile t)
    {
        var u = _units[_selectedUnit];

        u.Move();
    }
    void MoveUnit(Character c)
    {
        var u = _units[_selectedUnit];

        u.Move();
    }

    /// <summary>
    /// Unit finished its move
    /// </summary>
    /// <param name="c"></param>
    void MoveComplete(Unit c)
    {
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

    /// <summary>
    /// Notifies any listeners that the movement is complete
    /// </summary>
    void NotifyMovementComplete()
    {
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
        return _selectedUnit >= _units.Count;
    }
}
