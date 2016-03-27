using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public enum TileDecorationType
{
    NORMAL,
    ATTACK,
    MOVE
}

public class Tile : MonoBehaviour
{
    public Character _player;

    public List<Tile> neighbours;
    public float costSoFar, heuristicValue, totalEstimatedValue;
    public Tile prevNode;

    public GameObject _Attack, _Hover, _Move, _Selected;
    private GameObject _ActiveDecoration;
    private TileDecorationType _decoration = TileDecorationType.NORMAL;

    void Start()
    {
        neighbours = new List<Tile>();
        GetNeighbours();
    }

    /// <summary>
    /// Sets a tile's decoration
    /// </summary>
    /// <param name="t"></param>
    public void SetDecoration(TileDecorationType t)
    {
        ClearDecoration();

        _decoration = t;
        switch (t)
        {
            case TileDecorationType.MOVE:
                _ActiveDecoration = _Move;
                break;
            case TileDecorationType.ATTACK:
                _ActiveDecoration = _Attack;
                break;
            case TileDecorationType.NORMAL:
            default:
                _ActiveDecoration = null;
                break;
        }
        if (_ActiveDecoration != null)
            _ActiveDecoration.SetActive(true);
    }

    /// <summary>
    /// Is the tile a movement tile?
    /// </summary>
    /// <returns></returns>
    public bool IsMovementTile()
    {
        return GetDecoration() == TileDecorationType.MOVE;
    }

    /// <summary>
    /// Get current tile decoration
    /// </summary>
    /// <returns></returns>
    public TileDecorationType GetDecoration()
    {
        return _decoration;
    }

    /// <summary>
    /// TODO: Change when we get more attacks
    /// </summary>
    public void MovementUI()
    {
        if (_player == null)
            return;

        int movementRange = _player.GetComponent<PokemonStats>().MovementRange;

        // Set decorations
        foreach (var item in GetTiles())
        {
            if (item.Value <= movementRange)
                item.Key.SetDecoration(TileDecorationType.MOVE);
            else
                item.Key.SetDecoration(TileDecorationType.ATTACK);
        }
    }

    public void AttackUI()
    {
        if (_player == null)
            return;

        int attackRange = _player.GetComponent<PokemonStats>().AttackRange;

        foreach (var item in GetTiles(0, attackRange))
        {
            if (item.Value <= 0)
                item.Key.SetDecoration(TileDecorationType.MOVE);
            else
                item.Key.SetDecoration(TileDecorationType.ATTACK);
        }
    }
    public void ClearAttackUI()
    {
        if (_player == null)
            return;

        int maxAttackRange = _player.GetComponent<PokemonStats>().AttackRange;
        foreach (var item in GetTiles(0, maxAttackRange))
            item.Key.ResetDecoration();
    }

    void MovementUIRecursive(int reach, int min, Player p)
    {
        if (reach < min || ContainsEnemy(this, p))
        {
            if (_decoration != TileDecorationType.MOVE)
                SetDecoration(TileDecorationType.ATTACK);
            return;
        }

        // Draw self
        SetDecoration(TileDecorationType.MOVE);

        // REMOVE THIS LINE TO KEEP INITIAL REACH
        foreach (var n in neighbours)
            if (ContainsEnemy(n, p))
                --reach;

        foreach (var n in neighbours)
            n.MovementUIRecursive(reach - 1, min, p);
    }

    bool ContainsEnemy(Tile t, Player p)
    {
        return t._player != null && t._player.ControllingPlayer != p;
    }

    /// <summary>
    /// Returns a dictionary of Tiles and their range from current
    /// </summary>
    /// <returns></returns>
    public Dictionary<Tile, int> GetTiles()
    {
        PokemonStats stats;
        if (_player == null ||
            (stats = _player.GetComponent<PokemonStats>()) == null)
            return null;

        int maxAttackRange = stats.AttackRange;
        int movementRange = stats.MovementRange;
        var hs = GetTilesB(movementRange, maxAttackRange);

        return hs;
    }

    /// <summary>
    /// Returns a dictionary of Tiles and their range from current
    /// </summary>
    /// <returns></returns>
    public Dictionary<Tile, int> GetTiles(int movementRange)
    {
        PokemonStats stats;
        if (_player == null ||
            (stats = _player.GetComponent<PokemonStats>()) == null)
            return null;

        int maxAttackRange = stats.AttackRange;
        var hs = GetTilesB(movementRange, maxAttackRange);

        // TODO: Remove
        //foreach (var t in hs)
        //{
        //    t.Key.OnHover();
        //}

        return hs;
    }

