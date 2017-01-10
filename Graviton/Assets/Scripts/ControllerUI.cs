using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ControllerUI : MonoBehaviour
{

    private GameObject m_PausePanel;

	void Start ()
    {
        m_PausePanel = transform.FindChild("PausePanel").gameObject;
        if (m_PausePanel)
            m_PausePanel.SetActive(false);
	}

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
            TogglePaused();
    }

    public void TogglePaused()
    {
        Toolbox.Instance.m_IsPaused = !Toolbox.Instance.m_IsPaused;
        m_PausePanel.SetActive(Toolbox.Instance.m_IsPaused);

        if (Toolbox.Instance.m_IsPaused)
        {
            Time.timeScale = 0.0f;
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }
        else
        {
            Time.timeScale = 1.0f;
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void Restart()
    {
        TogglePaused();
        SceneManager.LoadScene(0);
    }
}
