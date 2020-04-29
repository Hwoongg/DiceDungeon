

// **************************************************
//
// UI 애니메이션 기능을 제공하는 정적 클래스.
// Coroutine 함수들로 구성. 
//
// 현재 MonoBehaviour 기능이 필요하다 생각되어
// 해당 클래스를 이용하는 싱글톤을 만들지 고려중
//
// **************************************************

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public static class UIAnimator
{
    static public IEnumerator Move(GameObject _gameObject, Vector2 _startPos,
        Vector2 _desPos, float _speed = 1.0f)
    {
        //Button button = _gameObject.GetComponent<Button>();
        //if(button != null) // 버튼 기능 비활성화
        //    button.enabled = false;
        
        
        float nowLerp = 0.0f;
        Vector3 nowPos;
        Vector3 startPos = _startPos;

        while (true)
        {
            nowPos = Vector3.Lerp(startPos, _desPos, nowLerp);

            _gameObject.transform.localPosition = nowPos;

            if (nowLerp < 1.0f)
            {
                nowLerp += _speed * Time.deltaTime;
            }
            else
            {
                break;
            }

            yield return null;
        }

        //if(button != null) // 기능 복구
        //    button.enabled = true;

        
        yield break;
    }

    
    static public IEnumerator Scale(GameObject _gameObject, Vector2 _startScale,
        Vector2 _desScale, float _speed = 1.0f)
    {
        //Button button = _gameObject.GetComponent<Button>();
        //if (button != null) // 버튼이라면 안눌리도록
        //    button.enabled = false;

        float nowLerp = 0.0f;
        Vector3 nowScale;
        //Vector3 _startScale = _gameObject.transform.localPosition;

        while (true)
        {
            nowScale = Vector3.Lerp(_startScale, _desScale, nowLerp);

            _gameObject.transform.localScale = nowScale;

            if (nowLerp < 1.0f)
            {
                nowLerp += _speed * Time.deltaTime;
            }
            else
            {
                break;
            }

            yield return null;
        }

        //if (button != null) // 기능 복구
        //    button.enabled = true;

        yield break;
    }

    static public IEnumerator Rotate(GameObject _gameObject,
        int rotDir = 1, float rotSpeed = 1)
    {


        yield break;
    }


    /// <summary>
    /// 지정된 시간동안 기다렸다가 비활성화.
    /// </summary>
    /// <param name="_gameObject">비활성화될 오브젝트</param>
    /// <param name="_disableTime">대기 시간</param>
    /// <returns></returns>
    static public IEnumerator WaitAndDisable(GameObject _gameObject, float _disableTime)
    {
        Timer timer = new Timer(_disableTime);


        while (true)
        {
            timer.Update();

            if (timer.IsTimeOver())
            {
                _gameObject.SetActive(false);
                break;
            }

            yield return null;
        }

        yield break;
    }

    /// <summary>
    /// 색상 변화 애니메이션
    /// </summary>
    /// <param name="_image"></param>
    /// <param name="_startAlpha"></param>
    /// <param name="_desAlpha"></param>
    /// <param name="_speed"></param>
    /// <returns></returns>
    static public IEnumerator Color(GameObject _object, Vector4 _startColor,
        Vector4 _desColor, float _speed = 1.0f)
    {
        float nowLerp = 0.0f;
        Vector4 nowColor;

        Image img = _object.GetComponent<Image>();

        while (true)
        {
            nowColor = Vector4.Lerp(_startColor, _desColor, nowLerp);

            img.color = nowColor;

            if (nowLerp < 1.0f)
            {
                nowLerp += _speed * Time.deltaTime;
            }
            else
            {
                break;
            }
            yield return null;
        }
        yield break;
    }

}
