using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using System.Collections;
using TMPro;

[TestFixture]
public class FontGlyphVerificationTest
{
    [UnityTest]
    public IEnumerator VerifyCurrentIconGlyphs()
    {
        yield return null;

        // Load the font asset
        var fontAsset = Resources.Load<TMP_FontAsset>("_UI/_Fonts/Font Awesome 651/fa-regular-400 SDF");
        
        if (fontAsset == null)
        {
            // Try direct path
            fontAsset = UnityEngine.Resources.FindObjectsOfTypeAll<TMP_FontAsset>()[0];
            foreach (var font in UnityEngine.Resources.FindObjectsOfTypeAll<TMP_FontAsset>())
            {
                if (font.name.Contains("fa-regular-400"))
                {
                    fontAsset = font;
                    break;
                }
            }
        }

        Assert.IsNotNull(fontAsset, "Font asset should be found");

        Debug.Log($"=== Testing {fontAsset.name} ===");
        Debug.Log($"Character table count: {fontAsset.characterTable.Count}");
        Debug.Log($"Character lookup table count: {fontAsset.characterLookupTable.Count}");

        // Test the icons we're using
        var iconsToTest = new System.Collections.Generic.Dictionary<string, uint>
        {
            { "play (NextUnitButton)", 0xf04b },
            { "sync/arrows-rotate (NextTurnButton)", 0xf021 },
            { "walking (MoveButton)", 0xf554 },
            { "bed (RestButton)", 0xf6bb }
        };

        foreach (var icon in iconsToTest)
        {
            bool exists = fontAsset.characterLookupTable.ContainsKey(icon.Value);
            string status = exists ? "✓ EXISTS" : "✗ MISSING";
            Debug.Log($"{status} - U+{icon.Value:X4} ({icon.Key})");

            if (!exists)
            {
                Debug.LogWarning($"Icon {icon.Key} (U+{icon.Value:X4}) is MISSING from {fontAsset.name}!");
            }
        }

        // Log some sample glyphs that DO exist
        Debug.Log("=== Sample of glyphs that exist in font ===");
        int count = 0;
        foreach (var kvp in fontAsset.characterLookupTable)
        {
            Debug.Log($"U+{kvp.Key:X4} exists");
            count++;
            if (count >= 10) break;
        }

        yield return null;
    }
}

