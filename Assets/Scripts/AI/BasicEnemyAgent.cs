using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Should alter this so we can return to the node that is currently running
public class BasicEnemyAgent : AIAgent
{
    public BasicEnemyAgent(Character c) : base(c){
        var children = new List<Node>();
        children.Add(new CoverIfLowDodgeSequence(this));
        children.Add(new ReloadSequence(this));
        children.Add(new AttackSequenceNode(this));
        rootNode = new SelectorNode(this, children);
    }
}

public class CoverIfLowDodgeSequence : SequenceNode{
    public CoverIfLowDodgeSequence(AIAgent agent, List<Node> childNodes = null) : base(agent, childNodes){
        this.agent = agent;
        children = new List<Node>();
        children.Add(new HasLowDodgeNode(agent));
        children.Add(new HunkerDownSequence(agent));
    }
}

public class HasLowDodgeNode : Node {
    public Character character;
    public float threshold;
    public HasLowDodgeNode(AIAgent agent, float threshold = 0.25f) : base(agent){
        character = agent.character;
        this.threshold = threshold;
    }

    public override NodeState Evaluate(){
        return (character.Dodge / character.data.dodgeMax ) < threshold ? NodeState.SUCCESS : NodeState.FAILURE; 
    }
}

public class FindCoverNode : Node { //Not written in a very modular way
    public FindCoverNode(AIAgent agent) : base(agent){
    }
    public override NodeState Evaluate(){
        //Get all tiles within range
        var options = new List<GridTile>();
        GridMap.OnVisit handler = (GridTile g)=>{
            if(g.visited <= agent.character.MovementRange + 1 && g.blocked){
                var adj = GridMap.map.GetAdjacent(g.x, g.y, (adjTile) => {
                    if(adjTile.Character != null) return false;
                    var path = GridMap.map.GetPath(adjTile.x, adjTile.y);
                    if(path == null || path.Count > agent.character.MovementRange) return false;
                    foreach(Character c in TurnManager.manager.playerParty){
                        if(GridMap.map.CheckLos(c.transform.position, adjTile.transform.position)) return false;
                    }
                    return true;
                });
                if(adj != null){
                    options.AddRange(adj);
                }
            }
        };
        GridMap.map.CalculateDistances(agent.character.x, agent.character.y, handler);
        if(options.Count < 1){
            return NodeState.FAILURE;
        } else if(options.Count == 1){
            object[] data = {options[0]};
            agent.SetValue("movementTarget", data);
            return NodeState.SUCCESS;
        } else {
            int random = Random.Range(0, options.Count);
            object[] data = {options[random]};
            agent.SetValue("movementTarget", data);
            return NodeState.SUCCESS;
        }
    }
}

public class HunkerDownSequence : SequenceNode{
        public HunkerDownSequence(AIAgent agent, List<Node> childNodes = null) : base(agent, childNodes){
        children = new List<Node>();
        children.Add(new FindCoverNode(agent));
        children.Add(new MoveActionNode(agent));
        children.Add(new HunkerActionNode(agent));
    }
}

//Set to move towards area if out of single movement range
public class MoveActionNode : Node {

    Character character;
    
    public MoveActionNode(AIAgent agent) : base(agent){
        this.character = agent.character;
    }
    public override NodeState Evaluate(){
        GridTile movementTarget = (GridTile) agent.GetValue("movementTarget")[0];
        if(movementTarget == null){
            return NodeState.FAILURE;
        }

        GridMap.map.CalculateDistances(character.x, character.y);
        var path = GridMap.map.GetPath(movementTarget.x, movementTarget.y);
        if(path == null) return NodeState.FAILURE;
        if(path.Count - 1 <= character.MovementRange){
            return character.MoveAction(movementTarget.x, movementTarget.y) ? NodeState.SUCCESS : NodeState.FAILURE;
        } else if(path != null){
            GridTile withinRangeTile = path[character.MovementRange];
            return character.MoveAction(withinRangeTile.x, withinRangeTile.y) ? NodeState.SUCCESS : NodeState.FAILURE;
        } else {
            return NodeState.FAILURE;
        }
    }
}

public class HunkerActionNode : Node {
    Character character;
    public HunkerActionNode(AIAgent agent) : base(agent){
        this.character = agent.character;
    }
    public override NodeState Evaluate(){
        if(!character.Action) return NodeState.FAILURE;
        character.Hunker();
        return NodeState.SUCCESS;
    }
}

public class ReloadSequence : SequenceNode {
        public ReloadSequence(AIAgent agent, List<Node> childNodes = null) : base(agent, childNodes){
        children = new List<Node>();
        children.Add(new HasLowAmmoNode(agent));
        children.Add(new ReloadActionNode(agent));

        var movementUsage = new List<Node>();
        movementUsage.Add(new DefensiveMovementSequence(agent));
        movementUsage.Add(new AggressiveMovementSequence(agent));

        children.Add(new SelectorNode(agent, movementUsage));
    }
}

public class HasLowAmmoNode : Node {
    Character character;
    int ammoMinimum;
    public HasLowAmmoNode(AIAgent agent, int ammoMinmium = 1) : base(agent){
        this.character = agent.character;

    }
    public override NodeState Evaluate(){
        return character.weapon.shotsCurrent < ammoMinimum ? NodeState.SUCCESS : NodeState.FAILURE;
    }
}

public class ReloadActionNode : Node {
    Character character;
    int ammoMinimum;
    public ReloadActionNode(AIAgent agent) : base(agent){
        this.character = agent.character;

    }
    public override NodeState Evaluate(){
        character.Reload();
        return NodeState.SUCCESS;
    }
}

