using UnityEngine;

namespace Graybox
{
    public class gb_Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        private static bool m_ShuttingDown = false;
        private static object m_Lock = new object();
        private static T m_Instance;

        public static T Instance
        {
            get
            {
                if (m_ShuttingDown)
                {
                    Debug.LogWarning("[Singleton] Instance '" + typeof(T) +
                        "' already destroyed. Returning null.");
                    return null;
                }

                lock (m_Lock)
                {
                    if (m_Instance == null)
                    {
                        m_Instance = (T)FindObjectOfType(typeof(T), true);

                        if (m_Instance == null)
                        {
                            var singletonObject = new GameObject("Singleton: " + typeof(T).Name);
                            m_Instance = singletonObject.AddComponent<T>();
                            DontDestroyOnLoad(singletonObject);
                        }
                    }

                    return m_Instance;
                }
            }
        }

        protected virtual void OnApplicationQuit()
        {
            m_ShuttingDown = true;
        }

        protected virtual void OnDestroy()
        {
            m_ShuttingDown = true;
        }
    }
}
