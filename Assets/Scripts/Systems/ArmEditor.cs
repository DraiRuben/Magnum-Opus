using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArmEditor : MonoBehaviour
{
    private int m_armLength = 1;
    private int armLength 
    { 
        get 
        { 
            return m_armLength; 
        } 
        set 
        { 
            m_armLength = value; 
            //lengthen arm
        } 
    }
}
