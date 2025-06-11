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
        /// �����ڰ� �ִ� �Ϲ� C# Ŭ���� ���(�����ؼ� �߰���)
        /// </summary>
        public void Register<T>()
            where T : class, new() //Ŭ���� �̸鼭 �����ڸ� ������ �ִ� T
        {
            T obj = new T();
            _registration[typeof(T)] = obj;
        }

        /// <summary>
        /// Monobehaviour ��ü�� �����ؼ� �߰�
        /// </summary>
        public void RegisterMonobehaviour<T>()
            where T : MonoBehaviour
        {
            T obj = new GameObject(typeof(T).Name).AddComponent<T>();
            _registration[typeof(T)] = obj;
        }

        /// <summary>
        /// Hierarchy�� �����ϴ� ��ü�� �߰�
        /// </summary>
        /// <param name="monobehaviour"></param>
        public void RegisterMonobehaviour(MonoBehaviour monobehaviour)
        {
            _registration[monobehaviour.GetType()] = monobehaviour;
        }

        /// <summary>
        /// ��ϵ� �� ������
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

