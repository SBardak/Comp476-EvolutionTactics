using UnityEngine;
using System.Collections;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    public float targetRadius;
    public float slowRadius;
    public float timeToTarget;
    public float maxAcceleration;
    public float turnSpeed;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
	
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

        Vector3 acceleration = (targetVel - new Vector3(rb.velocity.x, rb.velocity.y, rb.velocity.z)) * 1 / timeToTarget;
        if (acceleration.magnitude > maxAcceleration)
        {
            acceleration.Normalize();
            acceleration *= maxAcceleration;
        }

        return acceleration;
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
    }

    //Align to a target
    private Quaternion Align(Vector3 target)
    {
        float toRotation = (Mathf.Atan2(target.x, target.z) * Mathf.Rad2Deg);
        // rotate along y axis
        float rotation = Mathf.LerpAngle(transform.rotation.eulerAngles.y, toRotation, Time.deltaTime * turnSpeed * 2);

        return Quaternion.Euler(0, rotation, 0);
    }
}
