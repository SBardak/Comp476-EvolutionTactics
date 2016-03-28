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

    private Character enemyToAttack = null;

    public void Move(Tile t)
    {
        
    }

    public void Move(Character c)
    {
        
    }

    public void Move()
    { 
        enemyToAttack = null;
        Tile nextTile = FindBestTile();
        Debug.Log(gameObject.name + " " + nextTile);
        if (nextTile != null && nextTile._player == null)
        {
            _pathfinding.SetPath(nextTile);
        }
        else if (nextTile == Character._currentTile)
        {
            ReachedDestination();
            //Character.GetComponent<Pathfinding>().OnReachEnd();
        }
        else
        {
            _char.IsActivated = false;
            if (!_char.IsActivated)
                FinishedMove();
        }

        // For the moment, only consider going damage made to enemy
        // TODO: ADD MORE

        // Move the character
        // _char.Move() (or whatever)
    }

    void ReachedDestination()
    {
        // Attack ?
        if (enemyToAttack != null)
        {
            Attack();
        }
        else
        {
            FinishedMove();
            Debug.Log(gameObject.name + " Allo");
        }
    }

    public void Attack()
    { 
        // Select attack

        // Let char do its thing
        // _char.attack(); -> End of attack event link to finished attack
        _char.Attack(enemyToAttack);

        FinishedAttack();
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
        enemyToAttack = null;
    }


    void HandleDeath()
    {
        if (Death != null)
            Death(this);
    }

    public Tile FindBestTile()
    {
        // Get all possible tiles, including the ones where character can't move bu can attack to
        Dictionary<Tile, int> possibleTiles = Character._currentTile.GetTiles();
        List<Tile> tilesWithEnemy = new List<Tile>();

        int movementRange = GetComponent<PokemonStats>().MovementRange;

        //get a list of the tiles in range that contain an enemy
        foreach (Tile t in possibleTiles.Keys)
        {
            if (t._player != null && t._player.tag == "Human")
            {
                tilesWithEnemy.Add(t);
            }
        }

        //if there are at least one enemy in range
        // TODO add more conditions?
        if (tilesWithEnemy.Count > 0)
        {
            Tile bestTile = null;
            AttackAlgorithm attack = GetComponentInChildren<AttackAlgorithm>();
            int highestDamage = 0;
            //Find an initial empty best tile
            foreach (Tile t in tilesWithEnemy)
            {
                bestTile = tilesWithEnemy[0].neighbours[0];
                // when no player on the tile, break
                if (possibleTiles[t] <= movementRange && bestTile._player == null)
                {
                    highestDamage = attack.GetDamage(tilesWithEnemy[0]._player, bestTile);
                    enemyToAttack = tilesWithEnemy[0]._player;
                    break;
                }
                else
                {
                    bestTile = null;
                }
            }

            // for each tile t with an enemy in range
            foreach (Tile t in tilesWithEnemy)
            {
                // for each neighbour of t
                Dictionary<Tile, int> possibleAttackingTiles = t.GetTiles(0, GetComponent<PokemonStats>().AttackRange);
                Debug.Log("Possible " + possibleAttackingTiles.Count);
                foreach (Tile tt in possibleAttackingTiles.Keys)
                {
                    Debug.Log("Tile " + tt);
                    // if (neighbour is empty or neighbour is where we currently are) and (this neighbour is in range)
                    if ((tt._player == null || tt._player == Character) && (possibleTiles.ContainsKey(tt) && possibleTiles[tt] <= movementRange))
                    {
                        int damage = attack.GetDamage(t._player, tt);
                        int rangeDistance = possibleTiles[tt];

                        // if this tile would do more damage, set it as best tile
                        // TODO add more conditions, like damage received?
                        if (damage > highestDamage || (bestTile != _char._currentTile && damage == highestDamage && rangeDistance > possibleTiles[bestTile]))
                        {
                            bestTile = tt;
                            highestDamage = damage;
                            enemyToAttack = t._player;
                        }
                    }
                }
            }
            return bestTile;
        }
        return null;
    }
}
