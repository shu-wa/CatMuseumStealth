using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class GuardPatrol : MonoBehaviour
{
    [Header("patrol route")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private bool loop = true;
    [SerializeField] private float waitTimeAtPoint = 1.0f;

    [Header("debug")]
    [SerializeField] private bool showDebugLog = false;

    private NavMeshAgent agent;
    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;
    private int direction = 1;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        MoveToCurrentPoint();
    }

    private void Update()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                SetNextPoint();
                MoveToCurrentPoint();
            }

            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartWaiting();
        }
    }

    private void MoveToCurrentPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        Transform targetPoint = patrolPoints[currentPointIndex];

        if (targetPoint == null)
        {
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(targetPoint.position);

        if (showDebugLog)
        {
            Debug.Log("Guard moving to: " + targetPoint.name);
        }
    }

    private void StartWaiting()
    {
        isWaiting = true;
        waitTimer = waitTimeAtPoint;
        agent.isStopped = true;

        if (showDebugLog)
        {
            Debug.Log("Guard waiting");
        }
    }

    private void SetNextPoint()
    {
        if (loop)
        {
            currentPointIndex++;

            if (currentPointIndex >= patrolPoints.Length)
            {
                currentPointIndex = 0;
            }
        }
        else
        {
            currentPointIndex += direction;

            if (currentPointIndex >= patrolPoints.Length)
            {
                currentPointIndex = patrolPoints.Length - 2;
                direction = -1;
            }
            else if (currentPointIndex < 0)
            {
                currentPointIndex = 1;
                direction = 1;
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            return;
        }

        Gizmos.color = Color.green;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null)
            {
                continue;
            }

            Gizmos.DrawWireSphere(patrolPoints[i].position, 0.25f);

            int nextIndex = i + 1;

            if (nextIndex >= patrolPoints.Length)
            {
                if (!loop)
                {
                    continue;
                }

                nextIndex = 0;
            }

            if (patrolPoints[nextIndex] != null)
            {
                Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[nextIndex].position);
            }
        }
    }
}