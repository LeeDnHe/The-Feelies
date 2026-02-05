using UnityEditor;
using UnityEngine;

/// <summary>
/// LayerAttribute가 붙은 int 필드를 LayerField 드롭다운으로 렌더링합니다.
/// (Editor 전용)
/// </summary>
[CustomPropertyDrawer(typeof(LayerAttribute))]
public class LayerAttributeDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        if (property.propertyType != SerializedPropertyType.Integer)
        {
            EditorGUI.LabelField(position, label.text, "Use [Layer] with int.");
            return;
        }

        property.intValue = EditorGUI.LayerField(position, label, property.intValue);
    }
}

