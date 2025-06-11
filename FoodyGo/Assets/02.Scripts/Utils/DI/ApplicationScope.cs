using UnityEngine;
using UnityEngine.SceneManagement;

namespace FoodyGo.Utils.DI
{
    public class ApplicationScope : Scope
    {
        protected override void Awake()
        {
            base.Awake();

            SceneManager.sceneLoaded += (scene, mode) =>
            {
                //InjectAll(scene);
            };
        }

        protected void InjectAll(Scene scene)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            foreach (GameObject root in roots)
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
