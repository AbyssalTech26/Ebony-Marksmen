using Mono.Cecil;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public int health = 100;

    public GameObject bulletPrefab;
    public Transform bulletSpawnPoint;
    public GameObject weaponFlash;
    public float bloom;
    public float fireRate;
    private float lastShotTime = 0f;
     
    public Material hitMat;

    private Rigidbody rb;
    private Renderer rend;
    private Material originalMaterial;

    //AI Settings 
    public int currentPointIndex = 0; //Create points enemy can patrol between 
    public Vector3 currentTarget; //Poistion of current target. What enemy will be walkign toward 
    public float positionThreshhold; //How close enemy has to be before it thinks it reaches the target
    public float idleTime = 3f; //how long enemy stays idle
    public float attackDistance = 5f;
    public float maxVisionDistance = 20f;
    public float minChasingHealth = 30f; //If enemy has under 30 health, it will run away from player

    public Transform[] patrolPoints; // Array of points enemy can patrol between 
    private float idleTimeCounter; //Timer for idleing enemy 
    private Transform playerTransform;
    private bool canSeePlayer;
    private Vector3 lastKnownPlayerPosition;

    private NavMeshAgent agent;

    public enum State { Idle, Patrolling, Chasing, Attacking } //List of enem states
    public State state = State.Idle; //Default enemy state 

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rend = GetComponent<Renderer>();
        originalMaterial = rend.material; //Goes back to original material after bliking 

        agent = GetComponent<NavMeshAgent>();
        playerTransform = GameObject.FindWithTag("Player").GetComponent<Transform>();

        GameObject patrolPointParent = GameObject.FindWithTag("PatrolPoint"); //Patrol points will be under a parent object, containing a list of patrol points 
        patrolPoints = patrolPointParent.GetComponentsInChildren<Transform>().Where(t => t != patrolPointParent.transform).ToArray(); //Receiving transform from children objects 
    }


    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Damage")
        {
            health -= 10;

            if (health <= 0)
            {
                Die();
            }
            else
            {
                StartCoroutine(Blink());
            }
        }
    }

    void Die()
    {
        if (!this.enabled) return;

        rb.freezeRotation = false; //Enemy falls over
        transform.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, transform.rotation.z + 5f); // 
        this.enabled = false; // Enemy script is no longer active
    }

    IEnumerator Blink()
    {
        rend.material = hitMat;
        yield return new WaitForSeconds(0.1f);
        rend.material = originalMaterial;

    }

    private void Update()
    {
        LookForPlayer(); //Function

        switch (state) //Defing states
        {
            case State.Idle:
                Idle();
                break;
            case State.Patrolling:
                Patrolling();
                break;
            case State.Chasing:
                Chasing();
                break;
            case State.Attacking:
                Attacking();
                break;
        }
        rb.linearVelocity = Vector3.zero; //Rigid body doesnt interfear with navmesh agent

        LookAtPlayer();
        SetLastKnownPlayerPosition();
    }

    private void LookForPlayer()
    {
        Vector3 directionToPlayer = playerTransform.position - transform.position; //Equation for direction

        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, maxVisionDistance))
        {
            canSeePlayer = hit.transform == playerTransform; //if raycast hits player instead of a wall or other object 

            if (canSeePlayer && state != State.Attacking)
            {
                state = State.Chasing;
            }
        }
    }

    private void Idle()
    {
        agent.ResetPath(); //After resetting path, agent will stand still

        idleTimeCounter -= Time.deltaTime;

        if (idleTime < 0) //Wait for 5 sec. then swithc to patrolling
        {
            state = State.Patrolling;
            idleTimeCounter = idleTime;
        }
    }

    private void Patrolling()
    {
        if (Vector3.Distance(currentTarget, transform.position) < positionThreshhold) //Check if close enough to current target
        {
            float chance = Random.Range(0, 100);

            if (chance < 10)
            {
                state = State.Idle;
                return; //Set back to idling with 10% chance
            }

            currentPointIndex++; // add 1 to point index 
            currentTarget = patrolPoints[currentPointIndex % patrolPoints.Length].position; //Look on google for mor Modulo info
        }
        else
        {
            agent.SetDestination(currentTarget); //if not close enought to current target 
        }
    }

    private void Chasing()
    {
        idleTimeCounter = idleTime;
        agent.SetDestination(lastKnownPlayerPosition);

        if (health < minChasingHealth)
        {
            state = State.Patrolling;
        }
        else if (Vector3.Distance(transform.position, playerTransform.position) <= attackDistance && canSeePlayer) //chekc if distacne between enemy nd player is < or = attack distance 
        {
            state = State.Attacking;
        }
        else if (Vector3.Distance(transform.position, playerTransform.position) > maxVisionDistance)
        {
            state = State.Patrolling; 
        }
        else if (Vector3.Distance(transform.position, playerTransform.position) < positionThreshhold && !canSeePlayer)
        {
            state = State.Patrolling;
        }
    }

    private void Attacking()
    {
        idleTimeCounter = idleTime; //set back to 5 
        agent.ResetPath(); //want to stand still when shooting 

        Shoot();

        if (Vector3.Distance(transform.position, playerTransform.position) > attackDistance || !canSeePlayer)
        {
            if (health < minChasingHealth)
            {
                state = State.Patrolling; //Cautious
            }
            else
            {
                state = State.Chasing; //if player to too far away to shoot
            }
        }
    }

    private void LookAtPlayer()
    {
        transform.LookAt(new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z)); //doesnt include playerTranform.y cause player will be looing up 
    }

    private void SetLastKnownPlayerPosition()
    {
        if (canSeePlayer)
        {
            lastKnownPlayerPosition = playerTransform.position;
        }
    }

    private void Shoot()
    {
        if (Time.time > lastShotTime + fireRate)
        {
            Vector3 directionToPlyaer = playerTransform.position - transform.position;
            directionToPlyaer.Normalize();

            Quaternion bulletRotation = Quaternion.LookRotation(directionToPlyaer);

            float maxInaccuracy = 10f; //10 degrees off
            float currentInaccuracy = bloom + maxInaccuracy;
            float randomJaw = Random.Range(-currentInaccuracy, currentInaccuracy);
            float randomPitch = Random.Range(-currentInaccuracy, currentInaccuracy);

            bulletRotation *= Quaternion.Euler(randomPitch, randomJaw + 90, 0f);

            Instantiate(bulletPrefab, bulletSpawnPoint.position, bulletRotation);
            Instantiate(weaponFlash, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            lastShotTime = Time.time;
        }
    }
}
