using System.Collections;
using UnityEngine;
using Random = System.Random;

public class SpawnerNode : Node
{
    private string ActorToSpawn;
    private float SpawnInterval;

    public override void OnAdd()
    {
        IsBuildable = false;
        IsPassable = true;
        SpawnInterval = 0.5f;
    }

    public override void OnRemove()
    {
        Destroy(gameObject);
    }

    void Start()
    {
        StartCoroutine(SpawnLoop());
    }

    public override void OnEnter(Actor inActor)
    {
        // Nothing interesting happens...
    }

    void Spawn()
    {
        Parent.SpawnActorsAtPoint("BrainyActor", transform.position);
    }

    IEnumerator SpawnLoop()
    {
        for (;;)
        {
            Spawn();
            yield return new WaitForSeconds(SpawnInterval);
        }
    }
}