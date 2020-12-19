using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    public static CarController instance;
    public Rigidbody theRB;
    public float maxSpeed, forwardAccel = 8f, reverseAccel = 4f;
    private float speedInput;
    public float turnStrength = 180f;
    private float turnInput;
    private bool grounded;
    public Transform groundRayPoint, groundRayPoint2;
    public LayerMask whatIsGround;
    public float groundRayLength = .75f;
    private float dragOnGround;
    public float gravityMod = 10f;
    public Transform leftFrontWheel, rightFrontWheel;
    public float maxWheelTurn = 25f;
    public ParticleSystem[] dustTrail;
    public float maxEmission = 25f, emissionFadeSpeed = 20f;
    private float emissionRate;
    public AudioSource engineSound, tireSqueal;
    public float skidFadeSpeed;
    public int nextCheckpoint;
    public int currentLap;
    public float lapTime, bestLapTime;
    public bool isAI;
    public int currentTarget;
    private Vector3 targetPoint;
    public float aiAccelerateSpeed = 1f, aiTurnSpeed = .8f, aiReachPointRange = 5f, aiPointVariance = 3f; 
    private float aiSpeedInput, aiMaxTurn = 15f, aiSpeedMod;
    public float resetCooldown = 2f;
    private float resetCounter;
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    void Start()
    {
        theRB.transform.parent = null;
        dragOnGround = theRB.drag;

        UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;

        if(isAI)
        {
            targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
            RandomizeAITarget();

            aiSpeedMod = Random.Range(.8f, 1.1f);
        }

        resetCounter = resetCooldown;   
    }

    // Update is called once per frame
    void Update()
    {
        if(!RaceManager.instance.isStarting)
        {


        lapTime += Time.deltaTime;

        if (!isAI)
        {



            var ts = System.TimeSpan.FromSeconds(lapTime);
            UIManager.instance.currentLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

            speedInput = 0f;

            if (Input.GetAxis("Vertical") > 0)
            {
                speedInput = Input.GetAxis("Vertical") * forwardAccel;
            }
            else if (Input.GetAxis("Vertical") < 0)
            {
                speedInput = Input.GetAxis("Vertical") * reverseAccel;
            }

            turnInput = Input.GetAxis("Horizontal");

            /*
            if (Input.GetAxis("Vertical") != 0)
            {
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * (grounded ? turnStrength : turnStrength / 2) * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f));
            }
            */
            if(resetCounter > 0)
            {
                resetCounter -= Time.deltaTime;
            }

            if(Input.GetKeyDown(KeyCode.R) && resetCounter <= 0)
            {
                ResetToTrack();
            }
        }
        else
        {
            targetPoint.y = transform.position.y;
            if(Vector3.Distance(transform.position, targetPoint) < aiReachPointRange)
            {
               SetNextAITarget();
            }

            Vector3 targetDirection = targetPoint - transform.position;
            float angle = Vector3.Angle(targetDirection, transform.forward);
            Vector3 localPos = transform.InverseTransformPoint(targetPoint);
            if(localPos.x <0f)
            {
                angle = -angle;
            }
            turnInput = Mathf.Clamp(angle / aiMaxTurn, -1f, 1f);
            if(Mathf.Abs(angle) < aiMaxTurn)
            {
                aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, 1f, aiAccelerateSpeed);
            }
            else
            {
                aiSpeedInput = Mathf.MoveTowards(aiSpeedInput, aiTurnSpeed, aiAccelerateSpeed);
            }
            speedInput = aiSpeedInput * forwardAccel * aiSpeedMod;
        }

        //turning the wheels
        leftFrontWheel.localRotation = Quaternion.Euler(leftFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn) - 180f, leftFrontWheel.localRotation.eulerAngles.z);
        rightFrontWheel.localRotation = Quaternion.Euler(rightFrontWheel.localRotation.eulerAngles.x, (turnInput * maxWheelTurn), rightFrontWheel.localRotation.eulerAngles.z);


        //transform.position = theRB.transform.position;

        //control particle emissions
        emissionRate = Mathf.MoveTowards(emissionRate, 0f, emissionFadeSpeed * Time.deltaTime);

        if (grounded && (Mathf.Abs(turnInput) > .5f || (theRB.velocity.magnitude < maxSpeed * .5f && theRB.velocity.magnitude != 0)))
        {
            emissionRate = maxEmission;
        }

        if (theRB.velocity.magnitude <= .5f)
        {
            emissionRate = 0;
        }

        for (int i = 0; i < dustTrail.Length; i++)
        {
            var emissionModule = dustTrail[i].emission;

            emissionModule.rateOverTime = emissionRate;
        }

        if (engineSound != null)
        {
            engineSound.pitch = 1f + ((theRB.velocity.magnitude / maxSpeed) * 2f);
        }

        if (tireSqueal != null)
        {
            if (Mathf.Abs(turnInput) > .5f)
            {
                tireSqueal.volume = 1f;

            }
            else
            {
                tireSqueal.volume = Mathf.MoveTowards(tireSqueal.volume, 0, skidFadeSpeed * Time.deltaTime);
            }
        }
        }
    }

    private void FixedUpdate()
    {
        grounded = false;

        RaycastHit hit;
        Vector3 normalTarget = Vector3.zero;

        if (Physics.Raycast(groundRayPoint.position, -transform.up, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;

            normalTarget = hit.normal;
        }

        if (Physics.Raycast(groundRayPoint2.position, -transform.up, out hit, groundRayLength, whatIsGround))
        {
            grounded = true;
            normalTarget = (normalTarget + hit.normal) / 2;
        }
        //when on ground rotate to match the normal
        if (grounded)
        {
            transform.rotation = Quaternion.FromToRotation(transform.up, normalTarget) * transform.rotation;
        }

        //accelerates the car
        if (grounded)
        {
            theRB.drag = dragOnGround;
            theRB.AddForce(transform.forward * speedInput * 1000f);

        }
        else
        {
            theRB.drag = .1f;
            theRB.AddForce(-Vector3.up * gravityMod * 100f);
        }
        if (theRB.velocity.magnitude > maxSpeed)
        {
            theRB.velocity = theRB.velocity.normalized * maxSpeed;
        }

        transform.position = theRB.transform.position;

        if (grounded && speedInput != 0)
        {

            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles + new Vector3(0f, turnInput * (grounded ? turnStrength : turnStrength / 2) * Time.deltaTime * Mathf.Sign(speedInput) * (theRB.velocity.magnitude / maxSpeed), 0f));

        }


    }

    public void CheckpointHit(int cpNumber)
    {
        Debug.Log("hit checkpoint " + cpNumber);

        if (cpNumber == nextCheckpoint)
        {
            nextCheckpoint++;

            if (nextCheckpoint >= RaceManager.instance.allCheckpoints.Length)
            {
                nextCheckpoint = 0;
                LapCompleted();
            }
        }

        if(isAI)
        {
            if(cpNumber == currentTarget)
            {
                SetNextAITarget();
            }
        }
    }

    public void SetNextAITarget()
    {
            
                currentTarget++;
                if(currentTarget >= RaceManager.instance.allCheckpoints.Length)
                {
                    currentTarget = 0;
                }

                 targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
                RandomizeAITarget();
            
    }

    public void LapCompleted()
    {
        currentLap++;

        if (lapTime < bestLapTime || bestLapTime == 0)
        {
            bestLapTime = lapTime;
        }

        if(currentLap <= RaceManager.instance.totalLaps)
        {
            lapTime = 0;

        if (!isAI)
        {
            var ts = System.TimeSpan.FromSeconds(bestLapTime);
            UIManager.instance.bestLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

            UIManager.instance.lapCounterText.text = currentLap + "/" + RaceManager.instance.totalLaps;
        }
        }
        else 
        {
            if(!isAI)
            {
                isAI = true;
                aiSpeedMod = 1f;

                 targetPoint = RaceManager.instance.allCheckpoints[currentTarget].transform.position;
                RandomizeAITarget();

                var ts = System.TimeSpan.FromSeconds(bestLapTime);
            UIManager.instance.bestLapTimeText.text = string.Format("{0:00}m{1:00}.{2:000}s", ts.Minutes, ts.Seconds, ts.Milliseconds);

            RaceManager.instance.FinishRace();
            }
        }

        
    }

    public void RandomizeAITarget()
    {
        targetPoint += new Vector3(Random.Range(-aiPointVariance, aiPointVariance), 0f, Random.Range(-aiPointVariance, aiPointVariance));
    }

    private void ResetToTrack()
    {
        int pointToGoTo = nextCheckpoint == 0 ? RaceManager.instance.allCheckpoints.Length - 1 : nextCheckpoint - 1;

        transform.position = RaceManager.instance.allCheckpoints[pointToGoTo].transform.position;
        theRB.transform.position = transform.position;
        theRB.velocity = Vector3.zero;
        speedInput = 0;
        turnInput = 0;

        resetCounter = resetCooldown;
    }
}
