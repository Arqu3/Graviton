using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControllerCheckpoint : MonoBehaviour
{
    public List<Checkpoint> m_Checkpoints = new List<Checkpoint>();

	void Start()
    {
        var checkpoints = FindObjectsOfType<Checkpoint>();

        if (checkpoints.Length > 0)
        {
            for (int i = 0; i < checkpoints.Length; i++)
            {
                m_Checkpoints.Add(checkpoints[i]);
            }

            for (int write = 0; write < m_Checkpoints.Count; write++)
            {
                for (int sort = 0; sort  < m_Checkpoints.Count - 1; sort++)
                {
                    if (m_Checkpoints[sort].m_ID > m_Checkpoints[sort + 1].m_ID)
                    {
                        Checkpoint temp = m_Checkpoints[sort];
                        m_Checkpoints[sort] = m_Checkpoints[sort + 1];
                        m_Checkpoints[sort + 1] = temp;
                    }
                }
            }
        }
        else
            Debug.Log("Couldn't find any checkpoints!");
	}

    public Transform GetCurrentTransform()
    {
        return m_Checkpoints[Toolbox.Instance.m_CurrentCheckpoint - 1].transform;
    }
    public Vector3 GetCurrentSpawnDirection()
    {
        return m_Checkpoints[Toolbox.Instance.m_CurrentCheckpoint - 1].m_SpawnDirection.normalized;
    }
}
