using Game.Player;
using UnityEngine;
using UnityEngine.AI;

public class ChasingState : State
{
    private ChasingGhost _chasingGhost;

    public override void Initialize(Ghost ghost, NavMeshAgent agent, Animator animator)
    {
        base.Initialize(ghost, agent, animator);
        _chasingGhost = ghost as ChasingGhost;
    }

    public override void Enter()
    {
        agent.speed = _chasingGhost.chaseSpeed;
        GameEvents.RequestSoundWord(SoundType.GhostWalk, transform.position, Vector3.up, 1.2f);
    }

    private void Update()
    {
        if (agent == null || !_chasingGhost.playerTransform) return;

        float distanceToPlayer = Vector2.Distance(transform.position, _chasingGhost.playerTransform.position);
        if (distanceToPlayer > _chasingGhost.losePlayerRange)
        {
            ghost.ChangeState(ghost.PatrolState);
            return;
        }

        agent.SetDestination(_chasingGhost.playerTransform.position);

        float currentTime = Time.time;
        if (currentTime >= ghost.nextWalkSoundTime)
        {
            GameEvents.RequestSoundWord(SoundType.GhostWalk, transform.position, Vector3.right, 1f);
            ghost.nextWalkSoundTime = currentTime + 0.4f;
        }
    }

    public override void Exit()
    {
        agent.speed = _chasingGhost.originalSpeed;
    }
}

public class FleeingState : State
{
    private ChasingGhost _chasingGhost;

    public override void Initialize(Ghost ghost, NavMeshAgent agent, Animator animator)
    {
        base.Initialize(ghost, agent, animator);
        _chasingGhost = ghost as ChasingGhost;
    }

    public override void Enter()
    {
        agent.speed = _chasingGhost.fleeSpeed;
        StartCoroutine(FleeingTimer());
    }

    private void Update()
    {
        if (agent == null || !_chasingGhost.playerTransform) return;

        Vector2 fleeDirection = (Vector2)transform.position - (Vector2)_chasingGhost.playerTransform.position;

        if (fleeDirection.magnitude < _chasingGhost.minFleeDistance)
        {
            Vector2 targetPosition = (Vector2)transform.position + fleeDirection.normalized * _chasingGhost.minFleeDistance;

            if (NavMesh.SamplePosition(targetPosition, out var navHit, _chasingGhost.minFleeDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(navHit.position);
            }
        }

        float currentTime = Time.time;
        if (currentTime >= ghost.nextWalkSoundTime)
        {
            GameEvents.RequestSoundWord(SoundType.GhostWalk, transform.position, Vector3.right, 1.2f);
            ghost.nextWalkSoundTime = currentTime + 0.3f;
        }
    }

    private System.Collections.IEnumerator FleeingTimer()
    {
        yield return new WaitForSeconds(_chasingGhost.fleeDuration);
        ghost.ChangeState(ghost.IdleState);
    }

    public override void Exit()
    {
        agent.speed = _chasingGhost.originalSpeed;
    }
}