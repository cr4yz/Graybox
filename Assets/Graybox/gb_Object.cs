using UnityEngine;
using Newtonsoft.Json;

namespace Graybox 
{
    public class gb_Object
    {
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scale { get; set; }
        public bool Enabled { get; set; }
        public int ObjectId { get; set; }
        [JsonIgnore]
        public GameObject GameObject { get; private set; }

        public void IntegrateAndAdd(GameObject gameObject, gb_Map map)
        {
            Integrate(gameObject);
            map.AddObject(this);
            OnAdded(map);
        }

        public void Integrate(GameObject gameObject)
        {
            GameObject = gameObject;
            if(!GameObject.TryGetComponent(out gb_ObjectComponent gbComponent))
            {
                gbComponent = GameObject.AddComponent<gb_ObjectComponent>();
            }
            gbComponent.Object = this;
            Save();
            OnIntegrated();
        }

        public void Load()
        {
            GameObject.SetActive(Enabled);
            GameObject.transform.position = Position;
            GameObject.transform.eulerAngles = Rotation;
            GameObject.transform.localScale = Scale;
            OnLoad();
            Save();
        }

        public void Save() 
        {
            Position = GameObject.transform.position;
            Rotation = GameObject.transform.eulerAngles;
            Scale = GameObject.transform.localScale;
            Enabled = GameObject.activeSelf;
            ObjectId = GameObject.GetInstanceID();
            OnSave();
        }

        protected virtual void OnLoad() { }
        protected virtual void OnSave() { }
        protected virtual void OnIntegrated() { }
        protected virtual void OnAdded(gb_Map map) { }
    }
}


