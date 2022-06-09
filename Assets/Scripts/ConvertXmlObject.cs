using System;
using System.Xml;
using UnityEngine;
using UnityEditor;

public class ConvertXmlObject : MonoBehaviour
{
    public string objectToConvert;
    int length;
    float zRot;
    bool StopCounting;
    GameObject lastContent;
    GameObject actualObject;
    GameObject dummyObject;

    [MenuItem("Vectorier/Convert from objects.xml")]
    public static void ConvertXmlToObject()
    {
        Debug.Log("Converting...");

        //Open the object.xml 
        XmlDocument xml = new XmlDocument();
        xml.Load(Application.dataPath + "/XML/objects.xml");

        //Search for the selected object in the object.xml
        foreach (XmlNode node in xml.DocumentElement.SelectSingleNode("/Root/Objects"))
        {
            //Check if the object as the correct name
            if (node.OuterXml.Contains('"' + GameObject.FindObjectOfType<ConvertXmlObject>().objectToConvert + '"'))
            {
                //Search for each node in the object 
                foreach (XmlNode content in node.FirstChild)
                {
                    //Check if it is an image node
                    if (content.Name == "Image")
                    {
                        Debug.Log("Found Image : " + content.Attributes.GetNamedItem("ClassName").Value);
                        // vv  Only way to call an external function in an static function  vv
                        GameObject.FindObjectOfType<ConvertXmlObject>().InstantiateObject(content);
                    }
                    //Check if it is an trigger node
                    else if (content.Name == "Trigger")
                    {
                        Debug.Log("Found Trigger : " + content.Attributes.GetNamedItem("Name").Value);
                        GameObject.FindObjectOfType<ConvertXmlObject>().InstantiateObject(content);
                    }
                    //Check if it is an area (trick) node
                    else if (content.Name == "Area")
                    {
                        Debug.Log("Found Trick : " + content.Attributes.GetNamedItem("Name").Value);
                        GameObject.FindObjectOfType<ConvertXmlObject>().InstantiateObject(content);
                    }
                }
            }
        }
        GameObject.FindObjectOfType<ConvertXmlObject>().actualObject = null;
        Debug.Log("Convert done !");
    }

    void InstantiateObject(XmlNode content)
    {
        //Place the image using every information the xml provide (X, Y, Width, Height, ClassName)
        if (actualObject == null)
        {
            //Create a new GameObject with the selected object
            actualObject = Instantiate(new GameObject(GameObject.FindObjectOfType<ConvertXmlObject>().objectToConvert), new Vector3(0, 0, 0), Quaternion.identity);
            DestroyImmediate(GameObject.Find(GameObject.FindObjectOfType<ConvertXmlObject>().objectToConvert)); //Destroy duplicate
            actualObject.name = GameObject.FindObjectOfType<ConvertXmlObject>().objectToConvert; //Name it correctly
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
        else if (content.Name == "Area")
        {
            lastContent = Instantiate(
            dummyObject = new GameObject(content.Attributes.GetNamedItem("Name").Value), //Usage of Name value (To name the new object)
            new Vector3(float.Parse(content.Attributes.GetNamedItem("X").Value) / 100, -float.Parse(content.Attributes.GetNamedItem("Y").Value) / 100, 0), //Usage of X and Y value (divided by 100 to fit Unity scale)
            Quaternion.identity);

            lastContent.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/trick"); //REUsage of ClassName value (To place the texture)
        }

        // vv  Universal action  vv
        lastContent.GetComponent<SpriteRenderer>().transform.parent = actualObject.transform; //Place the new image into the selected object
        lastContent.transform.localScale = new Vector3(float.Parse(content.Attributes.GetNamedItem("Width").Value) / lastContent.GetComponent<SpriteRenderer>().sprite.texture.width, float.Parse(content.Attributes.GetNamedItem("Height").Value) / lastContent.GetComponent<SpriteRenderer>().sprite.texture.height, 0); //Usage of Width and Height value
        actualObject.tag = "Object"; //VERY IMPORTANT : Every GameObject with the tag "Object" will be counted in the final build, else ignored.
        DestroyImmediate(dummyObject); //Remove duplicated content
    }
}