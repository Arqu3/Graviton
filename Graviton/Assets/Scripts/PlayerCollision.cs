using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class PlayerCollision : MonoBehaviour
{
    private ControllerPlayer m_Player;

	void Start ()
    {
        m_Player = FindObjectOfType<ControllerPlayer>();
	}

    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag != "Hook")
        {
            m_Player.InterruptGrapple();
        }
    }

    void OnCollisionStay(Collision col)
    {
        if (m_Player)
        {
            if (!m_Player.GetIsOnGround())
            {
                m_Player.InterruptGrapple();
            }
        }
    }
}
