using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GateController : MonoBehaviour
{
    private Material gateMaterial;
    public static Color OpenedColor = new Color(0.3f, 0.9f, 0.6f, 1f);
    public static Color ClosedColor = new Color(0.1f, 0.35f, 0.2f, 1f); 
    
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
