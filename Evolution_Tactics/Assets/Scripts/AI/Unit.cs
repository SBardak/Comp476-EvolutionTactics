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
    PokemonStats _stats;

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
            Debug.Log(name + " Collect health");
            nextTile = collecTile;
        }
        else if (nextTile.IsOccupied)
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

        // if long range
        if (Character._unitType == UnitType.LONG_RANGE)
        {
            Debug.LogWarning("Long Range");

            nextTile = BestRangedAttack(possibleTiles);

            if (c != null)
            {
                if (enemyToAttack == null && AttackableInRange(c, possibleTiles))
                {
                    nextTile = BestRangedAttack(c, possibleTiles);
                    Debug.LogWarning(name + " attacks target " + c.name + " by going to " + nextTile);
                }
                else
                {
                    Debug.LogWarning(name + " attacks " + c.name + " by going to " + nextTile);
                }
            }
        }
        else if (Character._unitType == UnitType.ATTACKER)
        {
            Debug.LogWarning("Attacker");

            nextTile = FindBestTile(possibleTiles);

            if (c != null)
            {
                if (enemyToAttack == null && AttackableInRange(c, possibleTiles))
                {
                    nextTile = FindBestTile(possibleTiles, c);
                    Debug.LogWarning(name + " attacks target " + c.name + " by going to " + nextTile);
                }
                else
                {
                    Debug.LogWarning(name + " attacks " + c.name + " by going to " + nextTile);
                }
            }
        }
        else if (Character._unitType == UnitType.TANKER)
        {
            Debug.LogWarning("Tanker");

            if (c != null)
            {
                nextTile = FindBestTile(possibleTiles, c);

                if (!AttackableInRange(c, possibleTiles) || enemyToAttack == null)
                {
                    nextTile = FindBestTile(possibleTiles);
                    Debug.LogWarning(name + " attacks target " + c.name + " by going to " + nextTile);
                }
                else
                {
                    Debug.LogWarning(name + " attacks " + c.name + " by going to " + nextTile);
                }
            }
            else
            {
                nextTile = FindBestTile(possibleTiles);
            }
        }

        if (nextTile == null)
        {
            Debug.LogWarning("Cannot attack anybody");
            if (c != null)
                nextTile = FindNewTileAround(c._currentTile, possibleTiles);
            if (nextTile == null)
                nextTile = GoToClosestEnemy(possibleTiles);
            Debug.LogWarning("Go to " + nextTile);
        }

        MoveTo(nextTile);
    }

    private void MoveTo(Tile nextTile)
    {
        if (nextTile != null && !nextTile.IsOccupied)
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

    void ReachedDestination()
    {
        // Attack ?
        if (enemyToAttack != null && Attackable(enemyToAttack))
        {
            Attack();
        }
        else
        {
            if (enemyToAttack == null)
            {
                if (AttackableInRange())
                {
                    Attack();
                }
                else
                {
                    FinishedMove();
                }
            }
            else
            { 
                FinishedMove();
            }
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

    private Tile GoToClosestEnemy(Dictionary<Tile, int> possibleTiles)
    {
        Debug.LogError("Closest Enemy");
        List<Tile> tilesWithEnemy = new List<Tile>();

        foreach (Tile t in possibleTiles.Keys)
        {
            // if t has a human player on it
            if (t.HasPlayer && t._character.tag == "Human")
            {
                tilesWithEnemy.Add(t);
            }
        }

        float closest = 5;
        Tile closestEnemyTile = null;
        foreach (Tile t in tilesWithEnemy)
        {
            float distance = Vector3.Distance(t.transform.position, transform.position);
            if (distance < closest)
            {
                closest = distance;
                closestEnemyTile = t;
            }
        }

        if (closestEnemyTile == null)
        {
            return null;
        }

        return FindNewTileAround(closestEnemyTile, possibleTiles);
    }

    private Tile BestRangedAttack(Dictionary<Tile, int> possibleTiles)
    {
        // dictionnary that contains the tile to attack from and the target
        // tile with enemy in range
        List<Tile> tilesWithEnemy = new List<Tile>();

        //get a list of the tiles in range that contain an enemy
        foreach (Tile t in possibleTiles.Keys)
        {
            // if t has a human player on it
            if (t.HasPlayer && t._character.tag == "Human")
            {
                tilesWithEnemy.Add(t);
            }
        }

        //Dictionary<Tile, Character> possible = new Dictionary<Tile, Character>();
        List<Tile> possible = new List<Tile>();
        List<Character> enemy = new List<Character>();

        // get a list of possible tile it can attack from
        foreach (Tile t in tilesWithEnemy)
        {
            Dictionary<Tile, int> possibleAttack = t.GetTiles(0, 2);
            foreach (Tile tt in possibleAttack.Keys)
            {
                if ((!tt.IsOccupied || tt == Character._currentTile) && possibleTiles.ContainsKey(tt) && possibleTiles[tt] <= Stats.MovementRange && Attackable(t, tt))
                {
                    possible.Add(tt);
                    enemy.Add(t._character);
                }
            }
        }

        enemyToAttack = null;

        // if there are more than 1 possible tiles
        if (possible.Count > 1)
        {
            Tile best = null;
            //float furthest = Vector3.Distance(best.transform.position, transform.position);
            float highestReward = -1;

            // try to go to best tile
            int i = 0;
            foreach (Tile t in possible)
            {
                //float distance = Vector3.Distance(t.transform.position, transform.position);
                float reward = AttackReward(enemy[i], t);
                // the higher the reward, the better
                if (reward > highestReward)
                {
                    best = t;
                    highestReward = reward;
                    enemyToAttack = enemy[i];
                }
                i++;
            }
            return best;
        }
        else if (possible.Count == 1)
        {
            float reward = AttackReward(enemy[0], possible[0]);

            // rewards need to be high enough
            if (reward >= 0)
            {
                enemyToAttack = enemy[0];
                return possible[0];
            }
        }

        return null;
    }

    private Tile BestRangedAttack(Character target, Dictionary<Tile, int> possibleTiles)
    {
        // possible tile target can be attacked from
        Dictionary<Tile, int> possibleAttack = target._currentTile.GetTiles(0, 2);
        List<Tile> possible = new List<Tile>();

        // for each possible attacking tile from
        foreach (Tile t in possibleAttack.Keys)
        {        
            Debug.LogError(t);
            // if this tile is empty and it can go this tile and can attack from it
            if ((!t.IsOccupied || t == Character._currentTile) && possibleTiles.ContainsKey(t) && possibleTiles[t] <= Stats.MovementRange && Attackable(target._currentTile, t))
            {
                Debug.LogError("2 " + t);
                // set the target as being the next attack
                enemyToAttack = target;
                possible.Add(t);
            }
        }

        // if cannot attack target, then return null
        if (enemyToAttack == null)
        {
            return null;
        }

        // if more than one possible tile
        if (possible.Count > 1)
        {
            Tile best = null;
            //float furthest = Vector3.Distance(best.transform.position, transform.position);
            float highestReward = 0;

            // try to go to best tile
            foreach (Tile t in possible)
            {
                //float distance = Vector3.Distance(t.transform.position, transform.position);
                float reward = AttackReward(target, t);
                // the higher the reward, the better
                if (reward > highestReward)
                {
                    best = t;
                    highestReward = reward;
                }
            }
            return best;
        }
        // if only one possible tile
        else if (possible.Count == 1)
        {
            float reward = AttackReward(target, possible[0]);
            // rewards need to be high enough
            if (reward >= 0)
                return possible[0];
        }
            
        enemyToAttack = null;
        // return null if reward is not high enough or if no possible tile to go to
        return null;
    }

    private Tile FindNewTileAround(Tile t, Dictionary<Tile, int> possibleTiles)
    {
        Tile newTile = null;
        float closest = 5f;
        Dictionary<Tile, int> aroundT = t.GetTiles(0, 4);

        foreach (Tile tt in aroundT.Keys)
        {
            if (possibleTiles.ContainsKey(tt) && (!tt.IsOccupied || tt == Character._currentTile))
            {
                float distance = Vector3.Distance(t.transform.position, tt.transform.position);
                if (distance < closest)
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
        List<Unit> squadMembers = GetComponentInParent<Squad>().Units;

        int movementRange = Stats.MovementRange;

        //get a list of the tiles in range that contain an enemy
        foreach (Tile t in possibleTiles.Keys)
        {
            if (t.HasPlayer && t._character.tag == "Human")
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
                for (int i = 0; i < 4; i++)
                {
                    bestTile = t.neighbours[i];
                    // when no player on the tile, break
                    if (possibleTiles.ContainsKey(t) && possibleTiles[t] <= movementRange && !bestTile.HasPlayer)
                    {
                        highestDamage = attack.GetDamage(t._character, bestTile);
                        enemyToAttack = t._character;
                        if (bestTile._character.GetComponent<PokemonStats>()._currentHealth - highestDamage <= 0)
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
            }

            // for each tile t with an enemy in range
            foreach (Tile t in tilesWithEnemy)
            {
                // for each neighbour of t
                Dictionary<Tile, int> possibleAttackingTiles = t.GetTiles(0, Stats.AttackRange);
                foreach (Tile tt in possibleAttackingTiles.Keys)
                {
                    // if (neighbour is empty or neighbour is where we currently are) and (this neighbour is in range)
                    if ((!tt.HasPlayer || tt._character == Character) && (possibleTiles.ContainsKey(tt) && possibleTiles[tt] <= movementRange))
                    {
                        int damage = attack.GetDamage(t._character, tt);
                        int rangeDistance = possibleTiles[tt];

                        // if this tile would do more damage, set it as best tile
                        // TODO add more conditions, like damage received?
                        if (t._character.GetComponent<PokemonStats>()._currentHealth - damage <= 0)
                        {
                            bestTile = tt;
                            highestDamage = damage;
                            enemyToAttack = t._character;
                            return bestTile;
                        }
                    }
                }
            }
            return bestTile;
        }
        return null;
    }

    /* public Tile FindBestTile(Character c, Dictionary<Tile, int> possible)
    {
        Dictionary<Tile, int> possibleTiles = c._currentTile.GetTiles(0, 1);
        foreach (Tile t in possibleTiles.Keys)
        {
            if (!possible.ContainsKey(t))
            {
                possibleTiles.Remove(t);
            }
        }

        return FindBestTile(possibleTiles, c);
    }*/

    public Tile FindBestTile(Dictionary<Tile, int> possibleTiles, Character enemy = null)
    {
        // Get all possible tiles, including the ones where character can't move bu can attack to
        List<Tile> tilesWithEnemy = new List<Tile>();
        int movementRange = Stats.MovementRange;
        if (enemy == null)
        {
            //get a list of the tiles in range that contain an enemy
            foreach (Tile t in possibleTiles.Keys)
            {
                if (t.HasPlayer && t._character.tag == "Human")
                {
                    tilesWithEnemy.Add(t);
                }
            }
        }
        else
        {
            tilesWithEnemy.Add(enemy._currentTile);
        }

        //if there are at least one enemy in range
        // TODO add more conditions?
        if (tilesWithEnemy.Count > 0)
        {
            Tile bestTile = null;
            AttackAlgorithm attack = GetComponentInChildren<AttackAlgorithm>();
            float highestCost = 0f;
            //Find an initial empty best tile
            foreach (Tile t in tilesWithEnemy)
            {
                for (int i = 0; i < t.neighbours.Count; i++)
                {
                    if (possibleTiles.ContainsKey(t.neighbours[i]) && (!t.neighbours[i].IsOccupied || t.neighbours[i] == Character._currentTile))
                    {
                        // when no player on the tile, break
                        if (possibleTiles.ContainsKey(t) && possibleTiles[t] <= movementRange)
                        {
                            //highestDamage = attack.GetDamage(tilesWithEnemy[0]._character, bestTile);
                            bestTile = t.neighbours[i];
                            highestCost = AttackReward(t._character, bestTile);
                            enemyToAttack = t._character;
                            bestTile = t;
                            break;
                        }
                    }
                }
            }

            // for each tile t with an enemy in range
            foreach (Tile t in tilesWithEnemy)
            {
                // for each neighbour of t
                Dictionary<Tile, int> possibleAttackingTiles = t.GetTiles(0, 1);
                foreach (Tile tt in possibleAttackingTiles.Keys)
                {
                    // if (neighbour is empty or neighbour is where we currently are) and (this neighbour is in range)
                    if ((!tt.IsOccupied || tt._character == Character) && (possibleTiles.ContainsKey(tt) && possibleTiles[tt] <= movementRange))
                    {
                        //int cost = attack.GetDamage(t._character, tt);
                        float cost = AttackReward(t._character, tt);
                        int rangeDistance = possibleTiles[tt];

                        // if this tile would do more damage, set it as best tile
                        // TODO add more conditions, like damage received?
                        if (cost > highestCost)
                        {
                            bestTile = tt;
                            highestCost = cost;
                            enemyToAttack = t._character;
                        }
                    }
                }
            }
            return bestTile;
        }
        return null;
    }

    /// <summary>
    /// Calculate reward of an attack
    /// The highest the reward, the better 
    /// </summary>
    private float AttackReward(Character target, Tile goToTile)
    {
        AttackAlgorithm attack = GetComponentInChildren<AttackAlgorithm>();
        //calculate hypothetical damage done to target
        int damageDone = attack.GetDamage(target, goToTile);

        int targetHealth = target.GetComponent<PokemonStats>().CurrentHealth;
        int maxHealth = target.GetComponent<PokemonStats>().MaxHealth;

        // if attack would kill the target, return high cost
        if (targetHealth - damageDone < 0)
            return 1000f;

        // calculate hypothetical damage received from target
        int damageReceived = target.GetComponentInChildren<AttackAlgorithm>().GetDamage(GetComponent<Character>(), target._currentTile);

        // calculate reward of an attack. Emphasis on damage done over received
        float reward = damageDone + (maxHealth - targetHealth) / 5 - (0.5f * damageReceived);

        return reward;
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

    private bool Attackable(Character target)
    {
        return target.tag == "Human" && LukasIsTheBest(target.transform.position);
    }

    private bool Attackable(Tile t)
    {
        return t._character != null && t._character.tag == "Human" && LukasIsTheBest(t.transform.position);
    }

    private bool Attackable(Tile t, Tile tt)
    {
        return t._character != null && t._character.tag == "Human" && LukasIsTheBest2(t.transform.position, tt.transform.position);
    }

    private bool LukasIsTheBest(Vector3 position)
    {
        int x = (int)Mathf.Round(Mathf.Abs(position.x - transform.position.x));
        int z = (int)Mathf.Round(Mathf.Abs(position.z - transform.position.z));

        Debug.LogWarning("In Attackable " + (x + z) + " " + x + " " + z);

        return /*(x == Stats.AttackRange && y == 0) ||
        (y == Stats.AttackRange && x == 0) ||*/
        z + x == Stats.AttackRange;
    }

    private bool LukasIsTheBest2(Vector3 position, Vector3 position2)
    {
        int x = (int)Mathf.Round(Mathf.Abs(position.x - position2.x));
        int z = (int)Mathf.Round(Mathf.Abs(position.z - position2.z));

        return /*(x == Stats.AttackRange && y == 0) ||
        (y == Stats.AttackRange && x == 0) ||*/
            z + x == Stats.AttackRange;
    }

    private bool AttackableInRange()
    {
        Dictionary<Tile, int> possibleAttacks = Character._currentTile.GetTiles(0, Stats.AttackRange);
        List<Tile> attackable = new List<Tile>();

        float highestCost = -1;
        Tile bestTile = null;
        enemyToAttack = null;

        if (possibleAttacks.Count > 0)
        {
            foreach (Tile t in possibleAttacks.Keys)
            {
                if (t._character != null && t._character.tag == "Human" && Attackable(t._character))
                {
                    attackable.Add(t);
                }
            } 

            foreach (Tile t in attackable)
            {
                float cost = AttackReward(t._character, Character._currentTile);
                if (cost > highestCost)
                {
                    bestTile = t;
                    enemyToAttack = t._character;
                    highestCost = cost;
                }
            }
        }

        Debug.LogError(bestTile + " + " + highestCost + " + " + attackable.Count);
        if (bestTile != null)
            return true;

        return false;
    }

    private bool AttackableInRange(Character c, Dictionary<Tile, int> range)
    {
        Dictionary<Tile, int> possibleToAttackFrom = c._currentTile.GetTiles(0, Stats.AttackRange);
        //List<Tile> possibleEndNode = new List<Tile>();

        //Union of the keys of the two dictionnary
        foreach (Tile t in possibleToAttackFrom.Keys)
        {
            if (range.ContainsKey(t) && t._character == null)
            {
                return true;
            }
        }
        return false;
    }

    #endregion Search for tile helper methods
}
