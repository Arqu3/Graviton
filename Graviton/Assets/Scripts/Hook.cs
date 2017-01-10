using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Hook : MonoBehaviour
{
    //Public vars
    public float m_TravelSpeed = 50.0f;
    public float m_Distance = 20.0f;
    public float m_BreakDistance = 5.0f;

    //Component vars
    private Rigidbody m_Rigidbody;
    private ControllerPlayer m_Player;

    //Hit vars
    private bool m_HasHit = false;
    private Vector3 m_HitPosition = Vector3.zero;
    private bool m_HasHitPosition = false;
    private bool m_HasUpdatedPlayer = false;

    void Start()
    {
        m_Rigidbody = GetComponent<Rigidbody>();

        m_Rigidbody.velocity = transform.forward * m_TravelSpeed;
	}
	
	void Update()
    {
        DistanceUpdate();
	}

    void DistanceUpdate()
    {
        if (m_Player)
        {
            if (Vector3.Distance(transform.position, m_Player.transform.position) > m_Distance && !m_HasHit)
            {
                Destroy(gameObject);
            }

            if (Vector3.Distance(transform.position, m_HitPosition) < 0.5f && m_HasHitPosition)
            {
                m_HasHit = true;
                m_Rigidbody.velocity = Vector3.zero;
            }

            if (m_HasHit)
            {
                if (!m_HasUpdatedPlayer)
                {
                    m_Player.SetGrapple(m_HitPosition - m_Player.transform.position);
                    m_HasUpdatedPlayer = true;
                }

                if (Vector3.Distance(m_HitPosition, m_Player.transform.position) < m_BreakDistance)
                {
                    m_Player.InterruptGrapple();
                    Destroy(gameObject);
                }
            }
        }
    }

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag != "Player")
        {
            m_HasHit = true;
            m_Rigidbody.velocity = Vector3.zero;
            m_HitPosition = col.contacts[0].point;
        }
    }

    public void SetPlayer(ControllerPlayer player)
    {
        m_Player = player;
    }

    public void SetHitPosition(Vector3 pos)
    {
        m_HitPosition = pos;
        m_HasHitPosition = true;
    }
}
