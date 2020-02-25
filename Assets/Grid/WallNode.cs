using UnityEngine;
using Random = System.Random;

public class WallNode : Node
{
    public override void OnAdd()
    {
        IsBuildable = false;
        IsPassable = false;
    }

    public override void OnRemove()
    {
        Destroy(gameObject);
    }

    public override void OnEnter(Actor inActor)
    {
        // Shouldn't happen
    }
}