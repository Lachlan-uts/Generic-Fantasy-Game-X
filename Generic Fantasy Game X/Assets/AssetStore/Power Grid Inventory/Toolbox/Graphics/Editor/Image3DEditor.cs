/**********************************************
* Ancient Craft Games
* Copyright 2014-2017 James Clark
**********************************************/
using UnityEditor;
using Toolbox.Graphics;

namespace Toolbox.ToolboxEditor
{
    /// <summary>
    /// Custom inspector for the Image3D UI element.
    /// </summary>
    [CustomEditor(typeof(Image3D))]
    [CanEditMultipleObjects]
    public class Image3DEditor : AbstractSuperEditor
    {
    }

    /// <summary>
    /// Custome inspector for the UIRotate element.
    /// </summary>
    [CustomEditor(typeof(UIRotate))]
    [CanEditMultipleObjects]
    public class UIRotate : AbstractSuperEditor
    {

    }
}
