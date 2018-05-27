﻿/**********************************************
* Power Grid Inventory
* Copyright 2015-2016 James Clark
**********************************************/
using UnityEngine;
using System.Collections;
using UnityEditor;
using PowerGridInventory;

namespace PowerGridInventory.Editor
{
    [CustomEditor(typeof(PGIView))]
    [CanEditMultipleObjects]
    public class PGIViewEditor : PGIAbstractEditor
    {
        PGIView TargetView;

        protected override void OnEnable()
        {
            TargetView = target as PGIView;

            //we need to do this because CustomEditor is kinda dumb
            //and won't expose the type we passed to it. Plus relfection
            //can't seem to get at the data either.
            EditorTargetType = typeof(PGIView);
            base.OnEnable();
        }
        
        public override void OnSubInspectorGUI()
        {
            EditorGUILayout.Space();
            if(TargetView != null)
            {
                EditorGUILayout.LabelField("Grid Stats", EditorStyles.boldLabel);
                EditorGUI.BeginChangeCheck();

                //'DisableRendering' member
                TargetView.DisableRendering = EditorGUILayout.Toggle(
                    new GUIContent("Disable Rendering", "Used to disable all UI elements within the view's grid."),
                    TargetView.DisableRendering);
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("_DisableRendering"),
                //    new GUIContent("Disable Rendering", "Used to disable all UI elements within the view's grid."));
                EditorGUILayout.Separator();

                //'Model' member
                TargetView.Model = EditorGUILayout.ObjectField(
                    new GUIContent("Model", "The PGIModel whose data is displayed and manipulated by this view."),
                    TargetView.Model, typeof(PGIModel), true) as PGIModel;
                //EditorGUILayout.PropertyField(serializedObject.FindProperty("_Model"),
                //    new GUIContent("Model", "The PGIModel whose data is displayed and manipulated by this view."));


                if (EditorGUI.EndChangeCheck() || GUI.changed) this.serializedObject.ApplyModifiedProperties();
                EditorGUILayout.Space();

            }

        }
    }
}
