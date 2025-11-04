using UnityEngine;
using Game.Player;

public class ChasingGhost : Ghost
{
    [Header("Chase Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float chaseSpeed = 8f;
    [SerializeField] private float losePlayerRange = 8f;

    [Header("Flee Settings")]
    [SerializeField] private float fleeSpeed = 10f;
    [SerializeField] private float fleeDuration = 3f;
    [SerializeField] private float controlInversionDuration = 5f;
    [SerializeField] private float minFleeDistance = 10f;

    private float originalSpeed;
    private State previousState;
    private Transform playerTransform;

    protected void Start()
    {
        playerTransform = TopDownPlayerController.Instance.transform;
        originalSpeed = agent.speed;
    }

    protected override void HandleState()
    {
        if (state != State.Trapped && state != State.Dying && state != State.Fleeing)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);

            if (state != State.Chasing && distanceToPlayer <= detectionRange)
            {
                StartChasing();
            }
            else if (state == State.Chasing && distanceToPlayer > losePlayerRange)
            {
                StopChasing();
            }

            if (state == State.Chasing)
            {
                HandleChase();
                return;
            }
        }
        else if (state == State.Fleeing)
        {
            HandleFleeing();
            return;
        }

        base.HandleState();
    }

    private void StartChasing()
    {
        if (state != State.Chasing)
        {
            previousState = state;
            state = State.Chasing;
            agent.speed = chaseSpeed;
            GameEvents.RequestSoundWord(SoundType.GhostWalk, transform.position, Vector3.up, 1.2f);
        }
    }

    private void StopChasing()
    {
        if (state == State.Chasing)
        {
            agent.speed = originalSpeed;

            if (previousState == State.Patrol)
            {
                EnterPatrol();
            }
            else
            {
                EnterIdle();
            }
        }
    }

    private void HandleChase()
    {
        if (agent == null || !agent.isActiveAndEnabled)
            return;

        agent.SetDestination(playerTransform.position);

        float currentTime = Time.time;
        if (currentTime >= nextWalkSoundTime)
        {
            GameEvents.RequestSoundWord(SoundType.GhostWalk, transform.position, Vector3.right, 1f);
            nextWalkSoundTime = currentTime + 0.4f;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, losePlayerRange);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<TopDownPlayerController>() != null)
        {
            StartFleeing();
            InvertPlayerControls();
        }
    }

    private void StartFleeing()
    {
        state = State.Fleeing;
        agent.speed = fleeSpeed;
        StartCoroutine(FleeingTimer());
    }

    private System.Collections.IEnumerator FleeingTimer()
    {
        yield return new WaitForSeconds(fleeDuration);
        if (state == State.Fleeing)
        {
            StopFleeing();
        }
    }

    private void StopFleeing()
    {
        agent.speed = originalSpeed;
        EnterIdle();
    }

    private void HandleFleeing()
    {
        if (agent == null || !agent.isActiveAndEnabled || playerTransform == null)
            return;

        Vector2 fleeDirection = (Vector2)transform.position - (Vector2)playerTransform.position;

        if (fleeDirection.magnitude < minFleeDistance)
        {
            Vector2 targetPosition = (Vector2)transform.position + fleeDirection.normalized * minFleeDistance;

            if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out var navHit, minFleeDistance, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
            }
        }

        float currentTime = Time.time;
        if (currentTime >= nextWalkSoundTime)
        {
            GameEvents.RequestSoundWord(SoundType.GhostWalk, transform.position, Vector3.right, 1.2f);
            nextWalkSoundTime = currentTime + 0.3f;
        }
    }

    private void InvertPlayerControls()
    {
        var player = TopDownPlayerController.Instance;
        if (player != null)
        {
            player.InvertControls(true);
            StartCoroutine(ResetPlayerControlsTimer());
        }
    }

    private System.Collections.IEnumerator ResetPlayerControlsTimer()
    {
        yield return new WaitForSeconds(controlInversionDuration);
        var player = TopDownPlayerController.Instance;
        if (player != null)
        {
            player.InvertControls(false);
        }
    }

    public override void EnterTrapped(float duration)
    {
        StopChasing();
        base.EnterTrapped(duration);
    }

    protected override void EnterDying()
    {
        StopChasing();
        base.EnterDying();
    }
}