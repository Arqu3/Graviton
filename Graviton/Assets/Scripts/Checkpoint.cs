using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(Collider))]
public class Checkpoint : MonoBehaviour
{
    public int m_ID = 0;
    public Vector3 m_SpawnDirection = Vector3.zero;

#if UNITY_EDITOR
    void OnEnable()
    {
        if (Application.isPlaying)
            return;

        if (m_ID == 0)
        {
            var checkpoints = FindObjectsOfType<Checkpoint>();
            m_ID = checkpoints.Length;
        }
    }

    void Update()
    {
        if (Application.isPlaying)
            return;

        Debug.DrawRay(transform.position, m_SpawnDirection * 3, Color.blue);
    }
#endif

    void OnTriggerEnter(Collider col)
    {
        if (col.gameObject.tag == "Player")
        {
            Toolbox.Instance.m_CurrentCheckpoint = m_ID;
            Debug.Log("Set current checkpoint to " + m_ID);
        }
    }
}
