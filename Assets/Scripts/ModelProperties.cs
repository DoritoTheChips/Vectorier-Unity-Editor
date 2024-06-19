using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Vectorier/Model Properties")]
public class ModelProperties : MonoBehaviour
{
    [Range(0, 1)] public int Type;
    public bool UseLifeTime = false;
    public string LifeTime = "0";
}