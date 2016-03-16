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
    public Character player;

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
    /// <param name="reach"></param>
    /// <param name="t"></param>
    public void MovementUI(int reach)
    {
        if (reach < 0)
        {
            if (_decoration != TileDecorationType.MOVE)
                SetDecoration(TileDecorationType.ATTACK);
            return;
        }

        // Draw self
        SetDecoration(TileDecorationType.MOVE);

        foreach (var n in neighbours)
            n.MovementUI(reach - 1);
    }

    /// <summary>
    /// Clear all movement/attack UI
    /// </summary>
    /// <param name="reach">How far to go</param>
    public void ClearMovementUI(int reach)
    {
        if (reach < -1)
            return;

        ResetDecoration();
        foreach (var n in neighbours)
            n.ClearMovementUI(reach - 1);
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
