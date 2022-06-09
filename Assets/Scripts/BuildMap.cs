using System.Text.RegularExpressions;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Xml;
using System.Diagnostics;

public class BuildMap : MonoBehaviour
{
    public string vectorFilePath;
    public string mapToOverride = "DOWNTOWN_STORY_02";
    public bool autoLaunchGame;
    XmlNode objectToWrite;

    [MenuItem("Vectorier/BuildMap")]
    public static void Build()
    {
        UnityEngine.Debug.Log("Building...");
        //Erase last build
        File.Delete(Application.dataPath + "/XML/build-map.xml");
        File.Copy(Application.dataPath + "/XML/empty-map-DONT-MODIFY.xml", Application.dataPath + "/XML/build-map.xml");

        //Open the object.xml 
        XmlDocument xml = new XmlDocument();
        xml.Load(Application.dataPath + "/XML/build-map.xml");

        //Search for the selected object in the object.xml
        foreach (XmlNode node in xml.DocumentElement.SelectSingleNode("/Root/Track"))
        {
            if (node.Attributes.GetNamedItem("Factor").Value == "1")
            {
                foreach (GameObject objectInScene in GameObject.FindGameObjectsWithTag("Object"))
                {
                    //Write every Gameobject with tag "Object" in the build-map.xml
                    UnityEngine.Debug.Log("Writing object : " + Regex.Replace(objectInScene.name, @" \((.*?)\)", string.Empty));
                    GameObject.FindObjectOfType<BuildMap>().ConvertToObject(node, xml, objectInScene);
                }
            }
        }

        // vv  Build level directly into Vector (sweet !)  vv
        GameObject.FindObjectOfType<BuildMap>().StartDzip();
        UnityEngine.Debug.Log("Building done !");
    }

    void ConvertToObject(XmlNode node, XmlDocument xml, GameObject objectInScene)
    {
        XmlElement element = xml.CreateElement("Object"); //Create a new node from scratch
        element.SetAttribute("Name", Regex.Replace(objectInScene.name, @" \((.*?)\)", string.Empty)); //Add an name
        element.SetAttribute("X", ((int)(objectInScene.transform.position.x) * 100).ToString()); //Add X position (Refit into the Vector units)
        element.SetAttribute("Y", (-(int)(objectInScene.transform.position.y) * 100).ToString()); // Add Y position (Negative because Vector see the world upside down)
        node.FirstChild.AppendChild(element); //Place it into the Object node
        xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file
    }

    void StartDzip()
    {
        //Check if Vector.exe is running - if yes, close it
        Process[] pname = Process.GetProcessesByName("Vector.exe");
        if (pname.Length != 0)
        {
            UnityEngine.Debug.LogWarning("Vector.exe is still open !! Closing it... (be carful next time)");
            pname[0].Close();
            pname[0].WaitForExit();
        }

        //Start compressing levels into level_xml.dz
        Process process = Process.Start(Application.dataPath + "/XML/dzip/dzip.exe", Application.dataPath + "/XML/dzip/config-map.dcl");
        process.WaitForExit();

        //Move level_xml.dz if Vector path is found
        if (File.Exists(vectorFilePath + "/level_xml.dz"))
        {
            File.Delete(vectorFilePath + "/level_xml.dz");
            File.Copy(Application.dataPath + "/XML/dzip/level_xml.dz", vectorFilePath + "/level_xml.dz");
        }
        else UnityEngine.Debug.LogError("Level_xml.dz was not found !! Check if your Vector path is correct");
        /* if (autoLaunchGame)
            Process.Start(vectorFilePath + "/Vector.exe"); */
    }
}