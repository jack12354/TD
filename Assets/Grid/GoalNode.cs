using UnityEngine;
using Random = System.Random;

public class GoalNode : Node
{
    public override void OnAdd()
    {
        IsBuildable = false;
        IsPassable = true;
    }

    public override void OnRemove()
    {
        Destroy(gameObject);
    }

    public override void OnEnter(Actor inActor)
    {
        // Points
    }
}