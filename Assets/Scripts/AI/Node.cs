using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Node
{
    protected NodeState _nodeState;
    public AIAgent agent;
    protected bool _started = false;
    public NodeState NodeState { get{ return _nodeState; } }
    public abstract NodeState Evaluate();

    public Node(AIAgent agent){
        this.agent = agent;
        agent.nodes.Add(this);
    }

    public void Init(){
        _started = true;
    }

    public void End(){
        _started = false;
    }
}

public enum NodeState {
    SUCCESS, FAILURE, RUNNING,
}
