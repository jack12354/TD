using UnityEngine;
using Random = System.Random;

public class EmptyNode : Node
{
    public override void OnAdd()
    {
        IsBuildable = true;
        IsPassable = true;
    }

    public override void OnRemove()
    {
        Debug.LogWarning("You shouldn't be removing EmptyNodes from the grid, but feel free to carry on");
    }

    public override void OnEnter(Actor inActor)
    {
        // Nothing interesting happens...
    }
}