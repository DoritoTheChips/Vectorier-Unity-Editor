using UnityEngine;
using UnityEditor;
using System.Xml;

public class ConvertXmlObject : MonoBehaviour
{
    public string objectToConvert;
    int length;
    bool StopCounting;
    GameObject lastImage;

    [MenuItem("Vectorier/objects.xml")]
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
        //TODO : Fix the duplicate object
        //Place the image using every information the xml provide (X, Y, Width, Height, ClassName)

        lastImage = Instantiate(
        new GameObject(images.Attributes.GetNamedItem("ClassName").Value), //Usage of ClassName value (To name the new object)
        new Vector3(float.Parse(images.Attributes.GetNamedItem("X").Value), float.Parse(images.Attributes.GetNamedItem("Y").Value), 0), //Usage of X and Y value
        Quaternion.identity);
        lastImage.transform.localScale = new Vector3(float.Parse(images.Attributes.GetNamedItem("Width").Value), float.Parse(images.Attributes.GetNamedItem("Height").Value), 0); //Usage of Width and Height value
        lastImage.AddComponent<SpriteRenderer>().sprite = Resources.Load<Sprite>("Textures/" + images.Attributes.GetNamedItem("ClassName").Value); //REUsage of ClassName value (To place the texture)
    }
}