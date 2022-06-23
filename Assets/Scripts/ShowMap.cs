using System.Linq;
using System.Xml;
using UnityEngine;
using UnityEditor;

public class ShowMap : MonoBehaviour
{
    public string map;
    public string sequence;
    GameObject lastContent;
    GameObject actualObject;
    GameObject dummyObject;

    [MenuItem("Vectorier/Render object sequence")]

    public static void RenderSequence()
    {
        int layer = -1;
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
                                Debug.Log("Found object with name " + content.Attributes.GetNamedItem("Name").Value + "and coordinates" + content.Attributes.GetNamedItem("X").Value + content.Attributes.GetNamedItem("Y").Value);
                                ConvertXmlToObject(content.Attributes.GetNamedItem("Name").Value, content.Attributes.GetNamedItem("X").Value, content.Attributes.GetNamedItem("Y").Value, layer);
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
            Debug.Log("Found Trigger : " + content.Attributes.GetNamedItem("Name").Value);
        else if (content.Name == "Area")
            Debug.Log("Found Trick : " + content.Attributes.GetNamedItem("Name").Value);
        else if (content.Name == "Object")
            Debug.Log("Found Object : " + content.Attributes.GetNamedItem("Name").Value);

        //Place the image using every information the xml provide (X, Y, Width, Height, ClassName)
        if (actualObject == null)
        {
            //Create a new GameObject with the selected object
            actualObject = Instantiate(new GameObject(object_name), new Vector3(-float.Parse(x) / 100, float.Parse(y) / 100, 0), Quaternion.identity);
            actualObject.name = object_name; //Name it correctly
        }

        // vv  If the content is an image  vv
        if (content.Name == "Image")
        {
            lastContent = Instantiate(
                dummyObject = new GameObject(content.Attributes.GetNamedItem("ClassName").Value), //Usage of ClassName value (To name the new object)
                new Vector3(float.Parse(content.Attributes.GetNamedItem("X").Value) / 100, -float.Parse(content.Attributes.GetNamedItem("Y").Value) / 100, 0), //Usage of X and Y value (divided by 100 to fit Unity scale)
                Quaternion.identity);

            lastContent.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/" + content.Attributes.GetNamedItem("ClassName").Value); //REUsage of ClassName value (To place the texture)

            //Check if the image is rotated (by checking if there is a Matrix node)
            if (content.HasChildNodes)
                //Get into the Matrix node
                foreach (XmlNode matrixNode in content.LastChild.FirstChild)
                {
                    if (matrixNode.Name == "Matrix")
                    {
                        //TODO : You can take a look at a more stable way to convert the right rotation
                        //Set the image rotation to the A value divided by 1.665 (very wonky but kinda work for now)
                        lastContent.transform.rotation = Quaternion.Euler(0, 0, int.Parse(matrixNode.Attributes.GetNamedItem("A").Value) / 1.665f * 2);
                        //Recenter the image correctly
                        lastContent.transform.position = new Vector3
                        (lastContent.transform.localPosition.x + float.Parse(matrixNode.Attributes.GetNamedItem("Tx").Value) / 100,
                        lastContent.transform.localPosition.y + -float.Parse(matrixNode.Attributes.GetNamedItem("D").Value) / 100,
                        0);
                    }
                }
        }

        // vv  If the content is a trigger  vv
        else if (content.Name == "Trigger")
        {
            lastContent = Instantiate(
            dummyObject = new GameObject(content.Attributes.GetNamedItem("Name").Value), //Usage of Name value (To name the new object)
            new Vector3(float.Parse(content.Attributes.GetNamedItem("X").Value) / 100, -float.Parse(content.Attributes.GetNamedItem("Y").Value) / 100, 0), //Usage of X and Y value (divided by 100 to fit Unity scale)
            Quaternion.identity);

            lastContent.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/trigger"); //REUsage of ClassName value (To place the texture)
        }

        // vv  If the content is a Trick  vv
        else if (content.Name == "Area" || content.Name == "Object" && content.OuterXml.Contains("Trigger"))
        {
            lastContent = Instantiate(
            dummyObject = new GameObject(content.Attributes.GetNamedItem("Name").Value), //Usage of Name value (To name the new object)
            new Vector3(float.Parse(content.Attributes.GetNamedItem("X").Value) / 100, -float.Parse(content.Attributes.GetNamedItem("Y").Value) / 100, 0), //Usage of X and Y value (divided by 100 to fit Unity scale)
            Quaternion.identity);
            if (content.Attributes.GetNamedItem("Name").Value.Contains("Trigger") && Resources.Load<Sprite>("Textures/tricks/TRACK_" + content.Attributes.GetNamedItem("ItemName").Value))
                lastContent.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/tricks/TRACK_" + content.Attributes.GetNamedItem("ItemName").Value); //REUsage of ClassName value (To place the texture)
            else
                lastContent.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/trick"); //REUsage of ClassName value (To place the texture)
        }

        // vv  Universal action  vv
        lastContent.GetComponent<SpriteRenderer>().transform.parent = actualObject.transform; //Place the new image into the selected object
        if (lastContent.GetComponent<SpriteRenderer>().sprite.name.Contains("TRICK"))
            lastContent.transform.localScale = new Vector3(1, 1, 0);
        else if (content.Name != "Object")
            lastContent.transform.localScale = new Vector3(float.Parse(content.Attributes.GetNamedItem("Width").Value) / lastContent.GetComponent<SpriteRenderer>().sprite.texture.width, float.Parse(content.Attributes.GetNamedItem("Height").Value) / lastContent.GetComponent<SpriteRenderer>().sprite.texture.height, 0); //Usage of Width and Height value
        actualObject.tag = "Object"; //VERY IMPORTANT : Every GameObject with the tag "Object" will be counted in the final build, else ignored.
        lastContent.GetComponent<SpriteRenderer>().sortingOrder = layer;
        DestroyImmediate(dummyObject); //Remove duplicated content
    }
}