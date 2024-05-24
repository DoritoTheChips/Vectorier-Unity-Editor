using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Vectorier/Dynamic")]
public class Dynamic : MonoBehaviour
{
    public string TransformationName = "Transform_name";
    [Header("Movement")]
    [Tooltip("Move Duration in Second")] public float MoveDuration = 1.5f;
    [Tooltip("Move Delay in Second")] public float Delay = 0f;
    [Tooltip("Value should be half of the amount of Move X")] public float SupportXAxis = 0.0f;
    [Tooltip("Value should be half of the amount of Move Y")] public float SupportYAxis = 0.0f;
    [Tooltip("How much to move on X Axis")] public float MoveXAxis = 0.0f;
    [Tooltip("How much to move on Y Axis")] public float MoveYAxis = 0.0f;
}
