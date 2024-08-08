using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Player : MonoBehaviour
{
    // Start is called before the first frame update
    Vector2 moveDir = Vector2.zero;
    bool firing = false;
    public DefaultControls controls;
    private InputAction move;
    private InputAction fire;
    private InputAction look;
    Vector2 previousLookVector2 = Vector2.zero;
    Rigidbody rbody;
    bool grounded = false;
    //All these angles are in Radians ***Note***
    public float yRotSpeed = 0.000125f;
    public float xRotSpeed = 0.000125f;
    public float cameraYAngle = 0f;//the y angle (what direction the camera is away from the player)
    public float cameraXAngle = 0.5f; // the x angle, or the height of the arc of the cam (what direction it is above the ground)
    bool facingLeft = true;
    Vector3 floorNormal = Vector3.up;
    float jumpTime = -3f;//this is to calculate how long you have been falling. 0 is about to fall, -3 is when you start jumping and 3 is the highest it gets
    [SerializeField]
    public EngineSettings defaultSettings;
    EngineSettings currentSettings;
    List<EngineSettingsAffector> affectors = new List<EngineSettingsAffector>();    
    void Start()
    {
        
    }
    private void Awake()
    {
        controls = new DefaultControls();
        rbody = GetComponent<Rigidbody>();
        currentSettings = ScriptableObject.CreateInstance<EngineSettings>();
    }
    private void  OnEnable()
    {
        move = controls.Player.Move;
        move.Enable();
        fire = controls.Player.Fire;
        fire.Enable();
        look = controls.Player.Look;
        look.Enable();
    }
    private void OnDisable()
    {
        move.Disable();
        fire.Disable();
        look.Disable();
        fire.performed += Fire;
    }
    void CheckGrounded()
    { 
        RaycastHit hit;
        if(Physics.Raycast(transform.position, Vector3.down,out hit,0.25f,LayerMask.GetMask("Ground")))
        {
            floorNormal = hit.normal;
            transform.position = hit.point + Vector3.up * 0.24f;
            if(floorNormal.y > 0.5f){grounded = true;}
        }else{grounded = false;}
    }
    float GetYMovementThisFrame(float timePassed){
        float y = 0f;
        if(jumpTime > 0f){CheckGrounded();}
        if(grounded && jumpTime > 0f){return y;}
        float previousJump = MarioFunction(jumpTime);
        float previousJumpTimeSign = Mathf.Sign(jumpTime);
        jumpTime += timePassed * 6f; //total jump time is 6 seconds totaly (-3 to 3) multiply it to adjust jump speed
        float currentJumpTimeSign = Mathf.Sign(jumpTime);
        //if(Mathf.Abs(jumpTime) < 0.5f){return 0f;}
        //if(previousJumpTimeSign != currentJumpTimeSign){return 0f;}//this is just to take one frame where we're at the top then move down cause jumping makes the camera shake
        float currentJump = MarioFunction(jumpTime);
        jumpTime = Mathf.Clamp(jumpTime,-3f,3f);
        y = currentJump - previousJump;
        //Debug.Log(y + " at jumpt time " + jumpTime);
        return y * currentSettings.GetJumpAmount();
    }
    float MarioFunction(float f){
        if(grounded){return 0f;}
        return ((Mathf.Pow(f,2f)) * -1f) + 9f;
    }
    void UpdateCurrentSettings(float timePassed)    //this updates the current settings by copying the default settings and making adjustments based on teh settings affectors the player has
    {
        currentSettings.CopySettings(defaultSettings);
        for(int i = 0; i < affectors.Count;i++)
        {
            EngineSettingsAffector a = affectors[i];
            if(a.UpdateAffector(timePassed))//if the update returns true it has finished remove it from teh list (will not trigger if it does not end)
            {
                affectors.RemoveAt(i);
                i--;
            }else                           //because the update is not finished it will apply the affector as it is still active
            {
                EngineSettingsAffectorType affectTyp = a.GetTypeOfAffector();
                switch(affectTyp)
                {
                    case EngineSettingsAffectorType.addToSpeed: currentSettings.AddToMoveSpeed(a.GetAmount());break;
                    case EngineSettingsAffectorType.addToMaxSpeed: currentSettings.AddToMaxSpeed(a.GetAmount());break;
                    case EngineSettingsAffectorType.addToJump: currentSettings.AddToJumpAmount(a.GetAmount());break;
                    case EngineSettingsAffectorType.negateDownwardMomentum: currentSettings.SetAffectedByGravity(false);break;//negate downward momentum will also negate positive momentum
                }
            }
        }
    }
    public void UpdatePlayer(float timePassed) 
    {
        moveDir = move.ReadValue<Vector2>();
        Transform cam = Camera.main.transform;
        Vector3 camForward = cam.forward;camForward.y = 0f; camForward.Normalize();
        if(grounded){if(firing){jumpTime = -3f;grounded = false;floorNormal = Vector3.up;}}
        UpdateCurrentSettings(timePassed);
        float moveSpeed = currentSettings.GetMoveSpeed();
        float maxSpeed = currentSettings.GetMaxSpeed();
        Vector3 movement = cam.right * moveDir.x + (camForward * moveDir.y); movement *= moveSpeed; 
        if(movement.magnitude < maxSpeed){movement = movement.normalized * maxSpeed;}
        movement.y = GetYMovementThisFrame(timePassed);
        //if(floorNormal != Vector3.up){movement = Vector3.ProjectOnPlane(movement,floorNormal);}
        if(floorNormal != Vector3.up){movement = AdjustMovementForSlope(movement,floorNormal);}
        rbody.velocity = new Vector3(movement.x,movement.y,movement.z);
        //transform.Translate(movement * timePassed,Space.World);

        firing = false;
        Vector3 dirToCam = cam.position - transform.position;dirToCam.y = 0f;
        transform.rotation = Quaternion.LookRotation(dirToCam,Vector3.up);
    }
    Vector3 AdjustMovementForSlope(Vector3 moveDirection, Vector3 slopeNormal)
    {
        // Rotate the movement direction based on the slope's x and z values
        moveDirection = Quaternion.FromToRotation(Vector3.up, slopeNormal) * moveDirection;
        return moveDirection;
    }
    private void Fire(InputAction.CallbackContext context)
    {
        Debug.Log("fire");
    }
    // Update is called once per frame
    void Update()
    {
        if(fire.triggered){firing = true;}
        Vector2 currentLook = look.ReadValue<Vector2>();
        Vector2 lookDelta = currentLook - previousLookVector2;
        cameraYAngle += currentLook.x * yRotSpeed;
        //cameraYAngle += Time.deltaTime;

        previousLookVector2 = currentLook;
        //Debug.Log(look.ReadValue<Vector2>());
        //Debug.Log("for jumpt time : " + jumpTime + " it returns " + MarioFunction(jumpTime));
        //jumpTime+= Time.deltaTime * 3f;
    }
}
