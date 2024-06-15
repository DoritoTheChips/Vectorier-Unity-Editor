using System.IO;
using UnityEngine;
using UnityEditor;

public class BindecReader : EditorWindow
{
    private string filePath = "";
    private bool incrementNames = false;

    private static readonly string[] predefinedNames = new string[]
    {
        "NHip_1", "NHip_2", "NStomach", "NChest", "NNeck", "NShoulder_1", "NShoulder_2",
        "NKnee_1", "NKnee_2", "NAnkle_1", "NAnkle_2", "NToe_1", "NHeel_1", "NToeTip_1", "NToeS_1", "NHeel_2", "NToe_2",
        "NToeTip_2", "NToeS_2", "NElbow_1", "NElbow_2", "NWrist_1", "NWrist_2", "NKnuckles_1", "NFingertips_1",
        "NKnucklesS_1", "NKnuckles_2", "NFingertips_2", "NKnucklesS_2", "NHead", "NTop", "NChestS_1", "NChestS_2",
        "NStomachS_1", "NStomachS_2", "NChestF", "NStomachF", "NPelvisF", "NHeadS_1", "NHeadS_2", "NHeadF", "NPivot",
        "DetectorH", "DetectorV", "COM", "Camera"
    };

    [MenuItem("Vectorier/Bindec Read")]
    public static void ShowWindow()
    {
        GetWindow<BindecReader>("Bindec Reader");
    }

    void OnGUI()
    {
        GUILayout.Label("Select .bindec file", EditorStyles.boldLabel);

        if (GUILayout.Button("Select File"))
        {
            filePath = EditorUtility.OpenFilePanel("Select .bindec file", "", "bindec");
        }

        GUILayout.Label("File Path: " + filePath);

        incrementNames = GUILayout.Toggle(incrementNames, "Increment Sphere Names");

        if (!string.IsNullOrEmpty(filePath) && GUILayout.Button("Read and Place Objects"))
        {
            ReadFileAndPlaceObjects(filePath, incrementNames);
        }
    }

    private static void ReadFileAndPlaceObjects(string path, bool incrementNames)
    {
        if (!File.Exists(path))
        {
            Debug.LogError("File not found: " + path);
            return;
        }

        string[] lines = File.ReadAllLines(path);
        int frameIndex = 1; // Frame naming starts from 1

        foreach (string line in lines)
        {
            if (line.Contains("END"))
            {
                // Create a parent GameObject for each line
                GameObject parentObject = new GameObject("Frame" + frameIndex);
                frameIndex++;

                Vector3[] positions = ParseLine(line);
                for (int i = 0; i < positions.Length; i++)
                {
                    Vector3 position = positions[i];
                    if (position != Vector3.zero)
                    {
                        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        sphere.transform.position = position;
                        sphere.transform.parent = parentObject.transform; // Set the parent

                        if (incrementNames)
                        {
                            if (i < predefinedNames.Length)
                            {
                                sphere.name = predefinedNames[i];
                            }
                            else
                            {
                                sphere.name = "Sphere_" + (i + 1); // Default naming if names run out
                            }
                        }
                        // Else, keep the default name
                    }
                }
            }
        }
    }

    private static Vector3[] ParseLine(string line)
    {
        // Get everything between the square brackets and the "END" token
        int startIndex = line.IndexOf('[') + 1;
        int endIndex = line.LastIndexOf("END");
        if (startIndex == -1 || endIndex == -1)
        {
            Debug.LogError("Line format is incorrect: " + line);
            return new Vector3[0];
        }

        string coordinatePart = line.Substring(startIndex, endIndex - startIndex);
        // Extracting all coordinate sets between curly braces
        var matches = System.Text.RegularExpressions.Regex.Matches(coordinatePart, @"\{([^}]*)\}");
        Vector3[] positions = new Vector3[matches.Count];

        for (int i = 0; i < matches.Count; i++)
        {
            string triplet = matches[i].Groups[1].Value;
            string[] values = triplet.Split(',');

            if (values.Length != 3)
            {
                Debug.LogError("Triplet format is incorrect: " + triplet);
                continue;
            }

            if (float.TryParse(values[0], out float x) && float.TryParse(values[1], out float y) && float.TryParse(values[2], out float z))
            {
                // Invert the y-axis
                positions[i] = new Vector3(x, -y, z);
            }
            else
            {
                Debug.LogError("Error parsing float values from triplet: " + triplet);
                positions[i] = Vector3.zero;
            }
        }
        return positions;
    }
}
