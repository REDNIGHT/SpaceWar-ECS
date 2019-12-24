using UnityEngine;


namespace RN
{
    /// <summary>
    /// 
    ///  public class MyClass : Singleton<MyClass>
    ///  {
    ///      protected new void Awake()
    ///      {
    ///          base.Awake();
    ///          
    ///          //todo...
    ///      }
    ///  }
    ///     
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks></remarks>
    public abstract class Singleton<T> : MonoBehaviour
        where T : Singleton<T>
    {
        //
        protected static bool _appIsQuitting = false;
        public static bool appIsQuitting { get { return _appIsQuitting; } }
        protected void OnApplicationQuit() { _appIsQuitting = true; }


        /// <summary>
        /// Gets the singleton.
        /// </summary>
        /// <remarks></remarks>
        protected static T _singleton;
        public static T singleton
        {
            get
            {
#if UNITY_EDITOR
                if (_singleton == null)
                {
                    if (Application.isPlaying == true)
                        return null;
                    else
                        return singletonForce;
                }
#endif
                if (_appIsQuitting)
                    return null;

                return _singleton;
            }
        }
        public static Transform singletonT
        {
            get
            {
                if (singleton == null)
                    return null;
                return singleton.transform;
            }
        }
        public static GameObject singletonGO
        {
            get
            {
                if (singleton == null)
                    return null;
                return singleton.gameObject;
            }
        }
        public static T singletonForce
        {
            get
            {
                if (_singleton == null)
                    _singleton = GameObject.FindObjectOfType<T>();

                return _singleton;
            }
        }


        protected void Awake()
        {
            if (_singleton != null)
            {
                if (_singleton != this)
                {
                    doubleHandle();
                }

                return;
            }

            //
            _singleton = this as T;

            if (_singleton == null)
                Debug.LogError("_singleton == null  this=" + this, this);
        }


        protected virtual void doubleHandle()
        {
            Debug.LogError("_singleton != null  _singleton=" + _singleton + "  this=" + this, this);

            this.destroy();
        }

        protected void OnDestroy()
        {
            if (notThis)
                return;

            _singleton = null;
        }

        protected bool isThis { get { return _singleton == this; } }
        protected bool notThis { get { return _singleton != this; } }
    }

    /*public abstract class Singleton2<T> : Singleton<T>
        where T : Singleton2<T>
    {
        protected new void Awake()
        {
            //do nothing...
        }
        protected new void OnDestroy()
        {
            //do nothing...
        }

        protected void OnEnable()
        {
            base.Awake();
        }

        protected void OnDisable()
        {
            base.OnDestroy();
        }
    }


    public abstract class Singleton3<T> : Singleton<T>
        where T : Singleton3<T>
    {
        protected new void Awake()
        {
            if (_singleton != null)
                Debug.LogError("_singleton != null  _singleton=" + _singleton, this);

            //
            _singleton = this.GetComponent<T>();

            if (_singleton == null)
                Debug.LogError("_singleton == null", this);
        }
    }*/

    /*
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks></remarks>
    public class TSingleton<T> where T : class
    {
        /// <summary>
        /// Gets the singleton.
        /// </summary>
        /// <remarks></remarks>
        static public T singleton
        {
            get;
            protected set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TSingleton&lt;T&gt;"/> class.
        /// </summary>
        /// <remarks></remarks>
        public TSingleton()
        {
            if (singleton != null)
                Debug.LogError("_singleton != null");

            singleton = this as T;
        }
    }
    */
}