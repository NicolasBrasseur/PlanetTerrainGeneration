using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    //Public
    [HideInInspector] public static bool IsPaused = false;
    public GameObject PauseMenuCanvas;

    private void Start()
    {
        IsPaused = false;
        PauseMenuCanvas.SetActive(false);
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if(Input.GetButtonDown("Quit"))
        {
            ChangePauseState();
        }
    }

    private void ChangePauseState()
    {
        IsPaused = !IsPaused;

        if (PauseMenuCanvas == null) { return; }
        PauseMenuCanvas.SetActive(IsPaused);

        Cursor.visible = IsPaused;
        Cursor.lockState = IsPaused ? CursorLockMode.None : CursorLockMode.Locked;
    }

    public void ResumeGame()
    {
        ChangePauseState();
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
        Application.Quit();
    }
}
