using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MouseEvent : MonoBehaviour
{
    bool _isMouse = false;
    float _limitTime = 0.1f;
    float _checkTime = 0;
    Vector3 _sizeUp = new Vector3(0.005f, 0.005f, 0.005f);
    Vector3 _originScale;
    Color _originColor;

    void Start()
    {
        _originScale = gameObject.transform.localScale;
        _originColor = gameObject.GetComponent<Image>().color;
    }

    void Update()
    {
        if (_isMouse)
        {
            if (_limitTime > _checkTime)
            {
                gameObject.transform.localScale += _sizeUp;
                _checkTime += Time.deltaTime;
            }
        }
        else
        {
            gameObject.transform.localScale = _originScale;
        }
    }

    void OnMouseEnter()
    {
        _isMouse = true;

        Image image = gameObject.GetComponent<Image>();
        image.color = Color.gray;
    }
    void OnMouseExit()
    {
        _isMouse = false;
        _checkTime = 0;

        Image image = gameObject.GetComponent<Image>();
        image.color = _originColor;
    }



    public void MouseDown(GameObject gameObject)
    {
            Image image = gameObject.GetComponent<Image>();
            image.color = Color.black;
    }

    public void MouseUp(GameObject gameObject)
    {
        Image image = gameObject.GetComponent<Image>();
        image.color = _originColor;
    }
}
