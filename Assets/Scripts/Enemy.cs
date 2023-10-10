using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    [System.Flags]
    private enum AItype
    {
        STATIC = 2,
        PATROL = 4,
        WANDER = 8,
        FOLLOW = 16,
        FLEE = 32
    }


    [SerializeField]
    private NavMeshAgent agent;
    [SerializeField]
    private NewCharacterMovement player;
    [SerializeField] private float maxDistance;
    private float sqrMaxDistance;
    [SerializeField] private float FOV;
    [SerializeField] Transform[] patrolPoints;
    [SerializeField] private AItype aiType;
    [SerializeField] private int maxHealth;

    
    

    public int currentHealth = 50;

    private Coroutine rotationCoroutine = null;

    [SerializeField]
    private Transform healthsbarParent;

    [SerializeField]
    private HealthBarScript healthBarPrefab;

    public bool isStunned { get; private set; }

    [SerializeField]
    public Canvas worldCanvas;

   



    private Vector3 randomPoint = Vector3.zero;

    int Destination = 0;
    int maxDistanceFlee;
    // Start is called before the first frame update
    //int aiTypeTRAINING;
    //AItypeTRAINING = (int)enum | (int)enum;
    private void Reset()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        currentHealth = maxHealth;
        sqrMaxDistance = maxDistance * maxDistance;

        if (GetTriggerBehaviour() == AItype.PATROL)
            agent.SetDestination(patrolPoints[Destination].position);

        if (GetBaseBehaviour() == AItype.WANDER)
        {
            agent.SetDestination(getRandomPoint(20, 90f, 5f));
        }

        Instantiate(healthBarPrefab, healthsbarParent);
    }


    // Update is called once per frame
    void Update()
    {        
        if (currentHealth <= 0f)
        {
            isStunned = true;
        }

        worldCanvas.gameObject.SetActive(false);

        if (isStunned)
        {
            return;
        }

        if (GetBaseBehaviour() == AItype.WANDER)
        {
            if (Vector3.Distance(agent.destination, transform.position) < 1.5f)
            {
                agent.SetDestination(getRandomPoint(20, 90f, 5f));
            }
        }
        else if (GetBaseBehaviour() == AItype.STATIC)
        {
            if (rotationCoroutine == null)
            {
                rotationCoroutine = StartCoroutine(RotateCoroutine(90f, 30, 5f));
            }
        }

        Vector3 screenPos = Camera.main.WorldToScreenPoint(agent.gameObject.transform.position + Vector3.up * 1.5f);

        healthBarPrefab.GetComponent<RectTransform>().position = screenPos;
        healthBarPrefab.UpdateHealthBar(getCurrentHealthPercent());

        Vector3 agentToPlayer = player.transform.position - transform.position;
        float sqrDistance = agentToPlayer.sqrMagnitude;

        if (sqrDistance <= sqrMaxDistance)
        {
            if (Vector3.Dot(transform.forward, agentToPlayer.normalized) > Mathf.Cos(FOV / 2f))
            {
                if (GetTriggerBehaviour() == AItype.FOLLOW)
                {
                    agent.SetDestination(player.transform.position);
                }
                else
                {
                    SetTriggerBehaviour(AItype.FLEE);
                }

                if (rotationCoroutine != null)
                {
                    StopCoroutine(rotationCoroutine);
                    rotationCoroutine = null;
                }
            }
        }

        if (GetTriggerBehaviour() == AItype.FLEE)
        {

        }

        if (GetBaseBehaviour() == AItype.PATROL)
        {
            if (agent.remainingDistance <= 0.5f)
            {
                Destination = ++Destination % patrolPoints.Length;
                agent.SetDestination(patrolPoints[Destination].transform.position);
            }
        }

        if (currentHealth <= 0f)
        {
            die();
        }
    }

    public void OnDrawGizmos()
    {
        for (int i = 0; i < patrolPoints.Length; i++)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(patrolPoints[i].position, 1);
        }
    }

    private Vector3 getRandomPoint(int limit, float fieldofSearch, float radius)
    {
        Vector3 randomPos = Random.insideUnitSphere * radius + transform.position;

        if (Vector3.Dot(transform.forward, (randomPos - transform.position).normalized) < Mathf.Cos(FOV * Mathf.Deg2Rad / 2f))
        {
            if (limit == 0)
            {
                Debug.Log("yo");
                return getRandomPoint(20, fieldofSearch * 1.5f, radius);
            }
            return getRandomPoint(--limit, fieldofSearch, radius);
        }

        if (NavMesh.SamplePosition(randomPos, out NavMeshHit hit, radius, -1))
        {
            return hit.position;
        }

        if (limit == 0)
        {
            Debug.Log("yo");
            return getRandomPoint(20, fieldofSearch * 1.5f, radius);
        }
        return getRandomPoint(--limit, fieldofSearch, radius);
    }

    private AItype GetBaseBehaviour()
    {
        if ((aiType & AItype.STATIC) == AItype.STATIC)
        {
            return AItype.STATIC;
        }
        else if ((aiType & AItype.PATROL) == AItype.PATROL)
        {
            return AItype.PATROL;
        }

        return AItype.WANDER;
    }

    private AItype GetTriggerBehaviour()
    {
        if ((aiType & AItype.FOLLOW) == AItype.FOLLOW)
        {
            return AItype.FOLLOW;
        }
        return AItype.FLEE;
    }

    private void SetTriggerBehaviour(AItype behaviour)
    {
        aiType = behaviour;
    }

    private IEnumerator RotateCoroutine(float rotationDegrees, float rotationSpeed, float delayBetweenRotation)
    {
        float currentRotationApplied = 0f;
        while (currentRotationApplied < rotationDegrees)
        {
            currentRotationApplied += rotationSpeed * Time.deltaTime;
            transform.Rotate(transform.up, rotationSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(delayBetweenRotation);

        rotationCoroutine = null;
    }

    public void ActiveWorldCanvas()
    {
        worldCanvas.gameObject.SetActive(true);
    }

    public void takeDamage(int damage)
    {
        currentHealth -= damage;
    }

    private void die()
    {
        Destroy(gameObject);
    }

    public float getCurrentHealthPercent()
    {
        return currentHealth / maxHealth;
    }

   
}
