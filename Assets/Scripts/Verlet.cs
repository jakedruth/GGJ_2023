using UnityEngine;
public class VerletNode
{
    public Vector2 position;
    public Vector2 prevPosition;
    public bool locked;

    public VerletNode(Vector2 startPos, bool isLocked = false)
    {
        prevPosition = position = startPos;
        locked = isLocked;
    }

    public void Simulate(Vector2 force, float dt)
    {
        if (locked)
            return;

        Vector2 step = position - prevPosition + force * (dt * dt);
        prevPosition = position;
        position += step;
    }

    public bool ToggleLock()
    {
        return locked = !locked;
    }

    public void SetLocked(bool value)
    {
        locked = value;
    }
}

public class VerletSegment
{
    public VerletNode nodeA;
    public VerletNode nodeB;
    public float targetLength;

    public VerletSegment(VerletNode a, VerletNode b)
    {
        nodeA = a;
        nodeB = b;
        targetLength = Vector2.Distance(a.position, b.position);
    }

    public void ApplyConstraints()
    {
        Vector2 center = (nodeA.position + nodeB.position) / 2;
        Vector2 dir = (nodeA.position - nodeB.position).normalized;

        if (!nodeA.locked)
            nodeA.position = center + dir * targetLength / 2;
        if (!nodeB.locked)
            nodeB.position = center - dir * targetLength / 2;
    }

    public float GetLength()
    {
        return Vector2.Distance(nodeA.position, nodeB.position);
    }
}

