using UnityEngine;
using System.Collections;
using System.Collections.Generic;

// Started by: Lukas
// Modified by:

/*
  This script is for the moment only for testing purpose
  */
public enum TileDecorationType
{
    NORMAL, ATTACK, MOVE
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
        if (_player == null) return;

        int maxAttackRange = 1;
        int movementRange = _player.GetComponent<PokemonStats>().MovementRange + maxAttackRange;
        MovementUIRecursive(movementRange, maxAttackRange, _player.ControllingPlayer);
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
            if (ContainsEnemy(n, p)) --reach;

        foreach (var n in neighbours)
            n.MovementUIRecursive(reach - 1, min, p);
    }
    bool ContainsEnemy(Tile t, Player p)
    {
        return t._player != null && t._player.ControllingPlayer != p;
    }

    /// <summary>
    /// Clear all movement/attack UI
    /// </summary>
    public void ClearMovementUI()
    {
        if (_player == null) return;

        int maxAttackRange = 1;
        int movementRange = _player.GetComponent<PokemonStats>().MovementRange + maxAttackRange;
        ClearMovementUIRecursive(movementRange);
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
