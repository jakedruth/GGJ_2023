using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vine : MonoBehaviour
{
    [SerializeField] private int _iterations;
    [SerializeField] private int _totalNodes;
    public int TotalNodes { get { return _totalNodes; } }

    [SerializeField] private float _vineLength;
    [SerializeField] private float _vineLengthBuffer;
    [SerializeField] private Vector2 _gravity;

    private List<VerletPhysicsNode> _nodes;
    private List<VerletPhysicsSegment> _segments;

    [SerializeField] private float _moveSpeed;

    void Awake()
    {
        _nodes = new List<VerletPhysicsNode>();
        _segments = new List<VerletPhysicsSegment>();

        Vector2 pos = transform.position;
        float dist = (_vineLength + _vineLengthBuffer) / (_totalNodes - 1);
        for (int i = 0; i < _totalNodes; i++)
        {
            VerletPhysicsNode newNode = new VerletPhysicsNode();
            newNode.isLocked = i == 0 || i == _totalNodes - 1;
            _nodes.Add(newNode);
            pos.x += dist;

            if (i > 0)
            {
                _segments.Add(new VerletPhysicsSegment(_nodes[i - 1], _nodes[i]));
            }
        }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            VerletPhysicsNode end = _nodes[_totalNodes - 1];
            {
                Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                end.transform.position = Vector2.MoveTowards(end.transform.position, target, _moveSpeed * Time.deltaTime);
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            _nodes[_totalNodes - 1].ToggleLock();
        }
    }

    void FixedUpdate()
    {
        Simulate();
    }

    void Simulate()
    {
        for (int i = 0; i < _nodes.Count; i++)
        {
            VerletPhysicsNode node = _nodes[i];
            node.Simulate(_gravity, Time.fixedDeltaTime);
        }

        for (int i = 0; i < _iterations; i++)
        {
            for (int i1 = 0; i1 < _segments.Count; i1++)
            {
                VerletPhysicsSegment connection = _segments[i1];
                connection.Simulate();
            }
        }
    }

    public VerletPhysicsNode GetNode(int index)
    {
        return _nodes[index];
    }

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
}
