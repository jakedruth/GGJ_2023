using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VineHandler : MonoBehaviour
{
    [SerializeField] private Vine _vine;
    private Transform _vineTip;
    [SerializeField] private float _vineTipSpeed;

    void Start()
    {
        _vineTip = _vine.vineTip;
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 target = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            StopAllCoroutines();
            StartCoroutine(FollowPath(target));
        }
        //StartCoroutine(FollowPath(Camera.main.ScreenToWorldPoint(Input.mousePosition)));
    }

    IEnumerator FollowPath(Vector2 target)
    {
        // Get Path
        if (LevelHandler.instance.GetPath(_vine.transform.position, target, out List<Vector2> pathFromBase))
        {
            // Check to see if the path is short enough
            float pathLength = Vector2.Distance(transform.position, pathFromBase[0]);
            for (int i = 1; i < pathFromBase.Count; i++)
            {
                pathLength += Vector2.Distance(pathFromBase[i - 1], pathFromBase[i]);
            }

            if (pathLength > _vine.LengthBuffer)
                yield break;

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
