using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenu : MonoBehaviour
{
    public bool pauseGame = false;

    public GameObject PMenu;

    // Update is called once per frame
    void Update()
    {
      if(Input.GetKeyDown(KeyCode.Escape)) {
        if(pauseGame)
            Resume();
        else
            Pause();
      } 
    }

    public void Resume() {
        PMenu.SetActive(false);
        Time.timeScale = 1f;
        pauseGame = false;
    }

    public void Menu() {
        pauseGame = false;
        SceneManager.LoadScene("Menu");
    }

    public void Quit() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }

    void Pause() {
        PMenu.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Time.timeScale = 0f;
        pauseGame = true;
    }
}
