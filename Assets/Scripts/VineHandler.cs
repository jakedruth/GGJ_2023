using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineHandler : MonoBehaviour
{
    [SerializeField] private Vine _vine;
    private Transform _vineTip;

    void Awake()
    {

    }

    void Start()
    {
        _vineTip = _vine.vineTip;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            List<Vector2> path;
            if (LevelHandler.instance.GetPath(_vine.transform.position, target, out path))
            {
                for (int i = 0; i < path.Count - 1; i++)
                {
                    Debug.DrawLine(path[i], path[i + 1], Color.red, 0.5f, false);
                }
            }
        }
        //StartCoroutine(FollowPath(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
    }

    IEnumerator FollowPath(Vector2 target)
    {
        // Get Path
        List<Vector2> path;
        if (LevelHandler.instance.GetPath(_vine.transform.position, target, out path))
        {
            int i = 0;
            Debug.Log(path.Count);
        }
        else
        {
            Debug.Log("Could not find path");
        }

        yield return null;
    }
}
