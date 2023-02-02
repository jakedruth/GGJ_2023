using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vine : MonoBehaviour
{
    [SerializeField] private int _iterations;
    [SerializeField] private int _totalNodes;
    public int TotalNodes { get { return _totalNodes; } }

    [SerializeField] private float _nodeDistance;
    [SerializeField] private Vector2 _gravity;

    private List<VerletNode> _nodes;
    private List<VerletConnection> _connections;

    [SerializeField] private float _moveSpeed;

    void Awake()
    {
        _nodes = new List<VerletNode>();
        _connections = new List<VerletConnection>();

        Vector2 pos = transform.position;
        for (int i = 0; i < _totalNodes; i++)
        {
            _nodes.Add(new VerletNode(pos, i == 0));
            pos.x += _nodeDistance;

            if (i > 0)
            {
                _connections.Add(new VerletConnection(_nodes[i - 1], _nodes[i]));
            }
        }

        // Create Extra little 
        // int k = 0;
        // while (k < _totalNodes - 1)
        // {
        //     int count = UnityEngine.Random.Range(2, 6);
        //     VerletNode lastNode = _nodes[k];
        //     for (int i = 0; i < count; i++)
        //     {
        //         VerletNode nextNode = new(lastNode.position + Vector2.down * _nodeDistance);
        //         _nodes.Add(nextNode);
        //         _connections.Add(new VerletConnection(lastNode, nextNode));
        //         lastNode = nextNode;
        //     }
        //     k += UnityEngine.Random.Range(2, 5);
        // }
    }

    void Update()
    {
        if (Input.GetMouseButton(0))
        {
            VerletNode end = _nodes[_totalNodes - 1];
            //if (!end.locked)
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
            for (int i1 = 0; i1 < _connections.Count; i1++)
            {
                VerletConnection connection = _connections[i1];
                connection.Simulate();
            }
        }
    }

    private void OnDrawGizmos()
    {
        if (!Application.isPlaying)
        {
            return;
        }

        for (int i = 0; i < _connections.Count - 1; i++)
        {
            if (i % 2 == 0)
            {
                Gizmos.color = Color.green;
            }
            else
            {
                Gizmos.color = Color.white;
            }

            Gizmos.DrawLine(_connections[i].nodeA.position, _connections[i].nodeB.position);
        }
    }
}
