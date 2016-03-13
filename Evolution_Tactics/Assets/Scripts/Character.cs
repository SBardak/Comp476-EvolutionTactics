using UnityEngine;
using System.Collections;

public class Character : MonoBehaviour
{
    private Rigidbody _rb;
    private Animator _animator;

    public float _targetRadius;
    public float _slowRadius;
    public float _timeToTarget;
    public float _maxAcceleration;
    public float _maxVelocity;

    public float _turnSpeed;
    public float _kinematicSpeed;

    public Tile _currentTile;

    private UIManager _uiManager;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _animator = GetComponent<Animator>();
        if (GameObject.Find("UIManager") != null)
            _uiManager = GameObject.Find("UIManager").GetComponent<UIManager>();
    }

    void Start()
    {
    }

    void Update()
    {
        _animator.SetFloat("Walk", _rb.velocity.magnitude);
    }

    public void KinematicMovement(Vector3 target)
    {
        Rotate(target);
        MoveTowards(target);
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
        float rotation = Mathf.LerpAngle(transform.rotation.eulerAngles.y, toRotation, Time.deltaTime * _turnSpeed * 2);

        return Quaternion.Euler(0, rotation, 0);
    }

    //Move towards a point
    private void MoveTowards(Vector3 target)
    {
        Vector3 velocity = KinematicSeek(target);
        transform.position += velocity * Time.deltaTime;
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

        transform.rotation = Quaternion.RotateTowards(transform.rotation, 
            Quaternion.LookRotation(location - transform.position),
            _turnSpeed * Time.deltaTime);

        if (transform.rotation == prevRotation)
        {
            return true;
        }
        return false;
    }

    public bool HasEnemyNeighbours()
    {
        foreach (Tile neighbour in _currentTile.neighbours)
        {
            Character neighbourCharacter = neighbour.player;
            if (neighbourCharacter != null && neighbourCharacter.tag == "AI")
            {
                return true;
            }
        }
        return false;
    }

    public void SetCurrentTile(Tile tile)
    {
        if (_currentTile != null)
            _currentTile.player = null;
        
        tile.player = this;
        _currentTile = tile;
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
    }
    public void Deactivate()
    {
        IsActivated = false;
    }
    public bool Moved;
    public bool IsActivated;
}
