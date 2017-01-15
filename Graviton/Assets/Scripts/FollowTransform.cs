using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowTransform : MonoBehaviour
{
    //Pulic vars
    public Transform m_FollowTransform;

    //Offset vars
    private Vector3 m_Offset = Vector3.zero;

	void Start()
    {
		if (!m_FollowTransform)
        {
            Debug.Log(gameObject.name + " needs a follow transform!");
            enabled = false;
            return;
        }

        m_Offset = transform.position - m_FollowTransform.position;
	}
	
	void Update()
    {
        transform.position = m_FollowTransform.position + m_Offset;
	}
}
