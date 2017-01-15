using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(/*typeof(Rigidbody),*/ typeof(Collider), typeof(ControllerCheckpoint))]
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
    private Quaternion m_ToRotation;
    private bool m_WillRotate = false;
    private bool m_SameObject = false;
    private bool m_CanCheck = true;

    //Hook vars
    private float m_HookTimer = 0.0f;
    private Hook m_Hook;
    private Transform m_HookTransform;
    private bool m_IsHookCD = false;
    private bool m_IsGrapple = false;
    private RaycastHit m_HookHit;
    private GameObject m_Reticle;
    private GameObject m_GravityReticle;
    private RectTransform m_HookChargebar;
    private int m_HookMode = 0;
    private Vector3 m_GrappleDir = Vector3.zero;

	void Start()
    {
        m_HookTimer = m_HookCooldown;

        m_Rigidbody = transform.parent.GetComponent<Rigidbody>();
        m_Collider = GetComponent<Collider>();
        m_Checkpoints = GetComponent<ControllerCheckpoint>();

        m_GravityTransform = transform.parent;

        m_Camera = FindObjectOfType<ControllerCamera>();

        if (m_Camera)
            m_HookTransform = m_Camera.transform.FindChild("Hook");
        else
            Debug.Log("Player couldn't find camera!");

        m_Reticle = GameObject.Find("Reticle");
        m_GravityReticle = GameObject.Find("GravityReticle");
        m_HookChargebar = GameObject.Find("Foreground").GetComponent<RectTransform>();
        
	}
	
	void Update()
    {
        if (!Toolbox.Instance.m_IsPaused)
        {
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
        if (GroundCheck())
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

    bool GroundCheck(float distance = 1.1f)
    {
        bool state = false;
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
            {
                m_CanCheck = true;
                break;
            }
        }

        return state;
    }

    void GravityUpdate()
    {
        if (Input.GetKey(KeyCode.LeftShift) || m_IsGrapple)
        {
            if (!m_GravityReticle.activeSelf)
                m_GravityReticle.SetActive(true);

            if (RaycastInfo(m_Camera.transform.position, m_Camera.transform.forward, out m_RotationHit, 1.5f)
                || RaycastInfo(transform.position, m_Rigidbody.velocity.normalized, out m_RotationHit, 1.5f))
            {
                if (Physics.gravity != -m_RotationHit.normal * Toolbox.Instance.m_Gravity)
                {
                    UpdateGravity(m_RotationHit.normal);
                }
            }

            if (!GroundCheck() && !m_IsGrapple && Quaternion.Angle(m_GravityTransform.rotation, m_ToRotation) < 1.0f && CheckGravity())
            {
                if (Physics.gravity != -m_RotationHit.normal * Toolbox.Instance.m_Gravity)
                {
                    UpdateGravity(m_RotationHit.normal);
                }
            }
        }
        else
        {
            if (m_GravityReticle.activeSelf)
                m_GravityReticle.SetActive(false);
        }
        if (m_WillRotate)
            m_GravityTransform.rotation = Quaternion.Lerp(m_GravityTransform.rotation, m_ToRotation, 10 * Time.deltaTime);
    }

    bool CheckGravity()
    {
        if (m_CanCheck)
        {
            bool state = false;
            float distance = 2.0f;
            Vector3 pos = transform.position - transform.up;
            Vector3 direction = Vector3.zero;

            for (int i = 0; i < 4; i++)
            {
                switch (i)
                {
                    case 0:
                        direction = transform.right;
                        break;

                    case 1:
                        direction *= -1;
                        break;

                    case 2:
                        direction = transform.forward;
                        break;

                    case 3:
                        direction *= -1;
                        break;
                }
                state = RaycastInfo(pos, direction, out m_RotationHit, distance);
                if (state)
                {
                    m_SameObject = true;
                    m_CanCheck = false;
                    break;
                }
            }
            return state;
        }
        else
            return false;
    }

    void UpdateGravity(Vector3 dir)
    {
        Vector3 point = m_Camera.transform.position + m_Camera.transform.forward * 2;
        //m_GravityTransform.rotation = Quaternion.FromToRotation(Vector3.up, dir);
        m_ToRotation = Quaternion.FromToRotation(Vector3.up, dir);
        transform.localPosition = Vector3.zero;
        //m_Rigidbody.velocity = transform.up.normalized * 5;
        Physics.gravity = -dir * Toolbox.Instance.m_Gravity;
        m_WillRotate = true;

        if (m_SameObject)
        {
            m_Rigidbody.velocity *= 0.1f;
            m_Rigidbody.AddForce(-transform.up * 25f, ForceMode.Impulse);
            m_SameObject = false;
        }
        //m_Camera.transform.LookAt(point);
        //m_Camera.SetAbsoluteY(m_Camera.transform.localEulerAngles.x);
    }

    void HookUpdate()
    {
        if (m_HookTransform && m_HookPrefab)
        {

            if (RaycastInfo(m_Camera.transform.position, m_Camera.transform.forward, out m_HookHit, 20))
            {
                m_Reticle.SetActive(true);
                m_HookTransform.LookAt(m_HookHit.point);
            }
            else
            {
                m_Reticle.SetActive(false);
                m_HookTransform.localEulerAngles = Vector3.zero;
            }

            if (m_IsHookCD)
            {
                m_HookChargebar.sizeDelta = new Vector2(100 - (100 / m_HookCooldown) * m_HookTimer, m_HookChargebar.sizeDelta.y);

                if (m_HookTimer > 0)
                    m_HookTimer -= Time.deltaTime;
                else
                {
                    m_HookTimer = m_HookCooldown;
                    m_IsHookCD = false;
                }
            }
            else
                m_HookChargebar.sizeDelta = new Vector2(100, m_HookChargebar.sizeDelta.y);

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
        GameObject clone = (GameObject)Instantiate(m_HookPrefab, m_HookTransform.position, m_HookTransform.rotation);
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
        if (m_IsGrapple)
        {
            m_IsHookCD = true;
            m_IsGrapple = false;
            m_Rigidbody.velocity = Vector3.zero;
            if (m_Hook)
                Destroy(m_Hook.gameObject);
        }
    }

    public void SetGrapple(Vector3 dir)
    {
        m_IsGrapple = true;
        m_GrappleDir = dir.normalized;
    }

    public void ResetPosition()
    {
        transform.parent.position = m_Checkpoints.GetCurrentTransform().position;
        UpdateGravity(m_Checkpoints.GetCurrentSpawnDirection());
        m_Rigidbody.velocity = Vector3.zero;
        InterruptGrapple();
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

    public bool GetIsOnGround()
    {
        return m_IsOnGround;
    }
}
