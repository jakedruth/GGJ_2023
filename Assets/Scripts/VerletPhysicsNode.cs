using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class VerletPhysicsNode : MonoBehaviour
{
    private Rigidbody2D body;
    public Vector3 lastPos;
    public bool isLocked;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Simulate(Vector3 force, float dt)
    {
        if (isLocked)
            return;

        Vector3 step = transform.position - lastPos + force * (dt * dt);
        lastPos = transform.position;
        body.MovePosition(transform.position + step);
    }

    public bool ToggleLock()
    {
        return isLocked = !isLocked;
    }
}

public class VerletPhysicsSegment
{
    public VerletPhysicsNode nodeA;
    public VerletPhysicsNode nodeB;
    public float length;

    public VerletPhysicsSegment(VerletPhysicsNode a, VerletPhysicsNode b)
    {
        nodeA = a;
        nodeB = b;
        length = Vector3.Distance(a.transform.position, b.transform.position);
    }

    public void Simulate()
    {
        if (nodeA.isLocked && nodeB.isLocked)
            return;

        Vector2 center = (nodeA.transform.position + nodeB.transform.position) / 2;
        Vector2 dir = (nodeA.transform.position - nodeB.transform.position).normalized;

        if (!nodeA.isLocked)
            nodeA.transform.position = center + dir * length / 2;
        if (!nodeB.isLocked)
            nodeB.transform.position = center - dir * length / 2;
    }
}
