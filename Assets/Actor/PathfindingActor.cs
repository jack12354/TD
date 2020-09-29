using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using PathPair = System.Collections.Generic.KeyValuePair<System.Collections.Generic.List<Node>, Node>;
using PriorityQueuePair = System.Collections.Generic.KeyValuePair<int, int>;

public class PathfindingActor : Actor
{
    public int Wiggliness = 6;
    private List<Node> pathToGoal;
    private Vector3 randomOffset;

    public void Start()
    {
        float randomAngle = (float)(r.NextDouble() * 360);
        randomOffset = new Vector3(Mathf.Sin(randomAngle) / 4.0f, 0.18f, Mathf.Cos(randomAngle) / 4.0f);
        //  randomOffset = new Vector3((float)((r.NextDouble() * 0.5f) - 0.25f), (float)((r.NextDouble() * 0.5f) - 0.25f));
        Node startNode = WorldGrid.GetClosestNodeFromPosition(transform.position, true);
        pathToGoal = FindPathToGoalNode(startNode, WorldGrid.GetClosestGoalNode(startNode));
        nodeIndex = 0;
        lerp = Vector3.Lerp(Vector3.zero, pathToGoal[nodeIndex].transform.position + randomOffset - transform.position,
            0.05f);
    }

    private List<Node> FindPathToGoalNode(Node inStartNode, Node inGoalNode)
    {
        int expands = 0;
        HashSet<Node> closedList = new HashSet<Node>();
        List<KeyValuePair<float, PathPair>> openList = new List<KeyValuePair<float, PathPair>>();
        List<Node> tempList = new List<Node>();
        openList.Add(
            new KeyValuePair<float, PathPair>(
                Heuristic(inStartNode, inGoalNode),
                new PathPair(new List<Node> { inStartNode }, inStartNode)));

        while (openList.Count > 0 && expands < 1000)
        {
            float minNodeValue = openList.Min(n => n.Key);

            var currentNodePair = openList.FirstOrDefault(node => Math.Abs(node.Key - minNodeValue) < 0.001f);
            var currentNodePathPair = currentNodePair.Value;
            var currentPath = currentNodePathPair.Key;
            var currentNode = currentNodePathPair.Value;

            openList.Remove(currentNodePair);

            if (currentNode.GetType() == typeof(GoalNode))
            {
                currentPath.Add(currentNode);
                return currentPath;
            }

            tempList = WorldGrid.GetNeighbors(currentNode);
            foreach (var node in tempList)
            {
                node.Flagged = true;
            }

            tempList.Remove(currentPath.Last());
            tempList.RemoveAll(node => !node.IsPassable);

            if (!closedList.Contains(currentNode))
                closedList.Add(currentNode);

            foreach (var node in tempList)
            {
                expands++;
                if (node.IsPassable && !closedList.Contains(node))
                {
                    List<Node> newList = currentPath.ToList();
                    
                    newList.Add(currentNode);
                    openList.Add(
                        new KeyValuePair<float, PathPair>(
                            Heuristic(node, inGoalNode),// + newList.Count,
                            new PathPair(newList, node)));
                }
            }
        }
        Debug.Log("aw crud");
        Die();
        return new List<Node> { inStartNode };
    }

    private float Heuristic(Node inNode, Node inGoalNode)
    {
        if (inNode.GetType() == typeof(DeathNode))
            return 9999;
            //return (float)Math.Pow((inNode.transform.position - WorldGrid.GetClosestGoalNode(inNode).transform.position).magnitude, 2.0f);
        // return Vector3.Distance(inNode.transform.position, inGoalNode.transform.position);
        return (inNode.transform.position - WorldGrid.GetClosestGoalNode(inNode).transform.position).magnitude + r.Next(Wiggliness);
//        return (inNode.transform.position - inGoalNode.transform.position).magnitude + r.Next(6); // + r.Next(6) to add wigglyness)
    }

    private int nodeIndex = 0;
    private Vector3 lerp = Vector3.zero;
    void Update()
    {
        Vector3 nextPosition = transform.position + lerp;

        Node nextNode = WorldGrid.GetClosestNodeFromPosition(nextPosition);
        Node prevNode = WorldGrid.GetClosestNodeFromPosition(transform.position);

        if (nextNode != prevNode)
        {
            nextNode.OnEnter(this);
            if(!nextNode.IsPassable)
            {
                Start();
            }
        }


        if (pathToGoal[nodeIndex] == null || pathToGoal.Count > nodeIndex+1 && pathToGoal[nodeIndex+1] == null)
        {
            Start();
        }
        Vector3 directionToTarget = (pathToGoal[nodeIndex].transform.position + randomOffset) - transform.position;
        float dSqrToTarget = directionToTarget.sqrMagnitude;
        if (dSqrToTarget < 0.25f)
        {
            nodeIndex++;
            if (nodeIndex == pathToGoal.Count)
            {
                Win();
            }
            else
            {
                lerp = Vector3.Lerp(Vector3.zero, pathToGoal[nodeIndex].transform.position + randomOffset - transform.position, 0.05f);
            }
        }

        transform.position = nextPosition;
    }

    public void PathfindIfNecessary(Node inChangedNode)
    {
        if (pathToGoal.Contains(inChangedNode))
        {
            Start();
        }
    }

    public override void OnAdd(Grid inGrid, Node inAtNode)
    {
        WorldGrid = inGrid;
    }

    public override void OnRemove()
    {
        Destroy(gameObject);
    }

    public override void Win()
    {
        Destroy(Instantiate(Resources.Load("WinEffect"), transform.position, Quaternion.Euler(0, -180, -180)), 0.5f);
        WorldGrid.RemoveActor(this);
    }

    public override void Die()
    {
        Destroy(Instantiate(Resources.Load("DeathEffect"), transform.position, Quaternion.Euler(0, -180, -180)), 0.5f);
        WorldGrid.RemoveActor(this);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (pathToGoal[nodeIndex] != null)
        Gizmos.DrawLine(transform.position, pathToGoal[nodeIndex].transform.position + randomOffset);

        Gizmos.color = Color.green;
        //int percent = (int)((1.0f/Grid.PLANES) * 255.0f);
        for (int iter = nodeIndex; iter < pathToGoal.Count - 1; iter++)
        {
            //float colorVal = pathToGoal[iter].Parent.GetNodeCoordinates(pathToGoal[iter]).z * percent;
            //Gizmos.color = new Color(colorVal, colorVal, colorVal);
            if(pathToGoal[iter] != null && pathToGoal[iter+1] != null)
                Gizmos.DrawLine(pathToGoal[iter].transform.position + randomOffset, pathToGoal[iter + 1].transform.position + randomOffset);
        }
    }
}