using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Mutant : MonoBehaviour
{
    private Animator animation_controller;
    private CharacterController character_controller;
    public Vector3 movement_direction;
    public float walking_velocity;
    private GameObject fps_player_obj;
    private Level level;
    private float radius_of_search_for_player;
    private float mutant_speed;
    public float velocity;
    private float mutant_health;
    private bool hit_recently=false;
    private bool flag = true;
    private float hit_time;
    private AudioSource src;
    RaycastHit hit;

    // Start is called before the first frame update
    void Start()
    {
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        fps_player_obj = level.fps_player_obj;
        Bounds bounds = level.GetComponent<Collider>().bounds;
        radius_of_search_for_player = (bounds.size.x + bounds.size.z) / 15.0f;
        mutant_speed = level.mutant_speed;
        animation_controller = GetComponent<Animator>();
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
        src = level.source;
    }

    // Update is called once per frame
    void Update()
    {
        if (level.player_health < 0.001f || level.player_entered_house && level.num_tokens == level.num_tokens_collected)
            return;
        Vector3 correction = new Vector3(0.0f, 0.0f, 0.0f);
        Ray ray1 = new Ray(transform.position+0.5f*Vector3.Cross(transform.forward, Vector3.up).normalized, transform.forward);
        Ray ray2 = new Ray(transform.position-0.5f*Vector3.Cross(transform.forward, Vector3.up).normalized, transform.forward);
        if (Physics.Raycast(ray1, out hit,1.0f)){
                if (hit.transform.gameObject.name=="WALL"){
                    correction= hit.normal;
                }
        }
        if (Physics.Raycast(ray2, out hit,1.0f)){
                if (hit.transform.gameObject.name=="WALL"){
                    correction= hit.normal;
                }
        }

        if (hit_recently && flag){
            hit_time = Time.time;
            flag = false;
        }
        if (!flag && Time.time-hit_time>0.25f){
            Destroy(gameObject);
        }

        Vector3 v = (fps_player_obj.transform.position - transform.position);
        Vector3 d = v/v.magnitude;
        d = new Vector3(d.x, 0f, d.z);
        d = d/d.magnitude;
        if (correction.x==0.0f && correction.y==0.0f && correction.z==0.0f){
            d =d;
            d = d/d.magnitude;
        }
        else{
            d = 5.0f*correction;
        }
        
        transform.forward = d;
        if(2.5f< v.magnitude && v.magnitude < radius_of_search_for_player){
            animation_controller.SetBool("isWalking", true);
            animation_controller.SetBool("isIdle", false);
            transform.position += d * (mutant_speed * Time.deltaTime);
        }
        if (v.magnitude<=2.5f){
            animation_controller.SetBool("isFighting", true);
            transform.position += d * (mutant_speed * Time.deltaTime);
        }

        if(v.magnitude>2.5f){
            animation_controller.SetBool("isFighting", false);
        }
        if(v.magnitude > radius_of_search_for_player){
            animation_controller.SetBool("isWalking", false);
            animation_controller.SetBool("isIdle", true);
        }
        transform.position = new Vector3(transform.position.x, 0f, transform.position.z);
        
    }

    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.name == "PLAYER" && !hit_recently)
        {
            level.mutant_hit_player = true;
            hit_recently = true;
            src.PlayOneShot(level.infected);
            animation_controller.SetBool("isFighting", true);
        }
    }
}
