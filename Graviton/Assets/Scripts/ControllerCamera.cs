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
        if (!Toolbox.Instance.m_IsPaused)
        {
            RotationUpdate();
        }
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

        transform.localRotation = Quaternion.Euler(m_AbsoluteY, Mathf.Lerp(transform.localEulerAngles.y, 0, 100 * Time.deltaTime), Mathf.Lerp(transform.localEulerAngles.z, 0, 100 * Time.deltaTime));
        m_Player.transform.localRotation = Quaternion.Euler(0, m_AbsoluteX, 0);
    }

    public float GetAbsoluteY()
    {
        return m_AbsoluteY;
    }

    public void SetAbsoluteY(float value)
    {
        m_AbsoluteY = value;
    }

    public float GetAbsoluteX()
    {
        return m_AbsoluteX;
    }
}
