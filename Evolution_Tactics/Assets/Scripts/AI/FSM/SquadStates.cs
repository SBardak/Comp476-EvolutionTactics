using UnityEngine;
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class SquadState
{
    #region Fields
    SquadBaseState _bs;
    SquadIdle _idle;
    SquadFlee _flee;
    SquadAttack _attack;
    SquadWander _wander;
    public readonly Squad Squad;
    #endregion Fields

    #region State Changers
    public void ToIdle()
    {
        _bs = _idle;
        _bs.Reset();
    }
    public void ToFlee()
    {
        _bs = _flee;
        _bs.Reset();
    }
    public void ToAttack()
    {
        _bs = _attack;
        _bs.Reset();
    }
    public void ToWander()
    {
        _bs = _wander;
        _bs.Reset();
    }
    #endregion State Changers

    #region Constructor
    public SquadState(Squad s)
    {
        _idle = new SquadIdle(this);
        _flee = new SquadFlee(this);
        _attack = new SquadAttack(this);
        _wander = new SquadWander(this);

        Squad = s;

        ToWander();
    }
    #endregion Constructor

    #region Methods
    /// <summary>
    /// Changes the active state depending on variables
    /// and processes the information of the new active state
    /// </summary>
    public void Prepare()
    {
        // Change active state
        _bs.CheckState();
        // Process information
        _bs.Process();
    }
    /// <summary>
    /// Executes the action for a single Unit
    /// </summary>
    public void ExecuteAction()
    {
        _bs.ExecuteAction();
    }
    #endregion Methods
}

#region States

public abstract class SquadBaseState : IState
{
    protected readonly SquadState SquadState;
    public SquadBaseState(SquadState ss)
    {
        SquadState = ss;
    }

    public abstract void CheckState();
    public abstract void ExecuteAction();
    public abstract void Process();

    public virtual void Reset() { }

    /// <summary>
    /// Gets all the enemies within the squad range
    /// </summary>
    /// <returns></returns>
    public List<Character> GetEnemiesInRange()
    {
        return SquadState.Squad.GetReachableTiles().
            Where(kvp => kvp.Key.HasPlayer &&
                kvp.Key._character.ControllingPlayer != SquadState.Squad._controlling)
            .Select(kvp => kvp.Key._character).ToList();
    }

    /// <summary>
    /// Get all AI squads
    /// </summary>
    /// <returns></returns>
    protected Squad[] GetAISquads()
    {
        return SquadState.Squad.transform.parent.GetComponentsInChildren<Squad>();
    }
    protected int GetLiveSquads()
    {
        int alive = 0;
        if (SquadState.Squad.transform.parent != null)
        {
            foreach (var s in GetAISquads())
            {
                if (s == SquadState.Squad || s.GetUnitCount() == 0) continue;
                ++alive;
            }
        }
        return alive;
    }
}

public class SquadIdle : SquadBaseState
{
    int idleTimer = 20;

    public SquadIdle(SquadState ss): base(ss)
    { }

    public override void CheckState()
    {
        // Any enemies?
        if (GetEnemiesInRange().Count > 0)
        {
            SquadState.ToAttack();
            return;
        }

        // Random change to start wander
        if (UnityEngine.Random.Range(0, 101) < idleTimer)
        {
            SquadState.ToWander();
            return;
        }

        // Linear, maybe curve instead?
        idleTimer += 20;
    }

    public override void ExecuteAction()
    {
        SquadState.Squad.Idle();
    }

    public override void Process()
    {
        Debug.Log("Nothing to process while idle!");
    }
}

public class SquadFlee : SquadBaseState
{
    Vector3 _targetSquad = Vector3.zero;

    public SquadFlee(SquadState ss): base(ss)
    { }

    public override void CheckState()
    {
        if (GetLiveSquads() == 0)
            SquadState.ToAttack();
    }

    public override void ExecuteAction()
    {
        var tile = TileGenerator.Instance.Tiles[(int)_targetSquad.x, (int)_targetSquad.z];
        SquadState.Squad.Flee(tile);
    }

