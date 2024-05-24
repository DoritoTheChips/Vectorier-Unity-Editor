using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AddComponentMenu("Vectorier/Trigger Settings")]
public class TriggerSettings : MonoBehaviour
{
    [TextArea(3, 10)]
    public string Init = @"<SetVariable Name=""$Active"" Value=""1""/>
        <SetVariable Name=""$Node"" Value=""COM""/>
        <SetVariable Name=""$AI"" Value=""0""/>
        <SetVariable Name=""Flag1"" Value=""0""/>";

    public enum TemplateList
    {
        NoneType,
        AI_noFollow,
        AI_Follow,
        CameraZoom,
        CameraSmoothness,
        CameraFollow,
        ForcedAnimation,
        Death,
        Control,
        TriggerLoss,
        TriggerVictory,
        ModelAnimation,
        Respawn,
        Respawn_OnScreen,
        RespawnHelper,
        RespawnHunterMode
    }

    [SerializeField]
    public TemplateList Template;
}
