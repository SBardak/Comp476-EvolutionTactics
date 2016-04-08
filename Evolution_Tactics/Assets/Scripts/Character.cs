using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum UnitType
{
    ATTACKER,
    TANKER,
    LONG_RANGE
}

public class Character : MonoBehaviour
{
    #region Delegate

    public delegate void CharacterHandler(Character c);

    public event CharacterHandler OnDeath;

    #endregion Delegate

    #region Fields

    public Rigidbody _rb;

    public float _targetRadius;
    public float _slowRadius;
    public float _timeToTarget;
    public float _maxAcceleration;
    public float _maxVelocity;

    public float _turnSpeed;
    public float _kinematicSpeed;

    public Tile _currentTile;

    private Player _controllerPlayer;

    public bool Moved;
    public bool IsActivated;

    public UnitType _unitType = UnitType.TANKER;

    #endregion Fields

    #region Properties

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

    #endregion Properties

    #region Methods

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
//        _animator.SetFloat("Walk", _rb.velocity.magnitude);
    }

    #region Locomotion

    public void Attack(Character enemyToAttack)
    {
        AttackAction(enemyToAttack);

        if (enemyToAttack != null)
        {
            StartCoroutine(enemyToAttack.CounterAttack(this));
        }
    }

    public IEnumerator CounterAttack(Character enemyToAttack)
    {
        yield return new WaitForSeconds(0.5f);
        if (CanAttack(enemyToAttack))
        {
            AttackAction(enemyToAttack);
        }
    }

    private void AttackAction(Character enemyToAttack)
    {
        RotateDirectly(enemyToAttack.transform.position);
        AttackAlgorithm a = GetComponent<AttackAlgorithm>();
        //a.GetDamage(enemyToAttack);
        a.DoDamage(enemyToAttack);
    }

    public bool CanAttack(Character target)
    {
        float distance = Vector3.Distance(target.transform.position, transform.position);

        if (distance <= GetComponent<PokemonStats>().AttackRange)
        {
            return true;
        }
        return false;
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

    #endregion Locomotion

    #region Tile information

    public bool HasEnemyNeighbours()
    {
        Dictionary<Tile, int> possibleAttackingTiles = _currentTile.GetTiles(0);

        foreach (Tile t in possibleAttackingTiles.Keys)
        {
            Character neighbourCharacter = t._character;
            if (neighbourCharacter != null && neighbourCharacter.ControllingPlayer != ControllingPlayer)
            {
                return true;
            }
        }
        return false;
    }

    public List<Tile> GetNeighbourTiles()
    {
        return new List<Tile>(_currentTile.GetTiles(0).Keys);
    }

    public void SetCurrentTile(Tile tile)
    {
        if (_currentTile != null)
        {
            _currentTile._character = null;
        }

        if (!tile.HasPlayer)
        {
            tile._character = this;
            _currentTile = tile;
        }
        
    }

    #endregion Tile information

    #region Activation

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
            _currentTile._character = null;

        if (OnDeath != null)
            OnDeath(this);

        Destroy(gameObject);
    }

    #endregion Activation

    #endregion Methods
}
