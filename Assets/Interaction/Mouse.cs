using UnityEngine;
using System.Collections;
using Random = System.Random;

public class Mouse : MonoBehaviour
{
    Random r = new Random();
    public string SpawnType = "BrainyActor";
    private Grid grid;
    // Use this for initialization
    void Start()
    {
        grid = FindObjectOfType<Grid>();
    }

    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100))
        {
            transform.position = hit.point;

            if (Input.GetMouseButtonDown(0))
            {
                if (SpawnType.EndsWith("Actor"))
                {
                    Node hitNode = hit.transform.gameObject.GetComponent<Node>();
                    if (hitNode)
                    {
                        Vector3 spawnPoint =
                            grid.GetClosestNodeFromPosition(hit.point, true).transform.position;
                        grid.SpawnActorsAtPoint(SpawnType, spawnPoint
                                                                           + new Vector3(
                                                                               (float)((r.NextDouble() * 1f) - 0.5f),
                                                                               (float)((r.NextDouble() * 1f) - 0.5f),
                                                                               (float)((r.NextDouble() * 1f) - 0.5f))
                        );
                    }
                }
                else if (SpawnType.EndsWith("Node"))
                {
                    Node hitNode = hit.transform.gameObject.GetComponent<Node>();
                    if (hitNode)
                    {
                        if(!hitNode.name.StartsWith(SpawnType))
                        grid.ReplaceNode(hitNode, SpawnType == "DeleteNode" ? "EmptyNode" : SpawnType);

                        // if (SpawnType == "DeleteNode")
                        // {
                       // grid.ReplaceNode(hitNode, "EmptyNode");
                        //}
                       // else
                        //{
                          //  var emptySpacePoint = hit.point + hit.normal * (Grid.GRID_SIZE / 2.0f);
                          //  if (grid.IsPointWithinGrid(emptySpacePoint))
                          //  {
                           //     Node nodeToReplace = grid.GetClosestNodeFromPosition(emptySpacePoint);
                           //     grid.ReplaceNode(nodeToReplace, SpawnType);
                          //  }
                        //}
                    }
                }
                else if (SpawnType.EndsWith("Building"))
                {
                    if (SpawnType == "DeleteBuilding")
                    {
                        Building hitBuilding = hit.transform.gameObject.GetComponent<Building>();
                        if (hitBuilding)
                        {
                            grid.RemoveActor((Actor)hitBuilding);
                        }
                        return;
                    }

                    Node hitNode = hit.transform.gameObject.GetComponent<Node>();
                    if (hitNode)
                    {
                        if (hitNode.IsBuildable)
                        {
                            grid.AddBuildingAtPoint(SpawnType, hit.point);
                        }
                    }

                }
            }

        }
    }

    public void SetSpawningType(string inTypeString)
    {
        Debug.Log("SpawnType set to " + inTypeString);
        SpawnType = inTypeString;
    }
}