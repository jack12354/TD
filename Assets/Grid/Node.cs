using UnityEngine;
using System.Collections;

public abstract class Node : MonoBehaviour
{
    public bool IsBuildable;
    public bool IsPassable;
    protected Actor Occupant;
    public Grid Parent { get; set; }

    [HideInInspector]
    public bool Flagged;

    public abstract void OnAdd();
    public abstract void OnRemove();
    public abstract void OnEnter(Actor inActor);

    protected Random r = new Random();

    //public static bool operator ==(Node node1, Node node2)
    //{
     //   Vector3 node1Pos = node1.Parent.GetNodeCoordinates(node1);
     //   Vector3 node2Pos = node2.Parent.GetNodeCoordinates(node2);
     //   return node1Pos == node2Pos;
   // }

//    public static bool operator !=(Node node1, Node node2)
  //  {
   //     return !(node1 == node2);
    //}
}
