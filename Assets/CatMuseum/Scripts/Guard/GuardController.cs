using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(GuardVision))]
public class GuardController : MonoBehaviour
{
    private enum GuardState
    {
        Patrol,
        Chase,
        Search
    }

    [Header("patrol route")]
    [SerializeField] private Transform[] patrolPoints;
    [SerializeField] private bool loop = true;
    [SerializeField] private float waitTimeAtPoint = 1.0f;

    [Header("speed")]
    [SerializeField] private float patrolSpeed = 2.0f;
    [SerializeField] private float chaseSpeed = 4.0f;
    [SerializeField] private float searchSpeed = 2.5f;

    [Header("alert modifier")]
    [SerializeField] private bool useAlertSpeedModifier = true;

    [Header("chase")]
    [SerializeField] private float loseSightTime = 2.0f;
    [SerializeField] private float catchDistance = 1.2f;
    [SerializeField] private float catchCooldown = 2.0f;

    [Header("memory")]
    [SerializeField] private float communicationDelay = 6.0f;

    [Header("search")]
    [SerializeField] private float searchTime = 4.0f;
    [SerializeField] private float searchPointReachDistance = 0.5f;

    [Header("debug")]
    [SerializeField] private bool showDebugLog = true;

    private NavMeshAgent agent;
    private GuardVision vision;

    private GuardState currentState = GuardState.Patrol;

    private int currentPointIndex = 0;
    private int direction = 1;

    private bool isWaiting = false;
    private float waitTimer = 0f;

    private PlayerInteractor targetPlayer;
    private PlayerInteractor rememberedPlayer;
    private PlayerSuspicionStatus targetSuspicionStatus;

    private Vector3 lastKnownPlayerPosition;

    private float loseSightTimer = 0f;
    private float searchTimer = 0f;
    private float catchTimer = 0f;

    private bool remembersPlayer = false;
    private bool isCommunicating = false;
    private bool hasCommunicated = false;
    private float communicationTimer = 0f;

    private void OnEnable()
    {
        GuardMemoryNetwork.RegisterGuard(this);
    }

