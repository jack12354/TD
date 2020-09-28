using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Building : Actor
{
    private List<Node> Placement = new List<Node>();
    public int XDimension = 1, YDimension = 1;
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
        if (!CanBuildAtLocation(inGrid, inAtNode, XDimension, YDimension))
            return;

        WorldGrid = inGrid;

        var footprint = GetFootprintNodes(inGrid, inAtNode, XDimension, YDimension);

        Placement.AddRange(footprint);

        transform.position = inAtNode.transform.position;
        transform.position += new Vector3(0, 0.5f, 0);
      
        //Placement.AddRange(WorldGrid.GetNeighbors(inAtNode, true));
        foreach (var node in Placement)
        {
            node.IsPassable = false;
            node.IsBuildable = false;
        }
    }

    public static bool CanBuildAtLocation(Grid inGrid, Node inAtNode, int XDimension, int YDimension)
    {
        var footprint = GetFootprintNodes(inGrid, inAtNode, XDimension, YDimension);
        if (footprint.Count != XDimension * YDimension || footprint.Any(node => node == null || !node.IsBuildable))
        {
            return false;
        }
        return true;
    }

    public static List<Node> GetFootprintNodes(Grid inGrid, Node inAtNode, int XDimension, int YDimension)
    {
        var footprint = new List<Node>();
        footprint.Add(inAtNode);
        footprint.AddRange(inGrid.GetNeighbors(inAtNode, Direction.South, XDimension - 1));
        var squareWidth = new List<Node>();
        foreach (var node in footprint)
        {
            squareWidth.AddRange(inGrid.GetNeighbors(node, Direction.East, YDimension - 1));
        }

        footprint.AddRange(squareWidth);

        return footprint;
    }

    public void OnAdd(Grid inGrid, List<Node> inAtNodes)
    {
        WorldGrid = inGrid;

        Placement.AddRange(inAtNodes);
        transform.position = inAtNodes[0].transform.position;
        transform.position += new Vector3(0, 0.5f, 0);
        HashSet<Node> neighborSet = new HashSet<Node>();
        foreach (var node in inAtNodes)
        {
            foreach(var neighborNode in WorldGrid.GetNeighbors(node, true))
            {
                neighborSet.Add(neighborNode);
            }
        }
        Placement.AddRange(neighborSet);

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
