using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace App.Utility
{
    /// <summary>
    /// Singleton that persists across multiple scenes
    /// </summary>
    public class PersistentSingleton<T> : Singleton<T> where T : Singleton<T>
    {
        protected override void Awake()
        {
            base.Awake();
            DontDestroyOnLoad(gameObject);
        }
    }
}
