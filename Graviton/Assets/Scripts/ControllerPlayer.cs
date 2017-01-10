using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider), typeof(ControllerCheckpoint))]
public class ControllerPlayer : MonoBehaviour
{
    //Public vars
    public GameObject m_HookPrefab;
    public float m_MovementSpeed = 20.0f;
    public float m_JumpForce = 10.0f;
    public float m_HookCooldown = 3.0f;
    public float m_TravelToHookSpeed = 30.0f;
    public LayerMask m_RayMask;

    //Component vars
    private Collider m_Collider;
    private Rigidbody m_Rigidbody;
    private ControllerCheckpoint m_Checkpoints;

    //Jump vars
    private bool m_IsOnGround = true;
    private Vector3 m_JumpVector = Vector3.zero;
    private bool m_IsAirMovement = false;

    //Rotation vars
    private RaycastHit m_RotationHit;
    private Transform m_GravityTransform;
    private ControllerCamera m_Camera;

    //Hook vars
    private float m_HookTimer = 0.0f;
    private Hook m_Hook;
    private GameObject m_HookTransform;
    private bool m_IsHookCD = false;
    private bool m_IsGrapple = false;
    private RaycastHit m_HookHit;
    private GameObject m_Reticle;
    private int m_HookMode = 0;
    private Vector3 m_GrappleDir = Vector3.zero;

	void Start()
    {
        m_HookTimer = m_HookCooldown;

        m_Rigidbody = GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();
        m_Checkpoints = GetComponent<ControllerCheckpoint>();

        m_GravityTransform = transform.parent;

        m_Camera = FindObjectOfType<ControllerCamera>();

        if (m_Camera)
            m_HookTransform = m_Camera.transform.FindChild("Hook").gameObject;
        else
            Debug.Log("Player couldn't find camera!");

        m_Reticle = GameObject.Find("Reticle");
	}
	
	void Update()
    {
        if (!Toolbox.Instance.m_IsPaused)
        {
            if (transform.position.y < -15f)
                transform.position = new Vector3(0, 5, 0);

            MovementUpdate();

            JumpUpdate();

            GravityUpdate();

            HookUpdate();
        }
	}

