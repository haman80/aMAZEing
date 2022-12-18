using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// functionality of the house at the exit of the maze
// it simply has a trigger when player arrived in the house
public class House : MonoBehaviour
{
    private GameObject fps_player_obj;
    private Level level;
    private AudioSource src;

    // Use this for initialization
    void Start()
    {
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        if (level == null)
        {
            Debug.LogError("Internal error: could not find the Level object - did you remove its 'Level' tag?");
            return;
        }
        fps_player_obj = level.fps_player_obj;
        src = level.source;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.name == "PLAYER")
        {
            level.player_entered_house = true;
            if(level.num_tokens == level.num_tokens_collected)
                src.PlayOneShot(level.exit);
        }
    }
}