    /// <summary>
    /// Returns a dictionary of Tiles and their range from current
    /// </summary>
    /// <returns></returns>
    public Dictionary<Tile, int> GetTiles(int movementRange, int maxAttackRange)
    {
        return GetTilesB(movementRange, maxAttackRange);
    }

    /// <summary>
    /// Breadth first search
    /// </summary>
    /// <param name="reach"></param>
    /// <returns></returns>
    Dictionary<Tile, int> GetTilesB(int reach, int attack)
    {
        // Variables
        int total = reach + attack;
        int r = 0; // r is Tile current reach

        List<KeyValuePair<Tile, int>> open = new List<KeyValuePair<Tile, int>>();
        Dictionary<Tile, int> closed = new Dictionary<Tile, int>();
        open.Add(new KeyValuePair<Tile, int>(this, 0));

        // Open list should contain only reachable + attackable
        while (open.Count != 0)
        {
            // Take and remove first
            var kvp = open[0];
            open.RemoveAt(0);
            r = kvp.Value;

            // Out of bounds (This limits the open list)
            if (r > total)
                continue;

            // Tile contains an enemy, no point doing anything. Set reach to total to be seen as attack
            if (ContainsEnemy(kvp.Key, _player.ControllingPlayer))
            {
                closed.Add(kvp.Key, total);
                if (r <= reach)
                    r = total - attack + 1;
            }
            else {
                // Add to the closed list with current reach
                closed.Add(kvp.Key, r);

                // If it's a movement tile, reduce depending on neighbours
                if (r <= reach)
                {
                    foreach (var n in kvp.Key.neighbours)
                    {
                        if (ContainsEnemy(n, _player.ControllingPlayer))
                            ++r;
                    }
                    // Went to far, but still want to consider 'attack' count of Tiles to add
                    if (r >= (total - attack))
                        r = total - attack;
                }
            }

            // +1 for the next tile
            ++r;

            // Go through all neighbours. Add or update open list
            foreach (var n in kvp.Key.neighbours)
            {
                if (!closed.ContainsKey(n))
                {
                    int index = open.FindIndex(x => x.Key == n);
                    if (index != -1)
                    {
                        if (open[index].Value > r)
                            open[index] = new KeyValuePair<Tile, int>(n, r);
                        else
                            continue;
                    }
                    else
                        open.Add(new KeyValuePair<Tile, int>(n, r));
                }
            }
        }

        // Finished!
        return closed;
    }

    /// <summary>
    /// Clear all movement/attack UI
    /// </summary>
    public void ClearMovementUI()
    {
        if (_player == null)
            return;

        int maxAttackRange = _player.GetComponent<PokemonStats>().AttackRange;
        int movementRange = _player.GetComponent<PokemonStats>().MovementRange + maxAttackRange;
        //ClearMovementUIRecursive(movementRange);
        foreach (var item in GetTiles())
            item.Key.ResetDecoration();
    }

    void ClearMovementUIRecursive(int reach)
    {
        if (reach < 0)
            return;

        ResetDecoration();
        foreach (var n in neighbours)
            n.ClearMovementUIRecursive(reach - 1);
    }

    /// <summary>
    /// Resets a tile
    /// </summary>
    private void ResetDecoration()
    {
        ClearDecoration();
    }

    /// <summary>
    /// Clear active decoration on tile
    /// </summary>
    private void ClearDecoration()
    {
        _decoration = TileDecorationType.NORMAL;
        if (_ActiveDecoration != null)
            _ActiveDecoration.SetActive(false);
    }

    #region Selection

    /// <summary>
    /// Used for hovering the tile
    /// </summary>
    public void OnHover()
    {
        if (!_Selected.activeSelf)
            _Hover.SetActive(true);
    }

    /// <summary>
    /// Resets the hover
    /// </summary>
    public void ResetHover()
    {
        _Hover.SetActive(false);
    }

    public void SetSelected()
    {
        _Selected.SetActive(true);
    }

    public void Deselect()
    {
        _Selected.SetActive(false);
    }

    #endregion Selection

    // Get its neighbours
    // TODO Will need to add more checkers, for instance to see if neighbour can be visited
    private void GetNeighbours()
    {
        Tile[,] tiles = GameObject.Find("TileGenerator").GetComponent<TileGenerator>().Tiles;
        int x = (int)transform.position.x;
        int z = (int)transform.position.z;

        if (x - 1 >= 0)
        {
            neighbours.Add(tiles[x - 1, z]);
        }
        if (x + 1 < tiles.GetLength(0))
        {
            neighbours.Add(tiles[x + 1, z]);
        }
        if (z - 1 >= 0)
        {
            neighbours.Add(tiles[x, z - 1]);
        }
        if (z + 1 < tiles.GetLength(1))
        {
            neighbours.Add(tiles[x, z + 1]);
        }
    }
}
