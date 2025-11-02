using Game.Player;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Ghost : MonoBehaviour
{
    [SerializeField] float moveRadius;
    [SerializeField] float idleDuration = 3f;
    [SerializeField] int maxHealth = 10;
    [SerializeField] Image healthBar;
    [SerializeField] float batteryCharge = 0.3f;
    protected float nextWalkSoundTime;

    public enum State
    {
        Idle,
        Patrol,
        Trapped,
        Dying,
        Chasing,
        Fleeing  // Added for running away behavior
    }

    protected NavMeshAgent agent;
    protected float idleTimer;
    protected State state;
    protected int health;
    public bool isTracked;
    protected Animator animator;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.updateUpAxis = false;
        health = maxHealth;
        EnterIdle();
        isTracked = false;
        animator = GetComponentInChildren<Animator>();
        nextWalkSoundTime = Time.time; // Allow first sound immediately
    }

    protected virtual void Update()
    {
        HandleState();
        UpdateHealthBarVisibility();
    }

    protected virtual void HandleState()
    {
        switch (state)
        {
            case State.Idle:
                HandleIdle();
                break;
            case State.Patrol:
                HandlePatrol();
                break;
            case State.Trapped:
                HandleTrapped();
                break;
            case State.Dying:
                HandleDying();
                break;
            case State.Chasing:
                // Base class doesn't handle chasing
                break;
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
        if (agent != null)
        {
            agent.isStopped = true;
        }
        state = State.Trapped;
        StartCoroutine(TrappedTimer(duration));
    }

    protected virtual System.Collections.IEnumerator TrappedTimer(float duration)
    {
        yield return new WaitForSeconds(duration);
        if (agent != null)
        {
            agent.isStopped = false;
        }
        EnterIdle();
    }

    protected virtual void HandleTrapped()
    {
        // Do nothing while trapped
    }

    protected virtual void EnterIdle()
    {
        state = State.Idle;
        idleTimer = idleDuration;
        if (agent != null)
        {
            agent.ResetPath();
        }
    }

    protected virtual void HandleIdle()
    {
        if (idleTimer > 0f)
        {
            idleTimer -= Time.deltaTime;
        }

        if (idleTimer <= 0f)
        {
            EnterPatrol();
        }
    }

    protected virtual void EnterPatrol()
    {
        state = State.Patrol;
        if (agent != null)
        {
            agent.isStopped = false;  // Make sure the agent isn't stopped
            FindNextWaypoint();
        }
    }

    protected virtual void HandlePatrol()
    {
        if (agent == null || agent.pathPending)
        {
            return;
        }

        // Only play walk sound if we have a path and are moving
        if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
        {
            float currentTime = Time.time;
            if (currentTime >= nextWalkSoundTime)
            {
                GameEvents.RequestSoundWord(SoundType.GhostWalk, transform.position, Vector3.right, 0.7f);
                nextWalkSoundTime = currentTime + 0.5f; // Half second between footsteps
            }
        }

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude <= 0.01f)
            {
                EnterIdle();
            }
        }
    }

    protected virtual void EnterDying()
    {
        state = State.Dying;
        agent.isStopped = true;
        animator.SetTrigger("Die");
        TopDownPlayerController.Instance.ChargeBattery(batteryCharge);
        GameEvents.RequestSoundWord(SoundType.GhostDeath, transform.position, Vector3.up, 1.5f);
    }

    protected virtual void HandleDying()
    {

    }

    void FindNextWaypoint()
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh)
        {
            return;
        }

        var origin = transform.position;
        const int maxAttempts = 10;
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var randomDirection = UnityEngine.Random.insideUnitSphere * moveRadius;
            var target = origin + randomDirection;
            if (NavMesh.SamplePosition(target, out var navHit, moveRadius, NavMesh.AllAreas))
            {
                var path = new NavMeshPath();
                if (agent.CalculatePath(navHit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(navHit.position);
                    return;
                }
            }
        }
    }

    public void TakeDamage(int amount)
    {
        health -= amount;
        health = Mathf.Max(0, health);
        healthBar.fillAmount = (float)health / (float)maxHealth;
        if (health == 0 && state != State.Dying)
        {
            EnterDying();
        }
    }

    public void OnDyingAnimationCompleted()
    {
        Destroy(gameObject);
    }
}