using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text.RegularExpressions;

/// <summary>
/// Helper script to switch UXML files from fa-regular-400 to fa-solid-900 font.
/// Run via: Tools > Font Awesome > Switch to Solid Font
/// </summary>
public class SwitchToSolidFont : EditorWindow
{
    private bool hasSolidFont = false;
    private string solidFontPath = "Assets/_UI/_Fonts/Font Awesome 651/fa-solid-900 SDF.asset";
    private string regularFontPath = "Assets/_UI/_Fonts/Font Awesome 651/fa-regular-400 SDF.asset";

    [MenuItem("Tools/Font Awesome/Switch to Solid Font")]
    public static void ShowWindow()
    {
        GetWindow<SwitchToSolidFont>("Switch to Solid Font");
    }

    void OnGUI()
    {
        GUILayout.Label("Switch Font Awesome Icons to Solid", EditorStyles.boldLabel);
        
        EditorGUILayout.Space();
        
        // Check if solid font exists
        hasSolidFont = File.Exists(solidFontPath);
        
        if (hasSolidFont)
        {
            EditorGUILayout.HelpBox("✓ fa-solid-900 SDF.asset found!", MessageType.Info);
        }
        else
        {
            EditorGUILayout.HelpBox(
                "✗ fa-solid-900 SDF.asset not found!\n\n" +
                "Please generate it first:\n" +
                "1. Window > TextMesh Pro > Font Asset Creator\n" +
                "2. Source: fa-solid-900.ttf\n" +
                "3. Character Range: f001-f8ff\n" +
                "4. Generate and Save as 'fa-solid-900 SDF.asset'",
                MessageType.Warning
            );
        }

        EditorGUILayout.Space();

        GUI.enabled = hasSolidFont;
        if (GUILayout.Button("Switch All UXML Files to Solid Font"))
        {
            SwitchAllUXMLFiles();
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        if (GUILayout.Button("Revert All UXML Files to Regular Font"))
        {
            RevertAllUXMLFiles();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "This will update:\n" +
            "• Assets/_UI/GameButtons/GameButtons.uxml\n" +
            "• Assets/_UI/UnitButtons/UnitButtons.uxml",
            MessageType.Info
        );
    }

    void SwitchAllUXMLFiles()
    {
        int filesUpdated = 0;

        string[] uxmlFiles = new string[]
        {
            "Assets/_UI/GameButtons/GameButtons.uxml",
            "Assets/_UI/UnitButtons/UnitButtons.uxml"
        };

        foreach (string filePath in uxmlFiles)
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                string newContent = Regex.Replace(
                    content,
                    @"fa-regular-400 SDF\.asset",
                    "fa-solid-900 SDF.asset"
                );

                if (content != newContent)
                {
                    File.WriteAllText(filePath, newContent);
                    filesUpdated++;
                    Debug.Log($"Updated {filePath} to use solid font");
                }
            }
            else
            {
                Debug.LogWarning($"File not found: {filePath}");
            }
        }

        AssetDatabase.Refresh();
        
        if (filesUpdated > 0)
        {
            EditorUtility.DisplayDialog(
                "Success",
                $"Successfully updated {filesUpdated} UXML file(s) to use fa-solid-900!",
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "No Changes",
                "All files already using the correct font.",
                "OK"
            );
        }
    }

    void RevertAllUXMLFiles()
    {
        int filesUpdated = 0;

        string[] uxmlFiles = new string[]
        {
            "Assets/_UI/GameButtons/GameButtons.uxml",
            "Assets/_UI/UnitButtons/UnitButtons.uxml"
        };

        foreach (string filePath in uxmlFiles)
        {
            if (File.Exists(filePath))
            {
                string content = File.ReadAllText(filePath);
                string newContent = Regex.Replace(
                    content,
                    @"fa-solid-900 SDF\.asset",
                    "fa-regular-400 SDF.asset"
                );

                if (content != newContent)
                {
                    File.WriteAllText(filePath, newContent);
                    filesUpdated++;
                    Debug.Log($"Reverted {filePath} to use regular font");
                }
            }
            else
            {
                Debug.LogWarning($"File not found: {filePath}");
            }
        }

        AssetDatabase.Refresh();
        
        if (filesUpdated > 0)
        {
            EditorUtility.DisplayDialog(
                "Success",
                $"Successfully reverted {filesUpdated} UXML file(s) to use fa-regular-400!",
                "OK"
            );
        }
        else
        {
            EditorUtility.DisplayDialog(
                "No Changes",
                "All files already using the correct font.",
                "OK"
            );
        }
    }
}

