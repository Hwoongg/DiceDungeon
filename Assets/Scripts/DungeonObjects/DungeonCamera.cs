using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonCamera : MonoBehaviour
{
    GameObject Target;

    Vector3 camPos;
    Vector3 camRot;

    //Transform closeUpAnchor;

    [SerializeField] float speed;


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

        
        //camPos = Target.transform.position;

        //transform.position = camPos;
        //transform.rotation = Target.transform.rotation;


        // 밀착 방식에서 추적 방식으로
        transform.position = Vector3.Lerp(transform.position, Target.transform.position,
            Time.deltaTime * speed);

        transform.rotation = Quaternion.Lerp(transform.rotation, Target.transform.rotation,
            Time.deltaTime * speed);
    }

    public void SetTarget(GameObject _o)
    {
        Target = _o;
    }
}
