using UnityEngine;
using UnityEditor;
using TMPro;

public static class TextCreator
{
    [MenuItem("GameObject/3D Object/TextMeshPro - Waypoint Number")]
    static void CreateWaypointNumber()
    {
        // Crear un GameObject per al text
        GameObject textObj = new GameObject("WaypointNumber");
        
        // Afegir el component TextMeshPro
        TextMeshPro textMesh = textObj.AddComponent<TextMeshPro>();
        textMesh.text = "1";
        textMesh.alignment = TextAlignmentOptions.Center;
        textMesh.fontSize = 5;
        textMesh.color = Color.yellow;
        
        // Posicionar-lo si hi ha un objecte seleccionat
        if (Selection.activeGameObject != null)
        {
            textObj.transform.SetParent(Selection.activeGameObject.transform);
            textObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        }
        
        // Seleccionar el nou objecte
        Selection.activeGameObject = textObj;
        
        // Registrar aquesta acció per poder desfer-la
        Undo.RegisterCreatedObjectUndo(textObj, "Create Waypoint Number");
    }
}