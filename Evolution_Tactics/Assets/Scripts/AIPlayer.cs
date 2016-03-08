using UnityEngine;
using System.Collections;

public class AIPlayer : GamePlayer {
    public override void StartTurn()
    {
        // Reactivate all units

        StartCoroutine(ExecuteTurn());
    
        base.StartTurn();
    }

    IEnumerator ExecuteTurn()
    {
        yield return null;

        //GameManager.Instance.NextTurn();
    }
}
