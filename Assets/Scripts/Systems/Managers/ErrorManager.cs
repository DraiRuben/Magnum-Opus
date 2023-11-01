using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ErrorManager : MonoBehaviour
{
    public static ErrorManager instance;
    [SerializeField] private TextMeshProUGUI m_errorMessage;
    [SerializeField] private GameObject m_errorPrefab;

    private List<GameObject> m_instantiatedErrorSquares = new();

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
    public void StopDisplayingError()
    {
        m_errorMessage.transform.parent.gameObject.SetActive(false);
        foreach (GameObject errorSquare in m_instantiatedErrorSquares)
        {
            Destroy(errorSquare);
        }
        m_instantiatedErrorSquares.Clear();
    }
    public void RegisterInvalidOrderException(Order order, ActionExecutor origin)
    {
        m_instantiatedErrorSquares.Add(Instantiate(m_errorPrefab, origin.transform.parent));
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
            case Order.Grab:
                DisplayError("Cannot grab, this object is not an arm");
                break;
            case Order.Drop:
                DisplayError("Cannot drop, this object is not an arm");
                break;
        }
    }
    public void RegisterCollisionException(GameObject Obj, GameObject Colliding)
    {
        m_instantiatedErrorSquares.Add(Instantiate(m_errorPrefab, Obj.transform.root));
        m_instantiatedErrorSquares.Add(Instantiate(m_errorPrefab, Colliding.transform.root));
        DisplayError("Collision between these two objects is not allowed");
    }
    //not litteral children as in transform children, this is only a joke variable name
    public void RegisterMultiGrabException(List<Ressource> problemChildren)
    {
        foreach (Ressource obj in problemChildren)
        {
            Instantiate(m_errorPrefab, obj.transform);
        }
        DisplayError("Cannot grab a ressource with multiple arms");
    }
}
