using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Vine))]
public class VineHandler : MonoBehaviour
{
    private Vine _vine;
    private Transform _vineTip;
    private Vector2 _targetPos;
    [SerializeField] private float _vineTipSpeed;

    void Awake()
    {
        _vine = GetComponent<Vine>();
    }

    void Start()
    {

        _vineTip = _vine.vineTip;
    }

    void Update()
    {
        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 newTarget = LevelHandler.instance.GetPointOnGrid(mousePos);
        if ((newTarget - _targetPos).sqrMagnitude >= LevelHandler.instance.GridSize * LevelHandler.instance.GridSize)
        {
            _targetPos = newTarget;
            StopAllCoroutines();
            StartCoroutine(FollowPath(_targetPos));
        }

        // if (Input.GetMouseButtonDown(0))
        // {
        //     Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //     StopAllCoroutines();
        //     StartCoroutine(FollowPath(target));
        // }
        //StartCoroutine(FollowPath(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
    }

    IEnumerator FollowPath(Vector2 target)
    {
        // Get Path
        if (LevelHandler.instance.GetPath(transform.position, target, out List<Vector2> pathFromBase))
        {
            if (pathFromBase.Count == 0)
                yield break;

            // Calculate the path length
            float pathLength = Vector2.Distance(transform.position, pathFromBase[0]);
            if (pathFromBase.Count > 1)
            {
                for (int i = 1; i < pathFromBase.Count; i++)
                {
                    pathLength += Vector2.Distance(pathFromBase[i - 1], pathFromBase[i]);
                }
            }

            // Check to see if the path is short enough
            if (pathLength > _vine.LengthBuffer)
                yield break;

            // If the path is shorter than the vine's desired length, path find to new target point
            if (LevelHandler.instance.GetPath(_vineTip.transform.position, target, out List<Vector2> path))
            {
                int index = 0;
                while (index < path.Count)
                {
                    Vector3 moveTo = path[index];
                    Vector2 delta = _vineTip.position - moveTo;
                    if (delta.sqrMagnitude >= 0.001)
                    {
                        _vineTip.position = Vector2.MoveTowards(_vineTip.position, moveTo, _vineTipSpeed * Time.deltaTime);
                    }
                    else
                    {
                        _vineTip.position = moveTo;
                        index++;
                    }

                    yield return null;
                }
            }
        }
        else
        {
            Debug.Log("Could not find path");
        }

        yield return null;
    }
}
