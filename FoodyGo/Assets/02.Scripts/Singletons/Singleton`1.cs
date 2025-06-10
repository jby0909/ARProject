using UnityEngine;
using System.Collections.Generic;

namespace FoodyGo.Singletons
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    var type = typeof(T);
                    T component = GameObject.FindAnyObjectByType<T>();

                    if(component == null)
                    {
                        GameObject empty = new GameObject(typeof(T).Name);
                        component = empty.AddComponent<T>();
                    }
                    
                    _instance = component;
                    DontDestroyOnLoad(_instance);
                }
                return _instance;
            }
        }

        private static T _instance;
    }
}