    private void OnDisable()
    {
        UnregisterChaser();
        GuardMemoryNetwork.UnregisterGuard(this);
    }

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        vision = GetComponent<GuardVision>();
    }

    private void Start()
    {
        ChangeState(GuardState.Patrol);
    }

    private void Update()
    {
        UpdateTimers();
        UpdateCommunication();
        ApplyCurrentStateSpeed();

        ArtPiece visibleEmptyArt = vision.CheckVisibleEmptyArt();

        if (visibleEmptyArt != null)
        {
            ReportEmptyArt(visibleEmptyArt);
        }

        PlayerInteractor visiblePlayer = vision.CheckVisiblePlayer();

        if (visiblePlayer != null)
        {
            HandleVisiblePlayer(visiblePlayer);
        }

        switch (currentState)
        {
            case GuardState.Patrol:
                UpdatePatrol();
                break;

            case GuardState.Chase:
                UpdateChase();
                break;

            case GuardState.Search:
                UpdateSearch();
                break;
        }
    }

    private void UpdateTimers()
    {
        if (catchTimer > 0f)
        {
            catchTimer -= Time.deltaTime;
        }
    }

    private void UpdateCommunication()
    {
        if (!isCommunicating)
        {
            return;
        }

        communicationTimer -= Time.deltaTime;

        if (communicationTimer > 0f)
        {
            return;
        }

        isCommunicating = false;
        hasCommunicated = true;

        if (rememberedPlayer != null)
        {
            GuardMemoryNetwork.BroadcastRecognition(this, rememberedPlayer);

            if (showDebugLog)
            {
                Debug.Log(gameObject.name + " shared player information with other guards");
            }
        }
    }

    private void HandleVisiblePlayer(PlayerInteractor player)
    {
        PlayerSuspicionStatus suspicionStatus = player.GetComponent<PlayerSuspicionStatus>();
        bool playerIsBeingChased = suspicionStatus != null && suspicionStatus.IsBeingChased;

        if (player.IsInteracting)
        {
            WitnessTheft(player);
            return;
        }

        if (remembersPlayer && rememberedPlayer == player)
        {
            StartChase(player, "Recognized player");
            return;
        }

        if (playerIsBeingChased)
        {
            RememberPlayer(player, true);
            StartChase(player, "Joined chase");
            return;
        }
    }

    private void WitnessTheft(PlayerInteractor player)
    {
        player.ForceCancelInteraction("Seen by guard while interacting");
        player.ShowNotice("Caught while stealing!");

        RememberPlayer(player, true);
        StartChase(player, "Witnessed theft");

        if (showDebugLog)
        {
            Debug.Log(gameObject.name + " witnessed theft and remembered player");
        }
    }

    private void ReportEmptyArt(ArtPiece artPiece)
    {
        if (artPiece == null)
        {
            return;
        }

        if (!artPiece.CanReportEmpty)
        {
            return;
        }

        artPiece.ReportEmpty();

        if (AlertManager.Instance != null)
        {
            AlertManager.Instance.AddAlert(artPiece.GetEmptyAlertAmount());
        }

        if (showDebugLog)
        {
            Debug.Log(gameObject.name + " found empty pedestal: " + artPiece.ArtDisplayName);
        }
    }

    public void ReceiveRecognition(PlayerInteractor player)
    {
        if (player == null)
        {
            return;
        }

        rememberedPlayer = player;
        remembersPlayer = true;

        if (showDebugLog)
        {
            Debug.Log(gameObject.name + " received player information");
        }
    }

    private void RememberPlayer(PlayerInteractor player, bool startCommunication)
    {
        if (player == null)
        {
            return;
        }

        rememberedPlayer = player;
        remembersPlayer = true;

        if (startCommunication && !hasCommunicated)
        {
            isCommunicating = true;
            communicationTimer = communicationDelay;
        }
    }

    private void StartChase(PlayerInteractor player, string reason)
    {
        if (player == null)
        {
            return;
        }

        targetPlayer = player;
        lastKnownPlayerPosition = player.transform.position;
        loseSightTimer = loseSightTime;

        if (currentState != GuardState.Chase)
        {
            ChangeState(GuardState.Chase);
        }
        else
        {
            RegisterChaser();
        }

        agent.SetDestination(player.transform.position);

        if (showDebugLog)
        {
            Debug.Log(gameObject.name + " started chase: " + reason);
        }
    }

    private void UpdatePatrol()
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
                SetNextPatrolPoint();
                MoveToCurrentPatrolPoint();
            }

            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            StartWaiting();
        }
    }

    private void UpdateChase()
    {
        if (targetPlayer != null && vision.VisiblePlayer == targetPlayer)
        {
            lastKnownPlayerPosition = targetPlayer.transform.position;
            loseSightTimer = loseSightTime;

            agent.SetDestination(targetPlayer.transform.position);

            CheckCatchPlayer();
            return;
        }

        loseSightTimer -= Time.deltaTime;
        agent.SetDestination(lastKnownPlayerPosition);

        if (loseSightTimer <= 0f)
        {
            ChangeState(GuardState.Search);
        }
    }

    private void UpdateSearch()
    {
        searchTimer -= Time.deltaTime;

        agent.SetDestination(lastKnownPlayerPosition);

        bool reachedSearchPoint =
            !agent.pathPending &&
            agent.remainingDistance <= searchPointReachDistance;

        if (reachedSearchPoint && searchTimer <= 0f)
        {
            targetPlayer = null;
            ChangeState(GuardState.Patrol);
        }
    }

    private void CheckCatchPlayer()
    {
        if (targetPlayer == null)
        {
            return;
        }

        if (catchTimer > 0f)
        {
            return;
        }

        float distance = Vector3.Distance(transform.position, targetPlayer.transform.position);

        if (distance > catchDistance)
        {
            return;
        }

        catchTimer = catchCooldown;

        targetPlayer.ShowNotice("Caught by guard!");

        if (GameManager.Instance != null)
        {
            GameManager.Instance.GameOver("Caught by guard");
        }

        if (showDebugLog)
        {
            Debug.Log("Player caught by guard");
        }
    }

    private void ApplyCurrentStateSpeed()
    {
        if (agent == null)
        {
            return;
        }

        if (currentState == GuardState.Patrol)
        {
            agent.speed = GetPatrolSpeed();
        }
        else if (currentState == GuardState.Chase)
        {
            agent.speed = GetChaseSpeed();
        }
        else if (currentState == GuardState.Search)
        {
            agent.speed = GetSearchSpeed();
        }
    }

    private float GetPatrolSpeed()
    {
        if (!useAlertSpeedModifier || AlertManager.Instance == null)
        {
            return patrolSpeed;
        }

        return patrolSpeed * AlertManager.Instance.GuardPatrolSpeedMultiplier;
    }

    private float GetChaseSpeed()
    {
        if (!useAlertSpeedModifier || AlertManager.Instance == null)
        {
            return chaseSpeed;
        }

        return chaseSpeed * AlertManager.Instance.GuardChaseSpeedMultiplier;
    }

    private float GetSearchSpeed()
    {
        if (!useAlertSpeedModifier || AlertManager.Instance == null)
        {
            return searchSpeed;
        }

        return searchSpeed * AlertManager.Instance.GuardPatrolSpeedMultiplier;
    }

    private float GetSearchTime()
    {
        if (!useAlertSpeedModifier || AlertManager.Instance == null)
        {
            return searchTime;
        }

        return searchTime * AlertManager.Instance.GuardSearchTimeMultiplier;
    }

    private void ChangeState(GuardState newState)
    {
        if (currentState == GuardState.Chase && newState != GuardState.Chase)
        {
            UnregisterChaser();
        }

        currentState = newState;

        if (showDebugLog)
        {
            Debug.Log(gameObject.name + " State: " + currentState);
        }

        if (newState == GuardState.Patrol)
        {
            agent.speed = GetPatrolSpeed();
            isWaiting = false;
            MoveToCurrentPatrolPoint();
        }
        else if (newState == GuardState.Chase)
        {
            agent.speed = GetChaseSpeed();
            isWaiting = false;
            agent.isStopped = false;
            RegisterChaser();
        }
        else if (newState == GuardState.Search)
        {
            agent.speed = GetSearchSpeed();
            isWaiting = false;
            searchTimer = GetSearchTime();
            agent.isStopped = false;
            agent.SetDestination(lastKnownPlayerPosition);
        }
    }

    private void RegisterChaser()
    {
        if (targetPlayer == null)
        {
            return;
        }

        targetSuspicionStatus = targetPlayer.GetComponent<PlayerSuspicionStatus>();

        if (targetSuspicionStatus != null)
        {
            targetSuspicionStatus.RegisterChaser(this);
        }
    }

    private void UnregisterChaser()
    {
        if (targetSuspicionStatus != null)
        {
            targetSuspicionStatus.UnregisterChaser(this);
        }

        targetSuspicionStatus = null;
    }

    private void MoveToCurrentPatrolPoint()
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
    }

    private void StartWaiting()
    {
        isWaiting = true;
        waitTimer = waitTimeAtPoint;
        agent.isStopped = true;
    }

    private void SetNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length <= 1)
        {
            return;
        }

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
        if (patrolPoints != null && patrolPoints.Length > 0)
        {
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

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(lastKnownPlayerPosition, 0.3f);
    }
}