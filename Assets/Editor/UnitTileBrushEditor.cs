using UnityEditor;
using UnityEditor.Tilemaps;
using UnityEngine;
using UnityEngine.Tilemaps;

[CustomEditor(typeof(GridSelection))]
public class UnitTileBrushEditor : Editor
{
    SerializedObject gridSelectionSO;

    void OnEnable()
    {
        gridSelectionSO = new SerializedObject(target);
    }

    public override void OnInspectorGUI()
    {
        gridSelectionSO.Update();
        // Debug.Log("OnInspectorGUI");
        
        // Draw all properties from the GridSelection
        SerializedProperty iterator = gridSelectionSO.GetIterator();
        bool enterChildren = true;
        while (iterator.NextVisible(enterChildren))
        {
            EditorGUILayout.PropertyField(iterator, true);
            enterChildren = false;
        }
        gridSelectionSO.ApplyModifiedProperties();

        // Add our custom unit properties
        if (GridSelection.active && 
            GridSelection.target != null && 
            GridSelection.target.TryGetComponent<Tilemap>(out var tilemap))
        {
            var pos = GridSelection.position.position;
            if (tilemap.GetTile(pos) is UnitTile unitTile)
            {
                EditorGUILayout.Space(20);
                EditorGUILayout.LabelField("UNIT PROPERTIES", EditorStyles.boldLabel);
                EditorGUILayout.Space(5);
                
                Color color = unitTile.civ?.color ?? Color.white;
                EditorGUI.BeginDisabledGroup(true);
                EditorGUILayout.ObjectField("Civ", unitTile.civ, typeof(Civilization), false);
                EditorGUILayout.ColorField("Civ Color", color);
                EditorGUI.EndDisabledGroup();
            

                tilemap.SetColor(pos, color);
            }
        }
    }
} 