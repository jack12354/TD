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
        mGrid[inRow, inCol].Row = inRow;
        mGrid[inRow, inCol].Col = inCol;

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

    public List<Node> GetNeighbors(Node inNode, bool withDiagonals = false)
    {
        List<Node> nodes = new List<Node>();

        int row = inNode.Row, col = inNode.Col;
        nodes.Add(AddNeighborNode(row - 1, col));
        nodes.Add(AddNeighborNode(row + 1, col));
        nodes.Add(AddNeighborNode(row, col - 1));
        nodes.Add(AddNeighborNode(row, col + 1));

        if (withDiagonals)
        {
            nodes.Add(AddNeighborNode(row + 1, col + 1));
            nodes.Add(AddNeighborNode(row + 1, col - 1));
            nodes.Add(AddNeighborNode(row - 1, col + 1));
            nodes.Add(AddNeighborNode(row - 1, col - 1));
        }

        return nodes.Where(node => node != null).ToList();
    }

    public List<Node> GetNeighbors(Node inNode, Direction inDirection, int howMany = 1)
    {
        List<Node> nodes = new List<Node>();
        int row = inNode.Row, col = inNode.Col;

        int rowChange = 0, colChange = 0;
        switch (inDirection)
        {
            case Direction.North: rowChange = 0; colChange = 1; break;
            case Direction.South: rowChange = 0; colChange = -1; break;
            case Direction.East: rowChange = -1; colChange = 0; break;
            case Direction.West: rowChange = 1; colChange = 0; break;
            case Direction.Northwest: rowChange = 1; colChange = 1; break;
            case Direction.Northeast: rowChange = -1; colChange = 1; break;
            case Direction.Southwest: rowChange = 1; colChange = -1; break;
            case Direction.Southeast: rowChange = -1; colChange = -1; break;
        }
        for (int count = 1; count <= howMany; count++)
            nodes.Add(AddNeighborNode(row + (rowChange * count), col + (colChange * count)));

        return nodes.Where(node => node != null).ToList(); ;
    }

    private Node AddNeighborNode(int row, int col)
    {
        if (row < 0 || row > ROWS || col < 0 || col > COLUMNS)
            return null;
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
        float mindistSqr = GRID_SIZE * GRID_SIZE / 4;
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
            if (dSqrToTarget < mindistSqr)
                return node;
        }

        return bestNode;
    }

    public Vector3 GetNodeCoordinates(Node inNode)
    {
        return new Vector3(inNode.Row, inNode.Col);
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
        Building building = ((GameObject)Instantiate(Resources.Load(inActorType))).GetComponent<Building>();
        if (Building.CanBuildAtLocation(this, GetClosestNodeFromPosition(inMousePosition),
            building.XDimension, building.YDimension))
        {
            building.OnAdd(this, GetClosestNodeFromPosition(inMousePosition));
            mActors.Add(building);
        }
        else
            Destroy(building.gameObject);
    }

    public Node GetGoalNode()
    {
        return mGrid.Cast<Node>().FirstOrDefault(node => node.GetType() == typeof(GoalNode));
    }
    public List<Node> GetGoalNodes()
    {
        return mGrid.Cast<Node>().Where(node => node.GetType() == typeof(GoalNode)).ToList();
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

public enum Direction
{
    North, South, East, West, Northwest, Northeast, Southwest, Southeast
}