using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Vectorier/Custom Zoom")]
public class CustomZoom : MonoBehaviour
{

    //Camera zoom range, Zoom Minimal = 0.65, Zoom80 = 0.8, Zoom Normal = 1, Zoom Max = 1.1
    [Tooltip("Zoom Minimal = 0.65, Zoom80 = 0.8, Zoom Normal = 1, Zoom Max = 1.1")][Range(0.30f, 1.5f)] public float ZoomAmount;

}
