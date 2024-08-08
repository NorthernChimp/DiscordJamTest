using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainScript : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform playerTransform;
    public Player player;
    bool gamePaused = false;
    Vector3 offsetFromPlayer;
    public float camDistToPlayer = 4.20f;
    void Start()
    {
        offsetFromPlayer = playerTransform.position - transform.position ;
    }
    void UpdateCamera()
    {
        //Vector3 objectivePos = playerTransform.position - offsetFromPlayer;
        
        //transform.position = objectivePos;
        float yRot = player.cameraYAngle;//the camera angle is stored on the player because thats where the controls were enabled, retrieve them
        Vector3 offsetThisFrame = new Vector3(Mathf.Sin(yRot) * offsetFromPlayer.z,offsetFromPlayer.y,Mathf.Cos(yRot) * offsetFromPlayer.z);//this is based off the initial offset fromt eh camera and should be cahnged later
        Vector3 objectivePos = playerTransform.position - offsetThisFrame;//get the objective pos from the angle with the current offset from camera
        RaycastHit hit;
        Vector3 dirToCamFromPlayer = objectivePos - playerTransform.position;//get teh direction to the cam from the player
        if(Physics.Raycast(playerTransform.position, dirToCamFromPlayer,out hit,camDistToPlayer,LayerMask.GetMask("Ground")))//this is a raycast from the player to where the camera should be, in case the player is behind a wall or something
        {
            if(hit.distance < 0.75f){objectivePos = playerTransform.position + dirToCamFromPlayer.normalized  * 0.75f;}//if the distance is too close then you cant see the player anymore set it to a minimum distance
            else{objectivePos = hit.point;}
        }
        //transform.position = objectivePos;
        Vector3 dirToObjective = objectivePos - transform.position;
        transform.position = transform.position + dirToObjective * 0.25f;
        transform.LookAt(playerTransform);
    }
    void FixedUpdate()
    {
        if (!gamePaused)
        {
            UpdateCamera();
            player.UpdatePlayer(Time.fixedDeltaTime);
        }
        
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
