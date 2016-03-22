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

    public void Move()
    { 
        enemyToAttack = null;
        Tile nextTile = FindBestTile();
        Debug.Log(gameObject.name + " " + nextTile);
        if (nextTile != null && nextTile._player == null)
        {
            _pathfinding.SetPath(nextTile);
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
            enemyToAttack = tilesWithEnemy[0]._player;

            foreach (Tile t in tilesWithEnemy)
            {
                foreach (Tile neighbour in t.neighbours)
                {
                    if ((neighbour._player == null || neighbour._player == Character) && possibleTiles.ContainsKey(neighbour))
                    {
                        int damage = attack.GetDamage(t._player, neighbour);

                        if (damage > highestDamage)
                        {
                            bestTile = neighbour;
                            highestDamage = damage;
                            enemyToAttack = t._player;
                        }
                    }
                }
            }
            return bestTile;
        }
        enemyToAttack = null;
        return null;
    }
}
