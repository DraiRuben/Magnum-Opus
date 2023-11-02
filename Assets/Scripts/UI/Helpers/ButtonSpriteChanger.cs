using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ButtonSpriteChanger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private SpriteState m_state;
    private HoverState m_hoverState;
    private Image m_image;
    private Button m_button;

    [SerializeField] private Sprite m_hoverClicked;
    [SerializeField] private Sprite m_hoverUnclicked;
    [SerializeField] private Sprite m_Unclicked;
    [SerializeField] private Sprite m_Clicked;
    private void Awake()
    {
        m_image = GetComponent<Image>();
        m_button = GetComponent<Button>();
    }
    private void Start()
    {
        GetComponent<Button>().onClick.AddListener(delegate { SetState(m_state != SpriteState.Clicked); });
    }
    private enum SpriteState
    {
        Unclicked,
        Clicked
    }
    private enum HoverState
    {
        NotHovering,
        Hovering
    }
    private void UpdateSprite()
    {
        if (m_button.interactable)
        {
            if (m_state == SpriteState.Clicked)
            {
                if (m_hoverState == HoverState.Hovering)
                {
                    m_image.sprite = m_hoverClicked;
                }
                else
                {
                    m_image.sprite = m_Clicked;
                }
            }
            else
            {
                if (m_hoverState == HoverState.Hovering)
                {
                    m_image.sprite = m_hoverUnclicked;
                }
                else
                {
                    m_image.sprite = m_Unclicked;
                }
            }
        }

    }

    public void SetState(bool clicked)
    {
        m_state = !clicked ? SpriteState.Unclicked : SpriteState.Clicked;
        UpdateSprite();
    }
    public void ClearHoverState()
    {
        m_image.sprite = m_Unclicked;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        m_hoverState = HoverState.Hovering;

        if (m_button.interactable)
            UpdateSprite();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        m_hoverState = HoverState.NotHovering;
        if (m_button.interactable)
            UpdateSprite();
    }

}
