#if UNITY_EDITOR

using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using System;

[CustomEditor(typeof(CustomRuleTile), true)]
public class CustomRuleTileEditor : RuleTileEditor
{
    private ReorderableList m_ReorderableOtherList;

    public const float k_DefaultElementHeight = 48f;
    public const float k_PaddingBetweenRules = 4f;
    public const float k_SingleLineHeight = 18f;
    public const float k_LabelWidth = 80f;

    public override void OnEnable()
    {
        base.OnEnable();

        var customTile = target as CustomRuleTile;
        if (customTile == null) return;

        m_ReorderableOtherList = new ReorderableList(
            serializedObject,
            serializedObject.FindProperty("m_OtherTilingRules"),
            draggable: true, displayHeader: true, displayAddButton: true, displayRemoveButton: true
        );

        m_ReorderableOtherList.drawHeaderCallback = rect => EditorGUI.LabelField(rect, "Other Tile Rules");
        m_ReorderableOtherList.drawElementCallback = OnDrawOtherElement;
        m_ReorderableOtherList.elementHeightCallback = GetOtherElementHeight;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        EditorGUILayout.Space(10);

        serializedObject.Update();
        m_ReorderableOtherList.DoLayoutList();
        serializedObject.ApplyModifiedProperties();
    }

    // 높이 계산 콜백 (수정 없음)
    private float GetOtherElementHeight(int index)
    {
        var rule = m_ReorderableOtherList.serializedProperty.GetArrayElementAtIndex(index);
        var output = (RuleTile.TilingRuleOutput.OutputSprite)rule.FindPropertyRelative("m_Output").enumValueIndex;

        float height = k_SingleLineHeight * 4;

        if (output == RuleTile.TilingRuleOutput.OutputSprite.Animation)
        {
            height += k_SingleLineHeight * 2;
        }
        if (output == RuleTile.TilingRuleOutput.OutputSprite.Random)
        {
            height += k_SingleLineHeight;
        }
        if (output != RuleTile.TilingRuleOutput.OutputSprite.Single)
        {
            var sprites = rule.FindPropertyRelative("m_Sprites");
            height += k_SingleLineHeight * (1 + sprites.arraySize);
        }

        return Mathf.Max(k_DefaultElementHeight, height) + k_PaddingBetweenRules;
    }

    // 리스트 항목 그리기 콜백 (레이아웃 너비 수정)
    protected virtual void OnDrawOtherElement(Rect rect, int index, bool isActive, bool isfocused)
    {
        var rule = m_ReorderableOtherList.serializedProperty.GetArrayElementAtIndex(index);

        rect.y += 2f;
        rect.height -= k_PaddingBetweenRules;
        
        float spriteWidth = k_DefaultElementHeight + 5f;
        float matrixWidth = 54f;
        float spacing = 10f;

        Rect spriteRect = new Rect(rect.xMax - spriteWidth, rect.y, spriteWidth, k_DefaultElementHeight);
        Rect matrixRect = new Rect(rect.xMax - spriteWidth - matrixWidth - (spacing / 2), rect.y, matrixWidth, k_DefaultElementHeight);
        Rect inspectorRect = new Rect(rect.x, rect.y, rect.width - matrixWidth - spriteWidth - spacing, rect.height);

        OtherRuleInspectorOnGUI(inspectorRect, rule);
        OtherRuleMatrixOnGUI(matrixRect, rule.FindPropertyRelative("m_Neighbors"));
        OtherSpriteOnGUI(spriteRect, rule);
    }

