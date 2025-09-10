#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ItemData))]
public class ItemDataEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var itemId = serializedObject.FindProperty("itemId");
        var itemType = serializedObject.FindProperty("itemType");
        var itemName = serializedObject.FindProperty("itemName");
        var itemWeight = serializedObject.FindProperty("itemWeight");
        var icon = serializedObject.FindProperty("icon");
        var explanation = serializedObject.FindProperty("explanation");
        var priceRange = serializedObject.FindProperty("priceRange");
        var isOre = serializedObject.FindProperty("isOre");
        var oreTile = serializedObject.FindProperty("oreTile");
        var isSpawnable = serializedObject.FindProperty("isSpawnable");

        // 각 필드를 직접 그리기
        EditorGUILayout.PropertyField(itemId);
        EditorGUILayout.PropertyField(itemType);
        EditorGUILayout.PropertyField(itemName);
        EditorGUILayout.PropertyField(itemWeight);
        EditorGUILayout.PropertyField(icon);
        EditorGUILayout.PropertyField(explanation);
        EditorGUILayout.PropertyField(priceRange);

        EditorGUILayout.PropertyField(isOre);
        if (isOre.boolValue)
        {
            EditorGUILayout.PropertyField(oreTile);
        }

        EditorGUILayout.PropertyField(isSpawnable);

        serializedObject.ApplyModifiedProperties();
    }
}
#endif