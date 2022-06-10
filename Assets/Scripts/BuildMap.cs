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
    public bool spawnHunter;
    public bool debugObjectWriting;
    public bool hunterPlaced;

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
                //Write every GameObject with tag "Object" in the build-map.xml
                foreach (GameObject objectInScene in GameObject.FindGameObjectsWithTag("Object"))
                    GameObject.FindObjectOfType<BuildMap>().ConvertToObject(node, xml, objectInScene);
            }
        }

        // vv  Build level directly into Vector (sweet !)  vv
        GameObject.FindObjectOfType<BuildMap>().StartDzip();
        GameObject.FindObjectOfType<BuildMap>().hunterPlaced = false;
        UnityEngine.Debug.Log("Building done !");
    }

    void ConvertToObject(XmlNode node, XmlDocument xml, GameObject objectInScene)
    {
        //Debug in log every object it write
        if (debugObjectWriting)
            UnityEngine.Debug.Log("Writing object : " + Regex.Replace(objectInScene.name, @" \((.*?)\)", string.Empty));

        //Spawn Hunter if the option is checked
        if (spawnHunter == true && hunterPlaced == false)
        {
            hunterPlaced = true;
            if (debugObjectWriting)
                UnityEngine.Debug.Log("Placing hunter spawn");
            XmlElement hunterSpawn = xml.CreateElement("Spawn");
            hunterSpawn.SetAttribute("X", "0");
            hunterSpawn.SetAttribute("Y", "0");
            hunterSpawn.SetAttribute("Name", "DefaultSpawn");
            hunterSpawn.SetAttribute("Animation", "JumpOff|18");
            node.FirstChild.AppendChild(hunterSpawn);
        }

        if (objectInScene.name != "Camera")
        {
            XmlElement element = xml.CreateElement("Object"); //Create a new node from scratch
            element.SetAttribute("Name", Regex.Replace(objectInScene.name, @" \((.*?)\)", string.Empty)); //Add an name
            element.SetAttribute("X", (objectInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
            element.SetAttribute("Y", (-objectInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
            node.FirstChild.AppendChild(element); //Place it into the Object node
            xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
        }
        else if (objectInScene.name == "Camera")
        {
            XmlElement element = xml.CreateElement("Camera"); //Create a new node from scratch
            element.SetAttribute("X", (objectInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
            element.SetAttribute("Y", (-objectInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
            node.FirstChild.AppendChild(element); //Place it into the Object node
            xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
        }
    }

    void StartDzip()
    {
        //Check if Vector.exe is running - if yes, close it
        System.Diagnostics.Process[] p1 = System.Diagnostics.Process.GetProcesses();
        foreach (System.Diagnostics.Process pro in p1)
        {
            if (pro.ProcessName == "Vector")
            {
                UnityEngine.Debug.LogWarning("Vector.exe is still open !! Closing it... (be carful next time)");
                pro.Kill();
            }
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
    }
}
