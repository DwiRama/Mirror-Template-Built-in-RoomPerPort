using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FXSceneEditor {
    /// <summary>
    /// Helper class to generate a MonoBehaviour Singleton
    /// </summary>
    /// <typeparam name="T">Type of class to convert to singleton</typeparam>
    public abstract class MonoBehaviourSingleton<T> : MonoBehaviour where T : MonoBehaviour
    {   
        /// <summary>
        /// Instance of the singleton
        /// </summary>
        public static T Instance { get; private set; }

        /// <summary>
        /// Awake is used to initialize <see cref="Instance"/>
        /// to this object
        /// </summary>
        protected virtual void Awake()
        {
            if (Instance == null)
            {
                Instance = this as T;
            }
        }

        protected virtual void OnDestroy() {
            Instance = null;
        }
    }

    /// <summary>
    /// Helper class to generate a MonoBehaviour Singleton that
    /// survives scene changes
    /// </summary>
    /// <typeparam name="T">Type of class to convert to singleton that survives scene changes</typeparam>
    public abstract class MonoBehaviourSingletonPersistent<T> : MonoBehaviourSingleton<T> where T : MonoBehaviour
    {
        /// <summary>
        /// Awake is used to set the attached Singleton
        /// object to not be destroyed
        /// </summary>
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }

        protected override void OnDestroy()
        {
        }
    }
}
