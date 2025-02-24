using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
    private Material gateMaterial;
    public static Color OpenedColor = new Color(0.6428487f, 1f, 0.476415f, 1f);
    public static Color ClosedColor = new Color(0.1079309f, 0.4056604f, 0f, 1f);
    
    private void Start() {
        gateMaterial = GetComponent<Renderer>().material;
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
