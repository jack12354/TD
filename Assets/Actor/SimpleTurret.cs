using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

class SimpleTurret : Building
{
    public float FireInterval;
    public string Projectile;
    private float last = 0;

    // Update is called once per frame
    void Update()
    {
        foreach (var actor in WorldGrid.GetAllActorsOfType(ActorType.Enemy))
        {
            RaycastHit hit;
            Physics.Raycast(new Ray(transform.position, actor.transform.position - transform.position), out hit, 100f);
            if (hit.transform.gameObject == actor.gameObject && Time.time > last + FireInterval)
            {
                GameObject bullet =
                    Instantiate(Resources.Load(Projectile), transform.position, Quaternion.identity) as
                        GameObject;
                bullet.transform.LookAt(actor.transform.position);
                last = Time.time;
                heading.x = (float)((r.NextDouble() * 2.0f) - 1.0f);
                heading.y = (float)((r.NextDouble() * 2.0f) - 1.0f);
                heading.z = (float)((r.NextDouble() * 2.0f) - 1.0f);
                heading.Normalize();
                Destroy(bullet, 10);
            }
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (var actor in WorldGrid.GetAllActorsOfType(ActorType.Enemy))
        {
            RaycastHit hit;

            if (Physics.Raycast(new Ray(transform.position, actor.transform.position - transform.position), out hit, 100f))
                if (hit.transform.gameObject == actor.gameObject)
                    Gizmos.DrawRay(transform.position, actor.transform.position - transform.position);
            //Gizmos.DrawLine(transform.position, actor.transform.position);
        }
    }
}