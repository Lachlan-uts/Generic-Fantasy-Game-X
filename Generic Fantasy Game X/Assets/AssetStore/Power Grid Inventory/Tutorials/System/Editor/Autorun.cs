using UnityEngine;
using System.Collections;
using UnityEditor;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

namespace AncientCraftGames.Tutorial
{
    /// <summary>
    /// A utility class that tracks scene changes and display tutorials
    /// when specific scenes are loaded.
    /// </summary>
    [InitializeOnLoad]
    public class Autorun
    {
        static string CurrentScene = "";
        public GUIStyle Style;

        /// <summary>
        /// List of all scenes that should attempt to load a tutorial object
        /// //from the scene and display and editor window with that info.
        /// </summary>
        static List<string> TutorialScenes = new List<string>(new string[]
            {
                "01 Basic Setup",
                "02 Always Square Slots",
                "03 Items",
                "04 Equipment Slots",
                "05 Event Triggers", //sounds and tooltips
                "06 Item Filters",
                "07 Rotating Items",
                "08 Nestable Items",
                "09 Sockets",
                "10 Multiple Inventories",
                "11 Save and Load",
                "12 Customizing the Look",
                "13 Example Game (Pickups, SoundFx, etc)",
                "14 Extreme Customization", //scrolling grid list
                "15 Shopkeeper Example",
            });

        
        

        /// <summary>
        /// Startup method that gets called on compilation.
        /// </summary>
        static Autorun()
        {
            EditorApplication.hierarchyWindowChanged += CheckScene;
        }

        /// <summary>
        /// 
        /// </summary>
        static void CheckScene()
        {
            var scene = SceneManager.GetActiveScene();
            if(CurrentScene != scene.name)
            {
                CurrentScene = scene.name;
                if (TutorialScenes.Contains(scene.name))
                    HandleSceneChanged(scene);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="scene"></param>
        static void HandleSceneChanged(Scene scene)
        {
            //search the scene for out 'Tutorial' object and get the tutorial info
            var tutorial = GameObject.FindObjectOfType<TutorialPages>();

            if (tutorial != null) TutorialWindow.ShowWindow(tutorial.PageText);
            else TutorialWindow.ShowWindow(null);
            
        }
       
    }
}

