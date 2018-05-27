/**********************************************
* Ancient Craft Games
* Copyright 2014-2017 James Clark
**********************************************/
#define PGI_LITE
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Toolbox
{
    /// <summary>
    /// Base class to inherit from when creating a singleton MonoBehaviour.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    [DisallowMultipleComponent]
    public abstract class SingletonMonoBehaviour<T> : MonoBehaviour where T : MonoBehaviour
    {
        static bool AppQuit = false;
        static object _lock = new object();
        static protected T _Instance;

        [Header("Singleton")]
        [Tooltip("If this is set when the object awakens, it will be flagged for persistence through scene changes.")]
        public bool Persist = false;

        [Tooltip("If set, when another instance of this object is instantiated an exception will be thrown. Otherwise, the new instance will simply be destroyed and a message displayed.")]
        public bool ErrorIfMultiple = false;
        
        //this is just here to make reflection a bit easier to use
        private static T InternalInstance() { return Instance; }

        public static T Instance
        {
            get
            {
                if (AppQuit)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                        "' already destroyed on application quit." +
                        " Won't create again - returning null.");
                    return null;
                }

                lock (_lock)
                {
                    if (_Instance == null)
                    {
                        //BUG ALERT: if the object a prefab and is selected in the Project window list, this will give false positives!!
                        //we need to check for HideAndDontSave!
                        Object[] finds = FindObjectsOfType(typeof(T));
                        if(finds == null || finds.Length < 1)
                        {
                            var go = SingletonAutoStarter.Singleton;
#if !PGI_LITE
                            var asset = Resources.Load<TextAsset>(typeof(T).Name);
                            if(asset != null)
                            {
                                Pantagruel.Serializer.XmlDeserializer.DeserializeComponent(asset.text, 1, go, typeof(T));
                                _Instance = go.GetComponent<T>();
                                if (TypeHelper.IsReferenceNull(_Instance))
                                {
                                    Debug.LogError("Failed to cast deserialized data as component type '" + typeof(T).Name + "'.");
                                    _Instance = go.AddComponent<T>();
                                }
                                //else Debug.Log("Loaded singleton profile for " + typeof(T).Name);
                            }
                            else _Instance = go.AddComponent<T>();
#else
                            _Instance = go.AddComponent<T>();
#endif
                        }
                        else _Instance = (T)finds[0];

                        _Instance.Invoke("SingletonAwake", 0);
                        if (finds.Length > 1)
                        {
                            Debug.LogError("[Singleton] Something went really wrong " +
                                " - there should never be more than one singleton!" +
                                " Re-openning the scene might fix it.");
                            return _Instance;
                        }


                    }

                    return _Instance;
                }
            }
        }

        bool CopyProtection;
        protected virtual void Awake()
        {
            if (_Instance != null)
            {
                if (ErrorIfMultiple)
                    throw new UnityException("--------- There is already an instance of '" + gameObject.name + "' in the scene! ---------");
#if TOOLBOX_DEBUG
                else Debug.Log("<color=yellow>Already an instance of the singleton " + gameObject.name + " in the scene. Deleting copy now.</color>");
#endif
                CopyProtection = true;
                Destroy(gameObject);
                CopyProtection = false;
                return;
            }
            _Instance = this as T;

#if UNITY_5_5_OR_NEWER
            SceneManager.sceneLoaded += HandleSceneLoaded;
#endif
        }

        protected virtual void Start() { }

        protected abstract void SingletonAwake();

        /// <summary>
	    /// When Unity quits, it destroys objects in a random order.
	    /// In principle, a Singleton is only destroyed when application quits.
	    /// If any script calls Instance after it has been destroyed, 
	    /// it will create a buggy ghost object that will stay in the Editor scene
	    /// even after stopping playing the Application. Really bad!
	    /// So, this was made to be sure we're not creating that buggy ghost object.
	    /// </summary>
        protected virtual void OnDestroy()
        {
#if UNITY_5_5_OR_NEWER
            SceneManager.sceneLoaded -= HandleSceneLoaded;
#endif

            if (CopyProtection) return;

            if (Application.isPlaying)
            {
                if (Persist) AppQuit = false;
            }
            else AppQuit = true;
        }

        /// <summary>
	    /// When Unity quits, it destroys objects in a random order.
	    /// In principle, a Singleton is only destroyed when application quits.
	    /// If any script calls Instance after it has been destroyed, 
	    /// it will create a buggy ghost object that will stay in the Editor scene
	    /// even after stopping playing the Application. Really bad!
	    /// So, this was made to be sure we're not creating that buggy ghost object.
	    /// </summary>
        protected virtual void OnApplicationQuit()
        {
            AppQuit = true;
        }

#if !UNITY_5_5_OR_NEWER
        private void OnLevelWasLoaded(int level)
        {
            HandleSceneLoaded(SceneManager.GetSceneAt(level));
        }

        /// <summary>
        /// Called when the scene changes.
        /// </summary>
        /// <param name="scene"></param>
        protected virtual void HandleSceneLoaded(Scene scene)
        {

        }
#endif

        /// <summary>
        /// Called when the scene changes.
        /// </summary>
        /// <param name="scene"></param>
        /// <param name="mode"></param>
        protected virtual void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
        {

        }
    }


    /// <summary>
    /// Specialized singleton class that is automatically created at game startup.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class GlobalSingletonMonoBehaviour<T> : SingletonMonoBehaviour<T> where T : MonoBehaviour
    {
        protected abstract void AutoSingletonInit();
    }


    /// <summary>
    /// Internal Utility class that is used to generate AutoSingletonMonoBehaviours at game startup.
    /// </summary>
    public static class SingletonAutoStarter
    {
        public static readonly string SingletonSourceName = "Singleton";
        public static readonly string PersistentSingletonSourceName = "Persistent Singleton";

        static bool RanOnce;
        static GameObject Sing;

        /// <summary>
        /// The global, persistent, singleton container GameObject.
        /// </summary>
        public static GameObject Singleton
        {
            get
            {
                if (Sing != null) return Sing;

                //var go = GameObject.FindGameObjectWithTag(SingletonSourceName);
                var go = GameObject.Find(SingletonSourceName);
                if (go == null)
                {
                    go = new GameObject(SingletonSourceName);
                    //go.tag = SingletonSourceName;
                }
                Sing = go;
                GameObject.DontDestroyOnLoad(Sing);
                return go;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Init()
        {
            if (!Application.isPlaying || RanOnce) return;
            RanOnce = true;
            if(Application.isEditor && !Application.isPlaying)
            {
                //TODO: how to add tags to project - we want to add the 'Singleton' tag if we can
            }

            var types = TypeHelper.GetDerivedTypes(typeof(GlobalSingletonMonoBehaviour<>));

            if (types != null)
            {
                for (int i = 0; i < types.Length; i++)
                {
                    var type = types[i];

                    var instanceType = typeof(SingletonMonoBehaviour<>).MakeGenericType(type);
                    var prop = instanceType.GetProperty("Instance");
                    var instObj = prop.GetAccessors()[0].Invoke(type, null);

                    MonoBehaviour inst = instObj as MonoBehaviour;
                    if (inst == null) Debug.LogError("Failed to auto-initialize the singleton '" + type.Name + "'.");
                    else inst.Invoke("AutoSingletonInit", 0);
                }

            }
        }
    }
    
}
