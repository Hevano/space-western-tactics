using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//How do we return to the currently executing node
/*
- Each node contains an init and end method *
- Each node contains a reference to it's AIAgent *
- AIAgent contains a list of it's nodes *
- If a node is evaluated, and it is already init, change the logic based on the node*
- If the tree returns running, wait 1 second and run it again, otherwise reset it
*/
[System.Serializable]
public abstract class AIAgent
{
    public Node rootNode;
    public List<Node> nodes;
    public Character character;

    private Dictionary<string, object[]> blackboard;

    public AIAgent(Character character){
        this.character = character;
        blackboard = new Dictionary<string, object[]>();
        nodes = new List<Node>();
    }

    public NodeState Evaluate(){
        var status = rootNode.Evaluate();
        if(status != NodeState.RUNNING){
            Reset();
        }
        return status;
    }

    public void Reset(){
        foreach(Node n in nodes){
            n.End();
        }
    }

    public void SetValue(string key, object[] data){
        if(blackboard.ContainsKey(key)){
            blackboard[key] = data;
        } else {
            blackboard.Add(key, data);
        }
    }

    public object[] GetValue(string key){
        if(blackboard.ContainsKey(key)){
            return blackboard[key];
        } else {
            return null;
        }
    }

    public bool UpdateValue(string key, object[] data){
        if(blackboard.ContainsKey(key)){
            blackboard[key] = data;
            return true;
        } else {
            return false;
        }
    }
}
