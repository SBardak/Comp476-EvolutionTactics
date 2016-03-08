using UnityEngine;
using System.Collections;

public class Tile_Selector : Selector
{

    private Tile _myTile;
    private Pathfinding _pathFinder;

    void Start()
    {
        _myTile = gameObject.GetComponent<Tile>();
    }

    public override void RayHit(int number)
    {
        /* Couple of scenarios */

        // No selected units

        // Selected unit
        // Reachable terrain
        {
            if (_pathFinder == null)
                _pathFinder = GameObject.Find("PathFinding").GetComponent<Pathfinding>();
            _pathFinder._endNode = _myTile;
            _pathFinder.CalculateNewPath();
        }

        // Unreachable terrain (further than walkable)
    }

    protected override void OnMouseOver()
    {
        Debug.LogWarning("Fire element, +10 evade, etc");
        base.OnMouseOver();
    }
}
