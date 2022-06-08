using System.Security.Cryptography;
using System.Xml;
using UnityEngine;
using UnityEditor;

public class ConvertXmlObject : MonoBehaviour
{
    public string objectToConvert;
    int length;
    float zRot;
    bool StopCounting;
    GameObject lastImage;
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
                foreach (XmlNode images in node.FirstChild)
                {
                    //Check if it is an image node
                    if (images.Name == "Image")
                    {
                        Debug.Log("FoundImage : " + images.Attributes.GetNamedItem("ClassName").Value);
                        // vv  Only way to call an external function in an static function  vv
                        GameObject.FindObjectOfType<ConvertXmlObject>().InstantiateImage(images);
                    }
                }
            }
        }
        Debug.Log("Convert done !");
    }

    void InstantiateImage(XmlNode images)
    {
        //Place the image using every information the xml provide (X, Y, Width, Height, ClassName)
        if (actualObject == null)
        {
            //Create a new GameObject with the selected object
            actualObject = Instantiate(new GameObject(GameObject.FindObjectOfType<ConvertXmlObject>().objectToConvert), new Vector3(0, 0, 0), Quaternion.identity);
            DestroyImmediate(GameObject.Find(GameObject.FindObjectOfType<ConvertXmlObject>().objectToConvert)); //Destroy duplicate
            actualObject.name = GameObject.FindObjectOfType<ConvertXmlObject>().objectToConvert; //Name it correctly
        }

        lastImage = Instantiate(
        dummyObject = new GameObject(images.Attributes.GetNamedItem("ClassName").Value), //Usage of ClassName value (To name the new object)
        new Vector3(float.Parse(images.Attributes.GetNamedItem("X").Value) / 100, float.Parse(images.Attributes.GetNamedItem("Y").Value) / 100, 0), //Usage of X and Y value (divided by 100 to fit Unity scale)
        Quaternion.identity);

        lastImage.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/" + images.Attributes.GetNamedItem("ClassName").Value); //REUsage of ClassName value (To place the texture)
        lastImage.GetComponent<SpriteRenderer>().transform.parent = actualObject.transform; //Place the new image into the selected object
        lastImage.transform.localScale = new Vector3(float.Parse(images.Attributes.GetNamedItem("Width").Value) / lastImage.GetComponent<SpriteRenderer>().sprite.texture.width, float.Parse(images.Attributes.GetNamedItem("Height").Value) / lastImage.GetComponent<SpriteRenderer>().sprite.texture.height, 0); //Usage of Width and Height value

        //Check if the image is rotated (by checking if there is a Matrix node)
        if (images.HasChildNodes)
            //Get into the Matrix node
            foreach (XmlNode matrixNode in images.LastChild.FirstChild)
            {
                if (matrixNode.Name == "Matrix")
                {
                    //Set the image rotation to the A value divided by 1.665 (very wonky but kinda work for now)
                    lastImage.transform.rotation = Quaternion.Euler(0, 0, float.Parse(matrixNode.Attributes.GetNamedItem("A").Value) / 1.665f);
                    //Recenter the image correctly
                    lastImage.transform.position = new Vector3
                    (lastImage.transform.position.x + float.Parse(matrixNode.Attributes.GetNamedItem("Tx").Value) / 100,
                    lastImage.transform.position.y + float.Parse(matrixNode.Attributes.GetNamedItem("D").Value) / 100,
                    0);
                }
            }
        DestroyImmediate(dummyObject); //Remove duplicated images
    }
}