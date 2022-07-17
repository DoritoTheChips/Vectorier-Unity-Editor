using System.Linq;
using System.Xml;
using System;
using System.Globalization;
using UnityEngine;
using UnityEditor;

public class ShowMap : MonoBehaviour
{
    public string map;
    public string sequence;
    GameObject actualObject;
    GameObject lastContent;
    GameObject box;

    [MenuItem("Vectorier/Render object sequence")]

    public static void RenderSequence()
    {
        int layer = 0;
        Debug.Log("Rendering...");
        XmlDocument mapName = new XmlDocument();
        mapName.Load(Application.dataPath + "/XML/" + GameObject.FindObjectOfType<ShowMap>().map);
        foreach (XmlNode node in mapName.DocumentElement.SelectSingleNode("/Root/Objects"))
            {
                //Check if the object has the correct name
                if (node.Name == "Object") 
                    if (node.Attributes.GetNamedItem("Name").Value == GameObject.FindObjectOfType<ShowMap>().sequence)
                    {
                        //Search for each node in the object 
                        foreach (XmlNode content in node.FirstChild)
                            if (content.Name == "Object") {
                                layer+=1;
                                if (content.Attributes["Name"] != null) {
                                    Debug.Log("Found object with name " + content.Attributes.GetNamedItem("Name").Value + " and coordinates " + content.Attributes.GetNamedItem("X").Value + " " + content.Attributes.GetNamedItem("Y").Value);
                                    ConvertXmlToObject(content.Attributes.GetNamedItem("Name").Value, content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value, layer);
                                }
                                else {
                                    Debug.Log("Found object with coordinates " + content.Attributes.GetNamedItem("X").Value + " " + content.Attributes.GetNamedItem("Y").Value);
                                    foreach (XmlNode child in content.FirstChild)
                                        {
                                            if (child.Name == "Image")
                                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(child, "Unnamed-object", content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value, layer);
                                            else if (child.Name == "Trigger")
                                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(child, "Unnamed-object", content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value, layer);
                                            else if (child.Name == "Area")
                                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(child, "Unnamed-object", content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value, layer);
                                            else if (child.Name == "Object")
                                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(child, "Unnamed-object", content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value, layer);
                                            else if (child.Name == "Platform")
                                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(child, "Unnamed-object", content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value, layer);
                                        }
                                }
                            }
                            else if (content.Name == "Image") {
                                layer+=1;
                                Debug.Log("Found image with name " + content.Attributes.GetNamedItem("ClassName").Value + " and coordinates " + content.Attributes.GetNamedItem("X").Value + " " + content.Attributes.GetNamedItem("Y").Value);
                                GameObject.FindObjectOfType<ShowMap>().actualObject = new GameObject(content.Attributes.GetNamedItem("ClassName").Value);
                                GameObject.FindObjectOfType<ShowMap>().actualObject.name = content.Attributes.GetNamedItem("ClassName").Value;
                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, content.Attributes.GetNamedItem("ClassName").Value, content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value, layer);
                                GameObject.FindObjectOfType<ShowMap>().actualObject = null;
                            }
                            else if (content.Name == "Trigger" || content.Name == "Area") {
                                layer+=1;
                                if (content.Name == "Trigger")
                                    Debug.Log("Found trigger with name " + content.Attributes.GetNamedItem("Name").Value + " and coordinates " + content.Attributes.GetNamedItem("X").Value + " " + content.Attributes.GetNamedItem("Y").Value);
                                else 
                                    Debug.Log("Found area with name " + content.Attributes.GetNamedItem("Name").Value + " and coordinates " + content.Attributes.GetNamedItem("X").Value + " " + content.Attributes.GetNamedItem("Y").Value);
                                GameObject.FindObjectOfType<ShowMap>().actualObject = new GameObject(content.Attributes.GetNamedItem("Name").Value);
                                GameObject.FindObjectOfType<ShowMap>().actualObject.name = content.Attributes.GetNamedItem("Name").Value;
                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, content.Attributes.GetNamedItem("Name").Value, content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value, layer);
                                GameObject.FindObjectOfType<ShowMap>().actualObject = null;
                            }
                    }

            }
    }

    static void ConvertXmlToObject(string object_name, string x,  string y, int layer)
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
                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, object_name, x, y, layer);
                            else if (content.Name == "Trigger")
                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, object_name, x, y, layer);
                            else if (content.Name == "Area")
                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, object_name, x, y, layer);
                            else if (content.Name == "Object")
                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, object_name, x, y, layer);
                            else if (content.Name == "Platform")
                                GameObject.FindObjectOfType<ShowMap>().InstantiateObject(content, object_name, x, y, layer);
                        }
                    }
            }
            doc_num += 1;
        }
        GameObject.FindObjectOfType<ShowMap>().actualObject = null;
        Debug.Log("Convert done !");
    }

    void InstantiateObject(XmlNode content, string object_name, string x, string y, int layer)
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
            ConvertXmlToObject(content.Attributes.GetNamedItem("Name").Value, content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value, layer);
        else 
        {
            if (actualObject == null)
            {
                //Create a new GameObject with the selected object
                actualObject = new GameObject(object_name);
                actualObject.transform.position = new Vector3(float.Parse(x) / 100, -float.Parse(y) / 100, 0);
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
                            //TODO : You can take a look at a more stable way to convert the right rotation
                            //Set the image rotation to the A value divided by 1.665 (very wonky but kinda work for now)
                            lastContent.transform.rotation = Quaternion.Euler(0, float.Parse(content.Attributes.GetNamedItem("Width").Value) / float.Parse(matrixNode.Attributes.GetNamedItem("A").Value, CultureInfo.InvariantCulture) * 180f, 0);
                            //Recenter the image correctly
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
