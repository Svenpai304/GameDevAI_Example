using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class BTSelectHidingSpot : BTBaseNode
{
    private Player player;
    private Transform[] wayPoints;
    private float maxPlayerDistance;

    public BTSelectHidingSpot(Player player, Transform[] wayPoints, float maxPlayerDistance)
    {
        this.player = player;
        this.wayPoints = wayPoints;
        this.maxPlayerDistance = maxPlayerDistance;
    }

    protected override void OnEnter()
    {
        SetStatusUI("Selecting hiding spot");

        List<Transform> viable = new();
        foreach (Transform t in wayPoints)
        {
            if (t != null && !player.CheckPointInVision(t.position) && Vector3.Distance(player.transform.position, t.position) > maxPlayerDistance)
            {
                viable.Add(t);
            }
        }
        Vector3 final;
        if (viable.Count > 0)
        {
            final = viable[Random.Range(0, viable.Count)].position;
        }
        else
        {
            final = Vector3.zero;
        }
        Debug.Log("Selected hiding spot: " + final);
        blackboard.SetVariable(VariableNames.TARGET_POSITION, final);
    }

    protected override TaskStatus OnUpdate()
    {
        return TaskStatus.Success;
    }
}

public class BTKabouterMove : BTMoveToPosition
{
    Player player;
    public BTKabouterMove(NavMeshAgent agent, Player player, float moveSpeed, string BBtargetPosition, float keepDistance) : base(agent, moveSpeed, BBtargetPosition, keepDistance)
    {
        this.player = player;
    }

    protected override TaskStatus OnUpdate()
    {
        if (player.CheckPointInVision(agent.transform.position))
        {
            blackboard.SetVariable(VariableNames.IS_HIDDEN, false);
            blackboard.SetVariable(VariableNames.IN_VISION, true);
            agent.SetDestination(agent.transform.position);
            return TaskStatus.Success;
        }
        return base.OnUpdate();
    }
}

public class BTStayHidden : BTBaseNode
{
    private float timer;
    private float exhaustion;
    private float illusionSpawnTime;
    private float attackDistance;
    private Transform[] wayPoints;

    private GameObject illusionPrefab;
    private FakeKabouter illusionObject;
    private Player player;
    private NavMeshAgent agent;

    public BTStayHidden(NavMeshAgent agent, Player player, Transform[] wayPoints, float illusionSpawnTime, float attackDistance, GameObject illusionPrefab)
    {
        this.agent = agent;
        this.wayPoints = wayPoints;
        this.illusionSpawnTime = illusionSpawnTime;
        this.attackDistance = attackDistance;
        this.illusionPrefab = illusionPrefab;
        this.player = player;
    }

    protected override void OnEnter()
    {
        exhaustion = blackboard.GetVariable<float>(VariableNames.EXHAUSTION);
        SetStatusUI("Hidden");
    }
    protected override TaskStatus OnUpdate()
    {
        timer += Time.fixedDeltaTime;
        exhaustion = Mathf.Clamp(exhaustion - Time.fixedDeltaTime, 0, exhaustion);
        if (timer > illusionSpawnTime && illusionObject == null)
        {
            SpawnIllusion();
        }
        if (player.CheckPointInVision(agent.transform.position))
        {
            timer = 0;
            blackboard.SetVariable(VariableNames.IS_HIDDEN, false);
            blackboard.SetVariable(VariableNames.IN_VISION, true);
            return TaskStatus.Success;
        }
        Vector3 playerDist = player.transform.position - agent.transform.position;
        if (playerDist.magnitude < attackDistance)
        {
            if (Physics.Raycast(agent.transform.position, playerDist, playerDist.magnitude))
            {
                blackboard.SetVariable(VariableNames.CAN_ATTACK, true);
                return TaskStatus.Success;
            }
        }
        if (illusionObject != null && illusionObject.Found)
        {
            PlaySFX(SFXManager.SFXGroup.KabouterTrick);
            illusionObject.Die(true);
            illusionObject = null;
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }

    protected override void OnExit()
    {
        base.OnExit();
        timer = 0;
        blackboard.SetVariable(VariableNames.EXHAUSTION, exhaustion);
        if (illusionObject != null)
        {
            illusionObject.Die(false);
            illusionObject = null;
        }
    }

    private void SpawnIllusion()
    {
        List<Transform> viable = new();
        foreach (Transform t in wayPoints)
        {
            if (t != null && !player.CheckPointInVision(t.position) && t.position != blackboard.GetVariable<Vector3>(VariableNames.TARGET_POSITION))
            {
                viable.Add(t);
            }
        }
        Vector3 final;
        if (viable.Count > 0)
        {
            final = viable[Random.Range(0, viable.Count)].position;
        }
        else
        {
            final = wayPoints[0].position;
        }
        Debug.Log("Selected illusion spot: " + final);
        illusionObject = Object.Instantiate(illusionPrefab).GetComponent<FakeKabouter>();
        illusionObject.Setup(final, agent.transform.rotation);
    }
}

public class BTApproach : BTBaseNode
{
    private NavMeshAgent agent;
    private Player player;
    private float moveSpeed;
    private float hitDistance;

