using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class UI : MonoBehaviour
{
    public void Active() {
        gameObject.SetActive(true);
    }
        
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
}
