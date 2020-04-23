using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCamera : MonoBehaviour
{
    GameObject Target;

    Vector3 camPos;

    private void Awake()
    {
        camPos = new Vector3();
    }
    private void LateUpdate()
    {
        if (Target == null)
            return;

        camPos = Target.transform.position;
        camPos.z = -10;
        transform.position = camPos;
    }

    public void SetTarget(GameObject _o)
    {
        Target = _o;
    }
}
