using FoodyGo.Singletons;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace FoodyGo.UI
{
    public class UIManager : Singleton<UIManager>
    {
        Dictionary<Type, UI_Base> _uis = new Dictionary<Type, UI_Base>();

        public void Register(UI_Base ui)
        {
            if(_uis.ContainsKey(ui.GetType()))
            {
                throw new Exception();
            }

            _uis.Add(ui.GetType(), ui);
        }

        public void Unregister(UI_Base ui)
        {
            _uis.Remove(ui.GetType());
        }

        public T Resolve<T>()
            where T : UI_Base
        {
            return (T)_uis[typeof(T)];
        }
    }
}

