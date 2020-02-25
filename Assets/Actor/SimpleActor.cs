using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class SimpleActor : Actor
{

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

    void Start()
    {
        RandomHeading();
        StartCoroutine(RandomTurning());
    }

    private void RandomHeading()
    {
        heading.x = (float)((r.NextDouble() * 2.0f) - 1.0f);
        heading.y = 0;// (float)((r.NextDouble() * 2.0f) - 1.0f);
        heading.z = (float)((r.NextDouble() * 2.0f) - 1.0f);
        heading.Normalize();
    }

    void Update()
    {
        Node nextNode = WorldGrid.GetClosestNodeFromPosition(transform.position + (heading * Time.deltaTime));
        Node prevNode = WorldGrid.GetClosestNodeFromPosition(transform.position);
        if (nextNode != prevNode)
        {
            nextNode.OnEnter(this);
        }

        if (!nextNode.IsPassable)
        {
            RandomHeading();
        }
        else
        {
            transform.position += heading * Time.deltaTime;
        }

        if (nextNode.GetType() == typeof(GoalNode))
        {
            Win();
        }
    }

    IEnumerator RandomTurning()
    {
        while (true)
        {
            float degreesToTurn = (float)(r.NextDouble() * 90.0f), turnIter = 10.0f;
            while (degreesToTurn > 0)
            {
                heading = Quaternion.AngleAxis(turnIter, transform.up) * heading;
                degreesToTurn -= turnIter;
                yield return null;
            }

            yield return new WaitForSeconds((float)(r.NextDouble() * 3.0f));
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + heading);
    }

}