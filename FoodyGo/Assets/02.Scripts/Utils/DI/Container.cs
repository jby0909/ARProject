using System;
using System.Collections.Generic;
using UnityEngine;

namespace FoodyGo.Utils.DI
{
    public class Container
    {
        public Container()
        {
            _registration = new Dictionary<Type, object>();
        }

        Dictionary<Type, object> _registration;

        /// <summary>
        /// 생성자가 있는 일반 C# 클래스 등록(생성해서 추가함)
        /// </summary>
        public void Register<T>()
            where T : class, new() //클래스 이면서 생성자를 가지고 있는 T
        {
            T obj = new T();
            _registration[typeof(T)] = obj;
        }

        /// <summary>
        /// Monobehaviour 객체를 생성해서 추가
        /// </summary>
        public void RegisterMonobehaviour<T>()
            where T : MonoBehaviour
        {
            T obj = new GameObject(typeof(T).Name).AddComponent<T>();
            _registration[typeof(T)] = obj;
        }

        /// <summary>
        /// Hierarchy에 존재하는 객체를 추가
        /// </summary>
        /// <param name="monobehaviour"></param>
        public void RegisterMonobehaviour(MonoBehaviour monobehaviour)
        {
            _registration[monobehaviour.GetType()] = monobehaviour;
        }

        /// <summary>
        /// 등록된 것 가져옴
        /// </summary>
        public T Resolve<T>()
        {
            return (T)_registration[typeof(T)];
        }

        public object Resolve(Type type)
        {
            if (_registration.TryGetValue(type, out object obj))
                return obj;
            else
                return null;
        }
    }
}

