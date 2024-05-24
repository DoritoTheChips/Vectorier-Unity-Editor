using UnityEngine;
using UnityEditor;
using System.IO;

public class NameVariation : MonoBehaviour
{
    public string nameString = ""; // string to be add behind the name of each files

    [MenuItem("Vectorier/Rename Files")]
    static void RenameFiles()
    {
        string folderPath = EditorUtility.OpenFolderPanel("Select Folder", "", ""); //folder path
        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogError("Folder path is not selected!");
            return;
        }

        NameVariation fileRenamer = FindObjectOfType<NameVariation>(); //find the nameVariation component in the scene
        if (fileRenamer == null)
        {
            Debug.LogError("FileRenamer component not found in the scene!");
            return;
        }

        string nameString = fileRenamer.nameString;

        if (!Directory.Exists(folderPath))
        {
            Debug.LogError("Folder does not exist: " + folderPath);
            return;
        }

        string[] files = Directory.GetFiles(folderPath);

        foreach (string filePath in files)
        {
            string fileName = Path.GetFileName(filePath);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            string fileExtension = Path.GetExtension(fileName);
            string newFileName = fileNameWithoutExtension + nameString + fileExtension;

            string newFilePath = Path.Combine(Path.GetDirectoryName(filePath), newFileName);

            File.Move(filePath, newFilePath);
        }

        Debug.Log("File renaming completed!");
    }
}
