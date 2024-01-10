# Vectorier Unity Editor
Vectorier-Unity-Editor is an level editor for the game Vector using the Unity Engine.

A Discord server is open for the project : https://discord.com/invite/pVRuFBVwC2

# Feature
 * Object importer
 * Level importer
 * Level edition

# Installation
 * Download Unity [2020.3.35f1](https://download.unity3d.com/download_unity/18e4db7a9996/Windows64EditorInstaller/UnitySetup64-2020.3.35f1.exe) at the [official Unity Website](https://unity3d.com/get-unity/download/archive)
 * Download the [Vectorier Unity Editor project](https://github.com/DoritoTheChips/Vectorier-Unity-Editor/archive/refs/heads/main.zip)
 * Extract the zip file
 * Open the project in Unity Hub
 
# Usage
 * First, open the "editor" scene
 * In the hierarchy tab, click on the "ScriptManager" gameobject
 - **If you want to import objects in your level :**
    * In the inspector, enter in the "Object To Conevrt" textbox you wish to convert (The list of object is located in *Assets* > *XML* > *objects.xml*)
    * Then go to *Vectorier* > *Convert from object.xml*
    * **TIPS :** You can also get quick and usefull objects located at *Assets* > *Usefull-Objects*
    
 - **If you want to import a map :**
    * (If you don't see a script called "Show Map" in the inspector, add the script located at *Assets* > *Scripts* > *ShowMap* on the "ScriptManager" gameobject)
    * In the inspector, simply enter the name of the level you wish to import 
    * Then go to *Vectorier* > *Render object sequence*
    
 - **If you want to build a map :**
    * In the inspector, enter your file path in the "Vector File Path" textbox to the Vector directory (where the executable game is)
    * Modify the preference to your liking
        - *"Map To Override" is the map that will be replaced in the game*
        - *"Spawn Hunter" is if you wish to have the hunter spawn in your level (usefull for testing your level)*
    * Then go to *Vectorier* > *BuildMap*
