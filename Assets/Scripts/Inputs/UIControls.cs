using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UIControls : MonoBehaviour
{
    public void Click(InputAction.CallbackContext ctx)
    {
        if(ctx.performed || ctx.canceled)
        {
            RaycastHit2D Hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition), 5000, LayerMask.GetMask("Mechanisms"));
            if(Hit.collider != null)
            {
                Hit.collider.GetComponent<Selectable>().TryInteract(ctx.canceled);
            }
        }
    }
}
