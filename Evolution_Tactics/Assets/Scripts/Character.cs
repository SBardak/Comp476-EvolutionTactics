using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Character : MonoBehaviour
{
    public delegate void CharacterHandler (Character c);
    public event CharacterHandler OnDeath;

    public Rigidbody _rb;
    private Animator _animator;

    public float _targetRadius;
    public float _slowRadius;
    public float _timeToTarget;
    public float _maxAcceleration;
    public float _maxVelocity;

    public float _turnSpeed;
    public float _kinematicSpeed;

    public Tile _currentTile;

    private Player _controllerPlayer;

    public Player ControllingPlayer
    {
        get { return _controllerPlayer; }
        set
        {
            _controllerPlayer = value;
            if (GetComponent<HP_Tracker>() != null)
            {
                GetComponent<HP_Tracker>().SetColor(_controllerPlayer.TeamColor);
            }
        }
    }

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
    }

    void Start()
    {
    }

    void Update()
    {
//        _animator.SetFloat("Walk", _rb.velocity.magnitude);
    }

    public void Attack(Character enemyToAttack)
    {
        RotateDirectly(enemyToAttack.transform.position);
        GetComponent<AttackAlgorithm>().DoDamage(enemyToAttack);
    }

    public bool MoveTo(Vector3 location, Vector3 previousLocation)
    {
        Vector3 oldPosition = transform.position;
        _rb.velocity = Vector3.zero;

        Vector3 rotationDirection = location - previousLocation;

        transform.position = Vector3.MoveTowards(transform.position, location, Time.deltaTime); 
        transform.rotation = Quaternion.LookRotation(rotationDirection);

        return oldPosition == transform.position;
    }


    public bool KinematicMovement(Vector3 target)
    {
        _rb.velocity = Vector3.zero;
        Rotate(target);
        if (MoveTowards(target))
        {
            return true;
        }
        return false;
    }

    // Arrive to target and Look Where You're going
    public void ArriveAndLookWhereYoureGoing(Vector3 target)
    {
        Vector3 accel = Arrive(target);
        Steer(accel);
        LookWhereYoureGoing();
    }

    // Arrive towards a target
    private Vector3 Arrive(Vector3 target)
    {
        Vector3 targetVel = target - transform.position;

        // if close enough to target
        if (targetVel.magnitude < _targetRadius)
        {
            _rb.velocity = Vector2.zero;
            return Vector2.zero;
        }

        targetVel.Normalize();

        return _maxAcceleration * targetVel;
    }

    //Look in the diestion you are going
    private void LookWhereYoureGoing()
    {
        Vector3 direction = _rb.velocity;
        direction.Normalize();
        // Align
        transform.rotation = Align(direction);
    }

    // Steer given an acceleration
    private void Steer(Vector3 linearAcceleration)
    {
        _rb.velocity += linearAcceleration * Time.deltaTime;

        if (_rb.velocity.magnitude > _maxVelocity)
        {
            _rb.velocity.Normalize();
            _rb.velocity *= _maxVelocity;
        }
    }

    //Align to a target
    private Quaternion Align(Vector3 target)
    {
        float toRotation = (Mathf.Atan2(target.x, target.z) * Mathf.Rad2Deg);
        // rotate along y axis
        float rotation = Mathf.LerpAngle(transform.rotation.eulerAngles.y, toRotation, Time.deltaTime * _turnSpeed * 40);

        return Quaternion.Euler(0, rotation, 0);
    }

    //Move towards a point
    private bool MoveTowards(Vector3 target)
    {
        Vector3 oldPosition = transform.position;

        Vector3 velocity = KinematicSeek(target);
        transform.position += velocity * Time.deltaTime;

        return Vector3.Distance(transform.position, oldPosition) < 0.1f;
    }

    private Vector3 KinematicSeek(Vector3 target)
    {
        Vector3 direction = target - transform.position;
        direction.Normalize();
        return _maxVelocity * direction;
    }

    public bool Rotate(Vector3 location)
    {
        Quaternion prevRotation = transform.rotation;

        /*transform.rotation = Quaternion.RotateTowards(transform.rotation, 
            Quaternion.LookRotation(location - transform.position),
            _turnSpeed * Time.deltaTime);*/

        Vector3 targetDir = location - transform.position;
        float step = _turnSpeed * Time.deltaTime;
        Vector3 newDir = Vector3.RotateTowards(transform.forward, targetDir, step, 0.0F);
        transform.rotation = Quaternion.LookRotation(newDir);

        if (transform.rotation == prevRotation)
        {
            return true;
        }
        return false;
    }

    public void RotateDirectly(Vector3 location)
    {
        transform.LookAt(location);
    }

    public bool HasEnemyNeighbours()
    {
        foreach (Tile neighbour in _currentTile.neighbours)
        {
            Character neighbourCharacter = neighbour._player;
            if (neighbourCharacter != null && neighbourCharacter.tag == "AI")
            {
                return true;
            }
        }
        return false;
    }


    public List<Tile> GetNeighbourTiles()
    {
        return _currentTile.neighbours;
    }

    public void SetCurrentTile(Tile tile)
    {
        if (_currentTile != null)
        {
            _currentTile._player = null;
        }

        if (tile._player == null)
        {
            tile._player = this;
            _currentTile = tile;
        }
        
    }
        
    /* ADDED BY FRANCIS. RELOCATE WHEN DONE */
    public void ResetPosition()
    {
        // Maybe?
    }

    public void Activate()
    {
        IsActivated = true;
        Moved = false;
        GetComponent<HP_Tracker>().HP_Activated();
    }

    public void Deactivate()
    {
        IsActivated = false;
        GetComponent<HP_Tracker>().HP_Deactivated();
    }

    void HandleDeath()
    {
        if (_currentTile != null)
            _currentTile._player = null;

        if (OnDeath != null)
            OnDeath(this);

        Destroy(gameObject);
    }

    public bool Moved;
    public bool IsActivated;
}
