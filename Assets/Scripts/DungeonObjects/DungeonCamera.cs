using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCamera : MonoBehaviour
{
    GameObject Target;

    Vector3 camPos;
    Vector3 camRot;

    private void Awake()
    {
        camPos = new Vector3();
        camRot = Vector3.zero;
        camRot.x = -45;
        
    }
    private void LateUpdate()
    {
        if (Target == null)
            return;

        camPos = Target.transform.position;
        camPos.z = -10;
        camPos.x += 1.2f;
        camPos.y -= 7;
        transform.position = camPos;

        transform.rotation = Quaternion.Euler(camRot);
        
    }

    public void SetTarget(GameObject _o)
    {
        Target = _o;
    }
}
