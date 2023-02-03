using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// https://github.com/Toqozz/blog-code/blob/master/rope/Assets/Rope.cs

public class Vine : MonoBehaviour
{
    // Maximum number of colliders hitting the rope at once.
    private const int MAX_ROPE_COLLISIONS = 32;
    // Size of the collider buffer, also the maximum number of colliders that a single node can touch at once.
    private const int COLLIDER_BUFFER_SIZE = 8;

    private List<VerletNode> _nodes;
    private List<VerletSegment> _segments;

    [SerializeField] private int _iterations;
    [SerializeField] private int _totalNodes;
    public int TotalNodes { get { return _totalNodes; } }

    [SerializeField] private float _vineLength;
    [SerializeField] private float _vineLengthBuffer;
    [SerializeField] private Vector2 _gravity;

    [SerializeField] private float _collisionRadius;
    private CollisionInfo[] _collisionInfos;
    private int _numCollisions;
    private Collider2D[] _colliderBuffer;

    [SerializeField] private float _moveSpeed;

    void Awake()
    {
        _nodes = new List<VerletNode>();
        _segments = new List<VerletSegment>();

        Vector2 pos = transform.position;
        float dist = (_vineLength + _vineLengthBuffer) / (_totalNodes - 1);
        for (int i = 0; i < _totalNodes; i++)
        {
            VerletNode node = new(pos, i == 0 || i == _totalNodes - 1);
            _nodes.Add(node);
            pos.x += dist;

            if (i > 0)
            {
                VerletSegment segment = new(_nodes[i - 1], _nodes[i]);
                _segments.Add(segment);
            }
        }

        _collisionInfos = new CollisionInfo[MAX_ROPE_COLLISIONS];
        for (int i = 0; i < _collisionInfos.Length; i++)
        {
            _collisionInfos[i] = new CollisionInfo(_totalNodes);
        }

        _colliderBuffer = new Collider2D[COLLIDER_BUFFER_SIZE];
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            if (GetVineLength() < _vineLength + _vineLengthBuffer)
            {
                VerletNode end = _nodes[_totalNodes - 1];
                Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                end.position = Vector2.MoveTowards(end.position, target, _moveSpeed * Time.deltaTime);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            _nodes[_totalNodes - 1].ToggleLock();
        }
    }

    void FixedUpdate()
    {
        SnapshotCollisions();
        UpdateRope();
    }

    private void SnapshotCollisions()
    {
        _numCollisions = 0;

        // Loop Through each node and get collisions within a radius.
        for (int i = 0; i < _nodes.Count; i++)
        {
            int collisions = Physics2D.OverlapCircleNonAlloc(_nodes[i].position, _collisionRadius, _colliderBuffer);

            for (int j = 0; j < collisions; j++)
            {
                // Get the collider ID
                Collider2D collider = _colliderBuffer[j];
                int id = collider.GetInstanceID();

                // See if we already have the collider in our collision info
                int idx = -1;
                for (int k = 0; k < _numCollisions; k++)
                {
                    if (_collisionInfos[k].id == id)
                    {
                        idx = k;
                        break;
                    }
                }

                // If we didn't have the collider, add it here
                if (idx < 0)
                {
                    CollisionInfo info = _collisionInfos[_numCollisions];
                    info.id = id;
                    info.wtl = collider.transform.worldToLocalMatrix;
                    info.ltw = collider.transform.localToWorldMatrix;
                    info.scale.x = info.ltw.GetColumn(0).magnitude;
                    info.scale.y = info.ltw.GetColumn(1).magnitude;
                    info.position = collider.transform.position;
                    info.numCollisions = 1;
                    info.collidingNodes[0] = i;

                    switch (collider)
                    {
                        case CircleCollider2D circle:
                            info.type = CollisionInfo.ColliderType.CIRCLE;
                            info.size.x = info.size.y = circle.radius;
                            break;
                        case BoxCollider2D box:
                            info.type = CollisionInfo.ColliderType.BOX;
                            info.size = box.size;
                            break;
                        default:
                            info.type = CollisionInfo.ColliderType.NONE;
                            break;
                    }

                    _numCollisions++;
                    if (_numCollisions >= MAX_ROPE_COLLISIONS)
                    {
                        return;
                    }
                }
                else // Update the existing collision info
                {
                    CollisionInfo info = _collisionInfos[idx];
                    if (info.numCollisions >= _totalNodes)
                        continue;

                    info.collidingNodes[info.numCollisions++] = i;
                }
            }
        }
    }

