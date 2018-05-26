using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

namespace AncientCraftGames.Tutorial
{
    /// <summary>
    /// Custom inspector editor for the TutorialPages component.
    /// </summary>
    [UnityEditor.CustomEditor(typeof(TutorialPages))]
    public class TutorialPageEditor : UnityEditor.Editor
    {
        /*
        public override void OnInspectorGUI()
        { 
            EditorGUI.BeginChangeCheck();
            SerializedProperty pagesProp = serializedObject.FindProperty("PageText");


            for (int i = 0; i < pagesProp.arraySize; i++)
            {
                //remove page button
                GUILayout.BeginHorizontal();
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("-", GUILayout.Width(15)))
                {
                    pagesProp.DeleteArrayElementAtIndex(i);
                    break;
                }
                GUILayout.EndHorizontal();

                //text goes here
                var prop = pagesProp.GetArrayElementAtIndex(i);
                prop.stringValue = GUILayout.TextArea(prop.stringValue, GUILayout.Height(110));

            }

            //add new page button
            EditorGUILayout.Space();
            if (GUILayout.Button("+"))
            {
                pagesProp.InsertArrayElementAtIndex(pagesProp.arraySize - 1);
                pagesProp.GetArrayElementAtIndex(pagesProp.arraySize - 1).stringValue = "";
                //if (Tutorial.PageText == null) Tutorial.PageText = new List<string>();
                //Tutorial.PageText.Add("");
                //CurrentPage = Tutorial.PageText.Count - 1;
                //serializedObject.ApplyModifiedProperties();
            }

            if (EditorGUI.EndChangeCheck() || GUI.changed)
            {
                serializedObject.ApplyModifiedProperties();
            }

            
    
        }
        */
    }
}
