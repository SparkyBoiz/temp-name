using Game.Player;
using UnityEngine;
using UnityEngine.AI;

public class IdleState : State
{
    private float _idleTimer;

    public override void Enter()
    {
        Debug.Log($"{ghost.name} entering IdleState");
        agent.ResetPath();
        _idleTimer = ghost.idleDuration;
    }

    private void Update()
    {
        _idleTimer -= Time.deltaTime;
        if (_idleTimer <= 0f)
        {
            ghost.ChangeState(ghost.PatrolState);
        }
    }
}

public class PatrolState : State
{
    private bool _pathfindingFailed = false;
    private int frameEntered = -1;

    public override void Enter()
    {
        _pathfindingFailed = false;
        frameEntered = Time.frameCount;
        Debug.Log($"{ghost.name} entering PatrolState");
        agent.isStopped = false;
        if (!ghost.FindNextWaypoint())
        {
            _pathfindingFailed = true;
            ghost.StartCoroutine(FailAndReturnToIdle());
        }
    }

    private System.Collections.IEnumerator FailAndReturnToIdle()
    {
        yield return new WaitForSeconds(1f);
        ghost.ChangeState(ghost.IdleState);
    }

    private void Update()
    {
        if (_pathfindingFailed) return;

        if (Time.frameCount <= frameEntered)
        {
            return;
        }

        if (agent.pathPending)
        {
            return;
        }

        if (agent.hasPath && agent.remainingDistance > agent.stoppingDistance)
        {
            float currentTime = Time.time;
            if (currentTime >= ghost.nextWalkSoundTime)
            {
                GameEvents.RequestSoundWord(SoundType.GhostWalk, transform.position, Vector3.right, 0.7f);
                ghost.nextWalkSoundTime = currentTime + 0.5f;
            }
            return;
        }

        if (!agent.hasPath)
        {
            Debug.LogWarning($"{ghost.name}: Patrol failed because agent has no path. Returning to Idle.");
        }
        else
        {
            Debug.Log($"{ghost.name}: Patrol complete. Reached destination (remaining distance: {agent.remainingDistance}). Returning to Idle.");
        }
        ghost.ChangeState(ghost.IdleState);
    }
}

public class TrappedState : State
        {
    private float _duration;

    public void SetDuration(float duration)
    {
        _duration = duration;
    }

    public override void Enter()
    {
        agent.isStopped = true;
        StartCoroutine(TrappedTimer());
    }

    private System.Collections.IEnumerator TrappedTimer()
    {
        yield return new WaitForSeconds(_duration);
        ghost.ChangeState(ghost.IdleState);
    }

    public override void Exit()
    {
        agent.isStopped = false;
    }
}

public class DyingState : State
{
    public override void Enter()
    {
        agent.isStopped = true;
        animator.SetTrigger("Die");
        TopDownPlayerController.Instance.ChargeBattery(ghost.batteryCharge);
        GameEvents.RequestSoundWord(SoundType.GhostDeath, transform.position, Vector3.up, 1.5f);
    }
}