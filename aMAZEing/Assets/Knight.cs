using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Knight : MonoBehaviour {

    private Animator animation_controller;
    private CharacterController character_controller;
    public Vector3 movement_direction;
    public float walking_velocity;
    public float velocity;
    private Level level;
    private Vector3 lastValidPos;
    private Bounds bounds; 

	// Use this for initialization
	void Start ()
    {
        animation_controller = GetComponent<Animator>();
        character_controller = GetComponent<CharacterController>();
        movement_direction = new Vector3(0.0f, 0.0f, 0.0f);
        walking_velocity = 2.5f;
        velocity = 0.0f;
        character_controller.Move(movement_direction);
        GameObject level_obj = GameObject.FindGameObjectWithTag("Level");
        level = level_obj.GetComponent<Level>();
        bounds = level.bounds;
    }

    // Update is called once per frame
    void Update()
    {
        if(bounds.min[0] <= transform.position.x && transform.position.x <= bounds.max[0] && bounds.min[2] <= transform.position.z && bounds.max[2] >= transform.position.z) {
            lastValidPos = transform.position;
        }
        else{
            transform.position = new Vector3(lastValidPos.x, 0.0f, lastValidPos.z);
        }
        transform.position = new Vector3(transform.position.x, 0.0f, transform.position.z);
        //Walking Forward
        if(Input.GetKey(KeyCode.UpArrow)){
            animation_controller.SetBool("isWalkingForward", true);
        }
        else {
            animation_controller.SetBool("isWalkingForward", false);
        }

        //Walking Backwards
        if(Input.GetKey(KeyCode.DownArrow)){
            animation_controller.SetBool("isWalkingBackwards", true);
        }
        else {
            animation_controller.SetBool("isWalkingBackwards", false);
        }

        //Crouching Forward
        if(Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.RightControl)){
            animation_controller.SetBool("isWalkingForward", false);
            animation_controller.SetBool("isCrouchingForward", true);
        }
        else {
            animation_controller.SetBool("isCrouchingForward", false);
        }

        //Crouching Backward 
        if(Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.DownArrow) && Input.GetKey(KeyCode.RightControl)){
            animation_controller.SetBool("isWalkingBackwards", false);
            animation_controller.SetBool("isCrouchingBackwards", true);
        }
        else {
            animation_controller.SetBool("isCrouchingBackwards", false);
        }

        //Running Forward
        if(Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.UpArrow) && Input.GetKey(KeyCode.RightShift)){
            animation_controller.SetBool("isWalkingForward", false);
            animation_controller.SetBool("isRunningForward", true);
        }
        else {
            animation_controller.SetBool("isRunningForward", false);
        }

        //Rotation
        if(Input.GetKey(KeyCode.RightArrow)){
            transform.Rotate(new Vector3(0.0f, 0.5f, 0.0f));
        }
        if(Input.GetKey(KeyCode.LeftArrow)){
            transform.Rotate(new Vector3(0.0f, -0.5f, 0.0f));
        }
        
        // //Velocities
        if(animation_controller.GetBool("isWalkingForward") == true){
            velocity += 0.1f;
            if (velocity > walking_velocity){
                velocity = walking_velocity;
            }
        }
        else if(animation_controller.GetBool("isWalkingBackwards") == true){
            velocity += -0.1f;
            if (velocity < -walking_velocity/1.5f){
                velocity = -walking_velocity/1.5f;
            }
        }
        else if(animation_controller.GetBool("isCrouchingForward") == true){
            velocity += 0.1f;
            if (velocity > walking_velocity/2.0f){
                velocity = walking_velocity/2.0f;
            }
        }
        else if(animation_controller.GetBool("isCrouchingBackwards") == true){
            velocity += -0.1f;
            if (velocity < -walking_velocity/2.0f){
                velocity = -walking_velocity/2.0f;
            }
        }
        else if(animation_controller.GetBool("isRunningForward") == true){
            velocity += 0.1f;
            if (velocity > walking_velocity*2.0f){
                velocity = walking_velocity*2.0f;
            }
        }
        else {
            velocity = 0.0f;
        }

        // you don't need to change the code below (yet, it's better if you understand it). Name your FSM states according to the names below (or change both).
        // do not delete this. It's useful to shift the capsule (used for collision detection) downwards. 
        // The capsule is also used from turrets to observe, aim and shoot (see Turret.cs)
        // If the character is crouching, then she evades detection. 
        bool is_crouching = false;
        if ( (animation_controller.GetCurrentAnimatorStateInfo(0).IsName("Crouch Forward"))
         ||  (animation_controller.GetCurrentAnimatorStateInfo(0).IsName("Crouch Backward")) )
        {
            is_crouching = true;
        }

        if (is_crouching)
        {
            GetComponent<CapsuleCollider>().center = new Vector3(GetComponent<CapsuleCollider>().center.x, 0.0f, GetComponent<CapsuleCollider>().center.z);
        }
        else
        {
            GetComponent<CapsuleCollider>().center = new Vector3(GetComponent<CapsuleCollider>().center.x, 0.9f, GetComponent<CapsuleCollider>().center.z);
        }

        float xdirection = Mathf.Sin(Mathf.Deg2Rad * transform.rotation.eulerAngles.y);
        float zdirection = Mathf.Cos(Mathf.Deg2Rad * transform.rotation.eulerAngles.y);
        movement_direction = new Vector3(xdirection, 0.0f, zdirection);
        character_controller.Move(movement_direction * velocity * Time.deltaTime);
    }            
}