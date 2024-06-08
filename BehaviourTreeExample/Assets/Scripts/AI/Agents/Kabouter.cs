using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class Kabouter : MonoBehaviour
{
    public float moveSpeed = 3;
    public float runAwaySpeed;
    public float attackingMoveSpeed;
    public float keepDistance = 1f;
    public Transform[] wayPoints;
    public GameObject illusionPrefab;
    private BTBaseNode tree;
    private NavMeshAgent agent;
    private Animator animator;
    private AudioSource audioSource;
    public TMP_Text statusText;

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
        //Create your Behaviour Tree here!
        Blackboard blackboard = new Blackboard();
        blackboard.SetVariable(VariableNames.ENEMY_HEALTH, 1);
        blackboard.SetVariable(VariableNames.TARGET_POSITION, new Vector3(0, 0, 0));
        blackboard.SetVariable(VariableNames.CURRENT_PATROL_INDEX, -1);
        blackboard.SetVariable(VariableNames.IS_HIDDEN, true);
        blackboard.SetVariable(VariableNames.IN_VISION, false);
        blackboard.SetVariable(VariableNames.CAN_ATTACK, false);
        blackboard.SetVariable(VariableNames.STATUS_TEXT, statusText);
        blackboard.SetVariable(VariableNames.AUDIO_SOURCE, audioSource);

        tree =
            new BTConditionalSelector(
                new ConditionalNode(new BTApproach(agent, player, attackingMoveSpeed, 1), VariableNames.CAN_ATTACK),
                new ConditionalNode(
                    new BTSequence(
                        new BTSelectHidingSpot(player, wayPoints, 5),
                        new BTKabouterMove(agent, player, moveSpeed, VariableNames.TARGET_POSITION, keepDistance),
                        new BTStayHidden(agent, player, wayPoints, 5, 3, illusionPrefab)
                    ),
                VariableNames.IS_HIDDEN),
                new ConditionalNode(new BTRunAway(agent, player, 1, 12, runAwaySpeed), VariableNames.IN_VISION)
            );

        tree.SetupBlackboard(blackboard);
    }

    private void FixedUpdate()
    {
        TaskStatus result = tree.Tick();
    }

    private void Update()
    {
        bool isMoving = !(Mathf.Abs(agent.destination.x - transform.position.x) <= keepDistance && Mathf.Abs(agent.destination.z - transform.position.z) <= keepDistance);
        ChangeAnimation(isMoving ? "Run" : "Crouch Idle", isMoving ? 0.05f : 0.15f);
    }

    private void ChangeAnimation(string animationName, float fadeTime)
    {
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName(animationName) && !animator.IsInTransition(0))
        {
            animator.CrossFade(animationName, fadeTime);
        }
    }
}
