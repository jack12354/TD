using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Grid : MonoBehaviour
{
    public static int ROWS;
    public static int COLUMNS;
    public const float GRID_SIZE = 1.0f;
    private Node[,] mGrid;

    private List<Actor> mActors = new List<Actor>();

    private readonly Dictionary<char, string> charToNodeMap = new Dictionary<char, string>
    {
        {'.', "EmptyNode"},
        {'s', "SpawnNode"},
        {'w', "WallNode"},
        {'g', "GoalNode"},
        {'x', "KillNode"}
    };

    // Use this for initialization
    void Start()
    {
        Load("simpleworld");
    }

    // Update is called once per frame
    void Update()
    {

    }

    void Load(string inFilename)
    {
        string level = Resources.Load<TextAsset>(inFilename).text;
        COLUMNS = level.IndexOf('\r');
        ROWS = level.Count(x => x == '\r');
        mGrid = new Node[ROWS, COLUMNS];
        level = level.Replace("\r\n", "");
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLUMNS; col++)
            {
                char tile = level[row * COLUMNS + col];

                AddNode(charToNodeMap[tile], row, col);
            }
        }
    }

    private Node AddNode(string inNodeType, int inRow, int inCol)
    {
        mGrid[inRow, inCol] = ((GameObject)Instantiate(Resources.Load(inNodeType))).GetComponent<Node>();
        mGrid[inRow, inCol].OnAdd();
        mGrid[inRow, inCol].Parent = this;
        mGrid[inRow, inCol].transform.position = GetGridPosition(inRow, inCol);
        mGrid[inRow, inCol].transform.parent = transform;
        mGrid[inRow, inCol].name = inNodeType + " (" + inRow + ", " + inCol + ")";

        foreach (PathfindingActor actor in GetAllActorsOfType<PathfindingActor>())
        {
            actor.Start();
        }
        return mGrid[inRow, inCol];
    }

    public Node ReplaceNode(Node inNode, string inReplaceType)
    {
        Debug.Log(inNode.GetType() + " -> " + inReplaceType);
        Vector3 coordinates = GetNodeCoordinates(inNode);
        RemoveNode(inNode);
        return AddNode(inReplaceType, (int)coordinates.x, (int)coordinates.y);
    }

    private void RemoveNode(Node inNode)
    {
        inNode.OnRemove();
        Vector3 coordinates = GetNodeCoordinates(inNode);
        mGrid[(int)coordinates.x, (int)coordinates.y] = null;
    }

    public bool IsPointWithinGrid(Vector3 inPosition)
    {
        Vector3 zerozerozero = GetGridPosition(0, 0);
        Vector3 maxmaxmax = GetGridPosition(ROWS - 1, COLUMNS - 1);
        return
            inPosition.x >= zerozerozero.x && inPosition.x <= maxmaxmax.x &&
            //inPosition.y <= zerozerozero.y && inPosition.y >= maxmaxmax.y &&
            inPosition.z >= zerozerozero.z && inPosition.z <= maxmaxmax.z;
    }
    public void RemoveActor(Actor inActor)
    {
        inActor.OnRemove();
        mActors.Remove(inActor);
    }

    public List<Actor> GetAllActors()
    {
        return mActors;
    }

    public List<Actor> GetAllActorsOfType<T>()
    {
        return mActors.Where(a => a.GetType() == typeof(T)).ToList();
    }

    public List<Actor> GetAllActorsOfType(ActorType inActorType)
    {
        return mActors.Where(a => a.Type == inActorType).ToList();
    }

    public List<Node> GetNeighbors(Node inNode)
    {
        List<Node> nodes = new List<Node>();
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLUMNS; col++)
            {
                if (inNode == mGrid[row, col])
                {
                    if (row - 1 >= 0)
                    {
                        nodes.Add(AddNeighborNode(row - 1, col));
                    }
                    if (row + 1 < ROWS)
                    {
                        nodes.Add(AddNeighborNode(row + 1, col));
                    }
                    if (col - 1 >= 0)
                    {
                        nodes.Add(AddNeighborNode(row, col - 1));
                    }
                    if (col + 1 < COLUMNS)
                    {
                        nodes.Add(AddNeighborNode(row, col + 1));
                    }

                    return nodes;
                }
            }
        }
        return nodes;
    }

    private Node AddNeighborNode(int row, int col)
    {
        Node node = mGrid[row, col];
        return node;
    }

    public Vector3 GetGridPosition(int inRow, int inCol)
    {
        return transform.position + new Vector3((inCol + 0.5f) * GRID_SIZE, 0, (ROWS - inRow - 0.5f) * GRID_SIZE)
                             - new Vector3(COLUMNS * GRID_SIZE / 2.0f, 0, ROWS * GRID_SIZE / 2.0f);
    }

    public Node GetClosestNodeFromPosition(Vector3 inPosition, bool inPassableOnly = false)
    {
        Node bestNode = null;
        float closestDistanceSqr = Mathf.Infinity;
        foreach (var node in mGrid)
        {
            if (inPassableOnly && !node.IsPassable)
                continue;

            Vector3 directionToTarget = node.transform.position - inPosition;
            float dSqrToTarget = directionToTarget.sqrMagnitude;
            if (dSqrToTarget < closestDistanceSqr)
            {
                closestDistanceSqr = dSqrToTarget;
                bestNode = node;
            }
        }

        return bestNode;
    }

    public Vector3 GetNodeCoordinates(Node inNode)
    {
        for (int row = 0; row < ROWS; row++)
        {
            for (int col = 0; col < COLUMNS; col++)
            {
                if (inNode == mGrid[row, col])
                {
                    return new Vector3(row, col);
                }
            }
        }

        return -Vector3.one;
    }

    public void SpawnActorsAtPoint(string inActorType, Vector3 inMousePosition)
    {
        Actor actor = ((GameObject)Instantiate(Resources.Load(inActorType))).GetComponent<Actor>();
        Vector3 screenToWorldPos = Camera.main.ScreenToWorldPoint(inMousePosition);
        actor.transform.position = inMousePosition;//screenToWorldPos;
        actor.OnAdd(this, GetClosestNodeFromPosition(inMousePosition));
        mActors.Add(actor);
    }

    public void AddBuildingAtPoint(string inActorType, Vector3 inMousePosition)
    {
        Actor actor = ((GameObject)Instantiate(Resources.Load(inActorType))).GetComponent<Actor>();
        actor.OnAdd(this, GetClosestNodeFromPosition(inMousePosition));
        mActors.Add(actor);
    }

    public Node GetGoalNode()
    {
        return mGrid.Cast<Node>().FirstOrDefault(node => node.GetType() == typeof(GoalNode));
    }

    void OnDrawGizmos()
    {
         foreach (var node in mGrid)
         {
             if (node.Flagged)
             {
                Gizmos.color = Color.yellow;
                 
                 Gizmos.DrawCube(node.transform.position, Vector3.one * 0.5f);
             }
         }
        foreach (var node in mGrid)
        {
            if (node.IsBuildable)
            {
                Gizmos.color = new Color(0, 1, 0, 0.5f);
                Gizmos.DrawCube(node.transform.position, Vector3.one * 0.5f);
            }
            else
            {
                Gizmos.color = Color.red;
                Gizmos.DrawCube(node.transform.position, Vector3.one * 0.5f);
            }
        }
    }

}
