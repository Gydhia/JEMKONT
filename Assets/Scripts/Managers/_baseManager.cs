using UnityEngine;

namespace DownBelow.Managers
{
    public abstract class _baseManager : MonoBehaviour
    {

    }
    public abstract class _baseManager<ControllerType> : _baseManager
        where ControllerType : _baseManager
    {
        protected System.Random generator = new System.Random();

#if UNITY_EDITOR
        // Editor version of Instance Allows to run in Edit mode.
        private static ControllerType _instance = null;
        public static ControllerType Instance
        {
            get
            {
                // If we don't do this in editor, it's sometimes fcked up and make errors
                if (_instance == null)
                    _instance = FindObjectOfType<ControllerType>();
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }
#else
        public static ControllerType Instance { get; private set; }
#endif

        public virtual void Awake()
        {
            Instance = this as ControllerType;
        }

#if UNITY_EDITOR
        /// <summary>
        /// This OnEable method is used to replicate Awake within the editor.
        /// </summary>
        [ExecuteInEditMode]
        public virtual void OnEnable()
        {
            Instance = this as ControllerType;
        }
#endif
    }


}