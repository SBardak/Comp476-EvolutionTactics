using UnityEngine;
using System.Collections;

public class HumanPlayer : GamePlayer {
    public override void StartTurn()
    {
        // Reactivate all units

        // anything else ?

        base.StartTurn();
    }

    void Update()
    {
        if (Input.GetKeyDown("e"))
            GameManager.Instance.NextTurn();
    }
}