public class DefensiveMovementSequence : SequenceNode {
    public DefensiveMovementSequence(AIAgent agent, List<Node> childNodes = null) : base(agent, childNodes){
        children = new List<Node>();
        children.Add(new HasLOSNode(agent));
        children.Add(new FindCoverNode(agent));
        children.Add(new MoveActionNode(agent));
    }
}

public class HasLOSNode : Node {
    Character character;
    public HasLOSNode(AIAgent agent) : base(agent){
        this.character = agent.character;

    }
    public override NodeState Evaluate(){
        foreach(Character enemy in TurnManager.manager.playerParty){
            if(GridMap.map.CheckLos(enemy.transform.position, character.transform.position)) return NodeState.SUCCESS;
        }
        return NodeState.FAILURE;
    }
}

public class AggressiveMovementSequence : SequenceNode {
    public AggressiveMovementSequence(AIAgent agent, List<Node> childNodes = null) : base(agent, childNodes){
        children = new List<Node>();
        children.Add(new FindClosestEnemyNode(agent));
        children.Add(new MoveActionNode(agent));
    }

    public override NodeState Evaluate(){
        MonoBehaviour.print("Aggressive movement Sequence");
        return base.Evaluate();
    }
}


public class FindClosestEnemyNode : Node {
    
    public FindClosestEnemyNode(AIAgent agent) : base(agent){
    }

    public override NodeState Evaluate(){
        if(TurnManager.manager.playerParty.Count == 0) return NodeState.FAILURE;
        List<GridTile> possibilities = new List<GridTile>();
        foreach(Character c in TurnManager.manager.playerParty){
            var list = GridMap.map.GetAdjacent(c.x, c.y, (g)=>{ return g.Character == null; });
            if(list != null){
                possibilities.AddRange(list);
            }
        }
        if(possibilities.Count == 0) return NodeState.FAILURE;
        List<GridTile> path = GridMap.map.GetPath(possibilities[0].x, possibilities[0].y);
        for(int i = 1; i < possibilities.Count; i++){
            var otherPath = GridMap.map.GetPath(possibilities[i].x, possibilities[i].y);
            if(otherPath != null && path.Count > otherPath.Count){
                path = otherPath;
            }
        }
        if(path == null) return NodeState.FAILURE;
        object[] data = {path[path.Count - 1]};
        agent.SetValue("movementTarget", data);
        return NodeState.SUCCESS;
    }
}

public class AttackSequenceNode : SequenceNode {

    public AttackSequenceNode(AIAgent agent, List<Node> childNodes = null) : base(agent, childNodes){
        children = new List<Node>();
        var canAttackSelectorNode = new List<Node>();
        canAttackSelectorNode.Add(new AttackThenMoveSequence(agent));
        canAttackSelectorNode.Add(new MoveThenAttackSequence(agent));
        children.Add(new SelectorNode(agent, canAttackSelectorNode));
        children.Add(new MoveAfterAttackSequence(agent));
    }

}

public class AttackThenMoveSequence : SequenceNode {
    public AttackThenMoveSequence(AIAgent agent, List<Node> childNodes = null) : base(agent, childNodes){
        children = new List<Node>();
        children.Add(new HasLOSNode(agent));
        children.Add(new AttackRandomNode(agent));
    }

}

public class AttackRandomNode : Node {
    public AttackRandomNode(AIAgent agent) : base(agent){
    }

    public override NodeState Evaluate(){
        if(_started && agent.character.Action == false){
            return NodeState.SUCCESS;
        } else if (_started){
            return NodeState.RUNNING;
        } else {
            Init();
        }
        var targets = new List<Character>();
        foreach(Character enemy in TurnManager.manager.playerParty){
            if(GridMap.map.CheckLos(enemy.transform.position, agent.character.transform.position)){
                targets.Add(enemy);
            }
        }
        if(targets.Count == 0) return NodeState.FAILURE;
        var target = targets[Random.Range(0, targets.Count)];
        Character.AttackAppliedHandler attackHandler = null;
        attackHandler = (a, dodged)=>{
            agent.character.Action = false;
            target.onAttackApplied -= attackHandler;
        };
        target.onAttackApplied += attackHandler;
        agent.character.Attack(target);
        return NodeState.RUNNING;
    }
}

public class MoveThenAttackSequence : SequenceNode {
        public MoveThenAttackSequence(AIAgent agent, List<Node> childNodes = null) : base(agent, childNodes){
        children = new List<Node>();
        children.Add(new FindClosestEnemyNode(agent));
        children.Add(new MoveActionNode(agent));
        children.Add(new HasLOSNode(agent));
        children.Add(new AttackRandomNode(agent));
    }

}

public class MoveAfterAttackSequence : SequenceNode {
        public MoveAfterAttackSequence(AIAgent agent, List<Node> childNodes = null) : base(agent, childNodes){
        children = new List<Node>();
        children.Add(new HasMovementNode(agent));
        var movementUsage = new List<Node>();
        movementUsage.Add(new DefensiveMovementSequence(agent));
        movementUsage.Add(new AggressiveMovementSequence(agent));

        children.Add(new SelectorNode(agent, movementUsage));
    }

    public override NodeState Evaluate(){
        MonoBehaviour.print("Move After attack Sequence");
        return base.Evaluate();
    }

}

public class HasMovementNode : Node {
    public Character character;
    public int threshold;
    public HasMovementNode(AIAgent agent, int threshold = 0) : base(agent){
        this.character = agent.character;
        this.threshold = threshold;
    }

    public override NodeState Evaluate(){
        return (character.MovementRange > threshold) ? NodeState.SUCCESS : NodeState.FAILURE;
    }
}

