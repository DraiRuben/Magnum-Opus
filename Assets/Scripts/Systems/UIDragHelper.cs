using UnityEngine;
using UnityEngine.EventSystems;

public class UIDragHelper : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //this class is on every single UI Element, it's quite lightweight since those are only events and not updates
    //this class is here to determine if the mouse was on a ui element, useful when determining if a dragged object should be placed to reset
    public static UIDragHelper s_lastUIElementSetter;
    public static bool s_isOnUIElement = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        Enter();

    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Exit();
    }

    public void Enter()
    {
        if (s_lastUIElementSetter == null)
        {
            s_isOnUIElement = true;
            s_lastUIElementSetter = this;
        }
    }
    public void Exit()
    {
        if (s_lastUIElementSetter != null && s_lastUIElementSetter == this)
        {
            s_isOnUIElement = false;
            s_lastUIElementSetter = null;
        }
    }
}