    void MovementUpdate()
    {
        if (m_Rigidbody.velocity.magnitude < 1.0f && m_IsOnGround)
            m_Rigidbody.velocity = Vector3.zero;

        //Vector3 fwd = Camera.main.transform.forward;
        //fwd.y = 0.0f;
        //Quaternion rot = Quaternion.LookRotation(fwd);

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
        if (CheckGround())
        {
            m_IsOnGround = true;
            m_Rigidbody.useGravity = false;
        }
        else
        {
            m_IsOnGround = false;
            m_Rigidbody.useGravity = true;
        }

        if (m_IsOnGround)
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

    bool CheckGround()
    {
        bool state = false;
        float distance = 1.1f;
        float offset = 0.3f;
        Vector3 pos = Vector3.zero;

        for (int i = 0; i < 5; i++)
        {
            switch (i)
            {
                case 0:
                    pos = m_Collider.bounds.center;
                    break;

                case 1:
                    pos += transform.right * offset;
                    break;

                case 2:
                    pos = m_Collider.bounds.center - transform.right * offset;
                    break;

                case 3:
                    pos = m_Collider.bounds.center + transform.forward * offset;
                    break;

                case 4:
                    pos = m_Collider.bounds.center - transform.forward * offset;
                    break;
            }
            state = Raycast(pos, -transform.up, distance);
            if (state)
                break;
        }

        return state;
    }

    void GravityUpdate()
    {
        if (RaycastInfo(m_Camera.transform.position, m_Camera.transform.forward, out m_RotationHit, 1.5f))
        {
            if (Physics.gravity != -m_RotationHit.normal * Toolbox.Instance.m_Gravity)
            {
                UpdateGravity(m_RotationHit.normal);
            }
        }
        if (RaycastInfo(transform.position, m_Rigidbody.velocity.normalized, out m_RotationHit, 1.5f))
        {
            if (Physics.gravity != -m_RotationHit.normal * Toolbox.Instance.m_Gravity)
            {
                UpdateGravity(m_RotationHit.normal);
            }
        }
    }

    void UpdateGravity(Vector3 dir)
    {
        Vector3 pos = transform.position;
        Vector3 fwd = m_Camera.transform.forward;
        m_GravityTransform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        transform.position = pos;
        m_Rigidbody.velocity = transform.up.normalized * 5;
        Physics.gravity = -dir * Toolbox.Instance.m_Gravity;
        m_Camera.transform.forward = fwd;
        m_Camera.SetAbsoluteY(m_Camera.transform.localEulerAngles.x);
    }

    void HookUpdate()
    {
        if (m_HookTransform && m_HookPrefab)
        {
            if (RaycastInfo(m_Camera.transform.position, m_Camera.transform.forward, out m_HookHit, 20))
            {
                m_Reticle.SetActive(true);
                m_HookTransform.transform.LookAt(m_HookHit.point);
            }
            else
            {
                m_Reticle.SetActive(false);
                m_HookTransform.transform.localEulerAngles = Vector3.zero;
            }

            if (!m_IsGrapple)
            {
                if (!m_IsHookCD)
                {
                    if (Input.GetMouseButtonDown(0))
                    {
                        SpawnHook();
                        m_HookMode = 0;
                    }
                    else if (Input.GetMouseButtonDown(1))
                    {
                        SpawnHook();
                        m_HookMode = 1;
                    }
                }

                if (m_IsHookCD)
                {
                    if (m_HookTimer > 0)
                        m_HookTimer -= Time.deltaTime;
                    else
                    {
                        m_HookTimer = m_HookCooldown;
                        m_IsHookCD = false;
                    }
                }
            }
            else
            {
                m_Rigidbody.velocity = m_GrappleDir * m_TravelToHookSpeed;
            }
        }
    }

    void SpawnHook()
    {
        m_IsHookCD = true;
        GameObject clone = (GameObject)Instantiate(m_HookPrefab, m_HookTransform.transform.position, m_HookTransform.transform.rotation);
        if (clone.GetComponent<Hook>())
            m_Hook = clone.GetComponent<Hook>();

        if (m_Hook)
        {
            m_Hook.SetPlayer(this);
            if (m_HookHit.collider)
                m_Hook.SetHitPosition(m_HookHit.point);
        }
    }

    public void InterruptGrapple()
    {
        m_IsHookCD = true;
        m_IsGrapple = false;
        m_Rigidbody.velocity = Vector3.zero;
        if (m_Hook)
            Destroy(m_Hook);
    }

    public void SetGrapple(Vector3 dir)
    {
        m_IsGrapple = true;
        m_GrappleDir = dir.normalized;
    }

    public void ResetPosition()
    {
        transform.position = m_Checkpoints.GetCurrentTransform().position;
        UpdateGravity(m_Checkpoints.GetCurrentSpawnDirection());
        m_Rigidbody.velocity = Vector3.zero;
        InterruptGrapple();
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag != "Hook")
        {
            if (m_IsGrapple)
            {
                InterruptGrapple();
                if (m_Hook)
                    Destroy(m_Hook.gameObject);
            }
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
        hit = Physics.Raycast(position, direction, out rayhit, distance, m_RayMask);
        if (hit)
            hitCol = Color.red;

        Debug.DrawRay(position, direction * distance, hitCol);

        return hit;
    }

    bool RaycastInfo(Vector3 position, Vector3 direction, out RaycastHit rayhit, float distance)
    {
        bool hit = false;
        Color hitCol = Color.green;

        hit = Physics.Raycast(position, direction, out rayhit, distance, m_RayMask);
        if (hit)
            hitCol = Color.red;

        Debug.DrawRay(position, direction * distance, hitCol);

        return hit;
    }
}
