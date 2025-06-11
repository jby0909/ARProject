using System;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;

namespace FoodyGo.Utils.DI
{
    public abstract class Scope : MonoBehaviour
    {
        protected Container container;

        [SerializeField] List<MonoBehaviour> _monobehaviours;

        protected virtual void Awake()
        {
            container = new Container();
            Register();
            InjectAll();
        }

        public virtual void Register()
        {
            foreach(var monoBehaviour in _monobehaviours)
            {
                container.RegisterMonobehaviour(monoBehaviour);
            }
        }

        protected virtual void InjectAll()
        {
            MonoBehaviour[] monobehaviours = GameObject.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None);

            foreach(var monoBehaviour in monobehaviours)
            {
                Inject(monoBehaviour);
            }
        }

        /// <summary>
        /// 의존성을 주입함
        /// </summary>
        /// <param name="target"> 주입할 대상 </param>
        protected virtual void Inject(object target)
        {
            Type type = target.GetType();
            FieldInfo[] fieldInofs =
            type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);

            foreach(FieldInfo fieldInfo in fieldInofs)
            {
                if(fieldInfo.GetCustomAttribute<InjectAttribute>() != null)
                {
                    object value = container.Resolve(fieldInfo.FieldType);

                    if(value != null)
                        fieldInfo.SetValue(target, value);
                }
            }
        }
    }
}

