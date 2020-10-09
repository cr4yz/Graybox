using Graybox.Gui;
using Graybox.In;
using UnityEngine;

namespace Graybox.Tools
{
    public class gb_BlockTool : gb_Tool
    {
        public override string ToolName => "Block Tool";

        [SerializeField]
        private Material _defaultMaterial;

        private Rect _blockRect;
        private bool _creatingBlock;

        protected override void OnUpdate()
        {
            if (_creatingBlock)
            {
                gb_InputManager.ActiveSceneView.Draw.Draw2dRect(_blockRect, 5f, 0, false, Color.white);

                if (gb_Binds.JustDown(gb_Bind.Cancel))
                {
                    _creatingBlock = false;
                }
            }
        }

        protected override void OnDrag(Rect dragRect)
        {
            _blockRect = dragRect;
            _creatingBlock = true;
        }

        protected override void OnDragEnd(Rect dragRect)
        {
            if (_creatingBlock)
            {
                CreateBlock(dragRect);
                _creatingBlock = false;
            }
        }

        private void CreateBlock(Rect rect)
        {
            var min = gb_InputManager.Instance.SceneToWorld(rect.min);
            var max = gb_InputManager.Instance.SceneToWorld(rect.max);
            var position = (min + max) / 2;

            for (int i = 0; i < 3; i++)
            {
                if(max[i] == min[i])
                {
                    max[i] += 2f;
                }
            }

            gb_InputManager.ActiveSceneView.Draw.CreateLabel("Test", gb_InputManager.ActiveSceneView.SceneToWorld(rect.min));

            var obj = new gb_MeshShapeCube()
            {
                Min = min,
                Max = max
            }.Generate();

            obj.transform.position = position;
            obj.GetComponent<Renderer>().material = _defaultMaterial;
            obj.AddComponent<MeshCollider>();
            new gb_Mesh().IntegrateAndAdd(obj, gb_Map.ActiveMap);
        }

    }
}

