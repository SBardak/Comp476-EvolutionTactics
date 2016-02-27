using UnityEngine;
using System.Collections;

public class Tile_Selector : Selector {

    public override void RayHit(int number)
    {
        /* Couple of scenarios */

        // No selected units

        // Selected unit
            // Reachable terrain

            // Unreachable terrain (further than walkable)
    }

    protected override void OnMouseOver()
    {
        Debug.LogWarning("Fire element, +10 evade, etc");
        base.OnMouseOver();
    }
}
