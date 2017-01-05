using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera), typeof(Transform))]
public class ControllerCamera : MonoBehaviour
{
    //Public vars
    public bool m_InvertY = true;
    public Vector2 m_Sensitivity = new Vector2(3, 1);
    public float m_YClamp = 70.0f;

    //Component vars
    ControllerPlayer m_Player;

    //Rotation var
    private int m_InvertValue = 1;
    private Vector2 m_Inputs = new Vector2(0, 0);
    private float m_AbsoluteX = 0.0f;
    private float m_AbsoluteY = 0.0f;

	void Start ()
    {
        m_Player = FindObjectOfType<ControllerPlayer>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
	
	void Update ()
    {
        RotationUpdate();
	}

    void RotationUpdate()
    {
        m_InvertValue = Mathf.Clamp(m_InvertValue, -1, 1);
        if (m_InvertY)
            m_InvertValue = -1;
        else
            m_InvertValue = 1;

        m_Inputs = new Vector2(Input.GetAxis("Mouse X") * m_Sensitivity.x, Input.GetAxis("Mouse Y") * m_Sensitivity.y * m_InvertValue);

        m_AbsoluteX += m_Inputs.x;
        if (m_AbsoluteX >= 360)
            m_AbsoluteX -= 360;
        else if (m_AbsoluteX <= -360)
            m_AbsoluteX += 360;

        m_AbsoluteY += m_Inputs.y;
        m_AbsoluteY = Mathf.Clamp(m_AbsoluteY, -m_YClamp, m_YClamp);

        transform.rotation = Quaternion.Euler(m_AbsoluteY, m_Player.transform.rotation.eulerAngles.y, m_Player.transform.rotation.eulerAngles.z);
        //m_Player.transform.rotation = Quaternion.Euler(m_Player.transform.rotation.eulerAngles.x, m_AbsoluteX, m_Player.transform.rotation.eulerAngles.z);
        m_Player.transform.rotation = Quaternion.Euler(0, m_AbsoluteX, 0);
    }
}
