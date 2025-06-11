using UnityEngine;

namespace FoodyGo.Utils.DI
{
    public class SceneScope : Scope
    {
        protected override void InjectAll()
        {
            GameObject[] roots = gameObject.scene.GetRootGameObjects();

            foreach(GameObject root in roots)
            {
                MonoBehaviour[] monobehaviours = root.GetComponentsInChildren<MonoBehaviour>();

                foreach (var monoBehaviour in monobehaviours)
                {
                    Inject(monoBehaviour);
                }
            }
        }
    }
}
