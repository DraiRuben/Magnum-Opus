using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Selectable: MonoBehaviour
{
    public static bool s_IsSomethingSelected = false;
    [SerializeField] protected bool m_canBeSelected = true;
    protected bool m_isSelected = false;
    public void TryInteract(bool releasedInput)
    {
        //don't want it to interact on mouse release if we didn't select the thing prior to this
        if (releasedInput && !m_isSelected) return;
        
        if (GetIsInteractable())
        {
            Interact();
        }
    }

    protected abstract void Interact();
    protected abstract bool GetIsInteractable();

}