    // 좌측 인스펙터 UI 그리기 (수정 없음)
    private void OtherRuleInspectorOnGUI(Rect rect, SerializedProperty rule)
    {
        Rect lineRect = new Rect(rect.x, rect.y, rect.width, k_SingleLineHeight);

        var otherTileProp = rule.FindPropertyRelative("m_OtherTile");
        GUI.Label(lineRect, "Other Tile");
        EditorGUI.PropertyField(new Rect(lineRect.x + k_LabelWidth, lineRect.y, lineRect.width - k_LabelWidth, lineRect.height), otherTileProp, GUIContent.none);
        lineRect.y += k_SingleLineHeight;

        var goProp = rule.FindPropertyRelative("m_GameObject");
        var colliderProp = rule.FindPropertyRelative("m_ColliderType");
        var outputProp = rule.FindPropertyRelative("m_Output");

        GUI.Label(lineRect, "GameObject");
        EditorGUI.PropertyField(new Rect(lineRect.x + k_LabelWidth, lineRect.y, lineRect.width - k_LabelWidth, lineRect.height), goProp, GUIContent.none);
        lineRect.y += k_SingleLineHeight;

        GUI.Label(lineRect, "Collider");
        EditorGUI.PropertyField(new Rect(lineRect.x + k_LabelWidth, lineRect.y, lineRect.width - k_LabelWidth, lineRect.height), colliderProp, GUIContent.none);
        lineRect.y += k_SingleLineHeight;

        GUI.Label(lineRect, "Output");
        EditorGUI.PropertyField(new Rect(lineRect.x + k_LabelWidth, lineRect.y, lineRect.width - k_LabelWidth, lineRect.height), outputProp, GUIContent.none);
        lineRect.y += k_SingleLineHeight;

        var output = (RuleTile.TilingRuleOutput.OutputSprite)outputProp.enumValueIndex;
        var spritesProp = rule.FindPropertyRelative("m_Sprites");

        if (output == RuleTile.TilingRuleOutput.OutputSprite.Animation)
        {
            var minSpeedProp = rule.FindPropertyRelative("m_MinAnimationSpeed");
            var maxSpeedProp = rule.FindPropertyRelative("m_MaxAnimationSpeed");
            GUI.Label(new Rect(rect.x, lineRect.y, k_LabelWidth, k_SingleLineHeight), "Min Speed");
            minSpeedProp.floatValue = EditorGUI.FloatField(new Rect(rect.x + k_LabelWidth, lineRect.y, rect.width - k_LabelWidth, lineRect.height), minSpeedProp.floatValue);
            lineRect.y += k_SingleLineHeight;
            GUI.Label(new Rect(rect.x, lineRect.y, k_LabelWidth, k_SingleLineHeight), "Max Speed");
            maxSpeedProp.floatValue = EditorGUI.FloatField(new Rect(rect.x + k_LabelWidth, lineRect.y, rect.width - k_LabelWidth, lineRect.height), maxSpeedProp.floatValue);
            lineRect.y += k_SingleLineHeight;
        }
        if (output == RuleTile.TilingRuleOutput.OutputSprite.Random)
        {
            var perlinProp = rule.FindPropertyRelative("m_PerlinScale");
            GUI.Label(new Rect(rect.x, lineRect.y, k_LabelWidth, k_SingleLineHeight), "Noise");
            perlinProp.floatValue = EditorGUI.Slider(new Rect(rect.x + k_LabelWidth, lineRect.y, rect.width - k_LabelWidth, lineRect.height), perlinProp.floatValue, 0.001f, 1f);
            lineRect.y += k_SingleLineHeight;
        }

        if (output != RuleTile.TilingRuleOutput.OutputSprite.Single)
        {
            GUI.Label(new Rect(rect.x, lineRect.y, k_LabelWidth, k_SingleLineHeight), "Size");
            EditorGUI.BeginChangeCheck();
            int newSize = EditorGUI.DelayedIntField(new Rect(rect.x + k_LabelWidth, lineRect.y, rect.width - k_LabelWidth, k_SingleLineHeight), spritesProp.arraySize);
            if (EditorGUI.EndChangeCheck())
                spritesProp.arraySize = Mathf.Max(1, newSize);
            lineRect.y += k_SingleLineHeight;

            for (int i = 0; i < spritesProp.arraySize; i++)
            {
                EditorGUI.PropertyField(new Rect(rect.x + k_LabelWidth, lineRect.y, rect.width - k_LabelWidth, k_SingleLineHeight), spritesProp.GetArrayElementAtIndex(i), GUIContent.none);
                lineRect.y += k_SingleLineHeight;
            }
        }
    }

    // 중앙 규칙 격자 UI 그리기 (수정 없음)
    private void OtherRuleMatrixOnGUI(Rect rect, SerializedProperty neighborsProp)
    {
        if (neighborsProp.arraySize != 9) neighborsProp.arraySize = 9;

        Handles.color = EditorGUIUtility.isProSkin ? new Color(1f, 1f, 1f, 0.2f) : new Color(0f, 0f, 0f, 0.2f);
        int w = (int)rect.width / 3;
        int h = (int)rect.height / 3;
        for (int y = 0; y <= 3; y++) Handles.DrawLine(new Vector3(rect.xMin, rect.yMin + y * h), new Vector3(rect.xMax, rect.yMin + y * h));
        for (int x = 0; x <= 3; x++) Handles.DrawLine(new Vector3(rect.xMin + x * w, rect.yMin), new Vector3(rect.xMin + x * w, rect.yMax));
        Handles.color = Color.white;

        for (int y = 0; y < 3; y++)
        {
            for (int x = 0; x < 3; x++)
            {
                int index = y * 3 + x;
                Rect cellRect = new Rect(rect.x + x * w, rect.y + y * h, w, h);
                var neighborProp = neighborsProp.GetArrayElementAtIndex(index);
                var neighborValue = (OtherTilingRule.Neighbor)neighborProp.enumValueIndex;

                if (index == 4)
                {
                    if (RuleTileEditor.arrows.Length > 4 && RuleTileEditor.arrows[4] != null)
                        GUI.DrawTexture(cellRect, RuleTileEditor.arrows[4]);
                }
                else
                {
                    Vector3Int position = new Vector3Int(x - 1, 1 - y, 0);

                    if (neighborValue == OtherTilingRule.Neighbor.Is)
                    {
                        GUI.DrawTexture(cellRect, arrows[GetArrowIndex(position)]);
                    }
                    else if (neighborValue == OtherTilingRule.Neighbor.IsNot)
                    {
                        GUI.DrawTexture(cellRect, arrows[9]);
                    }

                    if (GUI.Button(cellRect, GUIContent.none, GUIStyle.none))
                    {
                        neighborProp.enumValueIndex = (neighborProp.enumValueIndex + 1) % Enum.GetValues(typeof(OtherTilingRule.Neighbor)).Length;
                    }
                }
            }
        }
    }

    // 우측 스프라이트 UI 그리기 (수정 없음)
    private void OtherSpriteOnGUI(Rect rect, SerializedProperty rule)
    {
        var spritesProp = rule.FindPropertyRelative("m_Sprites");
        if (spritesProp.arraySize == 0) spritesProp.arraySize = 1;

        var firstSprite = spritesProp.GetArrayElementAtIndex(0);
        firstSprite.objectReferenceValue = EditorGUI.ObjectField(new Rect(rect.x + 2, rect.y, rect.width - 4, k_DefaultElementHeight), firstSprite.objectReferenceValue, typeof(Sprite), false);
    }
}

#endif