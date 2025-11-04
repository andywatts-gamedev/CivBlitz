using UnityEngine;
using UnityEditor;
using TMPro;

/// <summary>
/// Editor test to verify which Font Awesome glyphs exist in our font assets.
/// Run via: Window > Font Awesome Glyph Test
/// </summary>
public class FontAwesomeGlyphTest : EditorWindow
{
    private TMP_FontAsset fontAsset;
    private string[] iconCodesToTest = new string[]
    {
        "f04b", // play (NextUnitButton)
        "f021", // arrows-rotate/sync (NextTurnButton)
        "f554", // walking (MoveButton)
        "f6bb", // bed (RestButton)
        "f144", // circle-play (alternative)
        "f061", // arrow-right (alternative)
        "f04e", // step-forward (alternative)
        "f01e", // arrows-rotate (alternative)
    };

    [MenuItem("Window/Font Awesome Glyph Test")]
    public static void ShowWindow()
    {
        GetWindow<FontAwesomeGlyphTest>("FA Glyph Test");
    }

    void OnGUI()
    {
        GUILayout.Label("Font Awesome Glyph Tester", EditorStyles.boldLabel);
        
        fontAsset = (TMP_FontAsset)EditorGUILayout.ObjectField(
            "Font Asset", 
            fontAsset, 
            typeof(TMP_FontAsset), 
            false
        );

        if (GUILayout.Button("Load fa-regular-400 SDF"))
        {
            fontAsset = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
                "Assets/_UI/_Fonts/Font Awesome 651/fa-regular-400 SDF.asset"
            );
        }

        if (GUILayout.Button("Test Icon Codes"))
        {
            TestIconCodes();
        }

        if (GUILayout.Button("List All Available Glyphs"))
        {
            ListAllGlyphs();
        }
    }

    void TestIconCodes()
    {
        if (fontAsset == null)
        {
            Debug.LogError("No font asset selected!");
            return;
        }

        Debug.Log($"=== Testing {fontAsset.name} ===");
        
        foreach (var hexCode in iconCodesToTest)
        {
            uint unicode = System.Convert.ToUInt32(hexCode, 16);
            bool exists = fontAsset.characterLookupTable.ContainsKey(unicode);
            
            string iconName = GetIconName(hexCode);
            string status = exists ? "✓ EXISTS" : "✗ MISSING";
            
            Debug.Log($"{status} - U+{hexCode.ToUpper()} ({iconName})");
            
            if (exists)
            {
                var glyph = fontAsset.characterLookupTable[unicode];
                Debug.Log($"  Glyph Index: {glyph.glyphIndex}, Scale: {glyph.scale}");
            }
        }
    }

    void ListAllGlyphs()
    {
        if (fontAsset == null)
        {
            Debug.LogError("No font asset selected!");
            return;
        }

        Debug.Log($"=== All glyphs in {fontAsset.name} ({fontAsset.characterLookupTable.Count} total) ===");
        
        var sortedKeys = new System.Collections.Generic.List<uint>(fontAsset.characterLookupTable.Keys);
        sortedKeys.Sort();
        
        foreach (var unicode in sortedKeys)
        {
            Debug.Log($"U+{unicode:X4} - Glyph Index: {fontAsset.characterLookupTable[unicode].glyphIndex}");
        }
    }

    string GetIconName(string hexCode)
    {
        switch (hexCode)
        {
            case "f04b": return "play";
            case "f021": return "arrows-rotate/sync";
            case "f554": return "walking";
            case "f6bb": return "bed";
            case "f144": return "circle-play";
            case "f061": return "arrow-right";
            case "f04e": return "step-forward";
            case "f01e": return "sync-alt";
            default: return "unknown";
        }
    }
}

