using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Vectorier/Trigger Settings")]
public class TriggerSettings : MonoBehaviour
{
    [TextArea(3, 10)]
    public string Content = @"<Init>
        <SetVariable Name=""$Active"" Value=""1""/>
        <SetVariable Name=""$Node"" Value=""COM""/>
        <SetVariable Name=""$AI"" Value=""0""/>
        <SetVariable Name=""Flag1"" Value=""0""/>
    </Init>
    <Template Name=""Control""/>";
}
