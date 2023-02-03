using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aoiti.Pathfinding;

public class LevelHandler : MonoBehaviour
{
    public static LevelHandler instance;
    private Pathfinder<Vector2> _pathfinder;
    [SerializeField] private float _gridSize;

    void Awake()
    {
        if (instance == null)
            instance = this;

        _pathfinder = new Pathfinder<Vector2>(Vector2.Distance, GetNeighbors, 100);
    }

    public bool GetPath(Vector2 start, Vector2 end, out List<Vector2> path)
    {
        return _pathfinder.GenerateAstarPath(GetPointOnGrid(start), GetPointOnGrid(end), out path);
    }

    private Vector2 GetPointOnGrid(Vector2 target)
    {
        return new Vector2(Mathf.Round(target.x / _gridSize) * _gridSize, Mathf.Round(target.y / _gridSize) * _gridSize);
    }

    private Dictionary<Vector2, float> GetNeighbors(Vector2 point)
    {
        Dictionary<Vector2, float> neighbors = new();
        for (int x = -1; x <= 1; x++)
        {
            for (int y = -1; y <= 1; y++)
            {
                if (x == 0 && y == 0)
                    continue;

                Vector2 dir = new Vector2(x, y) * _gridSize;
                if (!Physics2D.Linecast(point, point + dir))
                {
                    neighbors.Add(GetPointOnGrid(point + dir), dir.magnitude);
                }
            }
        }

        return neighbors;
    }
}
