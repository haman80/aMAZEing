using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyKnight : MonoBehaviour
{
    private GameObject fps_player_obj;
    private Level level;
    private float radius_of_search_for_player;
    private float enemy_speed;
    private float storey_height;
    // Start is called before the first frame update
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
        Bounds bounds = level.GetComponent<Collider>().bounds;
        radius_of_search_for_player = (bounds.size.x + bounds.size.z) / 10.0f;
        enemy_speed = level.enemy_speed;
        storey_height = level.storey_height;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (fps_player_obj==null){
            fps_player_obj = level.fps_player_obj;
        }
        if (level.player_health < 0.001f || level.player_entered_house)
            return;
        /*** implement the rest ! */
        Vector3 v = fps_player_obj.transform.position - transform.position;
        Vector3 dir = v/Vector3.Magnitude(v);
        if (Vector3.Magnitude(v)<radius_of_search_for_player){
            transform.position = transform.position + dir*enemy_speed * Time.deltaTime;
        }
        if (transform.position.y>storey_height){
            transform.position = new Vector3(transform.position.x, transform.position.y - 0.1f, transform.position.z) ;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {  
        if (collision.gameObject.name == "PLAYER")
        {
            if (!level.virus_landed_on_player_recently)
                level.timestamp_virus_landed = Time.time;
            level.num_virus_hit_concurrently++;
            level.virus_landed_on_player_recently = true;
            Destroy(gameObject);
        }
    }
}