    public override void Process()
    {
        Debug.Log("Flee");

        var squads = GetAISquads().Where(s => s != SquadState.Squad).ToList();
        Squad closest = squads[0];
        Vector3 avg = SquadState.Squad.GetAveragePosition();
        float dist = (closest.GetAveragePosition() - avg).sqrMagnitude;
        float distT;

        // Search for closest squad (not taking into account objects as of yet
        for (int i = 1; i < squads.Count; ++i)
        {
            var s = squads[i];
            distT = (s.GetAveragePosition() - avg).sqrMagnitude;

            if (distT < dist)
            {
                closest = s;
                dist = distT;
            }
        }

        _targetSquad = closest.GetAveragePosition();
    }
}

public class SquadAttack : SquadBaseState
{
    Character chosen = null;

    public SquadAttack(SquadState ss): base(ss)
    { }

    public override void CheckState()
    {
        // Outnumbered?
        if (SquadState.Squad.GetUnitCount() == 1 && GetEnemiesInRange().Count > SquadState.Squad.GetUnitCount())
        {
            // TODO: Change
            if (GetLiveSquads() > 0)
            {
                SquadState.ToFlee();
                return;
            }
        }

        // All killed
        if (GetEnemiesInRange().Count == 0)
        {
            SquadState.ToIdle();
            return;
        }
    }

    public override void ExecuteAction()
    {
        SquadState.Squad.Attack(chosen);
    }

    public override void Process()
    {
        var enemies = GetEnemiesInRange();
        var avg = SquadState.Squad.GetAveragePosition();
        chosen = enemies[0];
        var dist = (avg - chosen.transform.position).sqrMagnitude;

        // Find closest
        for (int i = 1; i < enemies.Count; i++)
        {
            var e = enemies[i];
            var d = (avg - e.transform.position).sqrMagnitude;
            if (d < dist)
            {
                dist = d;
                chosen = e;
            }
        }
    }

    public override void Reset()
    {
        chosen = null;
    }
}

public class SquadWander : SquadBaseState
{
    Vector3 squadAvg;
    Vector3 movementDirection;
    Tile squadTile;

    float idleProbability = 10;
    int number = 0;

    public SquadWander(SquadState ss): base(ss)
    {
        SelectDirection();
    }

    /// <summary>
    /// Selects a random movement direction
    /// </summary>
    void SelectDirection()
    {
        float x = UnityEngine.Random.Range(-1f, 1.01f),
            z = UnityEngine.Random.Range(-1f, 1.01f);

        if (z == 0 && z == x)
            z = 1;

        movementDirection = (new Vector3(x, 0, z)).normalized;
    }

    public override void CheckState()
    {
        // Any enemies?
        if (GetEnemiesInRange().Count > 0)
        {
            SquadState.ToAttack();
            return;
        }

        // Random change to go idle
        if (UnityEngine.Random.Range(0, 101) < idleProbability)
        {
            SquadState.ToIdle();
            return;
        }

        // Linear, maybe curve instead?
        idleProbability += 10;
    }

    public override void ExecuteAction()
    {
        // Search for a tile around the squad tile.
        // For now, first unit will always go to this tile
        if (number++ == 0 && !squadTile.IsOccupied)
            SquadState.Squad.Wander(squadTile);
        else
        {
            // Select a location around this tile
            int distance = 2;
            Tile t = null;
            while (distance < 4 && t == null)
            {
                t = TileGenerator.Instance.GetSurroundingAvailableTile(squadTile, distance);
                ++distance;
            }
            SquadState.Squad.Wander(t);
        }
    }

    public override void Process()
    {
        number = 0;
        squadTile = null;

        Vector3 pos = Vector3.zero;
        squadAvg = SquadState.Squad.GetAveragePosition();
        int move = SquadState.Squad.GetSmallestMovement();
    
        // 5 tries
        int redo = 5;
        do
        {
            pos = squadAvg + move * movementDirection;

            // Not inside the map
            if (!TileGenerator.Instance.InMap((int)pos.x, (int)pos.z))
            {
                // Switch movement direction
                SelectDirection();
                --redo;

                // Couldn't get a new one, go idle
                if (redo == 0)
                    SquadState.ToIdle();
            }
            else redo = 0;
        } while (redo > 0);

        squadTile = TileGenerator.Instance.Tiles[(int)pos.x, (int)pos.z];
    }

    public override void Reset()
    {
        SelectDirection();
        idleProbability = 10;
    }
}

#endregion States
