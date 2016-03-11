using UnityEngine;
using System.Collections;

public class HumanPlayer : Player
{
    private bool _isPlaying;

    public override void StartTurn()
    {
        Debug.Log("Human Start turn");
        // Reactivate all units

        // anything else ?
        _isPlaying = true;

        base.StartTurn();
    }

    void Update()
    {
        if (Input.GetKeyDown("e"))
            EndTurn();
    }

    public void EndTurn()
    {
        Debug.Log("Human end turn");
        _isPlaying = false;
        GameManager.Instance.NextTurn();
    }

    public bool IsPlaying
    {
        get
        {
            return _isPlaying;
        }
    }
}
