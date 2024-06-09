using System.Diagnostics;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using UnityEditor;
using UnityEngine;
using System.Linq;
using System;
using UnityEditor.Experimental.GraphView;
using System.Xml.Linq;
using static UnityEngine.UI.CanvasScaler;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using System.Xml.XPath;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEditor.Animations;

public class BuildMap : MonoBehaviour
{

    public string vectorFilePath;

    // Level Settings
    [Header("Level Settings")]
    [Tooltip("Level that will get overridden.")] public string mapToOverride = "DOWNTOWN_STORY_02";
    [Tooltip("Music that will be played on the level.")] public string levelMusic = "music_dinamic";
    [Tooltip("Volume of the music.")] public string MusicVolume = "0.3";
    [Tooltip("Background Image")] public string customBackground = "v_bg";
    [Tooltip("Background Width")] public string bg_Width = "2121";
    [Tooltip("Background Height")] public string bg_Height = "1116";

    // Gameplay
    [Serializable]
    public class PlayerSettings
    {
        public string playerModelName = "Player";
        [Tooltip("Player's Spawn Name")] public string playerSpawnName = "PlayerSpawn";
        [Tooltip("Duration until the player appears.")] public float playerSpawnTime = 0;
        [Tooltip("Player Appearance (Default: 1)")] public string playerSkin = "1";
    }
    [Serializable]
    public class HunterSettings
    {
        public string hunterModelName = "Hunter";
        [Tooltip("Hunter's Spawn Name")] public string hunterSpawnName = "DefaultSpawn";
        [Tooltip("Time it takes for the hunter to spawn in.")] public float hunterSpawnTime = 0;
        [Tooltip("Hunter Respawn Name")] public string hunterAllowedSpawn = "Respawn";
        [Tooltip("Hunter Appearance (Default: hunter)")] public string hunterSkin = "hunter";
        [Tooltip("Hunter is able do to tricks")] public bool hunterTrickAllowed = false;
        [Tooltip("Shows hunter icon or not")] public bool hunterIcon = true;
        [Tooltip("Ai Number (Default: 1)")] public int hunterAIType = 1;
    }
    [Header("Gameplay")]
    [SerializeField] private PlayerSettings Player;
    [SerializeField] private HunterSettings Hunter;
    [Tooltip("Uses custom properties instead of prefixed (Will ignore the settings for player and hunter above.)")] public bool useCustomProperties;

    [TextArea(5, 20)]
    public string CustomModelProperties = @"<Model Name=""Player"" Type=""1"" Color=""0"" BirthSpawn=""PlayerSpawn"" AI=""0"" Time=""0"" Respawns=""Hunter"" ForceBlasts=""Hunter"" Trick=""1"" Item=""1"" Victory=""1"" Lose=""1""/>
    <Model Name=""Hunter"" Type=""0"" Color=""0"" BirthSpawn=""DefaultSpawn"" AI=""1"" Time=""0.8"" AllowedSpawns=""Respawn"" Skins=""hunter"" Murders=""Player"" Arrests=""Player"" Icon=""1""/>";


    // Miscellaneous
    [Header("Miscellaneous")]
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

                GameObject.FindObjectOfType<BuildMap>().SetLevelProperties(xml, node); //Set the properties into the level

                // Get all GameObjects with tag "Image", then arrange them based on sorting order
                GameObject[] imagesInScene = GameObject.FindGameObjectsWithTag("Image")
                                            .OrderBy(obj => obj.GetComponent<SpriteRenderer>().sortingOrder)
                                            .ToArray();

                //Write every GameObject with tag "Object", "Image", "Platform", "Area" and "Trigger" in the build-map.xml
                foreach (GameObject spawnInScene in GameObject.FindGameObjectsWithTag("Spawn"))
                {
                    GameObject.FindObjectOfType<BuildMap>().ConvertToSpawn(node, xml, spawnInScene);
                }

                foreach (GameObject imageInScene in imagesInScene)
                {
                    UnityEngine.Transform parent = imageInScene.transform.parent;
                    if (parent != null && parent.CompareTag("Dynamic"))
                    {
                        // If the parent has the tag "Dynamic" skip this GameObject and continue.
                        continue;
                    }

                    GameObject.FindObjectOfType<BuildMap>().ConvertToImage(node, xml, imageInScene);
                }

                foreach (GameObject objectInScene in GameObject.FindGameObjectsWithTag("Object"))
                {
                    UnityEngine.Transform parent = objectInScene.transform.parent;
                    if (parent != null && parent.CompareTag("Dynamic"))
                    {
                        // If the parent has the tag "Dynamic" skip this GameObject and continue.
                        continue;
                    }

                    GameObject.FindObjectOfType<BuildMap>().ConvertToObject(node, xml, objectInScene);
                }

                foreach (GameObject platformInScene in GameObject.FindGameObjectsWithTag("Platform"))
                {
                    UnityEngine.Transform parent = platformInScene.transform.parent;
                    if (parent != null && parent.CompareTag("Dynamic"))
                    {
                        // If the parent has the tag "Dynamic" skip this GameObject and continue.
                        continue;
                    }
                    GameObject.FindObjectOfType<BuildMap>().ConvertToPlatform(node, xml, platformInScene);
                }

                foreach (GameObject triggerInScene in GameObject.FindGameObjectsWithTag("Trigger"))
                {
                    UnityEngine.Transform parent = triggerInScene.transform.parent;
                    if (parent != null && parent.CompareTag("Dynamic"))
                    {
                        // If the parent has the tag "Dynamic" skip this GameObject and continue.
                        continue;
                    }
                    GameObject.FindObjectOfType<BuildMap>().ConvertToTrigger(node, xml, triggerInScene);
                }

                foreach (GameObject areaInScene in GameObject.FindGameObjectsWithTag("Area"))
                {
                    UnityEngine.Transform parent = areaInScene.transform.parent;
                    if (parent != null && parent.CompareTag("Dynamic"))
                    {
                        // If the parent has the tag "Dynamic" skip this GameObject and continue.
                        continue;
                    }
                    GameObject.FindObjectOfType<BuildMap>().ConvertToArea(node, xml, areaInScene);
                }

                foreach (GameObject modelInScene in GameObject.FindGameObjectsWithTag("Model"))
                {
                    UnityEngine.Transform parent = modelInScene.transform.parent;
                    if (parent != null && parent.CompareTag("Dynamic"))
                    {
                        // If the parent has the tag "Dynamic" skip this GameObject and continue.
                        continue;
                    }
                    GameObject.FindObjectOfType<BuildMap>().ConvertToModel(node, xml, modelInScene);
                }
                foreach (GameObject camInScene in GameObject.FindGameObjectsWithTag("Camera"))
                {
                    GameObject.FindObjectOfType<BuildMap>().ConvertToCamera(node, xml, camInScene); //Note: This is actually a trigger, but with camera zoom properties
                }

