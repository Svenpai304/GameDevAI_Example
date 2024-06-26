﻿using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.AI;

public class Kabouter : MonoBehaviour
{
    [Header("Stats")]
    public float moveSpeed = 3;
    public float hidePlayerDistance = 12;
    public float illusionSpawnTime, attackDistance, attackHitDistance;
    public float spottedStunTime;
    public float runAwayDistance = 10;
    public float runAwaySpeed = 2;
    public float attackingMoveSpeed = 3;
    public float targetKeepDistance = 1f;
    public float tiredSpeed, tiredStartTime, tiredSpan;

    [Header("References")]
    public Transform wayPointParent;
    public GameObject illusionPrefab;
    public TMP_Text statusText;
    private BTBaseNode tree;
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    private Player player;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponentInChildren<Animator>();
        player = FindObjectOfType<Player>();
        audioSource = GetComponent<AudioSource>();
    }

    private void Start()
    {
        List<Transform> wayPoints = wayPointParent.GetComponentsInChildren<Transform>().ToList();
        wayPoints.Remove(wayPointParent);

        //Create your Behaviour Tree here!
        Blackboard blackboard = new Blackboard();
        blackboard.SetVariable(VariableNames.ENEMY_HEALTH, 1);
        blackboard.SetVariable(VariableNames.TARGET_POSITION, new Vector3(0, 0, 0));
        blackboard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, -1);
        blackboard.SetVariable(VariableNames.EXHAUSTION, 0f);
        blackboard.SetVariable(VariableNames.IS_HIDDEN, true);
        blackboard.SetVariable(VariableNames.IN_VISION, false);
        blackboard.SetVariable(VariableNames.CAN_ATTACK, false);
        blackboard.SetVariable(VariableNames.STATUS_TEXT, statusText);
        blackboard.SetVariable(VariableNames.AUDIO_SOURCE, audioSource);

        tree =
            new BTConditionalSelector(
                new ConditionalNode(
                    new BTApproach(
                        agent,
                        player,
                        attackingMoveSpeed,
                        attackHitDistance
                        ),
                    VariableNames.CAN_ATTACK),
                new ConditionalNode(
                    new BTSequence(
                        new BTSelectHidingSpot(
                            player,
                            wayPoints.ToArray(),
                            hidePlayerDistance),
                        new BTKabouterMove(
                            agent,
                            player,
                            moveSpeed,
                            VariableNames.TARGET_POSITION,
                            targetKeepDistance),
                        new BTStayHidden(
                            agent,
                            player,
                            wayPoints.ToArray(),
                            illusionSpawnTime,
                            attackDistance,
                            illusionPrefab)
                        ),
                    VariableNames.IS_HIDDEN),
                new ConditionalNode(
                    new BTRunAway(
                        agent,
                        player,
                        spottedStunTime,
                        runAwayDistance,
                        runAwaySpeed,
                        tiredSpeed,
                        tiredStartTime,
                        tiredSpan),
                    VariableNames.IN_VISION)
            );

        tree.SetupBlackboard(blackboard);
    }

    private void FixedUpdate()
    {
        TaskStatus result = tree.Tick();
    }

    private void Update()
    {
        bool isMoving = !(Mathf.Abs(agent.destination.x - transform.position.x) <= targetKeepDistance && Mathf.Abs(agent.destination.z - transform.position.z) <= targetKeepDistance);
        ChangeAnimation(isMoving ? "Run" : "Crouch Idle", isMoving ? 0.05f : 0.15f);
    }

    private void ChangeAnimation(string animationName, float fadeTime)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) && !animator.IsInTransition(0))
        {
            animator.CrossFade(animationName, fadeTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Player p = other.GetComponent<Player>();
        if (p != null)
        {
            audioSource.Stop();
            audioSource.clip = SFXManager.Instance.GetRandomSFX(SFXManager.SFXGroup.KabouterDefeat);
            audioSource.Play();
            GameManager.Instance.EndGame(true);
        }
    }
}
