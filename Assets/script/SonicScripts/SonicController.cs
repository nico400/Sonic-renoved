using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class SonicController : MonoBehaviour
{

    public static SonicController instace;

    [Header("Atributs Sonic")]
    [SerializeField] Rigidbody sonicRb;

    [SerializeField] Transform cam;

    [SerializeField] Animator animSonic;

    [SerializeField] LayerMask groundLayer;

    [SerializeField] Transform startPosray;

    [Header("Speed Configs")]
    [SerializeField] float currentSonicSpeed;
    [SerializeField] float acelerationSonic;
    [SerializeField] float decelerationSonic;
    [SerializeField] float AirdecelerationSonic;
    [SerializeField] float maxSpeedSonicNoSlope;
    [SerializeField] float maxSpeedSonicLimit;
    [SerializeField] float slopeAssitenceAce;
    [SerializeField] float slopeAssitenceDec;

    [Header("jump Configs")]
    [SerializeField] float jumpHeight;
    [SerializeField] bool inJump;

    [Header("Homing Attack Config")]
    [SerializeField] GameObject[] attackers;
    [SerializeField] Transform target;
    [SerializeField] float distanceTarget;
    [SerializeField] bool iCanHomingAttack;
    [SerializeField] bool inHomingAttack;
    Vector3 directionHomingAttack;

    [Header("slopes visialize")]
    [SerializeField] float angleSlope;
    [SerializeField] float dotSlope;

    float hori;
    float verti;
    bool jumped;
    Vector3 normalSlope;
    Vector3 dirtoDownSlope;
    Vector3 movePlayer;

    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main.GetComponent<Transform>();  
        instace = this;
    }

    // Update is called once per frame
    void Update()
    {
        getInputsSonic();
        rotateSonicSlope();
        SpeedSonicController();
        getAngleOfSlope();

        animSonic.SetBool("isGround", checkGround());
        animSonic.SetFloat("movementSpeed", currentSonicSpeed);
        animSonic.SetBool("inSpinDash", inHomingAttack); 

    }
    private void FixedUpdate() 
    {
        movementSonic();
        jumpSonic();
        HomingAttack();
    }

    void movementSonic()
    {
        if(inJump) return;
        if(checkGround()) 
        {
            movePlayer = transform.forward * currentSonicSpeed; 

            if(currentSonicSpeed >= 10 && !jumped)
            {
                movePlayer += -transform.up * 4;
            }

            sonicRb.velocity = movePlayer;
        }
    }

    void SpeedSonicController()
    {
        if(checkGround())
        {
            if(SetDirectionRelative() != Vector3.zero)
            {
                if(dotSlope < 0) //descer
                {
                    currentSonicSpeed += (dotSlope * -1) * slopeAssitenceAce * Time.deltaTime;
                    maxSpeedSonicNoSlope = Mathf.Lerp(maxSpeedSonicNoSlope, maxSpeedSonicLimit, Time.deltaTime);
                }else if(dotSlope > 0) //subir
                {
                    currentSonicSpeed -= (dotSlope * -1) * slopeAssitenceDec * Time.deltaTime;
                    maxSpeedSonicNoSlope = Mathf.Lerp(maxSpeedSonicNoSlope, maxSpeedSonicLimit - 15, 0.2f * Time.deltaTime);
                }else{
                    maxSpeedSonicNoSlope = Mathf.Lerp(maxSpeedSonicNoSlope, 10, 0.05f * Time.deltaTime);
                    currentSonicSpeed += acelerationSonic * Time.deltaTime;
                }
                
            }else
            {
                if(dotSlope < 0)
                {
                    currentSonicSpeed += (dotSlope * -1) * slopeAssitenceAce * Time.deltaTime;
                    maxSpeedSonicNoSlope = Mathf.Lerp(maxSpeedSonicNoSlope, maxSpeedSonicLimit, Time.deltaTime);
                }else{
                    maxSpeedSonicNoSlope = Mathf.Lerp(maxSpeedSonicNoSlope, 10, Time.deltaTime);
                    currentSonicSpeed -= decelerationSonic * Time.deltaTime;
                }
            }
        }else
        {
            currentSonicSpeed -= AirdecelerationSonic * Time.deltaTime;
        }
        
        currentSonicSpeed = Mathf.Clamp(currentSonicSpeed, 0, maxSpeedSonicNoSlope);
    }

    void HomingAttack()
    {
        //get target to homing attack
        attackers = GameObject.FindGameObjectsWithTag("attackers");
        
        target = null;
        float distanceINF = Mathf.Infinity;

        foreach(GameObject potentialTarget in attackers)
        {

            Vector3 directionToTarget = potentialTarget.transform.position - transform.position;
            float dSqrToTarget = directionToTarget.sqrMagnitude;

            if(dSqrToTarget < distanceINF)
            {
                distanceINF = dSqrToTarget;              
                if(dSqrToTarget <= distanceTarget)
                {
                    target = potentialTarget.transform;
                }else{
                    target = null;
                }
            }
        }
        //direction to target
        if(target != null)
            directionHomingAttack = target.position - transform.position;
        else
            directionHomingAttack = transform.forward;

        //homing attack
        if(iCanHomingAttack && jumped && !checkGround() && directionHomingAttack != Vector3.zero)
        {
            inHomingAttack = true;     
            iCanHomingAttack = false;         
        }
        if(inHomingAttack)
        {
            sonicRb.velocity = Vector3.zero;
            sonicRb.velocity += directionHomingAttack.normalized * 20;
            if(target == null)
                Invoke("resetHomingAttack", 0.2f);
            else{
                float distace = Vector3.Distance(transform.position, target.position);
                if(distace <= 3.5f)
                {
                    resetHomingAttack();
                }
            }
        }
    }

    void jumpSonic()
    {
        if(jumped && checkGround() && !inJump)
        {
            inJump = true;
            Invoke("icanHomingAttack", 0.3f);

            float jumpF = Mathf.Sqrt(2 * jumpHeight * 9.8f);
            movePlayer += transform.up * jumpF;
            sonicRb.velocity = movePlayer;

            animSonic.Play("spinJump");
        }
    }

    void rotateSonicSlope()
    {
        RaycastHit hit;
        Debug.DrawRay(startPosray.position, -transform.up * 0.7f, Color.yellow);
        if(Physics.Raycast(startPosray.position, -transform.up, out hit, 0.7f, groundLayer))
        {
            Vector3 normal = hit.normal;                                                                                                     
            normalSlope = normal;

            Vector3 rightDir = Vector3.Cross(cam.transform.forward, normal);                                
            Debug.DrawRay(transform.position, rightDir * 3, Color.green);
            Vector3 forwardDir = Vector3.Cross(rightDir, normal);                                           
            Debug.DrawRay(transform.position, forwardDir * 3, Color.green);

            Vector3 dirToWalk = -forwardDir * verti + -rightDir * hori;                                    
            Debug.DrawRay(transform.position, dirToWalk * 3, Color.red);

            if(SetDirectionRelative() != Vector3.zero)                                                      
            {
                Quaternion rotPlayer = Quaternion.LookRotation(dirToWalk.normalized, normal);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotPlayer, 3 * Time.deltaTime);
            }else if(angleSlope > 0){
                Quaternion rotPlayer = Quaternion.LookRotation(dirtoDownSlope, normal);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotPlayer, 2 * Time.deltaTime);
            }

            Quaternion deltaRot = Quaternion.FromToRotation(transform.up, normal) * transform.rotation;    
            transform.rotation = Quaternion.Lerp(transform.rotation, deltaRot, 5 * Time.deltaTime);   

        }else{

            if(SetDirectionRelative() != Vector3.zero)                                                      
            {
                Quaternion rotPlayer = Quaternion.LookRotation(SetDirectionRelative(), Vector3.up);
                transform.rotation = Quaternion.Lerp(transform.rotation, rotPlayer, 5 * Time.deltaTime);
            }

            Quaternion deltaRot = Quaternion.FromToRotation(transform.up, Vector3.up) * transform.rotation;    
            transform.rotation = Quaternion.Lerp(transform.rotation, deltaRot, 5 * Time.deltaTime);    
        }
    }

    void getAngleOfSlope()
    {
        angleSlope = Vector3.Angle(transform.up, Vector3.up);
        if(angleSlope > 15f)
        {
            dotSlope = Vector3.Dot(Vector3.up, transform.forward);  
        }else if(angleSlope <= 15 && SetDirectionRelative() == Vector3.zero)
        {
            angleSlope = 0;
            dotSlope = 0;
        }
        else{
            dotSlope = 0;
        }
        dirtoDownSlope = Vector3.ProjectOnPlane(Vector3.down, normalSlope).normalized;
    }
    
    void getInputsSonic()
    {
        hori = Input.GetAxisRaw("Horizontal");
        verti = Input.GetAxisRaw("Vertical");
        jumped = Input.GetKeyDown(KeyCode.Space);
    }

    Vector3 SetDirectionRelative()                                                 
    {
        Vector3 dirRel = cam.forward * verti;
        dirRel += cam.right * hori;
        dirRel.Normalize();

        if(!checkGround()) dirRel.y = 0;
        
        return dirRel;
    }

    bool checkGround()                                                              //verify if i am in ground
    {
        return Physics.Raycast(startPosray.position, -transform.up, 0.7f, groundLayer);
    }
    void icanHomingAttack()
    {
        iCanHomingAttack = true;
    }
    
    void resetHomingAttack()
    {
        inHomingAttack = false;
    }
    void OnCollisionEnter(Collision other) {
        if(other.gameObject.layer == 3)
        {
            inJump = false;
            iCanHomingAttack = false;
        }
    }

    //get/set VARIABLES
    public void SpeedToZeroInTramplin(bool ResetSpeed)
    {
        if(ResetSpeed) currentSonicSpeed = 0;
        
        animSonic.Play("jumpTramplin");
    }
    public Rigidbody getRigBodySonic()
    {
        return sonicRb;
    }
    public Transform targetToHomingAttack()
    {
        if(iCanHomingAttack)
        {
            return target;
        }
        return null;
    }
}
