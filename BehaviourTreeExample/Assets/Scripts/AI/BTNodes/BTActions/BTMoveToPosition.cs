using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BTMoveToPosition : BTBaseNode
{
    protected NavMeshAgent agent;
    protected float moveSpeed;
    protected float keepDistance;
    protected Vector3 targetPosition;
    protected string BBtargetPosition;

    public BTMoveToPosition(NavMeshAgent agent, float moveSpeed, string BBtargetPosition, float keepDistance)
    {
        this.agent = agent;
        this.moveSpeed = moveSpeed;
        this.BBtargetPosition = BBtargetPosition;
        this.keepDistance = keepDistance;
    }

    protected override void OnEnter()
    {
        SetStatusUI("Moving");
        agent.speed = moveSpeed;
        agent.stoppingDistance = keepDistance;
        targetPosition = blackboard.GetVariable<Vector3>(BBtargetPosition);
    }

    protected override TaskStatus OnUpdate()
    {
        if (agent == null) { return TaskStatus.Failed; }
        if (agent.pathPending) { return TaskStatus.Running; }
        if (agent.hasPath && agent.path.status == NavMeshPathStatus.PathInvalid) { return TaskStatus.Failed; }
        if (agent.pathEndPosition != targetPosition)
        {
            agent.SetDestination(targetPosition);
        }

        if(Mathf.Abs(agent.transform.position.x - targetPosition.x) <= keepDistance && Mathf.Abs(agent.transform.position.z - targetPosition.z) <= keepDistance)
        {
            return TaskStatus.Success;
        }
        return TaskStatus.Running;

    }
}

public class BTGetNextPatrolPosition : BTBaseNode
{
    private Transform[] wayPoints;
    public BTGetNextPatrolPosition(Transform[] wayPoints) 
    {
        this.wayPoints = wayPoints;
    }

    protected override void OnEnter()
    {
        int currentIndex = blackboard.GetVariable<int>(VariableNames.CURRENT_PATROL_INDEX);
        currentIndex++;
        if(currentIndex >= wayPoints.Length)
        {
            currentIndex = 0;
        }
        blackboard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, currentIndex);
        blackboard.SetVariable(VariableNames.TARGET_POSITION, wayPoints[currentIndex].position);
    }

    protected override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}
