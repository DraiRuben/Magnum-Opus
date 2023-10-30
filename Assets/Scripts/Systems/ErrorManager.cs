using TMPro;
using UnityEngine;

public class ErrorManager : MonoBehaviour
{
    public static ErrorManager instance;
    [SerializeField] private TextMeshProUGUI m_errorMessage;
    [SerializeField] private GameObject m_errorPrefab;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }
    public void DisplayError(string message)
    {
        m_errorMessage.transform.parent.gameObject.SetActive(true);
        m_errorMessage.text = message;
    }
    public void StopDisplayingMessage()
    {
        m_errorMessage.transform.parent.gameObject.SetActive(false);
    }
    public void RegisterInvalidOrderException(Order order, ActionExecutor origin)
    {
        Instantiate(m_errorPrefab, origin.transform.parent);
        switch (order)
        {
            case Order.Extend:
                DisplayError("Cannot extend, this object does not have an extendable arm");
                break;
            case Order.Retract:
                DisplayError("Cannot retract, this object does not have an extendable arm");
                break;
            case Order.Plus:
                DisplayError("Cannot move towards +, this object is not on tracks");
                break;
            case Order.Minus:
                DisplayError("Cannot move towards -, this object is not on tracks");
                break;
            case Order.RotLeft:
                DisplayError("Cannot rotate, this object is not an arm");
                break;
            case Order.RotRight:
                DisplayError("Cannot rotate, this object is not an arm");
                break;
            case Order.PivotRight:
                DisplayError("Cannot rotate hand, this object is not an arm");
                break;
            case Order.PivotLeft:
                DisplayError("Cannot rotate hand, this object is not an arm");
                break;
        }
    }

}
