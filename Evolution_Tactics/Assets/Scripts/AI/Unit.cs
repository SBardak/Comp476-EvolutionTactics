using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Unit : MonoBehaviour
{

    public delegate void UnitHandler(Unit u);

    public event UnitHandler MovementComplete;
    public event UnitHandler Death;

    Character _char;
    Pathfinding _pathfinding;

    Character Character
    {
        get
        {
            if (_char == null)
                _char = GetComponentInChildren<Character>();
            return _char;
        }
    }

    public void SetControllingPlayer(Player p)
    {
        Character.ControllingPlayer = p;
    }

    void Awake()
    { 
        if ((_pathfinding = GetComponentInChildren<Pathfinding>()) != null)
        {
            _pathfinding.OnReachEnd += ReachedDestination;
        }
        // Link char events to handlers
        // Change names?
        // _char.MoveComplete += ReachedDestination;
        // _char.AttackComplete += Attacked;
    }

    public void Activate()
    {
        _char.Activate();
    }

    public void Move()
    { 
        Tile nextTile = FindBestTile();
        Debug.Log(gameObject.name + " " + nextTile);
        if (nextTile != null && nextTile != Character._currentTile && nextTile._player == null)
        {
            _pathfinding.SetPath(nextTile);
        }
        else
        {
            _char.IsActivated = false;
        }

        if (!_char.IsActivated)
            FinishedMove();

        // Decide where to move

        // For the moment, only consider going towards enemies
        // TODO: ADD MORE

        // Move the character
        // _char.Move() (or whatever)
    }

    void ReachedDestination()
    {
        // Attack ?
        // if ( decided to attack )
        //{
        //    Attack();
        //}
        // else
        {
            FinishedMove();
        }
    }

    void Attack()
    { 
        // Select attack

        // Let char do its thing
        // _char.attack(); -> End of attack event link to finished attack
    }

    void FinishedAttack()
    {
        FinishedMove();
    }

    void FinishedMove()
    {
        Debug.Log("Unit finished movement");
        if (MovementComplete != null)
            MovementComplete(this);
    }


    void HandleDeath()
    {
        if (Death != null)
            Death(this);
    }

    public Tile FindBestTile()
    {
        Dictionary<Tile, int> possibleTiles = Character._currentTile.GetTiles();
        List<Tile> tilesWithEnemy = new List<Tile>();
        foreach (Tile t in possibleTiles.Keys)
        {
            if (t._player != null && t._player.tag == "Human")
            {
                tilesWithEnemy.Add(t);
            }
        }

        if (tilesWithEnemy.Count > 0)
        {
            Tile bestTile = tilesWithEnemy[0].neighbours[0];
            AttackAlgorithm attack = GetComponentInChildren<AttackAlgorithm>();
            int highestDamage = attack.GetDamage(tilesWithEnemy[0]._player, bestTile);

            foreach (Tile t in tilesWithEnemy)
            {
                foreach (Tile neighbour in t.neighbours)
                {
                    if ((neighbour._player == null || neighbour._player == Character) && possibleTiles.ContainsKey(neighbour))
                    {
                        int damage = attack.GetDamage(t._player, neighbour);

                        if (neighbour == Character._currentTile)
                        {
                            Debug.Log("Allo");
                        }

                        if (damage > highestDamage)
                        {
                            bestTile = neighbour;
                            highestDamage = damage;
                        }
                    }
                }
            }
            return bestTile;
        }
        return null;
    }
}
