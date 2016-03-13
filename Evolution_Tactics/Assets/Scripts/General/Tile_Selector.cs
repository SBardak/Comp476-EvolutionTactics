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

        // Not a human player's turn
        var player = GameManager.Instance.GetActivePlayer();
        if (!(player is HumanPlayer))
        {
            Debug.Log("Not human");
            return;
        }
        var human = (HumanPlayer)player;

        human.HandleSelection(_myTile);
    }

    protected override void OnMouseOver()
    {
        Debug.LogWarning("Fire element, +10 evade, etc");

        _myTile.OnHover();
        //base.OnMouseOver();
    }
    protected override void OnMouseExit()
    {
        _myTile.ResetHover();
    }
}
