using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private CollisionInfo[] _collisionInfos;
    private int _numCollisions;

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
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            VerletNode end = _nodes[_totalNodes - 1];
            {
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
        Simulate();
    }

    private void SnapshotCollisions()
    {

    }

    void Simulate()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            VerletNode node = _nodes[i];
            node.Simulate(_gravity, Time.fixedDeltaTime);
        }

        for (int i = 0; i < _iterations; i++)
        {
            ApplyConstraints();
            AdjustCollisions();
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

    }

    public VerletNode GetNode(int index)
    {
        return _nodes[index];
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
    public int[] CollidingNodes;

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
        CollidingNodes = new int[maxCollisions];
    }
}
