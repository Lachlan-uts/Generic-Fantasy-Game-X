using UnityEngine;
using System.Collections;
using UnityEditor;

namespace AncientCraftGames.Tutorial
{

    /// <summary>
    /// 
    /// </summary>
    [CustomPropertyDrawer(typeof(TextAreaArrayAttribute))]
    public class TextAreaArrayDrawer : PropertyDrawer
    {
        TextAreaArrayAttribute attr { get { return (TextAreaArrayAttribute)attribute; } }
        readonly int TextHeight = 16;

        public int GetHeight()
        {
            return (attr.Lines * TextHeight);
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return GetHeight() + attr.SpaceBetween;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            Rect textFieldPos = position;
            textFieldPos.height = GetHeight();
            textFieldPos.width -= 15;
            Rect scrollRect = position;
            scrollRect.xMin = textFieldPos.xMin + textFieldPos.width;

            GUIStyle s = new GUIStyle(EditorStyles.textArea);
            s.wordWrap = true;
            property.stringValue = EditorGUI.TextArea(textFieldPos, property.stringValue, s);
            
        }

        void DrawTextField(Rect rect, GUIContent content, SerializedProperty prop)
        {
            prop.stringValue = EditorGUI.TextField(rect, content, prop.stringValue);
        }
    }
}
