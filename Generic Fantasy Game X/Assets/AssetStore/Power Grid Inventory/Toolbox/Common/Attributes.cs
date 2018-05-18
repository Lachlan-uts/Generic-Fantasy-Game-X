/**********************************************
* Ancient Craft Games
* Copyright 2014-2017 James Clark
**********************************************/
using System;
using UnityEngine;

namespace Toolbox.Common
{
    /// <summary>
    /// Apply to types of Vector3 to make their inspector display much smaller.
    /// </summary>
    public class CompactAttribute : PropertyAttribute
    {
        public CompactAttribute() { }
    }


    /// <summary>
    /// Tells the inspector to draw the enum as a mask field rather than an enum dropdown.
    /// </summary>
    public class MaskedEnumAttribute : PropertyAttribute
    {
        public string EnumName;

        public MaskedEnumAttribute() { }

        public MaskedEnumAttribute(string name)
        {
            EnumName = name;
        }

    }


    /// <summary>
    /// Place this above a string in a MonoBehaviour to instruct the inspector
    /// to display a dropdown list with all class names derived from the named type.
    /// </summary>
    public class ClassListAttribute : PropertyAttribute
    {
        public readonly Type InheritsFrom;
        public readonly string Label;
        public readonly string DefaultChoice;

        [Obsolete("Use the version that no longer requires an assembly name.")]
        public ClassListAttribute(string assemblyName, Type inheritsFrom, string label, string defaultChoice)
        {
            InheritsFrom = inheritsFrom;
            Label = label;
            DefaultChoice = defaultChoice;
        }

        public ClassListAttribute(Type inheritsFrom, string label, string defaultChoice)
        {
            InheritsFrom = inheritsFrom;
            Label = label;
            DefaultChoice = defaultChoice;
        }
    }


    /// <summary>
    /// Place this above a string in a MonoBehaviour to instruct the inspector
    /// to display a dropdown list with all class names derived from the named type.
    /// </summary>
    public class InterfaceListAttribute : PropertyAttribute
    {
        public readonly Type InheritsFrom;
        public readonly string Label;
        public readonly string DefaultChoice;

        [Obsolete("Use the version that no longer requires an assembly name.")]
        public InterfaceListAttribute(string assemblyName, Type inheritsFrom, string label, string defaultLabel)
        {
            InheritsFrom = inheritsFrom;
            Label = label;
            DefaultChoice = defaultLabel;
        }

        public InterfaceListAttribute(Type inheritsFrom, string label, string defaultLabel)
        {
            InheritsFrom = inheritsFrom;
            Label = label;
            DefaultChoice = defaultLabel;
        }
    }


    /// <summary>
    /// Use this attribute on class properties
    /// that have explicit backing fields in order
    /// to let custom inspectors display them
    /// in the inspector. The inspectors will utilize
    /// this property to get and set values.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class ShowInInspectorAttribute : Attribute
    {
        public string BackingField;
        public ShowInInspectorAttribute(string backingField)
        {
            BackingField = backingField;
        }
    }


    /// <summary>
    /// Use this attribute on MonoBehviour properties that should be processed like normal
    /// serializable fields of an objects in custom inspectors. Note that this does not
    /// currently support Arrays, Lists, or any kind of generics! You must also be sure
    /// to manually get/set a backing field that is able to be serialized by Unity.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class InspectableAttribute : Attribute
    {
        public string Tooltip;
        public Type ForcedType;

        public InspectableAttribute(string tooltip = null) { Tooltip = tooltip; }
        public InspectableAttribute(Type forcedType) {ForcedType = forcedType; }
        public InspectableAttribute(string tooltip, Type forcedType)
        {
            Tooltip = tooltip;
            ForcedType = forcedType;
        }
    }


    /// <summary>
    /// Similar to Unity's Header attribute but this one works with Toolbox
    /// custom editors and class properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertyHeaderAttribute : Attribute
    {
        public string Title;

        public PropertyHeaderAttribute(string title) { Title = title; }
    }

    /// <summary>
    /// Similar to Unity's Space attribute but this one works with Toolbox
    /// custom editors and class properties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class PropertySpaceAttribute : Attribute
    {
        public float Height;

        public PropertySpaceAttribute(float height) { Height = height; }
    }


    /// <summary>
    /// Similar to Unity's Range attribute but this one works with Toolbox
    /// custom editors and class proeprties.
    /// </summary>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false)]
    public class LimitAttribute : Attribute
    {
        public float MinF, MaxF;
        public int MinI, MaxI;

        public LimitAttribute(int min, int max)
        {
            MinI = min;
            MaxI = max;
        }

        public LimitAttribute(float min, float max)
        {
            MinF = min;
            MaxF = max;
        }
    }


    /// <summary>
	/// Used to mark a field so that the Toolbox AbstractEditor
	/// knows which elements to hide under a single foldout.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FoldedGroupFieldAttribute : System.Attribute
    {
        public string GroupId;

        public FoldedGroupFieldAttribute(string groupId)
        {
            GroupId = groupId;
        }
    }


    /// <summary>
	/// Used to mark a boolean field that is used for storing the
    /// folded state of a group in a custom inspector.
	/// </summary>
	[AttributeUsage(AttributeTargets.Field, AllowMultiple = false)]
    public class FoldFlagAttribute : System.Attribute
    {
        public string GroupId;
        public FoldFlagAttribute(string groupId) { GroupId = groupId; }
    }
}