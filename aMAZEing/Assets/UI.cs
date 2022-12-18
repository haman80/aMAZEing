using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UI : MonoBehaviour
{
    public GameObject HomePage;
    public GameObject ControlsPage;
        
    public void Menu() {
        SceneManager.LoadScene("Menu");
    }
    
    public void Normal() {
        SceneManager.LoadScene("Normal");
    }

    public void Easy() {
        SceneManager.LoadScene("Easy");
    }

    public void Hard() {
        SceneManager.LoadScene("Hard");
    }

    public void Controls() {
        HomePage.SetActive(false);
        ControlsPage.SetActive(true);
    }

    public void Return() {
        HomePage.SetActive(true);
        ControlsPage.SetActive(false);
    }

    public void Quit() {
        #if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        #else
        Application.Quit();
        #endif
    }
}
