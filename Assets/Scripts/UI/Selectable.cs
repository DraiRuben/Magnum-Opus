using UnityEngine;

public abstract class Selectable : MonoBehaviour
{
    public static bool s_IsSomethingSelected = false;
    public static bool s_CanPlayerSelect = true; //for when the actions are getting read
    [SerializeField] protected bool m_canBeSelected = true;
    protected bool m_isSelected = false;
    protected SpriteRenderer m_sprite;
    private void Awake()
    {
        m_sprite = GetComponent<SpriteRenderer>();
    }
    private void Start()
    {
        MapManager.instance.UnselectAllEvent.AddListener(Unselect);
    }
    protected virtual void Unselect()
    {
        m_isSelected = false;
    }
    public void TryInteract(bool releasedInput)
    {
        //don't want it to interact on mouse release if we didn't select the thing prior to this
        if (releasedInput && !m_isSelected && s_IsSomethingSelected || !s_CanPlayerSelect)
        {
            MapManager.instance.m_unselectAll = true;
            return;
        }
        else if (releasedInput && !m_isSelected) return;

        if (GetIsInteractable())
        {
            Interact();
        }
    }

    protected abstract void Interact();
    protected abstract bool GetIsInteractable();

}
