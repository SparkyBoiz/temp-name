using UnityEngine;
using Game.Player;

public class ChasingGhost : Ghost
{
    [field: SerializeField] public float detectionRange { get; private set; } = 5f;
    [field: SerializeField] public float chaseSpeed { get; private set; } = 8f;
    [field: SerializeField] public float losePlayerRange { get; private set; } = 8f;

    [field: SerializeField] public float fleeSpeed { get; private set; } = 10f;
    [field: SerializeField] public float fleeDuration { get; private set; } = 3f;
    [field: SerializeField] public float controlInversionDuration { get; private set; } = 5f;
    [field: SerializeField] public float minFleeDistance { get; private set; } = 10f;

    public float originalSpeed { get; private set; }
    public Transform playerTransform { get; private set; }

    public ChasingState ChasingState { get; private set; }
    public FleeingState FleeingState { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        originalSpeed = agent.speed;

        ChasingState = gameObject.AddComponent<ChasingState>();
        ChasingState.Initialize(this, agent, animator);
        FleeingState = gameObject.AddComponent<FleeingState>();
        FleeingState.Initialize(this, agent, animator);
    }

    protected override void Start()
    {
        if (TopDownPlayerController.Instance != null)
        {
            playerTransform = TopDownPlayerController.Instance.transform;
        }
        base.Start();
    }

    protected override void Update()
    {
        if (currentState != TrappedState && currentState != DyingState && currentState != FleeingState && currentState != ChasingState)
        {
            if (playerTransform != null)
            {
                float distanceToPlayer = Vector2.Distance(transform.position, playerTransform.position);
                if (distanceToPlayer <= detectionRange)
                {
                    ChangeState(ChasingState);
                    return;
                }
            }
        }

        base.Update();
    }

#if UNITY_EDITOR
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, losePlayerRange);
    }
#endif
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<TopDownPlayerController>() != null)
        {
            ChangeState(FleeingState);
            InvertPlayerControls();
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
}