using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.AI;

public class Kabouter : MonoBehaviour
{
    public float moveSpeed = 3;
    public float runAwaySpeed;
    public float attackingMoveSpeed;
    public float targetKeepDistance = 1f;
    public float tiredSpeed, tiredStartTime, tiredSpan;
    public Transform wayPointParent;
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
                new ConditionalNode(new BTApproach(agent, player, attackingMoveSpeed, 1), VariableNames.CAN_ATTACK),
                new ConditionalNode(
                    new BTSequence(
                        new BTSelectHidingSpot(player, wayPoints.ToArray(), 12),
                        new BTKabouterMove(agent, player, moveSpeed, VariableNames.TARGET_POSITION, targetKeepDistance),
                        new BTStayHidden(agent, player, wayPoints.ToArray(), 5, 3, illusionPrefab)
                    ), VariableNames.IS_HIDDEN),
                new ConditionalNode(new BTRunAway(agent, player, 1, 12, runAwaySpeed, tiredSpeed, tiredStartTime, tiredSpan), VariableNames.IN_VISION)
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
        if(p != null) { GameManager.Instance.EndGame(true); }
    }
}
