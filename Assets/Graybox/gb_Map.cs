using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Graybox
{
    public class gb_Map : MonoBehaviour
    {
        public static gb_Map ActiveMap;

        static JsonSerializerSettings _serializerSettings
            = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                Formatting = Formatting.Indented
            };

        public gb_MapInfo MapInfo { get; private set; }
        public string FilePath { get; private set; }

        private void OnEnable()
        {
            if (ActiveMap)
            {
                ActiveMap.gameObject.SetActive(false);
            }
            ActiveMap = this;
        }

        private void OnDisable()
        {
            if (ActiveMap == this)
            {
                ActiveMap = null;
            }
        }

        public T Create<T>(GameObject obj = null)
            where T : gb_Object, new()
        {
            var result = new T();
            if (!obj)
            {
                obj = CreateObject();
            }
            result.Integrate(obj);
            AddObject(result);
            return result;
        }

        public GameObject CreateObject()
        {
            var obj = new GameObject();
            obj.transform.SetParent(transform, true);
            return obj;
        }

        public void AddObject(gb_Object obj)
        {
            if (MapInfo.Objects.Contains(obj))
            {
                throw new System.Exception("Adding object that is already added");
            }
            obj.GameObject.transform.SetParent(transform, true);
            MapInfo.Objects.Add(obj);
            obj.Map = this;
        }

        public void Save(string filePath)
        {
            FilePath = filePath;

            foreach (var obj in GetComponentsInChildren<gb_ObjectComponent>(true))
            {
                obj.Object.Save();
            }

            var mapJson = JsonConvert.SerializeObject(MapInfo, _serializerSettings);
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            File.WriteAllText(filePath, mapJson);
        }

        private void _Load(string filePath)
        {
            foreach (Transform tr in transform)
            {
                GameObject.Destroy(tr.gameObject);
            }

            var mapJson = File.ReadAllText(filePath);
            MapInfo = JsonConvert.DeserializeObject<gb_MapInfo>(mapJson, _serializerSettings);
            var objList = new List<gb_Object>(MapInfo.Objects);
            MapInfo.Objects.Clear();

            foreach (var gbObject in objList)
            {
                var gameObject = new GameObject($"gb_Object #{gbObject.ObjectId}");
                gbObject.Integrate(gameObject);
                gbObject.Load();
                AddObject(gbObject);
            }

            FilePath = filePath;
        }

        public static gb_Map Load(string filePath)
        {
            foreach(var map in FindObjectsOfType<gb_Map>(true))
            {
                if(map.FilePath == filePath)
                {
                    map._Load(filePath);
                    map.gameObject.SetActive(true);
                    return map;
                }
            }
            var fileName = Path.GetFileNameWithoutExtension(filePath);
            var obj = new GameObject("[gb_Map: " + fileName + "]");
            var result = obj.AddComponent<gb_Map>();
            result._Load(filePath);
            return result;
        }

        public static gb_Map Create(string mapName)
        {
            var obj = new GameObject("[gb_Map: " + mapName + "]");
            var result = obj.AddComponent<gb_Map>();
            result.MapInfo = new gb_MapInfo();
            result.MapInfo.Name = mapName;
            return result;
        }

    }
}
