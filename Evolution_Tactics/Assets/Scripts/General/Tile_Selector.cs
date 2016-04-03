using UnityEngine;
using System.Collections;

public class Tile_Selector : Selector
{
    #region Fields

    private Tile _myTile;

    #endregion Fields

    #region Methods

    void Start()
    {
        _myTile = gameObject.GetComponent<Tile>();
    }

    /// <summary>
    /// On ray hit
    /// </summary>
    /// <param name="number">Number in rayhit</param>
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

    /// <summary>
    /// On mouse over (hovering)
    /// </summary>
    protected override void OnMouseOver()
    {
//        Debug.LogWarning("Fire element, +10 evade, etc");

        _myTile.OnHover();

        // Not a human player's turn
        var player = GameManager.Instance.GetActivePlayer();
        //UIManager.Ins
        if (!(player is HumanPlayer))
        {
//            Debug.Log("Not human");
            return;
        }
        var human = (HumanPlayer)player;
        human.HandleHover(_myTile);
        //base.OnMouseOver();
    }

    /// <summary>
    /// On mouse exit (to remove hover decorations)
    /// </summary>
    protected override void OnMouseExit()
    {
        _myTile.ResetHover();

        // Not a human player's turn
        var player = GameManager.Instance.GetActivePlayer();
        if (!(player is HumanPlayer))
        {
            Debug.Log("Not human");
            return;
        }
        var human = (HumanPlayer)player;

        human.HandleHoverOut(_myTile);
    }

    #endregion Methods
}
