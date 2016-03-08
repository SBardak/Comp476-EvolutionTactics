using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    private Animator animator;

    public float targetRadius;
    public float slowRadius;
    public float timeToTarget;
    public float maxAcceleration;
    public float maxVelocity;

    public float turnSpeed;
    public float kinematicSpeed;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
    }

    void Update()
    {
        animator.SetFloat("Walk", rb.velocity.magnitude);
    }

    public void KinematicMovement(Vector3 target)
    {
        MoveTowards(target);
        Rotate(target);
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
        if (targetVel.magnitude < targetRadius)
        {
            rb.velocity = Vector2.zero;
            return Vector2.zero;
        }

        targetVel.Normalize();

        return maxAcceleration * targetVel;
    }

    //Look in the diestion you are going
    private void LookWhereYoureGoing()
    {
        Vector3 direction = rb.velocity;
        direction.Normalize();
        // Align
        transform.rotation = Align(direction);
    }

    // Steer given an acceleration
    private void Steer(Vector3 linearAcceleration)
    {
        rb.velocity += linearAcceleration * Time.deltaTime;

        Debug.Log(rb.velocity.magnitude);
        if (rb.velocity.magnitude > maxVelocity)
        {
            rb.velocity.Normalize();
            rb.velocity *= maxVelocity;
        }
    }

    //Align to a target
    private Quaternion Align(Vector3 target)
    {
        float toRotation = (Mathf.Atan2(target.x, target.z) * Mathf.Rad2Deg);
        // rotate along y axis
        float rotation = Mathf.LerpAngle(transform.rotation.eulerAngles.y, toRotation, Time.deltaTime * turnSpeed * 2);

        return Quaternion.Euler(0, rotation, 0);
    }

    //Move towards a point
    private void MoveTowards(Vector3 location)
    {
        transform.position = Vector3.MoveTowards(transform.position, location, kinematicSpeed * Time.deltaTime);
    }

    public bool Rotate(Vector3 location)
    {
        Quaternion prevRotation = transform.rotation;

        transform.rotation = Quaternion.RotateTowards(transform.rotation, 
            Quaternion.LookRotation(location - transform.localPosition),
            turnSpeed * Time.deltaTime);

        if (transform.rotation == prevRotation)
        {
            return true;
        }
        return false;
    }
}
