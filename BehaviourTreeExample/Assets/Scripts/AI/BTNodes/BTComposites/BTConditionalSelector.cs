///
/// The Conditional Selector uses the first of its children whose assigned value from the blackboard returns true.
///
public class BTConditionalSelector : BTBaseNode
{
    private ConditionalNode[] children;
    private BTBaseNode selected;

    public BTConditionalSelector(params ConditionalNode[] children)
    {
        this.children = children;
    }

    protected override void OnEnter()
    {
        base.OnEnter();
        selected = null;
        for (int i = 0; i < children.Length; i++)
        {
            if (blackboard.GetVariable<bool>(children[i].condition))
            {
                selected = children[i].node;
                break;
            }
        }
    }

    protected override TaskStatus OnUpdate()
    {
        if(selected == null) { return TaskStatus.Failed; }

        return selected.Tick();
    }

    public override void SetupBlackboard(Blackboard blackboard)
    {
        base.SetupBlackboard(blackboard);
        foreach(var child in children)
        {
            child.node.SetupBlackboard(blackboard);
        }
    }

}

public class ConditionalNode
{
    public BTBaseNode node;
    public string condition;

    public ConditionalNode(BTBaseNode node, string condition)
    {
        this.node = node;
        this.condition = condition;
    }
}
