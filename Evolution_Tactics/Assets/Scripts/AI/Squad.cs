using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// See AIPlayer script for main description of how this works
/// </summary>
public class Squad : MonoBehaviour {

    public delegate void MovementCompleteHandler(Squad s);
    public event MovementCompleteHandler MovementComplete;

    [SerializeField]
    Unit[] _units;
    int _selectedUnit = 0;

    public void SetControllingPlayer(Player p)
    {
        foreach (var u in _units)
            u.SetControllingPlayer(p);
    }

    void Start()
    {
        // Move this somewhere else probably
        foreach (var u in _units)
            u.MovementComplete += MoveComplete;
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
        if (_units.Length == 0)
        {
            NotifyMovementComplete();
            return;
        }

        // Reset unit selection
        _selectedUnit = 0;

        /* Squad should have an idea of where it is on the map */

        // Select 'best' move location. 
        // if ( knows where player is )
        {
            // Find a suitable character for the squad to attack
            // Let each squad member decide whether to go for it or not
        }
        // else 
        {
            // Choose a location in the general direction of the squad
        }

        MoveUnit();
    }

    /// <summary>
    /// Moves the current selected unit
    /// </summary>
    void MoveUnit()
    {
        var u = _units[_selectedUnit];

        // Character should move where it can possibly attack someone
        u.Move(); //general squad location
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
            MoveUnit();
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
        return _selectedUnit >= _units.Length;
    }
}
