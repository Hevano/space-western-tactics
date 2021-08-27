using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SequenceNode : Node
{
    public List<Node> children = new List<Node>();

    public SequenceNode(AIAgent agent, List<Node> childNodes) : base(agent) {
        children = childNodes;
    }

    public override NodeState Evaluate(){

        if(_started && NodeState == NodeState.RUNNING){
            foreach(Node child in children){
                switch(child.NodeState){
                    default:
                        break;
                    case NodeState.RUNNING:
                        for(int index = children.IndexOf(child); index < children.Count; index++){
                            switch(children[index].Evaluate()){
                                case NodeState.SUCCESS:
                                    break;
                                case NodeState.FAILURE:
                                    _nodeState = NodeState.FAILURE;
                                    return _nodeState;
                                case NodeState.RUNNING:
                                    _nodeState = NodeState.RUNNING;
                                    return _nodeState;
                            }
                        }
                        _nodeState = NodeState.SUCCESS;
                        return _nodeState;
                }
            }
        } else if(_started){
            return NodeState;
        } else {
            Init();
        }

        foreach(Node child in children){
            switch(child.Evaluate()){
                case NodeState.SUCCESS:
                    break;
                case NodeState.FAILURE:
                    _nodeState = NodeState.FAILURE;
                    return _nodeState;
                case NodeState.RUNNING:
                    _nodeState = NodeState.RUNNING;
                    return _nodeState;
            }
        }
        _nodeState = NodeState.SUCCESS;
        return _nodeState;
    }
}
