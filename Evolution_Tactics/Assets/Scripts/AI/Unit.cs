using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UnitType
{
    ATTACKER,
    TANKER,
    LONG_RANGE
}

public class Unit : MonoBehaviour
{
    public delegate void UnitHandler(Unit u);

    public event UnitHandler MovementComplete;
    public event UnitHandler Death;

    Character _char;
    Pathfinding _pathfinding;
    PokemonStats _stats;

    public UnitType _unitType = UnitType.ATTACKER;

    Character Character
    {
        get
        {
            if (_char == null)
                _char = GetComponentInChildren<Character>();
            return _char;
        }
    }

    PokemonStats Stats
    {
        get
        {
            if (_stats == null)
                _stats = GetComponent<PokemonStats>();
            return _stats;
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
        enemyToAttack = null;
        Dictionary<Tile, int> possibleTiles = Character._currentTile.GetTiles();
        Tile collecTile = CollectibleInRange(possibleTiles);
        Tile nextTile = t;

        //if collectible in range and is the more hurt in squad
        if (collecTile != null && IsMoreHurtInSquad())
        {
            Debug.Log(name + " Collect tile");
            nextTile = collecTile;
        }
        else if (nextTile._player != null)
        {
            nextTile = FindNewTileAround(t, possibleTiles);
        }

        Debug.Log("Wander or flee " + nextTile);
        MoveTo(nextTile);
    }

    public void Move(Character c)
    {
        enemyToAttack = null;
        Dictionary<Tile, int> possibleTiles = Character._currentTile.GetTiles();
        Tile nextTile = null;

        if (_unitType == UnitType.ATTACKER)
        {
            Debug.Log(name + " Attacker");
            nextTile = FindBestTile(possibleTiles);
        }
        else if (_unitType == UnitType.TANKER)
        {
            nextTile = CanKillInRange(possibleTiles);
            if (nextTile == null)
            {
                nextTile = FurthestNeighbourTileFrom(c);
                enemyToAttack = null;
            }
        }
        else if (_unitType == UnitType.LONG_RANGE)
        {

        }

        MoveTo(nextTile);
    }

    private void MoveTo(Tile nextTile)
    {
        if (nextTile != null && nextTile._player == null)
        {
            _pathfinding.SetPath(nextTile);
        }
        else if (nextTile == Character._currentTile)
        {
            ReachedDestination();
        }
        else
        {
            _char.IsActivated = false;
            if (!_char.IsActivated)
                FinishedMove();
        }
    }

    public void Move()
    { 
        /*if (nextTile != null && nextTile._player == null)
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
        }*/

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

    #region Search for tile helper methods

    private Tile FindNewTileAround(Tile t, Dictionary<Tile, int> possibleTiles)
    {
        Tile newTile = null;
        float closest = float.MaxValue;
        Dictionary<Tile, int> aroundT = t.GetTiles(4, 0);

        foreach (Tile tt in aroundT.Keys)
        {
            if (possibleTiles.ContainsKey(tt) && tt._player == null)
            {
                float distance = Vector3.Distance(Character._currentTile.transform.position, tt.transform.position);
                if (newTile == null || distance < closest)
                {
                    closest = distance;
                    newTile = tt;
                } 
            }
        }
        return newTile;
    }

    private Tile FurthestNeighbourTileFrom(Character c)
    {
        Dictionary<Tile, int> possibleTiles = c._currentTile.GetTiles(0, 1);
        float furthest = 0f;
        Tile furtestTile = null, currentTile = Character._currentTile;

        foreach (Tile t in possibleTiles.Keys)
        {
            float distance = Vector3.Distance(t.transform.position, currentTile.transform.position);

            if (distance > furthest)
            {
                furthest = distance;
                furtestTile = t;
            }
        }
        return furtestTile;
    }

    public Tile CanKillInRange(Dictionary<Tile, int> possibleTiles)
    {
        // Get all possible tiles, including the ones where character can't move bu can attack to
        List<Tile> tilesWithEnemy = new List<Tile>();

        int movementRange = Stats.MovementRange;

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
                    if (bestTile._player.GetComponent<PokemonStats>()._currentHealth - highestDamage <= 0)
                    {
                        return bestTile;
                    }
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
                Dictionary<Tile, int> possibleAttackingTiles = t.GetTiles(0, Stats.AttackRange);
                foreach (Tile tt in possibleAttackingTiles.Keys)
                {
                    // if (neighbour is empty or neighbour is where we currently are) and (this neighbour is in range)
                    if ((tt._player == null || tt._player == Character) && (possibleTiles.ContainsKey(tt) && possibleTiles[tt] <= movementRange))
                    {
                        int damage = attack.GetDamage(t._player, tt);
                        int rangeDistance = possibleTiles[tt];

                        // if this tile would do more damage, set it as best tile
                        // TODO add more conditions, like damage received?
                        if (bestTile._player.GetComponent<PokemonStats>()._currentHealth - damage <= 0)
                        {
                            bestTile = tt;
                            highestDamage = damage;
                            enemyToAttack = t._player;
                            return bestTile;
                        }
                    }
                }
            }
            return bestTile;
        }
        return null;
    }

    public Tile FindBestTile(Dictionary<Tile, int> possibleTiles)
    {
        // Get all possible tiles, including the ones where character can't move bu can attack to
        List<Tile> tilesWithEnemy = new List<Tile>();

        int movementRange = Stats.MovementRange;

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
                    bestTile = t;
                    break;
                }
            }

            // for each tile t with an enemy in range
            foreach (Tile t in tilesWithEnemy)
            {
                // for each neighbour of t
                Dictionary<Tile, int> possibleAttackingTiles = t.GetTiles(0, Stats.AttackRange);
                foreach (Tile tt in possibleAttackingTiles.Keys)
                {
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

    private Tile CollectibleInRange(Dictionary<Tile, int> possibleTiles)
    {
        foreach (Tile t in possibleTiles.Keys)
        {
            if (t._hCollectible != null)
            {
                return t;
            }
        }
        return null;
    }

    private bool IsMoreHurtInSquad()
    {
        List<Unit> squadMembers = GetComponentInParent<Squad>().Units;
        int c_health = Stats.CurrentHealth;

        // if not alone in squad
        if (squadMembers.Count > 1)
        {

            foreach (Unit u in squadMembers)
            {
                int u_health = u.GetComponent<PokemonStats>().CurrentHealth;
                if (u_health < c_health)
                {
                    return false;
                }
            }
        }

        //TODO smaller than some threshold?
        if (c_health < Stats.MaxHealth)
            return true;
        else
            return false;
    }

    #endregion Search for tile helper methods
}
