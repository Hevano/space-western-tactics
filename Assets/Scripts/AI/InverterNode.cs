using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InverterNode : Node
{
    public Node child;
    public InverterNode(AIAgent agent, Node child) : base(agent){
        this.child = child;
    }

    public override NodeState Evaluate(){
        if(_started  && NodeState != NodeState.RUNNING){
            return NodeState;
        } else if(!_started) {
            Init();
        }
        switch(child.Evaluate()){
            case NodeState.SUCCESS:
                _nodeState = NodeState.FAILURE;
                break;
            case NodeState.FAILURE:
                _nodeState = NodeState.SUCCESS;
                break;
            case NodeState.RUNNING:
                _nodeState = NodeState.RUNNING;
                break;
        }
        return _nodeState;
    }
}
