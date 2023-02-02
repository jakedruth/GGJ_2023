using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Vine))]
[RequireComponent(typeof(LineRenderer))]
public class VineRenderer : MonoBehaviour
{
    private Vine _vine;
    private LineRenderer _lineRenderer;

    [SerializeField] private int _numLineSegments;
    private List<VerletNode> _nodes;

    void Awake()
    {
        _vine = GetComponent<Vine>();
        _lineRenderer = GetComponent<LineRenderer>();

        if (_vine.TotalNodes % _numLineSegments > 0)
            Debug.LogWarning("Number of line segments does not divide evenly into the total number of nodes in the vine");
    }

    void Start()
    {
        _nodes = new List<VerletNode>();
        int space = _vine.TotalNodes / _numLineSegments;
        for (int i = 0; i < _vine.TotalNodes; i += space)
        {
            _nodes.Add(_vine.GetNode(i));
        }
        _nodes.Add(_vine.GetNode(_vine.TotalNodes - 1));

        UpdateLineRenderer();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateLineRenderer();
    }

    private void UpdateLineRenderer()
    {
        Vector3[] positions = new Vector3[_nodes.Count];
        for (int i = 0; i < _nodes.Count; i++)
        {
            positions[i] = _nodes[i].position;
        }
        _lineRenderer.positionCount = _nodes.Count;
        _lineRenderer.SetPositions(positions);
    }

    // private void OnDrawGizmos()
    // {
    //     if (!Application.isPlaying)
    //     {
    //         return;
    //     }

    //     Gizmos.color = Color.blue;
    //     for (int i = 0; i < _nodes.Count - 1; i++)
    //     {
    //         Gizmos.DrawLine(_nodes[i].position, _nodes[i + 1].position);
    //     }
    // }
}
