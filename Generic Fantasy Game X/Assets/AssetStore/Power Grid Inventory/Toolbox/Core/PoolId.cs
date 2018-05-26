/**********************************************
* Ancient Craft Games
* Copyright 2014-2017 James Clark
**********************************************/
using UnityEngine;


namespace Toolbox
{
    /// <summary>
    /// A special component that is automatically and invisibly added to any object
    /// within the context of the Lazurus for GameObjects. This should never
    /// be manually placed on an object by the user.
    /// 
    /// WARNING: This component should never be created or destroyed manually! Doing so 
    /// could cause inconsitant states and memory leaks with the <see cref="AutoPool"/>  class 
    /// that uses it.
    /// </summary>
    [DisallowMultipleComponent]
    [ExecuteInEditMode]
    public sealed class PoolId : MonoBehaviour
    {
        public int Id;

        /// <summary>
        /// 
        /// </summary>
        void Awake()
        {
            if (Application.isEditor && !Application.isPlaying)
            {
                Debug.Log("<color=red>PoolId is not meant to be manually placed in a scene. One will be generated automatically at runtime. Deleting component now.</color>");
                if(!Application.isPlaying) DestroyImmediate(this);
                else Destroy(this);
                return;
            }

            hideFlags = HideFlags.NotEditable;
        }

    }
}
