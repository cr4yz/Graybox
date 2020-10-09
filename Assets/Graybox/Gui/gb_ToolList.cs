using Graybox.Tools;
using System.Collections.Generic;
using UnityEngine;

namespace Graybox.Gui
{
    public class gb_ToolList : MonoBehaviour
    {

        [SerializeField]
        private gb_ToolListButton _buttonTemplate;

        private List<GameObject> _clones = new List<GameObject>();

        private void Start()
        {
            _buttonTemplate.gameObject.SetActive(false);
            SpawnButtons();
        }

        private void SpawnButtons()
        {
            foreach(var clone in _clones)
            {
                GameObject.Destroy(clone);
            }
            _clones.Clear();

            foreach (var tool in gb_ToolManager.Instance.Tools)
            {
                var clone = GameObject.Instantiate(_buttonTemplate, _buttonTemplate.transform.parent);
                clone.gameObject.SetActive(true);
                clone.Initialize(tool);
                _clones.Add(clone.gameObject);
            }
        }

    }
}