    void UpdateRope()
    {
        Simulate();

        for (int i = 0; i < _iterations; i++)
        {
            ApplyConstraints();
            AdjustCollisions();
        }
    }

    private void Simulate()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            VerletNode node = _nodes[i];
            node.Simulate(_gravity, Time.fixedDeltaTime);
        }
    }

    private void ApplyConstraints()
    {
        for (int i = 0; i < _segments.Count; i++)
        {
            VerletSegment connection = _segments[i];
            connection.ApplyConstraints();
        }
    }

    private void AdjustCollisions()
    {
        for (int i = 0; i < _numCollisions; i++)
        {
            CollisionInfo info = _collisionInfos[i];

            switch (info.type)
            {
                case CollisionInfo.ColliderType.CIRCLE:
                    float radius = info.size.x * Mathf.Max(info.scale.x, info.scale.y);

                    // See if any nodes are touching the circle
                    for (int j = 0; j < info.numCollisions; j++)
                    {
                        VerletNode node = _nodes[info.collidingNodes[j]];
                        Vector2 delta = node.position - info.position;

                        if (delta.sqrMagnitude > radius * radius) // Not Colliding
                            continue;

                        Vector2 dir = delta.normalized;
                        Vector2 hitPos = info.position + dir * radius;
                        node.position = hitPos;
                    }
                    break;
                case CollisionInfo.ColliderType.BOX:
                    for (int j = 0; j < info.numCollisions; j++)
                    {
                        VerletNode node = _nodes[info.collidingNodes[j]];
                        Vector2 localPoint = info.wtl.MultiplyPoint(node.position);

                        // If distance from center is more than the box size, then we are not colliding
                        Vector2 half = info.size * 0.5f;
                        Vector2 scalar = info.scale;

                        // Check X-bounds
                        float dx = localPoint.x;
                        float px = half.x - Mathf.Abs(dx);
                        if (px <= 0)
                            continue;

                        // Check Y-bounds
                        float dy = localPoint.y;
                        float py = half.y - Mathf.Abs(dy);
                        if (py <= 0)
                            continue;

                        // Need to multiply distance by scale or we'll mess up on scaled box corners.
                        if (px * scalar.x < py * scalar.y)
                        {
                            float sx = Mathf.Sign(dx);
                            localPoint.x = half.x * sx;
                        }
                        else
                        {
                            float sy = Mathf.Sign(dy);
                            localPoint.y = half.y * sy;
                        }

                        Vector2 hitPos = info.ltw.MultiplyPoint(localPoint);
                        node.position = hitPos;
                    }
                    break;
                default:
                case CollisionInfo.ColliderType.NONE:
                    break;
            }
        }
    }

    public VerletNode GetNode(int index)
    {
        return _nodes[index];
    }

    public float GetVineLength()
    {
        float length = 0;
        for (int i = 0; i < _segments.Count; i++)
        {
            VerletSegment segment = _segments[i];
            length += segment.GetLength();
        }
        return length;
    }

    #region Debug Gizmo
    // private void OnDrawGizmos()
    // {
    //     if (!Application.isPlaying)
    //     {
    //         return;
    //     }

    //     for (int i = 0; i < _segments.Count; i++)
    //     {
    //         if (i % 2 == 0)
    //         {
    //             Gizmos.color = Color.green;
    //         }
    //         else
    //         {
    //             Gizmos.color = Color.white;
    //         }

    //         Gizmos.DrawLine(_segments[i].nodeA.position, _segments[i].nodeB.position);
    //     }
    // }
    #endregion
}

class CollisionInfo
{
    public enum ColliderType
    {
        CIRCLE,
        BOX,
        NONE
    }

    public int id;

    public ColliderType type;
    public Vector2 size;
    public Vector2 position;
    public Vector2 scale;
    public Matrix4x4 wtl;
    public Matrix4x4 ltw;
    public int numCollisions;
    public int[] collidingNodes;

    public CollisionInfo(int maxCollisions)
    {
        id = -1;
        type = ColliderType.NONE;
        size = Vector2.zero;
        position = Vector2.zero;
        scale = Vector2.zero;
        wtl = Matrix4x4.zero;
        ltw = Matrix4x4.zero;

        numCollisions = 0;
        collidingNodes = new int[maxCollisions];
    }
}
