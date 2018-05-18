/**********************************************
* Ancient Craft Games
* Copyright 2014-2017 James Clark
**********************************************/
using UnityEngine;

namespace Toolbox.Common
{
    /// <summary>
    /// Represents a pool of objects with Transforms - which can store any kind of GameObject.
    /// </summary>
    public class TransformPool : PoolBehaviour<Transform>
    {
        public int PoolId;

        
    }
}
