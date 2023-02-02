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

    private List<VerletNode> _nodes;
    private List<VerletSegment> _segments;

    [SerializeField] private float _moveSpeed;

    void Awake()
    {
        _nodes = new List<VerletNode>();
        _segments = new List<VerletSegment>();

        Vector2 pos = transform.position;
        float dist = (_vineLength + _vineLengthBuffer) / (_totalNodes - 1);
        for (int i = 0; i < _totalNodes; i++)
        {
            _nodes.Add(new VerletNode(pos, i == 0 || i == _totalNodes - 1));
            pos.x += dist;

            if (i > 0)
            {
                _segments.Add(new VerletSegment(_nodes[i - 1], _nodes[i]));
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
        Simulate();
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
            for (int i1 = 0; i1 < _segments.Count; i1++)
            {
                VerletSegment connection = _segments[i1];
                connection.Simulate();
            }
        }
    }

    public VerletNode GetNode(int index)
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
