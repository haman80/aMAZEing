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
    
    public void Play() {
        SceneManager.LoadScene("level");
    }
}
