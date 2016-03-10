using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour {

    public delegate void MovementCompleteHandler(Unit u);
    public event MovementCompleteHandler MovementComplete;

    Character _char;

    void Awake()
    { 
        // Link char events to handlers
        // Change names?
        // _char.MoveComplete += ReachedDestination;
        // _char.AttackComplete += Attacked;
    }

    public void Activate()
    { 
        // _char.Activate();
    }

    public void Move()
    { 
        // if (!_char.IsActivated)
        FinishedMove();

        // Decide where to move

        // Move the character
        // _char.Move() (or whatever)
    }

    void ReachedDestination()
    {
        // Attack ?
        // if ( decided to attack )
        //{
        //    Attack();
        //}
        // else
        {
            FinishedMove();
        }
    }

    void Attack()
    { 
        // Select attack

        // Let char do its thing
        // _char.attack(); -> End of attack event link to finished attack
    }

    void FinishedAttack()
    {
        FinishedMove();
    }

    void FinishedMove()
    {
        Debug.Log("Unit finished movement");
        if (MovementComplete != null)
            MovementComplete(this);
    }
}
