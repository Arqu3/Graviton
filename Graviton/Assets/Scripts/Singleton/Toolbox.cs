using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Toolbox singleton-class, use this to set and get any public variables needed,
/// note that no more than 1 instance of this class should be present at any time!
/// </summary>
public class Toolbox : Singleton<Toolbox>
{
    //Make sure constructor cannot be used, true singleton
    protected Toolbox(){}

    public float m_Gravity = 20;
    public int m_CurrentCheckpoint = 1;
    public bool m_IsPaused = false;

    void Awake()
    {
        m_Gravity = Physics.gravity.magnitude;
        DontDestroyOnLoad(this);
    }

    public void ResetGravity()
    {
        Physics.gravity = new Vector3(0, -1, 0) * m_Gravity;
    }

    static public T RegisterComponent<T>() where T : Component
    {
        return Instance.GetOrAddComponent<T>();
    }
}