    public BTApproach(NavMeshAgent agent, Player player, float moveSpeed, float hitDistance)
    {
        this.agent = agent;
        this.player = player;
        this.moveSpeed = moveSpeed;
        this.hitDistance = hitDistance;
    }

    protected override void OnEnter()
    {
        base.OnEnter();
        SetStatusUI("Approaching");
        agent.speed = moveSpeed;
    }

    protected override TaskStatus OnUpdate()
    {
        agent.SetDestination(player.transform.position);
        if (player.CheckPointInVision(agent.transform.position))
        {
            blackboard.SetVariable(VariableNames.IS_HIDDEN, false);
            blackboard.SetVariable(VariableNames.IN_VISION, true);
            return TaskStatus.Success;
        }
        Vector3 playerDist = player.transform.position - agent.transform.position;
        if (playerDist.magnitude < hitDistance)
        {
            if (Physics.Raycast(agent.transform.position, playerDist, playerDist.magnitude))
            {
                PlaySFX(SFXManager.SFXGroup.KabouterAttack);
                GameManager.Instance.EndGame(false);
                blackboard.SetVariable(VariableNames.CAN_ATTACK, false);
                return TaskStatus.Success;
            }
        }
        return TaskStatus.Running;
    }
}

public class BTRunAway : BTBaseNode
{
    private float timer = 0;
    private float exhaustion;
    private NavMeshAgent agent;
    private Player player;
    private float waitTime;
    private float finishDistance;
    private float speed;
    private float tiredSpeed;
    private float tiredStartTime;
    private float tiredSpan;

    public BTRunAway(NavMeshAgent agent, Player player, float waitTime, float finishDistance, float speed, float tiredSpeed, float tiredStartTime, float tiredSpan)
    {
        this.agent = agent;
        this.player = player;
        this.waitTime = waitTime;
        this.finishDistance = finishDistance;
        this.speed = speed;
        this.tiredSpeed = tiredSpeed;
        this.tiredStartTime = tiredStartTime;
        this.tiredSpan = tiredSpan;
    }

    protected override void OnEnter()
    {
        exhaustion = blackboard.GetVariable<float>(VariableNames.EXHAUSTION);
        SetStatusUI("Running away");
        PlaySFX(SFXManager.SFXGroup.KabouterSpotted);
        agent.transform.LookAt(player.transform.position);
    }

    protected override TaskStatus OnUpdate()
    {
        timer += Time.fixedDeltaTime;
        exhaustion += Time.fixedDeltaTime;
        float tiredFactor = 0;
        if (exhaustion > tiredStartTime)
        {
            tiredFactor = Mathf.Clamp((exhaustion - tiredStartTime) / tiredSpan, 0, 1);
        }
        agent.speed = Mathf.Lerp(speed, tiredSpeed, tiredFactor);

        if (timer > waitTime)
        {
            Vector3 dir = (agent.transform.position - player.transform.position).normalized * 5;
            agent.SetDestination(agent.transform.position + dir);
        }
        if (Vector3.Distance(agent.transform.position, player.transform.position) > finishDistance)
        {
            blackboard.SetVariable(VariableNames.IS_HIDDEN, true);
            blackboard.SetVariable(VariableNames.IN_VISION, false);
            return TaskStatus.Success;
        }
        return TaskStatus.Running;
    }

    protected override void OnExit()
    {
        timer = 0f;
        blackboard.SetVariable(VariableNames.EXHAUSTION, exhaustion);
    }
}