                foreach (GameObject dynamicInScene in GameObject.FindGameObjectsWithTag("Dynamic"))
                {
                    UnityEngine.Transform dynamicInSceneTransform = dynamicInScene.transform;
                    GameObject.FindObjectOfType<BuildMap>().ConvertToDynamic(node, xml, dynamicInScene, dynamicInSceneTransform);
                }
            }
            if (node.Attributes.GetNamedItem("Factor").Value == "0.5")
            {
                //Write every GameObject with tag "Backdrop" in the build-map.xml
                foreach (GameObject bdInScene in GameObject.FindGameObjectsWithTag("Backdrop"))
                {
                    GameObject.FindObjectOfType<BuildMap>().ConvertToBackdrop(node, xml, bdInScene);
                }
            }
            if (node.Attributes.GetNamedItem("Factor").Value == "1.001")
            {
                foreach (GameObject frontimageInScene in GameObject.FindGameObjectsWithTag("Top Image"))
                {
                    GameObject.FindObjectOfType<BuildMap>().ConvertToTopImage(node, xml, frontimageInScene);
                }
            }
        }

        // vv  Build level directly into Vector (sweet !)  vv
        GameObject.FindObjectOfType<BuildMap>().StartDzip();
        GameObject.FindObjectOfType<BuildMap>().hunterPlaced = false;
        UnityEngine.Debug.Log("Building done !");
    }

    void ConvertToTopImage(XmlNode node, XmlDocument xml, GameObject frontimageInScene)
    {

        if (frontimageInScene.name != "Camera")
        {
            XmlElement ielement = xml.CreateElement("Image"); //Create a new node from scratch
            ielement.SetAttribute("X", (frontimageInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
            ielement.SetAttribute("Y", (-frontimageInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
            ielement.SetAttribute("ClassName", Regex.Replace(frontimageInScene.name, @" \((.*?)\)", string.Empty)); //Add a name
            SpriteRenderer spriteRenderer = frontimageInScene.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Image Size in Width and Height
            {

                Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                Vector3 scale = frontimageInScene.transform.localScale; // Get the GameObject scale

                // Retrieve the image resolution of the sprite
                float width = bounds.size.x * 100;
                float height = bounds.size.y * 100;

                // Set the width and height accordingly to the scale in the editor
                ielement.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                ielement.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image

                // Set the Native resolution of sprite
                ielement.SetAttribute("NativeX", width.ToString()); //Native Resolution of the Image in X
                ielement.SetAttribute("NativeY", height.ToString()); //Native Resolution of the Image in Y
            }

            // create a new node
            XmlElement propertiesElement = xml.CreateElement("Properties");
            XmlElement staticElement = xml.CreateElement("Static");
            XmlElement matrixElement = xml.CreateElement("Matrix");

            // Check if the sprite image is flipped x,y or both
            if (spriteRenderer.flipX || spriteRenderer.flipY || (spriteRenderer.flipX && spriteRenderer.flipY))
            {
                Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                Vector3 scale = frontimageInScene.transform.localScale; // Get the GameObject scale

                float width = bounds.size.x * 100 * scale.x;
                float height = bounds.size.y * 100 * scale.y;
                float negative_width = width * -1;
                float negative_height = height * -1;

                // matrix, "A" represents X axis, while "D" represents the Y axis.
                if (spriteRenderer.flipX && spriteRenderer.flipY)
                {
                    matrixElement.SetAttribute("A", negative_width.ToString());
                    matrixElement.SetAttribute("B", "0");
                    matrixElement.SetAttribute("C", "0");
                    matrixElement.SetAttribute("D", negative_height.ToString());
                    matrixElement.SetAttribute("Tx", "0");
                    matrixElement.SetAttribute("Ty", "0");
                }
                else if (spriteRenderer.flipX)
                {
                    matrixElement.SetAttribute("A", negative_width.ToString());
                    matrixElement.SetAttribute("B", "0");
                    matrixElement.SetAttribute("C", "0");
                    matrixElement.SetAttribute("D", height.ToString());
                    matrixElement.SetAttribute("Tx", "0");
                    matrixElement.SetAttribute("Ty", "0");
                }
                else if (spriteRenderer.flipY)
                {
                    matrixElement.SetAttribute("A", width.ToString());
                    matrixElement.SetAttribute("B", "0");
                    matrixElement.SetAttribute("C", "0");
                    matrixElement.SetAttribute("D", negative_height.ToString());
                    matrixElement.SetAttribute("Tx", "0");
                    matrixElement.SetAttribute("Ty", "0");
                }

                //writes everything under the image node
                staticElement.AppendChild(matrixElement);
                propertiesElement.AppendChild(staticElement);
                ielement.AppendChild(propertiesElement);
            }
            node.FirstChild.AppendChild(ielement); //Place it into the Object node
            xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
        }
    }

    void ConvertToSpawn(XmlNode node, XmlDocument xml, GameObject spawnInScene)
    {

        Respawn RespawnComponent = spawnInScene.GetComponent<Respawn>(); //Respawn component
        Spawn Spawn = spawnInScene.GetComponent<Spawn>(); //spawn component
        XmlElement spawnElement = xml.CreateElement("Spawn");
        Spawn[] SpawnComponent = FindObjectsOfType<Spawn>();


        if (RespawnComponent != null)
        {
            // Root
            XmlElement objectElement = xml.CreateElement("Object");
            objectElement.SetAttribute("X", "0");
            objectElement.SetAttribute("Y", "0");

            // Content
            XmlElement contentElement = xml.CreateElement("Content");


            foreach (Spawn spawns in SpawnComponent)
            {
                GameObject gameObjwithSpawnComponent = spawns.gameObject; //check every game object that has the spawn component
                if (RespawnComponent.RespawnName == gameObjwithSpawnComponent.GetComponent<Spawn>().SpawnName)
                {
                    if (gameObjwithSpawnComponent.GetComponent<Spawn>().RefersToRespawn == true)
                    {
                        // spawn element
                        XmlElement spawnInsideElement = xml.CreateElement("Spawn");
                        spawnInsideElement.SetAttribute("X", (gameObjwithSpawnComponent.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                        spawnInsideElement.SetAttribute("Y", (-gameObjwithSpawnComponent.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
                        spawnInsideElement.SetAttribute("Name", gameObjwithSpawnComponent.GetComponent<Spawn>().SpawnName);
                        spawnInsideElement.SetAttribute("Animation", gameObjwithSpawnComponent.GetComponent<Spawn>().SpawnAnimation);
                        contentElement.AppendChild(spawnInsideElement);
                    }
                }
            }

            //trigger element
            XmlElement triggerElement = xml.CreateElement("Trigger");
            triggerElement.SetAttribute("Name", RespawnComponent.TriggerName);
            triggerElement.SetAttribute("X", (spawnInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
            triggerElement.SetAttribute("Y", (-spawnInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)


            SpriteRenderer spriteRenderer = spawnInScene.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Sprite Size in Width and Height
            {

                Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                Vector3 scale = spawnInScene.transform.localScale; // Get the GameObject scale

                // Retrieve the image resolution of the sprite
                float width = bounds.size.x * 100;
                float height = bounds.size.y * 100;

                // Set the width and height accordingly to the scale in the editor
                triggerElement.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                triggerElement.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image
            }

            // create the properties element and its child static element
            XmlElement propertiesElement = xml.CreateElement("Properties");
            XmlElement staticElement = xml.CreateElement("Static");
            XmlElement selectionElement = xml.CreateElement("Selection");
            selectionElement.SetAttribute("Choice", "AITriggers");
            selectionElement.SetAttribute("Variant", "CommonMode");

            staticElement.AppendChild(selectionElement);
            propertiesElement.AppendChild(staticElement);
            triggerElement.AppendChild(propertiesElement);

            XmlElement triggerContentElement = xml.CreateElement("Content"); // create content element inside trigger element
            XmlElement initElement = xml.CreateElement("Init"); // create the init element and its child setVariable element

            float Frames = RespawnComponent.RespawnSecond * 60;

            string[][] setVariables = new string[][]
            {
                new[] { "Name", "$Active", "Value", "1" },
                new[] { "Name", "$Node", "Value", "COM" },
                new[] { "Name", "Spawn", "Value", RespawnComponent.RespawnName },
                new[] { "Name", "Frames", "Value", Frames.ToString() },
                new[] { "Name", "SpawnModel", "Value", RespawnComponent.Spawnmodel },
                new[] { "Name", "Reversed", "Value", "0" },
                new[] { "Name", "$AI", "Value", "0" },
                new[] { "Name", "Flag1", "Value", "0" },
            };

            // add each setVariable element to the init element
            foreach (var setVariable in setVariables)
            {
                XmlElement setVariableElement = xml.CreateElement("SetVariable");
                setVariableElement.SetAttribute(setVariable[0], setVariable[1]);
                setVariableElement.SetAttribute(setVariable[2], setVariable[3]);
                initElement.AppendChild(setVariableElement);
            }

            triggerContentElement.AppendChild(initElement);

            // create template element inside content element
            XmlElement templateElement = xml.CreateElement("Loop");
            templateElement.SetAttribute("Template", "Respawn_OnScreen.Player");
            XmlElement templateElement2 = xml.CreateElement("Loop");
            templateElement2.SetAttribute("Template", "Respawn_OnScreen.Timeout");

            triggerContentElement.AppendChild(templateElement);
            triggerContentElement.AppendChild(templateElement2);
            triggerElement.AppendChild(triggerContentElement);
            contentElement.AppendChild(triggerElement);
            objectElement.AppendChild(contentElement);

            node.FirstChild.AppendChild(objectElement);

        }
        else if (RespawnComponent == null && Spawn != null)
        {
            if (Spawn.RefersToRespawn == false)
            {
                spawnElement.SetAttribute("X", (spawnInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                spawnElement.SetAttribute("Y", (-spawnInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
                spawnElement.SetAttribute("Name", Spawn.SpawnName); // name in the spawn component
                spawnElement.SetAttribute("Animation", Spawn.SpawnAnimation); // spawnanim in spawn component

                node.FirstChild.AppendChild(spawnElement); //Place it into the Object node
            }
            
        }

        xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
    }

    void SetLevelProperties(XmlDocument xml, XmlNode objectNode)
    {
        GameObject[] allObj = FindObjectsOfType<GameObject>(); //find all object
        XmlNode rootNode = xml.DocumentElement.SelectSingleNode("/Root");

        //set the background
        XmlNode objNode = xml.SelectSingleNode("/Root/Track/Object[@Factor='0.05']");
        if (objNode != null)
        {
            XmlNode contentNode = objNode.SelectSingleNode("Content");
            if (contentNode != null)
            {
                XmlNodeList imageNodes = contentNode.SelectNodes("Image");
                foreach (XmlNode imageNode in imageNodes)
                {
                    imageNode.Attributes["ClassName"].Value = customBackground;
                    imageNode.Attributes["Width"].Value = bg_Width;
                    imageNode.Attributes["Height"].Value = bg_Height;
                }
            }
        }


        //set the music
        if (levelMusic != null)
        {
            XmlNode musicNode = xml.DocumentElement.SelectSingleNode("/Root/Music");
            XmlAttribute musicAttribute = musicNode.Attributes["Name"];
            XmlAttribute musicVolAttribute = musicNode.Attributes["Volume"];
            if (musicAttribute.Value != null)
            {
                musicAttribute.Value = levelMusic;
                musicVolAttribute.Value = MusicVolume;
            }
        }
        else UnityEngine.Debug.LogWarning("No music name specified.");


        //set player, hunter properties
        foreach (GameObject allObjects in allObj) //loop to see if the object has buildmap component under it
        {
            BuildMap buildMap = allObjects.GetComponent<BuildMap>();
            if (useCustomProperties) //if use custom properties is true
            {
                foreach (XmlNode modelsNode in rootNode)
                {
                    if (modelsNode.Name == "Models" && modelsNode.Attributes["Variant"].Value == "CommonMode") //search for the models node
                    {
                        while (modelsNode.HasChildNodes) //if there is child node then remove it
                        {
                            modelsNode.RemoveChild(modelsNode.FirstChild); //im not gonna lie, just trying to remove childnode took me solid 2 hours
                        }

                        XmlDocument tempDoc = new XmlDocument();
                        tempDoc.LoadXml($"<root>{CustomModelProperties}</root>");
                        foreach (XmlNode childNode in tempDoc.DocumentElement.ChildNodes)
                        {
                            XmlNode importedNode = xml.ImportNode(childNode, true);
                            modelsNode.AppendChild(importedNode);
                        }
                    }
                }
            }

            else if (!useCustomProperties) // if use custom properties is false
            {
                foreach (XmlNode modelsNode in rootNode)
                {
                    if (modelsNode.Name == "Models" && modelsNode.Attributes["Variant"].Value == "CommonMode")
                    {
                        foreach (XmlNode modelNode in modelsNode.ChildNodes)
                        {
                            if (modelNode.Attributes["Name"].Value == "Player" || modelNode.Attributes["Name"].Value == Player.playerModelName)
                            {
                                //player model name
                                modelNode.Attributes["Name"].Value = Player.playerModelName;

                                //spawn time
                                modelNode.Attributes["Time"].Value = Player.playerSpawnTime.ToString();

                                //spawn name
                                modelNode.Attributes["BirthSpawn"].Value = Player.playerSpawnName;


                                //skin
                                XmlAttribute playerskin = xml.CreateAttribute("Skins");

                                if (!string.IsNullOrEmpty(Player.playerSkin)) //check if playerskin is specified
                                {
                                    playerskin.Value = Player.playerSkin;
                                }
                                else
                                {
                                    playerskin.Value = "1";
                                    UnityEngine.Debug.LogWarning("Player skin isn't specified, setting to default..");
                                }
                                modelNode.Attributes.Append(playerskin);

                            }
                            if (modelNode.Attributes["Name"].Value == "Hunter" || modelNode.Attributes["Name"].Value == Hunter.hunterModelName)
                            {
                                //hunter model name
                                modelNode.Attributes["Name"].Value = Hunter.hunterModelName;

                                //spawn time
                                modelNode.Attributes["Time"].Value = Hunter.hunterSpawnTime.ToString();

                                //spawn name
                                modelNode.Attributes["BirthSpawn"].Value = Hunter.hunterSpawnName;

                                //ai number
                                modelNode.Attributes["AI"].Value = Hunter.hunterAIType.ToString();

                                //huntericon
                                if (Hunter.hunterIcon == true)
                                {
                                    modelNode.Attributes["Icon"].Value = "1";
                                }
                                else
                                {
                                    modelNode.Attributes["Icon"].Value = "0";
                                }

                                //skin
                                XmlAttribute hunterskin = xml.CreateAttribute("Skins");

                                if (!string.IsNullOrEmpty(Hunter.hunterSkin)) //check if hunterskin is specified
                                {
                                    hunterskin.Value = Hunter.hunterSkin;
                                }
                                else
                                {
                                    hunterskin.Value = "hunter";
                                    UnityEngine.Debug.LogWarning("Hunter skin isn't specified, setting to default..");
                                }
                                modelNode.Attributes.Append(hunterskin);

                                //trick

                                if (Hunter.hunterTrickAllowed == true) //check if hunter is allowed to do trick
                                {
                                    XmlAttribute hunterTrick = xml.CreateAttribute("Trick");
                                    hunterTrick.Value = "1";
                                    modelNode.Attributes.Append(hunterTrick);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
       

    void ConvertToBackdrop(XmlNode node, XmlDocument xml, GameObject bdInScene)
    {
        //Debug in log every backdrop it writes
        if (debugObjectWriting)
            UnityEngine.Debug.Log("Writing object : " + Regex.Replace(bdInScene.name, @" \((.*?)\)", string.Empty));

        // Check if the GameObject has a SpriteRenderer component
        SpriteRenderer spriteRenderer = bdInScene.GetComponent<SpriteRenderer>();

        if (bdInScene.name != "Camera")
        {
            if (spriteRenderer == null)
            {
                XmlElement BD_element = xml.CreateElement("Object"); //Create a new node from scratch
                BD_element.SetAttribute("Name", Regex.Replace(bdInScene.name, @" \((.*?)\)", string.Empty)); //Add an name
                BD_element.SetAttribute("X", (bdInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                BD_element.SetAttribute("Y", (-bdInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)

                node.FirstChild.AppendChild(BD_element); //Place it into the Object node
                xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
            }
            else if (spriteRenderer.sprite != null)
            {
                XmlElement BD_element = xml.CreateElement("Image"); //Create a new node from scratch
                BD_element.SetAttribute("X", (bdInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                BD_element.SetAttribute("Y", (-bdInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
                BD_element.SetAttribute("ClassName", Regex.Replace(bdInScene.name, @" \((.*?)\)", string.Empty)); //Add a name

                Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                Vector3 scale = bdInScene.transform.localScale; // Get the GameObject scale

                // Retrieve the image resolution of the sprite
                float width = bounds.size.x * 100;
                float height = bounds.size.y * 100;

                // Set the width and height accordingly to the scale in the editor
                BD_element.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                BD_element.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image

                // Set the Native resolution of sprite
                BD_element.SetAttribute("NativeX", width.ToString()); //Native Resolution of the Image in X
                BD_element.SetAttribute("NativeY", height.ToString()); //Native Resolution of the Image in Y

                node.FirstChild.AppendChild(BD_element); //Place it into the Object node
                xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
            }
        }
    }

    void ConvertToImage(XmlNode node, XmlDocument xml, GameObject imageInScene)
    {
        //Debug in log every images it write
        if (debugObjectWriting)
            UnityEngine.Debug.Log("Writing object : " + Regex.Replace(imageInScene.name, @" \((.*?)\)", string.Empty));

        if (imageInScene.name != "Camera")
        {
            XmlElement ielement = xml.CreateElement("Image"); //Create a new node from scratch
            ielement.SetAttribute("X", (imageInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
            ielement.SetAttribute("Y", (-imageInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
            ielement.SetAttribute("ClassName", Regex.Replace(imageInScene.name, @" \((.*?)\)", string.Empty)); //Add a name
            SpriteRenderer spriteRenderer = imageInScene.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Image Size in Width and Height
            {

                Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                Vector3 scale = imageInScene.transform.localScale; // Get the GameObject scale

                // Retrieve the image resolution of the sprite
                float width = bounds.size.x * 100;
                float height = bounds.size.y * 100;

                // Set the width and height accordingly to the scale in the editor
                ielement.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                ielement.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image

                // Set the Native resolution of sprite
                ielement.SetAttribute("NativeX", width.ToString()); //Native Resolution of the Image in X
                ielement.SetAttribute("NativeY", height.ToString()); //Native Resolution of the Image in Y
            }

            // create a new node
            XmlElement propertiesElement = xml.CreateElement("Properties");
            XmlElement staticElement = xml.CreateElement("Static");
            XmlElement matrixElement = xml.CreateElement("Matrix");

            // Check if the sprite image is flipped x,y or both
            if (spriteRenderer.flipX || spriteRenderer.flipY || (spriteRenderer.flipX && spriteRenderer.flipY))
            {
                Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                Vector3 scale = imageInScene.transform.localScale; // Get the GameObject scale

                float width = bounds.size.x * 100 * scale.x;
                float height = bounds.size.y * 100 * scale.y;
                float negative_width = width * -1;
                float negative_height = height * -1;

                // matrix, "A" represents X axis, while "D" represents the Y axis.
                if (spriteRenderer.flipX && spriteRenderer.flipY)
                {
                    matrixElement.SetAttribute("A", negative_width.ToString());
                    matrixElement.SetAttribute("B", "0");
                    matrixElement.SetAttribute("C", "0");
                    matrixElement.SetAttribute("D", negative_height.ToString());
                    matrixElement.SetAttribute("Tx", "0");
                    matrixElement.SetAttribute("Ty", "0");
                }
                else if (spriteRenderer.flipX)
                {
                    matrixElement.SetAttribute("A", negative_width.ToString());
                    matrixElement.SetAttribute("B", "0");
                    matrixElement.SetAttribute("C", "0");
                    matrixElement.SetAttribute("D", height.ToString());
                    matrixElement.SetAttribute("Tx", width.ToString());
                    matrixElement.SetAttribute("Ty", "0");
                }
                else if (spriteRenderer.flipY)
                {
                    matrixElement.SetAttribute("A", width.ToString());
                    matrixElement.SetAttribute("B", "0");
                    matrixElement.SetAttribute("C", "0");
                    matrixElement.SetAttribute("D", negative_height.ToString());
                    matrixElement.SetAttribute("Tx", "0");
                    matrixElement.SetAttribute("Ty", negative_height.ToString());
                }
                
                //writes everything under the image node
                staticElement.AppendChild(matrixElement);
                propertiesElement.AppendChild(staticElement);
                ielement.AppendChild(propertiesElement);
            }
            node.FirstChild.AppendChild(ielement); //Place it into the Object node
            xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
        }
    }

    void ConvertToModel(XmlNode node, XmlDocument xml, GameObject modelInScene)
    {
        if (modelInScene.name != "Camera")
        {
            ModelProperties modelProperties = modelInScene.GetComponent<ModelProperties>();

            XmlElement Melement = xml.CreateElement("Model"); //Create a new node from scratch
            Melement.SetAttribute("X", (modelInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
            Melement.SetAttribute("Y", (-modelInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
            Melement.SetAttribute("Type", modelProperties.Type.ToString()); //Add an name
            Melement.SetAttribute("ClassName", Regex.Replace(modelInScene.name, @" \((.*?)\)", string.Empty)); //Add an name

            if (modelProperties.UseLifeTime)
            {
                Melement.SetAttribute("LifeTime", modelProperties.LifeTime); //Add an name
            }

            node.FirstChild.AppendChild(Melement); //Place it into the Object node
            xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
        }
    }

    void ConvertToObject(XmlNode node, XmlDocument xml, GameObject objectInScene)
    {
        //Debug in log every object it writes
        if (debugObjectWriting)
            UnityEngine.Debug.Log("Writing object : " + Regex.Replace(objectInScene.name, @" \((.*?)\)", string.Empty));

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

    void ConvertToPlatform(XmlNode node, XmlDocument xml, GameObject platformInScene) // Platform Collision (Invisible block that is collide-able)
    {
        //Debug in log every platform it writes
        if (debugObjectWriting)
            UnityEngine.Debug.Log("Writing object : " + Regex.Replace(platformInScene.name, @" \((.*?)\)", string.Empty));

        if (platformInScene.name != "trapezoid_type2" && platformInScene.name != "trapezoid_type1") // Use a texture called "collision" which should come with this buildmap update folder.
        {
            XmlElement P_element = xml.CreateElement("Platform"); //Create a new node from scratch
            P_element.SetAttribute("X", (platformInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
            P_element.SetAttribute("Y", (-platformInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)

            SpriteRenderer spriteRenderer = platformInScene.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Sprite Size in Width and Height
            {

                Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                Vector3 scale = platformInScene.transform.localScale; // Get the GameObject scale

                // Retrieve the image resolution of the sprite
                float width = bounds.size.x * 100;
                float height = bounds.size.y * 100;

                // Set the width and height accordingly to the scale in the editor
                P_element.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                P_element.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image

            }
            node.FirstChild.AppendChild(P_element); //Place it into the Object node
            xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
        }

        // Trapezoid Slope (Type 2 is Downhill, Type 1 is Uphill)
        else if (platformInScene.name == "trapezoid_type2")
        {
            XmlElement T_element = xml.CreateElement("Trapezoid"); //Create a new node from scratch
            T_element.SetAttribute("X", (platformInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
            T_element.SetAttribute("Y", (-platformInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
            T_element.SetAttribute("Width", "1000"); //Width of the Trapezoid
            T_element.SetAttribute("Height", "600"); //Height of the Trapezoid
            T_element.SetAttribute("Height1", "100"); //Height1 of the Trapezoid
            T_element.SetAttribute("Type", "2"); //Type of the Trapezoid
            node.FirstChild.AppendChild(T_element); //Place it into the Object node
            xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
        }

        else if (platformInScene.name == "trapezoid_type1")
        {
            XmlElement T_element = xml.CreateElement("Trapezoid"); //Create a new node from scratch
            T_element.SetAttribute("X", (platformInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
            T_element.SetAttribute("Y", (-platformInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
            T_element.SetAttribute("Width", "1000"); //Width of the Trapezoid
            T_element.SetAttribute("Height", "250"); //Height of the Trapezoid
            T_element.SetAttribute("Height1", "750"); //Height1 of the Trapezoid
            T_element.SetAttribute("Type", "1"); //Type of the Trapezoid
            node.FirstChild.AppendChild(T_element); //Place it into the Object node
            xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
        }
    }


    void ConvertToTrigger(XmlNode node, XmlDocument xml, GameObject triggerInScene)
    {
        //Debug in log every trigger it writes
        if (debugObjectWriting)
            UnityEngine.Debug.Log("Writing object : " + Regex.Replace(triggerInScene.name, @" \((.*?)\)", string.Empty));

        if (triggerInScene.name != "Camera")
        {
            if (triggerInScene.GetComponent<TriggerSettings>() != null) //Checks if the trigger has a setting component
            {
                XmlElement T_element = xml.CreateElement("Trigger"); //Create a new node from scratch
                TriggerSettings triggerSettings = triggerInScene.GetComponent<TriggerSettings>(); //Trigger Settings.cs
                T_element.SetAttribute("Name", Regex.Replace(triggerInScene.name, @" \((.*?)\)", string.Empty)); //Add an name
                T_element.SetAttribute("X", (triggerInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                T_element.SetAttribute("Y", (-triggerInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)

                SpriteRenderer spriteRenderer = triggerInScene.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Sprite Size in Width and Height
                {

                    Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                    Vector3 scale = triggerInScene.transform.localScale; // Get the GameObject scale

                    // Retrieve the image resolution of the sprite
                    float width = bounds.size.x * 100;
                    float height = bounds.size.y * 100;

                    // Set the width and height accordingly to the scale in the editor
                    T_element.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                    T_element.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image

                    // Create the content node and add it to the trigger node
                    XmlElement contentElement = xml.CreateElement("Content");

                    //xml doesn't format correctly so we load them into a separate doc
                    XmlDocument tempDoc = new XmlDocument();
                    tempDoc.LoadXml("<Content>" + triggerSettings.Content + "</Content>");
                    foreach (XmlNode childNode in tempDoc.DocumentElement.ChildNodes)
                    {
                        XmlNode importedNode = xml.ImportNode(childNode, true);
                        contentElement.AppendChild(importedNode);
                    }

                    T_element.AppendChild(contentElement);

                    node.FirstChild.AppendChild(T_element); //Place it into the Object node

                }
            }
            else //continues as normal without any setting attached
            {
                XmlElement T_element = xml.CreateElement("Trigger"); //Create a new node from scratch
                T_element.SetAttribute("Name", Regex.Replace(triggerInScene.name, @" \((.*?)\)", string.Empty)); //Add an name
                T_element.SetAttribute("X", (triggerInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                T_element.SetAttribute("Y", (-triggerInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)

                SpriteRenderer spriteRenderer = triggerInScene.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Sprite Size in Width and Height
                {

                    Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                    Vector3 scale = triggerInScene.transform.localScale; // Get the GameObject scale

                    // Retrieve the image resolution of the sprite
                    float width = bounds.size.x * 100;
                    float height = bounds.size.y * 100;

                    // Set the width and height accordingly to the scale in the editor
                    T_element.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                    T_element.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image
                    node.FirstChild.AppendChild(T_element); //Place it into the Object node
                }
            }

            //apply the modification to the build-map.xml with proper format
            XmlWriterSettings settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\r\n",
                NewLineHandling = NewLineHandling.Replace
            };

            using (XmlWriter writer = XmlWriter.Create(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml", settings))
            {
                xml.Save(writer);
            }
        }
    }
    //  ^^^ ExtractAttributeValue is for the method above ^^^
    private string ExtractAttributeValue(string line, string attributeName)
    {
        int startIndex = line.IndexOf(attributeName + "=\"") + (attributeName + "=\"").Length;
        int endIndex = line.IndexOf("\"", startIndex);
        if (startIndex != -1 && endIndex != -1)
        {
            return line.Substring(startIndex, endIndex - startIndex);
        }
        return null;
    }





    void ConvertToArea(XmlNode node, XmlDocument xml, GameObject areaInScene)
    {
        //Debug in log every Area it writes
        if (debugObjectWriting)
            UnityEngine.Debug.Log("Writing object : " + Regex.Replace(areaInScene.name, @" \((.*?)\)", string.Empty));

        if (areaInScene.name != "Camera")
        {
            XmlElement A_element = xml.CreateElement("Area"); //Create a new node from scratch
            A_element.SetAttribute("Name", Regex.Replace(areaInScene.name, @" \((.*?)\)", string.Empty)); //Add an name
            A_element.SetAttribute("X", (areaInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
            A_element.SetAttribute("Y", (-areaInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)

            SpriteRenderer spriteRenderer = areaInScene.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Sprite Size in Width and Height
            {

                Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                Vector3 scale = areaInScene.transform.localScale; // Get the GameObject scale

                // Retrieve the image resolution of the sprite
                float width = bounds.size.x * 100;
                float height = bounds.size.y * 100;

                // Set the width and height accordingly to the scale in the editor
                A_element.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                A_element.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image

            }
            A_element.SetAttribute("Type", "Animation"); //Type="Animation"/>
            node.FirstChild.AppendChild(A_element); //Place it into the Object node
            xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
        }
    }


    void ConvertToCamera(XmlNode node, XmlDocument xml, GameObject camInScene)
    {


        //Important Note: If the specific TriggerZoom already exists in the object.xml, no need to tag those as Camera. Instead, tag it as an object!



        // Debug in log every Area it writes
        if (debugObjectWriting)
            UnityEngine.Debug.Log("Writing object : " + Regex.Replace(camInScene.name, @" \((.*?)\)", string.Empty));


        if (camInScene.name != "Camera") //kinda ironic
        {
            SpriteRenderer spriteRenderer = camInScene.GetComponent<SpriteRenderer>();
            CustomZoom customZoomValue = camInScene.GetComponent<CustomZoom>(); //Zoom value from object with tag "Camera" that have CustomZoom component
            Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
            Vector3 scale = camInScene.transform.localScale; // Get the GameObject scale
            // Retrieve the image resolution of the sprite
            float width = bounds.size.x * 100;
            float height = bounds.size.y * 100;

            //Trigger Childs
            XmlElement contentElement = xml.CreateElement("Content");
            XmlElement initElement = xml.CreateElement("Init");

            //trigger variable
            string[] variableNames = { "$Active", "$Node", "Zoom", "$AI", "Flag1" };
            string[] variableValues = { "1", "COM", customZoomValue.ZoomAmount.ToString(), "0", "0" };


            XmlElement triggerElement = xml.CreateElement("Trigger");
            triggerElement.SetAttribute("Name", Regex.Replace(camInScene.name, @" \((.*?)\)", string.Empty));
            triggerElement.SetAttribute("X", (camInScene.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
            triggerElement.SetAttribute("Y", (-camInScene.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
            triggerElement.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
            triggerElement.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image

            //writes <content> and <init> under the trigger node
            for (int i = 0; i < variableNames.Length; i++)
            {
                XmlElement setVariableElement = xml.CreateElement("SetVariable");
                setVariableElement.SetAttribute("Name", variableNames[i]);
                setVariableElement.SetAttribute("Value", variableValues[i]);
                initElement.AppendChild(setVariableElement);
            }

            XmlElement templateElement = xml.CreateElement("Template");
            templateElement.SetAttribute("Name", "CameraZoom");

            // Append elements
            contentElement.AppendChild(initElement);
            contentElement.AppendChild(templateElement);
            triggerElement.AppendChild(contentElement);

            // Append the Trigger element to the XmlDocument
            node.FirstChild.AppendChild(triggerElement);
            xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
        }
    }

    void ConvertToDynamic(XmlNode node, XmlDocument xml, GameObject dynamicInScene, UnityEngine.Transform dynamicInSceneTransform)
    {

        Dynamic dynamicComponent = dynamicInScene.GetComponent<Dynamic>(); //dynamic component in the hierachy

        //object
        XmlElement objectElement = xml.CreateElement("Object");
        objectElement.SetAttribute("X", "0");
        objectElement.SetAttribute("Y", "0");

        // properties
        XmlElement propertiesElement = xml.CreateElement("Properties");

        //dynamic
        XmlElement dynamicElement = xml.CreateElement("Dynamic");

        // Create Transformation element
        XmlElement transformationElement = xml.CreateElement("Transformation");
        transformationElement.SetAttribute("Name", dynamicComponent.TransformationName);

        // Create Move element
        XmlElement moveElement = xml.CreateElement("Move");

        // MoveInterval element

        // Move Interval 1
        if (dynamicComponent.MovementUsage.UseMovement1)
        {
            XmlElement moveIntervalElement = xml.CreateElement("MoveInterval");
            moveIntervalElement.SetAttribute("Number", "1");
            moveIntervalElement.SetAttribute("FramesToMove", (dynamicComponent.MoveInterval1.MoveDuration * 60).ToString()); //multiply second by 60 frames per second
            moveIntervalElement.SetAttribute("Delay", (dynamicComponent.MoveInterval1.Delay * 60).ToString()); //multiply second by 60 frames per second

            // Create Points (Start, Support, Finish)
            XmlElement startPointElement = xml.CreateElement("Point");
            startPointElement.SetAttribute("Name", "Start");
            startPointElement.SetAttribute("X", "0.0");
            startPointElement.SetAttribute("Y", "0.0");

            XmlElement supportPointElement = xml.CreateElement("Point");
            supportPointElement.SetAttribute("Name", "Support");
            supportPointElement.SetAttribute("Number", "1");
            supportPointElement.SetAttribute("X", (dynamicComponent.MoveInterval1.SupportXAxis * 100).ToString());
            supportPointElement.SetAttribute("Y", (-dynamicComponent.MoveInterval1.SupportYAxis * 100).ToString());

            XmlElement finishPointElement = xml.CreateElement("Point");
            finishPointElement.SetAttribute("Name", "Finish");
            finishPointElement.SetAttribute("X", (dynamicComponent.MoveInterval1.MoveXAxis * 100).ToString());
            finishPointElement.SetAttribute("Y", (-dynamicComponent.MoveInterval1.MoveYAxis * 100).ToString());

            // Append points to MoveInterval
            moveIntervalElement.AppendChild(startPointElement);
            moveIntervalElement.AppendChild(supportPointElement);
            moveIntervalElement.AppendChild(finishPointElement);

            moveElement.AppendChild(moveIntervalElement);
        }

        // Move Interval 2
        if (dynamicComponent.MovementUsage.UseMovement2)
        {
            XmlElement moveIntervalElement = xml.CreateElement("MoveInterval");
            moveIntervalElement.SetAttribute("Number", "2");
            moveIntervalElement.SetAttribute("FramesToMove", (dynamicComponent.MoveInterval2.MoveDuration * 60).ToString()); //multiply second by 60 frames per second
            moveIntervalElement.SetAttribute("Delay", (dynamicComponent.MoveInterval2.Delay * 60).ToString()); //multiply second by 60 frames per second

            // Create Points (Start, Support, Finish)
            XmlElement startPointElement = xml.CreateElement("Point");
            startPointElement.SetAttribute("Name", "Start");
            startPointElement.SetAttribute("X", "0.0");
            startPointElement.SetAttribute("Y", "0.0");

            XmlElement supportPointElement = xml.CreateElement("Point");
            supportPointElement.SetAttribute("Name", "Support");
            supportPointElement.SetAttribute("Number", "2");
            supportPointElement.SetAttribute("X", (dynamicComponent.MoveInterval2.SupportXAxis * 100).ToString());
            supportPointElement.SetAttribute("Y", (-dynamicComponent.MoveInterval2.SupportYAxis * 100).ToString());

            XmlElement finishPointElement = xml.CreateElement("Point");
            finishPointElement.SetAttribute("Name", "Finish");
            finishPointElement.SetAttribute("X", (dynamicComponent.MoveInterval2.MoveXAxis * 100).ToString());
            finishPointElement.SetAttribute("Y", (-dynamicComponent.MoveInterval2.MoveYAxis * 100).ToString());

            // Append points to MoveInterval
            moveIntervalElement.AppendChild(startPointElement);
            moveIntervalElement.AppendChild(supportPointElement);
            moveIntervalElement.AppendChild(finishPointElement);

            moveElement.AppendChild(moveIntervalElement);
        }

        // Move Interval 3
        if (dynamicComponent.MovementUsage.UseMovement3)
        {
            XmlElement moveIntervalElement = xml.CreateElement("MoveInterval");
            moveIntervalElement.SetAttribute("Number", "3");
            moveIntervalElement.SetAttribute("FramesToMove", (dynamicComponent.MoveInterval3.MoveDuration * 60).ToString()); //multiply second by 60 frames per second
            moveIntervalElement.SetAttribute("Delay", (dynamicComponent.MoveInterval3.Delay * 60).ToString()); //multiply second by 60 frames per second

            // Create Points (Start, Support, Finish)
            XmlElement startPointElement = xml.CreateElement("Point");
            startPointElement.SetAttribute("Name", "Start");
            startPointElement.SetAttribute("X", "0.0");
            startPointElement.SetAttribute("Y", "0.0");

            XmlElement supportPointElement = xml.CreateElement("Point");
            supportPointElement.SetAttribute("Name", "Support");
            supportPointElement.SetAttribute("Number", "2");
            supportPointElement.SetAttribute("X", (dynamicComponent.MoveInterval3.SupportXAxis * 100).ToString());
            supportPointElement.SetAttribute("Y", (-dynamicComponent.MoveInterval3.SupportYAxis * 100).ToString());

            XmlElement finishPointElement = xml.CreateElement("Point");
            finishPointElement.SetAttribute("Name", "Finish");
            finishPointElement.SetAttribute("X", (dynamicComponent.MoveInterval3.MoveXAxis * 100).ToString());
            finishPointElement.SetAttribute("Y", (-dynamicComponent.MoveInterval3.MoveYAxis * 100).ToString());

            // Append points to MoveInterval
            moveIntervalElement.AppendChild(startPointElement);
            moveIntervalElement.AppendChild(supportPointElement);
            moveIntervalElement.AppendChild(finishPointElement);

            moveElement.AppendChild(moveIntervalElement);
        }

        // Move Interval 4
        if (dynamicComponent.MovementUsage.UseMovement4)
        {
            XmlElement moveIntervalElement = xml.CreateElement("MoveInterval");
            moveIntervalElement.SetAttribute("Number", "4");
            moveIntervalElement.SetAttribute("FramesToMove", (dynamicComponent.MoveInterval4.MoveDuration * 60).ToString()); //multiply second by 60 frames per second
            moveIntervalElement.SetAttribute("Delay", (dynamicComponent.MoveInterval4.Delay * 60).ToString()); //multiply second by 60 frames per second

            // Create Points (Start, Support, Finish)
            XmlElement startPointElement = xml.CreateElement("Point");
            startPointElement.SetAttribute("Name", "Start");
            startPointElement.SetAttribute("X", "0.0");
            startPointElement.SetAttribute("Y", "0.0");

            XmlElement supportPointElement = xml.CreateElement("Point");
            supportPointElement.SetAttribute("Name", "Support");
            supportPointElement.SetAttribute("Number", "2");
            supportPointElement.SetAttribute("X", (dynamicComponent.MoveInterval4.SupportXAxis * 100).ToString());
            supportPointElement.SetAttribute("Y", (-dynamicComponent.MoveInterval4.SupportYAxis * 100).ToString());

            XmlElement finishPointElement = xml.CreateElement("Point");
            finishPointElement.SetAttribute("Name", "Finish");
            finishPointElement.SetAttribute("X", (dynamicComponent.MoveInterval4.MoveXAxis * 100).ToString());
            finishPointElement.SetAttribute("Y", (-dynamicComponent.MoveInterval4.MoveYAxis * 100).ToString());

            // Append points to MoveInterval
            moveIntervalElement.AppendChild(startPointElement);
            moveIntervalElement.AppendChild(supportPointElement);
            moveIntervalElement.AppendChild(finishPointElement);

            moveElement.AppendChild(moveIntervalElement);
        }

        // Move Interval 5
        if (dynamicComponent.MovementUsage.UseMovement5)
        {
            XmlElement moveIntervalElement = xml.CreateElement("MoveInterval");
            moveIntervalElement.SetAttribute("Number", "5");
            moveIntervalElement.SetAttribute("FramesToMove", (dynamicComponent.MoveInterval5.MoveDuration * 60).ToString()); //multiply second by 60 frames per second
            moveIntervalElement.SetAttribute("Delay", (dynamicComponent.MoveInterval5.Delay * 60).ToString()); //multiply second by 60 frames per second

            // Create Points (Start, Support, Finish)
            XmlElement startPointElement = xml.CreateElement("Point");
            startPointElement.SetAttribute("Name", "Start");
            startPointElement.SetAttribute("X", "0.0");
            startPointElement.SetAttribute("Y", "0.0");

            XmlElement supportPointElement = xml.CreateElement("Point");
            supportPointElement.SetAttribute("Name", "Support");
            supportPointElement.SetAttribute("Number", "2");
            supportPointElement.SetAttribute("X", (dynamicComponent.MoveInterval5.SupportXAxis * 100).ToString());
            supportPointElement.SetAttribute("Y", (-dynamicComponent.MoveInterval5.SupportYAxis * 100).ToString());

            XmlElement finishPointElement = xml.CreateElement("Point");
            finishPointElement.SetAttribute("Name", "Finish");
            finishPointElement.SetAttribute("X", (dynamicComponent.MoveInterval5.MoveXAxis * 100).ToString());
            finishPointElement.SetAttribute("Y", (-dynamicComponent.MoveInterval5.MoveYAxis * 100).ToString());

            // Append points to MoveInterval
            moveIntervalElement.AppendChild(startPointElement);
            moveIntervalElement.AppendChild(supportPointElement);
            moveIntervalElement.AppendChild(finishPointElement);

            moveElement.AppendChild(moveIntervalElement);
        }


        transformationElement.AppendChild(moveElement);
        dynamicElement.AppendChild(transformationElement);
        propertiesElement.AppendChild(dynamicElement);
        objectElement.AppendChild(propertiesElement);


        // Create Content element
        XmlElement contentElement = xml.CreateElement("Content");

        foreach (UnityEngine.Transform childObject in dynamicInSceneTransform)
        {
            //check if the gameobject has specific tag

            if (childObject.gameObject.CompareTag("Object"))
            {
                if (childObject.name != "Camera")
                {
                    XmlElement objElement = xml.CreateElement("Object"); //Create a new node from scratch
                    objElement.SetAttribute("Name", Regex.Replace(childObject.name, @" \((.*?)\)", string.Empty)); //Add an name
                    objElement.SetAttribute("X", (childObject.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                    objElement.SetAttribute("Y", (-childObject.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
                    contentElement.AppendChild(objElement);
                }
            }
            else if (childObject.gameObject.CompareTag("Image"))
            {
                XmlElement ielement = xml.CreateElement("Image"); //Create a new node from scratch
                ielement.SetAttribute("X", (childObject.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                ielement.SetAttribute("Y", (-childObject.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
                ielement.SetAttribute("ClassName", Regex.Replace(childObject.name, @" \((.*?)\)", string.Empty)); //Add a name
                SpriteRenderer spriteRenderer = childObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Image Size in Width and Height
                {

                    Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                    Vector3 scale = childObject.transform.localScale; // Get the GameObject scale

                    // Retrieve the image resolution of the sprite
                    float width = bounds.size.x * 100;
                    float height = bounds.size.y * 100;

                    // Set the width and height accordingly to the scale in the editor
                    ielement.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                    ielement.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image

                    // Set the Native resolution of sprite
                    ielement.SetAttribute("NativeX", width.ToString()); //Native Resolution of the Image in X
                    ielement.SetAttribute("NativeY", height.ToString()); //Native Resolution of the Image in Y
                }

                // create a new node
                XmlElement propertiesElementimage = xml.CreateElement("Properties");
                XmlElement staticElement = xml.CreateElement("Static");
                XmlElement matrixElement = xml.CreateElement("Matrix");

                // Check if the sprite image is flipped x,y or both
                if (spriteRenderer.flipX || spriteRenderer.flipY || (spriteRenderer.flipX && spriteRenderer.flipY))
                {
                    Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                    Vector3 scale = childObject.transform.localScale; // Get the GameObject scale

                    float width = bounds.size.x * 100 * scale.x;
                    float height = bounds.size.y * 100 * scale.y;
                    float negative_width = width * -1;
                    float negative_height = height * -1;

                    // matrix, "A" represents X axis, while "D" represents the Y axis.
                    if (spriteRenderer.flipX && spriteRenderer.flipY)
                    {
                        matrixElement.SetAttribute("A", negative_width.ToString());
                        matrixElement.SetAttribute("B", "0");
                        matrixElement.SetAttribute("C", "0");
                        matrixElement.SetAttribute("D", negative_height.ToString());
                        matrixElement.SetAttribute("Tx", "0");
                        matrixElement.SetAttribute("Ty", "0");
                    }
                    else if (spriteRenderer.flipX)
                    {
                        matrixElement.SetAttribute("A", negative_width.ToString());
                        matrixElement.SetAttribute("B", "0");
                        matrixElement.SetAttribute("C", "0");
                        matrixElement.SetAttribute("D", height.ToString());
                        matrixElement.SetAttribute("Tx", "0");
                        matrixElement.SetAttribute("Ty", "0");
                    }
                    else if (spriteRenderer.flipY)
                    {
                        matrixElement.SetAttribute("A", width.ToString());
                        matrixElement.SetAttribute("B", "0");
                        matrixElement.SetAttribute("C", "0");
                        matrixElement.SetAttribute("D", negative_height.ToString());
                        matrixElement.SetAttribute("Tx", "0");
                        matrixElement.SetAttribute("Ty", "0");
                    }

                    //writes everything under the image node
                    staticElement.AppendChild(matrixElement);
                    propertiesElementimage.AppendChild(staticElement);
                    ielement.AppendChild(propertiesElement);
                }
                contentElement.AppendChild(ielement);
            }
            else if (childObject.gameObject.CompareTag("Platform"))
            {
                //Platform
                if (childObject.name != "trapezoid_type2" && childObject.name != "trapezoid_type1" && childObject.gameObject.CompareTag("Platform")) // Use a texture called "collision" which should come with this buildmap update folder.
                {
                    XmlElement P_element = xml.CreateElement("Platform"); //Create a new node from scratch
                    P_element.SetAttribute("X", (childObject.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                    P_element.SetAttribute("Y", (-childObject.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)

                    SpriteRenderer spriteRenderer = childObject.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Sprite Size in Width and Height
                    {

                        Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                        Vector3 scale = childObject.transform.localScale; // Get the GameObject scale

                        // Retrieve the image resolution of the sprite
                        float width = bounds.size.x * 100;
                        float height = bounds.size.y * 100;

                        // Set the width and height accordingly to the scale in the editor
                        P_element.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                        P_element.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image

                    }
                    contentElement.AppendChild(P_element);
                }
                // Trapezoid Slope (Type 2 is Downhill, Type 1 is Uphill)
                else if (childObject.name == "trapezoid_type2" && childObject.CompareTag("Platform"))
                {
                    XmlElement T_element = xml.CreateElement("Trapezoid"); //Create a new node from scratch
                    T_element.SetAttribute("X", (childObject.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                    T_element.SetAttribute("Y", (-childObject.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
                    T_element.SetAttribute("Width", "1000"); //Width of the Trapezoid
                    T_element.SetAttribute("Height", "600"); //Height of the Trapezoid
                    T_element.SetAttribute("Height1", "100"); //Height1 of the Trapezoid
                    T_element.SetAttribute("Type", "2"); //Type of the Trapezoid
                    contentElement.AppendChild(T_element);
                }

                else if (childObject.name == "trapezoid_type1" && childObject.CompareTag("Platform"))
                {
                    XmlElement T_element = xml.CreateElement("Trapezoid"); //Create a new node from scratch
                    T_element.SetAttribute("X", (childObject.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                    T_element.SetAttribute("Y", (-childObject.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
                    T_element.SetAttribute("Width", "1000"); //Width of the Trapezoid
                    T_element.SetAttribute("Height", "250"); //Height of the Trapezoid
                    T_element.SetAttribute("Height1", "750"); //Height1 of the Trapezoid
                    T_element.SetAttribute("Type", "1"); //Type of the Trapezoid
                    contentElement.AppendChild(T_element);
                }
            }
            else if (childObject.gameObject.CompareTag("Area"))
            {
                XmlElement A_element = xml.CreateElement("Area"); //Create a new node from scratch
                A_element.SetAttribute("Name", Regex.Replace(childObject.name, @" \((.*?)\)", string.Empty)); //Add an name
                A_element.SetAttribute("X", (childObject.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                A_element.SetAttribute("Y", (-childObject.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)

                SpriteRenderer spriteRenderer = childObject.GetComponent<SpriteRenderer>();
                if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Sprite Size in Width and Height
                {

                    Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                    Vector3 scale = childObject.transform.localScale; // Get the GameObject scale

                    // Retrieve the image resolution of the sprite
                    float width = bounds.size.x * 100;
                    float height = bounds.size.y * 100;

                    // Set the width and height accordingly to the scale in the editor
                    A_element.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                    A_element.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image

                }
                A_element.SetAttribute("Type", "Animation"); //Type="Animation"/>
                contentElement.AppendChild(A_element);
            }
            else if (childObject.gameObject.CompareTag("Trigger"))
            {
                DynamicTrigger dynamicTrigger = childObject.GetComponent<DynamicTrigger>();

                if (dynamicTrigger != null)
                {
                    XmlElement T_element = xml.CreateElement("Trigger");
                    T_element.SetAttribute("Name", "");
                    T_element.SetAttribute("Name", Regex.Replace(childObject.name, @" \((.*?)\)", string.Empty)); //Add an name
                    T_element.SetAttribute("X", (childObject.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                    T_element.SetAttribute("Y", (-childObject.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)

                    SpriteRenderer spriteRenderer = childObject.GetComponent<SpriteRenderer>();
                    if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Sprite Size in Width and Height
                    {

                        Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                        Vector3 scale = childObject.transform.localScale; // Get the GameObject scale

                        // Retrieve the image resolution of the sprite
                        float width = bounds.size.x * 100;
                        float height = bounds.size.y * 100;

                        // Set the width and height accordingly to the scale in the editor
                        T_element.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                        T_element.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image
                    }

                    // Create Init element
                    XmlElement initElement = xml.CreateElement("Init");

                    // Add SetVariable elements to Init
                    XmlElement setVariable1 = xml.CreateElement("SetVariable");
                    setVariable1.SetAttribute("Name", "$Active");
                    setVariable1.SetAttribute("Value", "1");
                    initElement.AppendChild(setVariable1);

                    XmlElement setVariable2 = xml.CreateElement("SetVariable");
                    setVariable2.SetAttribute("Name", "$AI");
                    setVariable2.SetAttribute("Value", dynamicTrigger.AIAllowed.ToString());
                    initElement.AppendChild(setVariable2);

                    XmlElement setVariable3 = xml.CreateElement("SetVariable");
                    setVariable3.SetAttribute("Name", "$Node");
                    setVariable3.SetAttribute("Value", "COM");
                    initElement.AppendChild(setVariable3);

                    if (dynamicTrigger.PlaySound == true)
                    {
                        XmlElement setVariable4 = xml.CreateElement("SetVariable");
                        setVariable4.SetAttribute("Name", "Sound");
                        setVariable4.SetAttribute("Value", dynamicTrigger.Sound);
                        initElement.AppendChild(setVariable4);
                    }

                    XmlElement setVariable5 = xml.CreateElement("SetVariable");
                    setVariable5.SetAttribute("Name", "Flag1");
                    setVariable5.SetAttribute("Value", "0");
                    initElement.AppendChild(setVariable5);

                    // Create Trigger Content element
                    XmlElement triggerContentElement = xml.CreateElement("Content");

                    // Append Init to Content
                    triggerContentElement.AppendChild(initElement);

                    // Create Loop element
                    XmlElement loopElement = xml.CreateElement("Loop");

                    // Create Events element and EventBlock element
                    XmlElement eventsElement = xml.CreateElement("Events");
                    XmlElement eventBlockElement = xml.CreateElement("EventBlock");
                    eventBlockElement.SetAttribute("Template", "FreqUsed.Enter");
                    eventsElement.AppendChild(eventBlockElement);

                    // Append Events to Loop
                    loopElement.AppendChild(eventsElement);

                    // Create Actions element and ActionBlock element
                    XmlElement actionsElement = xml.CreateElement("Actions");
                    XmlElement actionBlockElement = xml.CreateElement("ActionBlock");
                    actionBlockElement.SetAttribute("Template", "FreqUsed.SwitchOff");
                    actionsElement.AppendChild(actionBlockElement);

                    // Create Transform element and append to Loop
                    XmlElement transformElement = xml.CreateElement("Transform");
                    transformElement.SetAttribute("Name", dynamicTrigger.TriggerTransformName);
                    actionsElement.AppendChild(transformElement);

                    if (dynamicTrigger.PlaySound == true)
                    {
                        // Create Actionsblock sound
                        XmlElement actionBlockSoundElement = xml.CreateElement("ActionBlock");
                        actionBlockSoundElement.SetAttribute("Template", "CommonLib.Sound");
                        actionsElement.AppendChild(actionBlockSoundElement);
                    }

                    // Append Actions to Loop
                    loopElement.AppendChild(actionsElement);

                    // Append Loop to Trigger
                    triggerContentElement.AppendChild(loopElement);

                    // Append Content to Trigger
                    T_element.AppendChild(triggerContentElement);

                    // Append Trigger to Content
                    contentElement.AppendChild(T_element);

                }
                else if (dynamicTrigger == null)
                {
                    if (childObject.name != "Camera")
                    {
                        if (childObject.GetComponent<TriggerSettings>() != null) //Checks if the trigger has a setting component
                        {
                            XmlElement T_element = xml.CreateElement("Trigger"); //Create a new node from scratch
                            TriggerSettings triggerSettings = childObject.GetComponent<TriggerSettings>(); //Trigger Settings.cs
                            T_element.SetAttribute("Name", Regex.Replace(childObject.name, @" \((.*?)\)", string.Empty)); //Add an name
                            T_element.SetAttribute("X", (childObject.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                            T_element.SetAttribute("Y", (-childObject.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)

                            SpriteRenderer spriteRenderer = childObject.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Sprite Size in Width and Height
                            {

                                Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                                Vector3 scale = childObject.transform.localScale; // Get the GameObject scale

                                // Retrieve the image resolution of the sprite
                                float width = bounds.size.x * 100;
                                float height = bounds.size.y * 100;

                                // Set the width and height accordingly to the scale in the editor
                                T_element.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                                T_element.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image

                                // Create the content node and add it to the trigger node
                                XmlElement cElement = xml.CreateElement("Content");

                                //xml doesn't format correctly so we load them into a separate doc
                                XmlDocument tempDoc = new XmlDocument();
                                tempDoc.LoadXml("<Content>" + triggerSettings.Content + "</Content>");
                                foreach (XmlNode childNode in tempDoc.DocumentElement.ChildNodes)
                                {
                                    XmlNode importedNode = xml.ImportNode(childNode, true);
                                    cElement.AppendChild(importedNode);
                                }

                                T_element.AppendChild(cElement);

                            }
                            contentElement.AppendChild(T_element);
                        }
                        else //continues as normal without any setting attached
                        {
                            XmlElement T_element = xml.CreateElement("Trigger"); //Create a new node from scratch
                            T_element.SetAttribute("Name", Regex.Replace(childObject.name, @" \((.*?)\)", string.Empty)); //Add an name
                            T_element.SetAttribute("X", (childObject.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                            T_element.SetAttribute("Y", (-childObject.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)

                            SpriteRenderer spriteRenderer = childObject.GetComponent<SpriteRenderer>();
                            if (spriteRenderer != null && spriteRenderer.sprite != null) //Get the Sprite Size in Width and Height
                            {

                                Bounds bounds = spriteRenderer.sprite.bounds;// Get the bounds of the sprite
                                Vector3 scale = childObject.transform.localScale; // Get the GameObject scale

                                // Retrieve the image resolution of the sprite
                                float width = bounds.size.x * 100;
                                float height = bounds.size.y * 100;

                                // Set the width and height accordingly to the scale in the editor
                                T_element.SetAttribute("Width", (width * scale.x).ToString()); //Width of the Image
                                T_element.SetAttribute("Height", (height * scale.y).ToString()); //Height of the Image
                            }
                            contentElement.AppendChild(T_element);
                        }
                    }

                    //apply the modification to the build-map.xml with proper format
                    XmlWriterSettings settings = new XmlWriterSettings
                    {
                        Indent = true,
                        IndentChars = "  ",
                        NewLineChars = "\r\n",
                        NewLineHandling = NewLineHandling.Replace
                    };

                    using (XmlWriter writer = XmlWriter.Create(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml", settings))
                    {
                        xml.Save(writer);
                    }
                }
                
            }
            else if (childObject.gameObject.CompareTag("Model"))
            {
                if (childObject.name != "Camera")
                {
                    ModelProperties modelProperties = childObject.GetComponent<ModelProperties>();

                    XmlElement Modelelement = xml.CreateElement("Model"); //Create a new node from scratch
                    Modelelement.SetAttribute("X", (childObject.transform.position.x * 100).ToString().Replace(',', '.')); //Add X position (Refit into the Vector units)
                    Modelelement.SetAttribute("Y", (-childObject.transform.position.y * 100).ToString().Replace(',', '.')); // Add Y position (Negative because Vector see the world upside down)
                    Modelelement.SetAttribute("Type", modelProperties.Type.ToString()); //Add an name
                    Modelelement.SetAttribute("ClassName", Regex.Replace(childObject.name, @" \((.*?)\)", string.Empty)); //Add an name

                    if (modelProperties.UseLifeTime)
                    {
                        Modelelement.SetAttribute("LifeTime", modelProperties.LifeTime); //Add an name
                    }

                    contentElement.AppendChild(Modelelement);
                }
            }

            //add content to the object
            objectElement.AppendChild(contentElement);
        }

        node.FirstChild.AppendChild(objectElement); //Place it into the Object node
        xml.Save(Application.dataPath + "/XML/dzip/level_xml/" + mapToOverride + ".xml"); //Apply the modification to the build-map.xml file}
    }


    void StartDzip()
    {

        // Check if Vector.exe is running - if yes, close it
        Process[] processes = Process.GetProcessesByName("Vector");
        foreach (Process process in processes)
        {
            if (!process.HasExited)
            {
                UnityEngine.Debug.LogWarning("Vector.exe is still open !! Closing it... (be careful next time)");
                process.Kill();
                process.WaitForExit();
            }
        }

        // Start compressing levels into level_xml.dz
        Process dzipProcess = Process.Start(Application.dataPath + "/XML/dzip/dzip.exe", Application.dataPath + "/XML/dzip/config-map.dcl");
        if (dzipProcess != null)
        {
            dzipProcess.WaitForExit();

            //check if the process has exited properly
            if (dzipProcess.HasExited)
            {
                // Move level_xml.dz if Vector path is found
                string sourceFilePath = Application.dataPath + "/XML/dzip/level_xml.dz";
                string destinationFilePath = Path.Combine(vectorFilePath, "level_xml.dz");

                if (File.Exists(sourceFilePath))
                {
                    if (File.Exists(destinationFilePath))
                    {
                        File.Delete(destinationFilePath);
                    }
                    File.Copy(sourceFilePath, destinationFilePath);
                }
                else
                {
                    UnityEngine.Debug.LogError("level_xml.dz was not found !! Check if your Vector path is correct");
                }
            }
            else
            {
                UnityEngine.Debug.LogError("dzip.exe did not exit properly.");
            }
        }
        else
        {
            UnityEngine.Debug.LogError("Failed to start dzip.exe");
        }
    }

}
