using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class VerletPhysicsNode : MonoBehaviour
{
    private Rigidbody2D body;
    public Vector3 lastPos;
    private bool _isLocked;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    public void Simulate(Vector3 force, float dt)
    {
        if (_isLocked)
            return;

        Vector3 step = (transform.position - lastPos) + force * (dt * dt);
        lastPos = transform.position;
        body.MovePosition(transform.position + step);
    }

    public bool ToggleLock()
    {
        return SetIsLocked(!_isLocked);
    }

    public bool SetIsLocked(bool value)
    {
        if (_isLocked == value)
            return _isLocked;

        _isLocked = value;
        body.simulated = !_isLocked;

        return _isLocked;
    }

    public bool GetIsLocked()
    {
        return _isLocked;
    }
}

public class VerletPhysicsSegment
{
    public VerletPhysicsNode nodeA;
    public VerletPhysicsNode nodeB;
    public float targetLength;

    public VerletPhysicsSegment(VerletPhysicsNode a, VerletPhysicsNode b)
    {
        nodeA = a;
        nodeB = b;
        targetLength = Vector3.Distance(a.transform.position, b.transform.position);
    }

    public void Simulate()
    {
        Vector2 center = (nodeA.transform.position + nodeB.transform.position) / 2;
        Vector2 dir = (nodeA.transform.position - nodeB.transform.position).normalized;

        if (!nodeA.GetIsLocked())
            nodeA.transform.position = center + dir * targetLength / 2;
        if (!nodeB.GetIsLocked())
            nodeB.transform.position = center - dir * targetLength / 2;
    }

    public float GetLength()
    {
        return Vector2.Distance(nodeA.transform.position, nodeB.transform.position);
    }
}
