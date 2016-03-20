using UnityEngine;
using System.Collections;

public class Unit : MonoBehaviour
{

    public delegate void UnitHandler(Unit u);
    public event UnitHandler MovementComplete;
    public event UnitHandler Death;

    Character _char;
    Pathfinding _pathfinding;

    Character Character {
        get {
            if (_char == null)
               _char = GetComponentInChildren<Character>();
            return _char;
        }
    }

    public void SetControllingPlayer(Player p)
    {
        Character.ControllingPlayer = p;
    }

    void Awake()
    { 
        if ((_pathfinding = GetComponentInChildren<Pathfinding>()) != null)
        {
            _pathfinding.OnReachEnd += ReachedDestination;
        }
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
        _pathfinding.RandomPath();

        // if (!_char.IsActivated)
        //FinishedMove();

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


    void HandleDeath()
    {
        if (Death != null)
            Death(this);
    }
}
