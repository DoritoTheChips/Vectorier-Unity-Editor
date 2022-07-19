using System.Linq;
using System.Xml;
using System;
using System.Globalization;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class ShowMap : MonoBehaviour
{
    public string level_name;
    int layer;
    GameObject actualObject;
    GameObject lastContent;
    GameObject lv;
    GameObject part;

    [MenuItem("Vectorier/Render object sequence")]

    public static void RenderMap()
    {
        List<string> buildings = new List<string>();
        List<string> objects = new List<string>();

        Debug.Log("Rendering level " + GameObject.FindObjectOfType<ShowMap>().level_name);
        GameObject.FindObjectOfType<ShowMap>().lv = new GameObject(GameObject.FindObjectOfType<ShowMap>().level_name);
        GameObject.FindObjectOfType<ShowMap>().lv.name = GameObject.FindObjectOfType<ShowMap>().level_name;

        XmlDocument level = new XmlDocument();
        level.Load(Application.dataPath + "/XML/" + GameObject.FindObjectOfType<ShowMap>().level_name);
        foreach (XmlNode node in level.DocumentElement.SelectSingleNode("/Root/Sets"))
        {
            buildings.Add(node.Attributes.GetNamedItem("FileName").Value);
            XmlDocument building = new XmlDocument();
            building.Load(Application.dataPath + "/XML/" + node.Attributes.GetNamedItem("FileName").Value);
            foreach (XmlNode b_node in building.DocumentElement.SelectSingleNode("/Root/Sets"))
                objects.Add(b_node.Attributes.GetNamedItem("FileName").Value);
        }
        foreach (XmlNode node in level.DocumentElement.SelectSingleNode("/Root/Track"))
        {
            if (node.Name == "Object")
            if (node.HasChildNodes) 
                if (node.FirstChild.FirstChild.Attributes["Name"] != null) 
                    foreach (XmlNode content in node.FirstChild) 
                {
                    if (content.Name == "Object") {
                        GameObject.FindObjectOfType<ShowMap>().layer+=1;
                        bool foundInBuildings = false;
                        foreach (string building_name in buildings) {
                            XmlDocument building = new XmlDocument();
                            building.Load(Application.dataPath + "/XML/" + building_name);
                            foreach (XmlNode b_node in building.DocumentElement.SelectSingleNode("/Root/Objects"))
                            {
                                //Check if the object has the correct name
                                if (b_node.Name == "Object")
                                    if (b_node.Attributes["Name"] != null)
                                        if (b_node.Attributes.GetNamedItem("Name").Value == content.Attributes.GetNamedItem("Name").Value) 
                                        {
                                            Debug.Log("Rendering part " + content.Attributes.GetNamedItem("Name").Value + " at X=" + content.Attributes.GetNamedItem("X").Value + " and Y=" + content.Attributes.GetNamedItem("Y").Value);
                                            foundInBuildings = true;
                                            RenderSequence(content.Attributes.GetNamedItem("Name").Value, building_name, content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value);
                                        }

                            }
                        }
                        if (foundInBuildings == false)
                        {
                            GameObject.FindObjectOfType<ShowMap>().layer+=1;
                            Debug.Log("Rendering object " + content.Attributes.GetNamedItem("Name").Value + " at X=" + content.Attributes.GetNamedItem("X").Value + " and Y=" + content.Attributes.GetNamedItem("Y").Value);
                            ConvertXmlToObject(content.Attributes.GetNamedItem("Name").Value, content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value);
                        }
                    }
                }
                else
                    foreach (XmlNode empty_object in node.FirstChild)
                        foreach (XmlNode empty_object_content in empty_object.FirstChild)
                        {
                            if (empty_object_content.Name == "Object" && empty_object_content.Attributes["Name"] != null) {
                                GameObject.FindObjectOfType<ShowMap>().layer+=1;
                                bool foundInBuildings = false;
                                foreach (string building_name in buildings) {
                                    XmlDocument building = new XmlDocument();
                                    building.Load(Application.dataPath + "/XML/" + building_name);
                                    foreach (XmlNode b_node in building.DocumentElement.SelectSingleNode("/Root/Objects"))
                                    {
                                        //Check if the object has the correct name
                                        if (b_node.Name == "Object")
                                            if (b_node.Attributes["Name"] != null)
                                                if (b_node.Attributes.GetNamedItem("Name").Value == empty_object_content.Attributes.GetNamedItem("Name").Value) 
                                                {
                                                    Debug.Log("Rendering part " + empty_object_content.Attributes.GetNamedItem("Name").Value + " at X=" + empty_object_content.Attributes.GetNamedItem("X").Value + " and Y=" + empty_object_content.Attributes.GetNamedItem("Y").Value);
                                                    foundInBuildings = true;
                                                    RenderSequence(empty_object_content.Attributes.GetNamedItem("Name").Value, building_name, empty_object_content.Attributes.GetNamedItem("X").Value, empty_object_content.Attributes.GetNamedItem("Y").Value);
                                                }

                                    }
                                }
                                if (foundInBuildings == false)
                                {
                                    GameObject.FindObjectOfType<ShowMap>().layer+=1;
                                    Debug.Log("Rendering object " + empty_object_content.Attributes.GetNamedItem("Name").Value + " at X=" + empty_object_content.Attributes.GetNamedItem("X").Value + " and Y=" + empty_object_content.Attributes.GetNamedItem("Y").Value);
                                    ConvertXmlToObject(empty_object_content.Attributes.GetNamedItem("Name").Value, empty_object_content.Attributes.GetNamedItem("X").Value, empty_object_content.Attributes.GetNamedItem("Y").Value);
                                }
                            }
                        }
        }
    }
    
    static void RenderSequence(string seq_name, string building_name, string x, string y)
    {
        Debug.Log("Rendering...");
        XmlDocument building = new XmlDocument();
        building.Load(Application.dataPath + "/XML/" + building_name);
        foreach (XmlNode node in building.DocumentElement.SelectSingleNode("/Root/Objects"))
        {
            //Check if the object has the correct name
            if (node.Name == "Object") 
                if (node.Attributes.GetNamedItem("Name").Value == seq_name)
                {
                    GameObject.FindObjectOfType<ShowMap>().part = new GameObject(seq_name);
                    GameObject.FindObjectOfType<ShowMap>().part.name = seq_name;
                    GameObject.FindObjectOfType<ShowMap>().part.transform.SetParent(GameObject.FindObjectOfType<ShowMap>().lv.transform);
                    GameObject.FindObjectOfType<ShowMap>().part.transform.localPosition = new Vector3(float.Parse(x) / 100, -float.Parse(y) / 100, 0);
                    //Search for each node in the object 
                    foreach (XmlNode content in node.FirstChild)
                        if (content.Name == "Object") {
                            GameObject.FindObjectOfType<ShowMap>().layer+=1;
                            if (content.Attributes["Name"] != null) {
                                Debug.Log("Found object with name " + content.Attributes.GetNamedItem("Name").Value + " and coordinates " + content.Attributes.GetNamedItem("X").Value + " " + content.Attributes.GetNamedItem("Y").Value);
                                ConvertXmlToObject(content.Attributes.GetNamedItem("Name").Value, content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value);
                            }
                            else {
                                Debug.Log("Found object with coordinates " + content.Attributes.GetNamedItem("X").Value + " " + content.Attributes.GetNamedItem("Y").Value);
                                foreach (XmlNode child in content.FirstChild)
                                    {
                                        if (child.Name == "Image")
                                            GameObject.FindObjectOfType<ShowMap>().InstantiateObject(child, "Unnamed-object", content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value);
                                        else if (child.Name == "Trigger")
                                            GameObject.FindObjectOfType<ShowMap>().InstantiateObject(child, "Unnamed-object", content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value);
                                        else if (child.Name == "Area")
                                            GameObject.FindObjectOfType<ShowMap>().InstantiateObject(child, "Unnamed-object", content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value);
                                        else if (child.Name == "Object")
                                            GameObject.FindObjectOfType<ShowMap>().InstantiateObject(child, "Unnamed-object", content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value);
                                        else if (child.Name == "Platform")
                                            GameObject.FindObjectOfType<ShowMap>().InstantiateObject(child, "Unnamed-object", content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value);
                                    }
                            }
                        }
                        else if (content.Name == "Image") {
                            GameObject.FindObjectOfType<ShowMap>().layer+=1;
                            Debug.Log("Found image with name " + content.Attributes.GetNamedItem("ClassName").Value + " and coordinates " + content.Attributes.GetNamedItem("X").Value + " " + content.Attributes.GetNamedItem("Y").Value);
                            GameObject.FindObjectOfType<ShowMap>().actualObject = new GameObject(content.Attributes.GetNamedItem("ClassName").Value);
                            GameObject.FindObjectOfType<ShowMap>().actualObject.name = content.Attributes.GetNamedItem("ClassName").Value;
                            GameObject.FindObjectOfType<ShowMap>().actualObject.transform.SetParent(GameObject.FindObjectOfType<ShowMap>().part.transform);
                            GameObject.FindObjectOfType<ShowMap>().actualObject.transform.localPosition = new Vector3(0,0,0);
                            GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, content.Attributes.GetNamedItem("ClassName").Value, content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value);
                            GameObject.FindObjectOfType<ShowMap>().actualObject = null;
                        }
                        else if (content.Name == "Trigger" || content.Name == "Area") {
                            GameObject.FindObjectOfType<ShowMap>().layer+=1;
                            if (content.Name == "Trigger")
                                Debug.Log("Found trigger with name " + content.Attributes.GetNamedItem("Name").Value + " and coordinates " + content.Attributes.GetNamedItem("X").Value + " " + content.Attributes.GetNamedItem("Y").Value);
                            else 
                                Debug.Log("Found area with name " + content.Attributes.GetNamedItem("Name").Value + " and coordinates " + content.Attributes.GetNamedItem("X").Value + " " + content.Attributes.GetNamedItem("Y").Value);
                            GameObject.FindObjectOfType<ShowMap>().actualObject = new GameObject(content.Attributes.GetNamedItem("Name").Value);
                            GameObject.FindObjectOfType<ShowMap>().actualObject.name = content.Attributes.GetNamedItem("Name").Value;
                            GameObject.FindObjectOfType<ShowMap>().actualObject.transform.SetParent(GameObject.FindObjectOfType<ShowMap>().part.transform);
                            GameObject.FindObjectOfType<ShowMap>().actualObject.transform.localPosition = new Vector3(0,0,0);
                            GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, content.Attributes.GetNamedItem("Name").Value, content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value);
                            GameObject.FindObjectOfType<ShowMap>().actualObject = null;
                        }
                }

        }
        GameObject.FindObjectOfType<ShowMap>().part = null;
    }

    static void ConvertXmlToObject(string object_name, string x,  string y)
    {
        Debug.Log("Converting...");

        //Load all object XMLs
        XmlDocument obj = new XmlDocument();
        obj.Load(Application.dataPath + "/XML/objects.xml");
        bool objectFound = false;
        int doc_num = 0;

        //Search for the selected object in the object XMLs
        while (objectFound == false & doc_num < 3) 
        {
            if (doc_num == 0) 
                obj.Load(Application.dataPath + "/XML/objects.xml");
            else if (doc_num == 1) 
                obj.Load(Application.dataPath + "/XML/objects_downtown.xml");
            else if (doc_num == 2) 
                obj.Load(Application.dataPath + "/XML/objects_construction.xml");
            foreach (XmlNode node in obj.DocumentElement.SelectSingleNode("/Root/Objects"))
            {
                //Check if the object has the correct name
                if (node.Name == "Object")
                    if (node.Attributes.GetNamedItem("Name").Value == object_name)
                    {
                        objectFound = true;
                        //Search for each node in the object 
                        foreach (XmlNode content in node.FirstChild)
                        {
                            if (content.Name == "Image")
                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, object_name, x, y);
                            else if (content.Name == "Trigger")
                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, object_name, x, y);
                            else if (content.Name == "Area")
                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, object_name, x, y);
                            else if (content.Name == "Object")
                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, object_name, x, y);
                            else if (content.Name == "Platform")
                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, object_name, x, y);
                        }
                    }
            }
            doc_num += 1;
        }
        GameObject.FindObjectOfType<ShowMap>().actualObject = null;
        Debug.Log("Convert done !");
    }

    void InstantiateObject(XmlNode content, string object_name, string x, string y)
    {
        //Debug all content found in the object
        if (content.Name == "Image")
            Debug.Log("Found Image : " + content.Attributes.GetNamedItem("ClassName").Value);
        else if (content.Name == "Trigger")
            if (content.Attributes["Name"] != null)
                Debug.Log("Found Trigger : " + content.Attributes.GetNamedItem("Name").Value);
            else
                Debug.Log("Found Trigger : " + "Trigger-" + object_name);
        else if (content.Name == "Area")
            Debug.Log("Found Trick : " + content.Attributes.GetNamedItem("Name").Value);
        else if (content.Name == "Object")
            Debug.Log("Found Object : " + content.Attributes.GetNamedItem("Name").Value);

        //Place the image using every information the xml provide (X, Y, Width, Height, ClassName)
        if (content.Name == "Object" && !content.Attributes.GetNamedItem("Name").Value.Contains("Trigger"))
            ConvertXmlToObject(content.Attributes.GetNamedItem("Name").Value, content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value);
        else 
        {
            if (actualObject == null)
            {
                //Create a new GameObject with the selected object
                actualObject = new GameObject(object_name);
                if (part != null )
                    actualObject.transform.SetParent(part.transform);
                else
                    actualObject.transform.SetParent(lv.transform);
                actualObject.transform.localPosition = new Vector3(float.Parse(x) / 100, -float.Parse(y) / 100, 0);
                actualObject.name = object_name; //Name it correctly
            }

            // vv  If the content is an image  vv
            if (content.Name == "Image")
            {
                lastContent = new GameObject(content.Attributes.GetNamedItem("ClassName").Value); //Usage of ClassName value (To name the new object)

                lastContent.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/" + content.Attributes.GetNamedItem("ClassName").Value); //REUsage of ClassName value (To place the texture)

                //Check if the image is rotated (by checking if there is a Matrix node)
                if (content.HasChildNodes)
                    //Get into the Matrix node
                    foreach (XmlNode matrixNode in content.LastChild.FirstChild)
                    {
                        if (matrixNode.Name == "Matrix" && matrixNode.Attributes.GetNamedItem("A").Value != content.Attributes.GetNamedItem("Width").Value)
                        {
                            if (matrixNode.Attributes.GetNamedItem("A").Value != "")
                            lastContent.transform.rotation = Quaternion.Euler(0, float.Parse(content.Attributes.GetNamedItem("Width").Value, CultureInfo.InvariantCulture) / float.Parse(matrixNode.Attributes.GetNamedItem("A").Value, CultureInfo.InvariantCulture) * 180f, 0);
                        }
                    }
            }

            // vv  If the content is a trigger  vv
            else if (content.Name == "Trigger")
            {
                if (content.Attributes["Name"] != null)
                    lastContent = new GameObject(content.Attributes.GetNamedItem("Name").Value); //Usage of Name value (To name the new object)
                else
                    lastContent = new GameObject("Trigger-" + object_name); //Usage of Statistic value (To name the new object)
                lastContent.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/trigger"); //REUsage of ClassName value (To place the texture)
            }

            // vv  If the content is a Trick  vv
            else if (content.Name == "Area" || content.Name == "Object" && content.OuterXml.Contains("Trigger"))
            {
                lastContent = new GameObject(content.Attributes.GetNamedItem("Name").Value); //Usage of Name value (To name the new object)
                if (content.Attributes.GetNamedItem("Name").Value.Contains("Trigger") && content.Attributes["ItemName"] != null)
                    lastContent.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/tricks/TRACK_" + content.Attributes.GetNamedItem("ItemName").Value); //REUsage of ClassName value (To place the texture)
                else
                    lastContent.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/trigger"); //REUsage of ClassName value (To place the texture)
            }
            
            // vv  If the content is a hitbox  vv
            else if (content.Name == "Platform") {
                lastContent = new GameObject("Platform-" + object_name);
                lastContent.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/trick");
            }

            // vv  Universal action  vv
            lastContent.GetComponent<SpriteRenderer>().transform.SetParent(actualObject.transform); //Place the new image into the selected object
            if (content.Name != "Object")
                lastContent.transform.localPosition = new Vector3(float.Parse(content.Attributes.GetNamedItem("X").Value) / 100 + Math.Abs(lastContent.transform.rotation.y) * float.Parse(content.Attributes.GetNamedItem("Width").Value) / 100, -float.Parse(content.Attributes.GetNamedItem("Y").Value) / 100, 0);
            else
                lastContent.transform.localPosition = new Vector3(float.Parse(content.Attributes.GetNamedItem("X").Value) / 100, -float.Parse(content.Attributes.GetNamedItem("Y").Value) / 100, 0);
            if (lastContent.GetComponent<SpriteRenderer>().sprite.name.Contains("TRICK"))
                lastContent.transform.localScale = new Vector3(1, 1, 0);
            else if (content.Attributes["Width"] != null)
                lastContent.transform.localScale = new Vector3(float.Parse(content.Attributes.GetNamedItem("Width").Value) / lastContent.GetComponent<SpriteRenderer>().sprite.texture.width, float.Parse(content.Attributes.GetNamedItem("Height").Value) / lastContent.GetComponent<SpriteRenderer>().sprite.texture.height, 0); //Usage of Width and Height value
            actualObject.tag = "Object"; //VERY IMPORTANT : Every GameObject with the tag "Object" will be counted in the final build, else ignored.
            lastContent.GetComponent<SpriteRenderer>().sortingOrder = layer;
        }
    }
}