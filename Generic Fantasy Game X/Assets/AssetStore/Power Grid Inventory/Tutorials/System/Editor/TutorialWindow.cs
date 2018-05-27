using UnityEditor;
using UnityEngine;
using System.Collections.Generic;
using System.Text.RegularExpressions;


namespace AncientCraftGames.Tutorial
{
    /// <summary>
    /// Used for displaying popup windows within a tutorial scene in the editor.
    /// 
    /// TODO:
    /// -Add 'ASSET' command for hilighting items in Project window.
    /// -Add parenthetical nesting to special commands
    /// -Add commands for creating objects, adding components, and setting fields on components.
    /// </summary>
    public class TutorialWindow : EditorWindow
    {
        List<string> PageText;
        int Page = 0;
        string CurrentPageText = "";
        GameObject PingTarget;
        GUIStyle TextStyle;
        //string PropertyPingTarget;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pageText"></param>
        /// <returns></returns>
        public static TutorialWindow ShowWindow(string[] pageText)
        {
            TutorialWindow window =  EditorWindow.GetWindow(typeof(TutorialWindow), true, "PGI Tutorial") as TutorialWindow;
            window.PageText = new List<string>(pageText);
            window.Page = 0;
            if (window.PageText.Count > 0)
                window.CurrentPageText = window.ProcessPage(window.PageText[0]);

            //setup the style of the text
            window.TextStyle = EditorStyles.textArea;
            window.TextStyle.richText = true;
            window.TextStyle.padding = new RectOffset(5, 5, 10, 20);
            window.TextStyle.fontSize = 12;
            window.TextStyle.wordWrap = true;
            return window;
        }

        /// <summary>
        /// 
        /// </summary>
        public void OnGUI()
        {
            EditorGUILayout.TextArea(CurrentPageText,
                TextStyle,
                GUILayout.ExpandHeight(true), 
                GUILayout.ExpandWidth(true));

            if (PageText != null)
            {
                GUILayout.BeginHorizontal();
                if (GUILayout.Button("Prev Page", GUILayout.Height(35), GUILayout.Width(100)))
                {
                    Page--;
                    if (Page < 0) Page = 0;
                    if (PageText.Count > 0) CurrentPageText = ProcessPage(PageText[Page]);
                }
                GUILayout.FlexibleSpace();
                GUILayout.Label(Page+1 + "/" + PageText.Count);
                GUILayout.FlexibleSpace();
                if (GUILayout.Button("Next Page", GUILayout.Height(35), GUILayout.Width(100)))
                {
                    Page++;
                    if (Page >= PageText.Count) Page = PageText.Count - 1;
                    if (PageText.Count > 0) CurrentPageText = ProcessPage(PageText[Page]);
                }
                GUILayout.EndHorizontal();
            }

            //display anything that was defered for 'pinging' then clear the list
            if(PingTarget != null)
            {
                EditorGUIUtility.PingObject(PingTarget);
                //Highlighter.Highlight("Hierarchy", "View");
                PingTarget = null;
            }
            //if(PropertyPingTarget != null)
            //{
                //Highlighter.Highlight("Inspector", PropertyPingTarget);
                //PropertyPingTarget = null;
            //}
        }

        /// <summary>
        /// Scans the text for special command sequences.
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        string ProcessPage(string text)
        {
            //TODO: Add support for highlighting assets
            //too using ASSET='path to asset'

            //this is used to hilight gameobjects in the scene that are tagged in the page text
            Regex sceneReg = new Regex(@"SCENE='.*'\s");
            foreach(var match in sceneReg.Matches(text))
            {
                //GameObject.Find(match.)
                string goName = match.ToString().Trim();
                goName = goName.Remove(0, 7); //removes the substring SCENE='
                goName = goName.Replace("\'", ""); //removes the trailing '
                var go = GameObject.Find(goName);

                //can't ping directly here, Unity won't allow it
                //so we'll store it for later
                if (go != null)
                {
                    //if (h) Highlighter.Stop();
                    //h = true;
                    PingTarget = go;
                    //Highlighter.Highlight("Hierarchy", go.name);
                }
            }
            text = sceneReg.Replace(text, "");

            /*
            //this is used to hilight gameobjects in the scene that are tagged in the page text
            Regex inspectorReg = new Regex(@"PROP='.*'\s");
            foreach (var match in inspectorReg.Matches(text))
            {
                //GameObject.Find(match.)
                string goName = match.ToString().Trim();
                goName = goName.Remove(0, 6); //removes the substring PROP='
                goName = goName.Replace("\'", ""); //removes the trailing '

                //can't ping directly here, Unity won't allow it
                //so we'll store it for later
                PropertyPingTarget = goName;
            }
            text = inspectorReg.Replace(text, "");
            */

            return text;
        }
    }

}
