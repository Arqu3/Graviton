using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class ControllerPlayer : MonoBehaviour
{
    //Public vars
    public float m_MovementSpeed = 20.0f;
    public float m_JumpForce = 10.0f;
    private bool m_IsAirMovement = false;

    //Component vars
    private Collider m_Collider;
    private Rigidbody m_Rigidbody;

    //Jump vars
    private bool m_IsGoingUp = false;
    private bool m_IsOnGround = true;
    private Vector3 m_JumpVector = Vector3.zero;

    //Rotation vars
    private RaycastHit m_RotationHit;
    private Transform m_GravityTransform;


	void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();

        m_GravityTransform = transform.parent;
	}
	
	void Update()
    {
        if (transform.position.y < -15f)
            transform.position = new Vector3(0, 5, 0);

        MovementUpdate();

        JumpUpdate();

        GravityUpdate();
	}

    void MovementUpdate()
    {
        if (m_Rigidbody.velocity.magnitude < 1.0f && m_IsOnGround)
            m_Rigidbody.velocity = Vector3.zero;

        Vector3 fwd = Camera.main.transform.forward;
        fwd.y = 0.0f;
        Quaternion rot = Quaternion.LookRotation(fwd);

        if (m_IsAirMovement)
        {
            m_Rigidbody.velocity = (transform.right * Input.GetAxis("Horizontal") * m_MovementSpeed) + (transform.forward * Input.GetAxis("Vertical") * m_MovementSpeed) + m_JumpVector;
            if (m_IsOnGround)
                m_JumpVector = Vector3.zero;
        }
        else
        {
            if (m_IsOnGround)
                m_Rigidbody.velocity = (transform.right * Input.GetAxis("Horizontal") * m_MovementSpeed) + (transform.forward * Input.GetAxis("Vertical") * m_MovementSpeed) + m_JumpVector;
        }
    }

    void JumpUpdate()
    {
        if (Raycast(m_Collider.bounds.center, -transform.up, 1.1f))
        {
            m_IsOnGround = true;
            m_Rigidbody.useGravity = false;
        }
        else
        {
            m_IsOnGround = false;
            m_Rigidbody.useGravity = true;
        }

        m_IsGoingUp = Mathf.Round(m_Rigidbody.velocity.y) > 0;

        if (m_IsOnGround && !m_IsGoingUp)
        {
            if (Input.GetKey(KeyCode.Space))
            {
                //m_Rigidbody.velocity = new Vector3(m_Rigidbody.velocity.x, m_JumpForce, m_Rigidbody.velocity.z);
                m_JumpVector = transform.up.normalized * m_JumpForce;
            }
            else
                m_JumpVector = Vector3.zero;
        }
    }

    void GravityUpdate()
    {
        if (RaycastInfo(m_Collider.bounds.center, transform.forward, out m_RotationHit, 1.5f))
        {
            Vector3 pos = transform.position;
            m_GravityTransform.rotation = Quaternion.FromToRotation(Vector3.up, m_RotationHit.normal);
            transform.position = pos;
            m_Rigidbody.velocity = transform.up.normalized * 5;
            Physics.gravity = -m_RotationHit.normal * Toolbox.Instance.m_Gravity;
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

    bool RaycastInfo(Vector3 position, Vector3 direction, out RaycastHit rayhit, float distance)
    {
        bool hit = false;
        Color hitCol = Color.green;

        hit = Physics.Raycast(position, direction, out rayhit, distance);
        if (hit)
            hitCol = Color.red;

        Debug.DrawRay(position, direction * distance, hitCol);

        return hit;
    }
}
