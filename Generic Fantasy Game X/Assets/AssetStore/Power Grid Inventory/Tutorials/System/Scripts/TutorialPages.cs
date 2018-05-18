#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace AncientCraftGames.Tutorial
{
    /// <summary>
    /// Attach this component to some object in a scene to enable an edit-time
    /// 'tutorial' window to popup when that scene is loaded.
    /// </summary>
    public class TutorialPages : MonoBehaviour
    {
        [TextAreaArray(4,15)]
        public string[] PageText;
        
    }
}
