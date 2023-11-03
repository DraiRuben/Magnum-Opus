using UnityEngine;
using UnityEngine.InputSystem;

public class UIControls : MonoBehaviour
{
    public void Click(InputAction.CallbackContext ctx)
    {
        if (ctx.performed || ctx.canceled)
        {
            RaycastHit2D Hit = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Mouse.current.position.ReadValue()), float.PositiveInfinity, LayerMask.GetMask("Mechanisms", "Action"));
            if (Hit.collider != null)
            {
                Hit.collider.GetComponent<Selectable>().TryInteract(ctx.canceled);
            }
            else
            {
                MapManager.instance.m_unselectAll = true;
            }
        }
    }
}
