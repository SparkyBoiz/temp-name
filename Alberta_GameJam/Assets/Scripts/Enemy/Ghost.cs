using Game.Player;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Ghost : MonoBehaviour
{
    [field: SerializeField] public float moveRadius { get; private set; } = 5f;
    [field: SerializeField] public float idleDuration { get; private set; } = 3f;
    [field: SerializeField] public int maxHealth { get; private set; } = 10;
    [SerializeField] Image healthBar;
    [field: SerializeField] public float batteryCharge { get; private set; } = 0.3f;
    public float nextWalkSoundTime;

    protected State currentState;
    public IdleState IdleState { get; protected set; }
    public PatrolState PatrolState { get; protected set; }
    public TrappedState TrappedState { get; protected set; }
    public DyingState DyingState { get; protected set; }

    protected NavMeshAgent agent;
    protected int health;
    public bool isTracked;
    protected Animator animator;

#if UNITY_EDITOR
    private Vector3? m_LastRandomPoint;
    private Vector3? m_LastHitPoint;
#endif

    protected virtual void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        health = maxHealth;
        animator = GetComponentInChildren<Animator>();
        nextWalkSoundTime = Time.time;

        IdleState = gameObject.AddComponent<IdleState>();
        IdleState.Initialize(this, agent, animator);
        PatrolState = gameObject.AddComponent<PatrolState>();
        PatrolState.Initialize(this, agent, animator);
        TrappedState = gameObject.AddComponent<TrappedState>();
        TrappedState.Initialize(this, agent, animator);
        DyingState = gameObject.AddComponent<DyingState>();
        DyingState.Initialize(this, agent, animator);
    }

    protected virtual void Start()
    {
        if (agent.isActiveAndEnabled && !agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name}: Agent started off-mesh. Attempting to warp to nearest NavMesh...");
            if (NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 10f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                Debug.Log($"{name}: Warped to {hit.position}");
            }
        }

        ChangeState(IdleState);
    }

    protected virtual void Update()
    {
        UpdateHealthBarVisibility();
        UpdateRotation();
    }

    protected virtual void UpdateRotation()
    {
        if (agent.velocity.sqrMagnitude > 0.1f)
        {
            Vector3 direction = agent.velocity.normalized;

            if (direction.x < -0.1f)
            {
                transform.localScale = new Vector3(-1, 1, 1);
            }
            else if (direction.x > 0.1f)
            {
                transform.localScale = new Vector3(1, 1, 1);
            }
        }
    }

    public void ChangeState(State newState)
    {
        if (currentState != null)
        {
            currentState.Exit();
            currentState.enabled = false;
        }
        currentState = newState;
        if (currentState != null)
        {
            currentState.enabled = true;
            currentState.Enter();
        }
    }

    protected virtual void UpdateHealthBarVisibility()
    {
        if (healthBar != null)
        {
            healthBar.gameObject.SetActive(isTracked);
        }
    }
    
    public virtual void EnterTrapped(float duration)
    {
        TrappedState.SetDuration(duration);
        ChangeState(TrappedState);
    }

    public bool FindNextWaypoint()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            Debug.LogWarning($"{name}: Agent is not active or not on NavMesh. Cannot find waypoint.");
            return false;
        }

        Vector3 origin = transform.position;
        if (agent.isOnNavMesh)
        {
            origin.z = agent.nextPosition.z;
        }

        const int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            Vector2 randomDirection = Random.insideUnitCircle * moveRadius;
            Vector3 randomPoint = origin + new Vector3(randomDirection.x, randomDirection.y, 0);
            
#if UNITY_EDITOR
            m_LastRandomPoint = randomPoint;
            m_LastHitPoint = null;
#endif

            float sampleRadius = moveRadius * 2f; 
            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, sampleRadius, NavMesh.AllAreas))
            {
#if UNITY_EDITOR
                m_LastHitPoint = hit.position;
#endif
                var path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.path = path;
                    Debug.Log($"<color=green>{name}: Found new patrol point at {hit.position}. Path has {path.corners.Length} corners.</color>");
                    return true;
                }
                else
                {
                    Debug.LogWarning($"{name} (Attempt {attempt + 1}): Found a point on NavMesh at {hit.position}, but cannot calculate a complete path to it. It might be on a separate NavMesh island.");
                }
            }
            else
            {
            }
        }
        Debug.LogWarning($"{name}: Failed to find a valid waypoint after {maxAttempts} attempts.");
        return false;
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        health = Mathf.Max(0, health);
        if (healthBar != null)
        {
            healthBar.fillAmount = (float)health / (float)maxHealth;
        }
        if (health == 0 && currentState != DyingState)
        {
            ChangeState(DyingState);
        }
    }

    public void OnDyingAnimationCompleted()
    {
        Destroy(gameObject);
    }

#if UNITY_EDITOR
    protected virtual void OnDrawGizmosSelected()
    {
        if (agent != null)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, moveRadius);

            if (m_LastRandomPoint.HasValue)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawSphere(m_LastRandomPoint.Value, 0.2f);
            }

            if (m_LastHitPoint.HasValue)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(m_LastHitPoint.Value, 0.2f);
            }
        }
    }
#endif
}