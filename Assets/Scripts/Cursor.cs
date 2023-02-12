using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Cursor : MonoBehaviour
{
    [SerializeField] private float _moveSpeed;

    // Update is called once per frame
    void Update()
    {
        Vector2 mousePos = LevelHandler.instance.GetPointOnGrid(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        transform.position = mousePos;
        // Vector3 displacement = LevelHandler.instance.GetPointOnGrid(mousePos) - (Vector2)transform.position;

        // Vector3 input = Vector3.zero;
        // if (displacement.x > 0)
        //     input.x = 1;
        // if (displacement.x < 0)
        //     input.x = -1;
        // if (displacement.y > 0)
        //     input.y = 1;
        // if (displacement.y < 0)
        //     input.y = -1;

        // transform.position += input * _moveSpeed * Time.deltaTime;
    }
}
