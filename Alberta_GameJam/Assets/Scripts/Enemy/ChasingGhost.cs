using UnityEngine;
using Game.Player;

public class ChasingGhost : Ghost
{
    [Header("Chase Settings")]
    [SerializeField] private float detectionRange = 5f;
    [SerializeField] private float chaseSpeed = 8f;  // Faster than normal movement
    [SerializeField] private float losePlayerRange = 8f;  // Distance at which ghost gives up chase
    
    [Header("Flee Settings")]
    [SerializeField] private float fleeSpeed = 10f;  // Even faster when fleeing
    [SerializeField] private float fleeDuration = 3f;  // How long to flee for
    [SerializeField] private float controlInversionDuration = 5f;  // How long player controls are inverted
    [SerializeField] private float minFleeDistance = 10f;  // Minimum distance to flee
    
    private float originalSpeed;
    private State previousState;
    private Transform playerTransform;

    protected void Start()
    {
        // Cache the player transform and original speed
        playerTransform = TopDownPlayerController.Instance.transform;
        originalSpeed = agent.speed;
    }

    protected override void HandleState()
    {
        if (state != State.Trapped && state != State.Dying && state != State.Fleeing)
        {
            // Check for player detection
            float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
            
            if (state != State.Chasing && distanceToPlayer <= detectionRange)
            {
                StartChasing();
            }
            else if (state == State.Chasing && distanceToPlayer > losePlayerRange)
            {
                StopChasing();
            }

            // Handle current state
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

        // If not chasing or fleeing, use normal ghost behavior
        base.HandleState();
    }

    private void StartChasing()
    {
        if (state != State.Chasing)
        {
            previousState = state;
            state = State.Chasing;
            agent.speed = chaseSpeed;
            
            // Play a detection sound effect (larger and higher-pitched for alert)
            GameEvents.RequestSoundWord(SoundType.GhostWalk, transform.position, Vector3.up, 1.2f);
        }
    }

    private void StopChasing()
    {
        if (state == State.Chasing)
        {
            agent.speed = originalSpeed;
            
            // Return to previous state
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

        // Update destination to player position
        agent.SetDestination(playerTransform.position);
        
        // Play chase sound effect with larger size and faster interval
        float currentTime = Time.time;
        if (currentTime >= nextWalkSoundTime)
        {
            GameEvents.RequestSoundWord(SoundType.GhostWalk, transform.position, Vector3.right, 1f);
            nextWalkSoundTime = currentTime + 0.4f; // Slightly faster sounds during chase
        }
    }

    // Visualize the detection range in the editor
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
        EnterIdle(); // Return to normal behavior
    }

    private void HandleFleeing()
    {
        if (agent == null || !agent.isActiveAndEnabled || playerTransform == null)
            return;

        // Calculate direction away from player
        Vector2 fleeDirection = (Vector2)transform.position - (Vector2)playerTransform.position;
        
        if (fleeDirection.magnitude < minFleeDistance)
        {
            // Find a point further away from the player
            Vector2 targetPosition = (Vector2)transform.position + fleeDirection.normalized * minFleeDistance;
            
            // Try to find a valid NavMesh position
            if (UnityEngine.AI.NavMesh.SamplePosition(targetPosition, out var navHit, minFleeDistance, UnityEngine.AI.NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
            }
        }

        // Play fleeing sound effect
        float currentTime = Time.time;
        if (currentTime >= nextWalkSoundTime)
        {
            GameEvents.RequestSoundWord(SoundType.GhostWalk, transform.position, Vector3.right, 1.2f);
            nextWalkSoundTime = currentTime + 0.3f; // Even faster sounds while fleeing
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

    // Override trap behavior to stop chasing when trapped
    public override void EnterTrapped(float duration)
    {
        StopChasing();
        base.EnterTrapped(duration);
    }

    // Override dying behavior to stop chasing when dying
    protected override void EnterDying()
    {
        StopChasing();
        base.EnterDying();
    }
}