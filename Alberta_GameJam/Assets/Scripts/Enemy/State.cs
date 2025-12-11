using UnityEngine;
using UnityEngine.AI;

public abstract class State : MonoBehaviour
{
    protected Ghost ghost;
    protected NavMeshAgent agent;
    protected Animator animator;

    public virtual void Initialize(Ghost ghost, NavMeshAgent agent, Animator animator)
    {
        this.ghost = ghost;
        this.agent = agent;
        this.animator = animator;
        enabled = false;
    }

    public virtual void Enter() { }
    public virtual void Exit() { }
}