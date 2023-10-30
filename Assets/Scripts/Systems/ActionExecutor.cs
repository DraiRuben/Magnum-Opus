using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public class ActionExecutor : MonoBehaviour
{
    [SerializeField] private bool m_isExtendable = false;
    [SerializeField] private bool m_hasHand = false;
    [SerializeField] private bool m_canRotate = false;
    [SerializeField] private bool m_isOnTracks = false;
    public static float s_cycleDuration = 1f;
    private ObjectDraggable m_draggable;
    private void Awake()
    {
        m_draggable = GetComponent<ObjectDraggable>();
    }

    public void ExecuteOrder(Order order)
    {
        switch (order)
        {
            case Order.Empty:
                break;
            case Order.RotLeft:
                if (m_canRotate)
                {
                    StartCoroutine(RotateRoutine(false,transform.parent));
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.RotRight:
                if (m_canRotate)
                {
                    StartCoroutine(RotateRoutine(true,transform.parent));
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.Extend:
                if (m_isExtendable)
                {

                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.Retract:
                if (m_isExtendable)
                {

                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.Plus:
                if (m_isOnTracks)
                {

                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
            case Order.Minus:
                if (m_isOnTracks)
                {

                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);

                }
                break;
            case Order.PivotRight:
                if (m_hasHand)
                {
                    StartCoroutine(RotateRoutine(true, m_draggable.m_arm.transform.parent)); 
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order,this);
                }
                break;
            case Order.PivotLeft:
                if (m_hasHand)
                {
                    StartCoroutine(RotateRoutine(false, m_draggable.m_arm.transform.parent));
                }
                else
                {
                    ErrorManager.instance.RegisterInvalidOrderException(order, this);
                }
                break;
        }
    }
    private IEnumerator RotateRoutine(bool right, Transform _toRotate)
    {
        float timer = 0f;
        Quaternion oldRotation = _toRotate.rotation;
        Quaternion newRotation = Quaternion.Euler(0, 0, _toRotate.rotation.eulerAngles.z + (right ? -60 : 60));
        while (timer < s_cycleDuration)
        {
            transform.parent.rotation = Quaternion.Slerp(oldRotation, newRotation, timer / s_cycleDuration);
            timer += Time.deltaTime;
            yield return null;
        }
    }
}