using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Building : Actor
{
    private List<Node> Placement = new List<Node>();
    // Use this for initialization
    void Start ()
    {
        Type = ActorType.Building;
    }
    
    // Update is called once per frame
    void Update () {
    
    }

    public override void OnAdd(Grid inGrid, Node inAtNode)
    {
        WorldGrid = inGrid;
        
        Placement.Add(inAtNode);
        transform.position = inAtNode.transform.position;
        transform.position += new Vector3(0, 0.5f, 0);
        //Placement.AddRange(WorldGrid.GetNeighbors(inAtNode));
        foreach (var node in Placement)
        {
            node.IsPassable = false;
            node.IsBuildable = false;
        }
    }

    public override void OnRemove()
    {
        foreach (var node in Placement)
        {
            node.IsPassable = true;
            node.IsBuildable = true;
        }
        Destroy(gameObject);
    }

    public override void Die()
    {
        
    }

    public override void Win()
    {

    }
}
