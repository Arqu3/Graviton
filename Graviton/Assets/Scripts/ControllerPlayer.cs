using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ControllerPlayer : MonoBehaviour
{
    //Public vars
    public float m_MovementSpeed = 20.0f;
    public float m_JumpForce = 10.0f;
    public bool m_IsAirMovement = false;

    //Component vars
    private Collider m_Collider;
    private Rigidbody m_Rigidbody;

    //Jump vars
    private bool m_IsGoingUp = false;
    private bool m_IsOnGround = true;

	void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();
	}
	
	void Update()
    {
        if (transform.position.y < -15f)
            transform.position = new Vector3(0, 5, 0);

        MovementUpdate();

        JumpUpdate();
	}

    void MovementUpdate()
    {
        if (m_Rigidbody.velocity.magnitude < 1.0f && m_IsOnGround)
            m_Rigidbody.velocity = Vector3.zero;

        if (Raycast(m_Collider.bounds.center, Vector3.down, 1.2f))
        {
            m_IsOnGround = true;
            m_Rigidbody.useGravity = false;
        }
        else
        {
            m_IsOnGround = false;
            m_Rigidbody.useGravity = true;
        }

        Vector3 fwd = Camera.main.transform.forward;
        fwd.y = 0.0f;
        Quaternion rot = Quaternion.LookRotation(fwd);

        if (m_IsAirMovement)
            m_Rigidbody.velocity = rot * new Vector3(Input.GetAxis("Horizontal") * m_MovementSpeed, m_Rigidbody.velocity.y, Input.GetAxis("Vertical") * m_MovementSpeed);
        else
        {
            if (m_IsOnGround)
                m_Rigidbody.velocity = rot * new Vector3(Input.GetAxis("Horizontal") * m_MovementSpeed, m_Rigidbody.velocity.y, Input.GetAxis("Vertical") * m_MovementSpeed);
        }
    }

    void JumpUpdate()
    {
        m_IsGoingUp = Mathf.Round(m_Rigidbody.velocity.y) > 0;

        if (m_IsOnGround && !m_IsGoingUp)
        {
            if (Input.GetKey(KeyCode.Space))
                m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpForce, m_Rigidbody.velocity.z);
        }
    }

    /// <summary>
    /// Raycasts from given position with given direction to distance, and also draws the ray.
    /// The rays base-color is green, if it hits something it changes to red.
    /// </summary>
    /// <param name="position"></param>
    /// <param name="direction"></param>
    /// <param name="distance"></param>
    /// <returns></returns>
    bool Raycast(Vector3 position, Vector3 direction, float distance)
    {
        bool hit = false;
        Color hitCol = Color.green;

        RaycastHit rayhit;
        hit = Physics.Raycast(position, direction, out rayhit, distance);
        if (hit)
            hitCol = Color.red;

        Debug.DrawRay(position, direction * distance, hitCol);

        return hit;
    }
}
