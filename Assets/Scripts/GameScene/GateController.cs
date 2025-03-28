using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
    private Material gateMaterial;
    public static Color OpenedColor = new Color(0.3f, 0.9f, 0.6f, 1f);
    public static Color ClosedColor; 
    
    private void Start() {
        gateMaterial = GetComponent<Renderer>().material;
        ClosedColor = gateMaterial.color;
    }
    
    public void IsOpen(bool winCondition)
    {
        if (winCondition)
        {
            gateMaterial.color = OpenedColor;
        }
        else 
        {
            gateMaterial.color = ClosedColor;
        }
    }


}
