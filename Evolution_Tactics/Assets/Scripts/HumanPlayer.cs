using UnityEngine;
using System.Collections;

public class HumanPlayer : Player {
    public override void StartTurn()
    {
        // Reactivate all units

        // anything else ?

        base.StartTurn();
    }

    void Update()
    {
        if (Input.GetKeyDown("e"))
            EndTurn();
    }

    void EndTurn()
    {
        Debug.Log("Human end turn");
        GameManager.Instance.NextTurn();
    }
}
