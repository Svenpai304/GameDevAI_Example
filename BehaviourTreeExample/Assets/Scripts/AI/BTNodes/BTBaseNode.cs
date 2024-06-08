using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum TaskStatus { Success, Failed, Running }
public abstract class BTBaseNode
{
    //Members
    protected Blackboard blackboard;
    private bool wasEntered = false;

    //Public Methods
    public virtual void OnReset() { }

    public TaskStatus Tick()
    {
        if (!wasEntered)
        {
            OnEnter();
            wasEntered = true;
        }

        var result = OnUpdate();
        if(result != TaskStatus.Running)
        {
            OnExit();
            wasEntered = false;
        }
        return result;
    }

    public virtual void SetupBlackboard(Blackboard blackboard)
    {
        this.blackboard = blackboard;
    }

    //Protected Methods
    protected abstract TaskStatus OnUpdate();
    protected virtual void OnEnter() { }
    protected virtual void OnExit() { }

    protected void SetStatusUI(string text)
    {
        TMP_Text status = blackboard.GetVariable<TMP_Text>(VariableNames.STATUS_TEXT);
        if (status != null) { status.text = text; }
    }

    protected void PlaySFX(SFXManager.SFXGroup group)
    {
        AudioSource src = blackboard.GetVariable<AudioSource>(VariableNames.AUDIO_SOURCE);
        if(src == null) { return; }
        src.Stop();
        src.clip = SFXManager.Instance.GetRandomSFX(group);
        src.Play();
    }
}

public abstract class BTComposite : BTBaseNode
{
    protected BTBaseNode[] children;
    public BTComposite (params BTBaseNode[] children)
    {
        this.children = children;
    }

    public override void SetupBlackboard(Blackboard blackboard)
    {
        base.SetupBlackboard(blackboard);
        foreach(BTBaseNode node in children)
        {
            node.SetupBlackboard(blackboard);
        }
    }
}

public abstract class BTDecorator : BTBaseNode
{
    protected BTBaseNode child;
    public BTDecorator(BTBaseNode child)
    {
        this.child = child;
    }

    public override void SetupBlackboard(Blackboard blackboard)
    {
        base.SetupBlackboard(blackboard);
        child.SetupBlackboard(blackboard);
    }
}
