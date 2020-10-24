using Graybox.Gui;
using Graybox.In;
using Graybox.Utility;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Graybox.Tools
{
    public class gb_BlockTool : gb_Tool
    {
        public override string ToolName => "Block Tool";
        public override bool DontDrawDragRect => true;

        [SerializeField]
        private Material _defaultMaterial;

        private Rect _blockRect;
        private bool _creatingBlock;
        private bool _3d;
        private Plane _blockPlane;
        private Vector3 _worldStart;
        private Vector3 _worldCurrent;
        private gb_Mesh _template;

        protected override void OnDisabled()
        {
            if (_creatingBlock)
            {
                _creatingBlock = false;
                if(_template != null)
                {
                    GameObject.Destroy(_template.GameObject);
                    _template = null;
                }
            }
        }

        protected override void OnUpdate()
        {
            //var sceneScreenPos = gb_InputManager.ActiveSceneView.ScreenToScene(Input.mousePosition);
            //GetWorldHitPointAndNormal(sceneScreenPos, out Vector3 gridCenter, out Vector3 gridNormal);

            if (_creatingBlock)
            {
                gb_InputManager.ActiveSceneView.Draw.Add(new gb_DrawMeshOutline(gb_InputManager.ActiveSceneView, _template.GameObject.GetComponent<ProBuilderMesh>(), false));

                gb_InputManager.ActiveSceneView.Draw.Add(new gb_Draw3dGrid()
                {
                    Center = _worldStart + _blockPlane.normal * .01f,
                    Normal = _blockPlane.normal
                });

                if (gb_Binds.JustDown(gb_Bind.Cancel))
                {
                    _creatingBlock = false;
                    if (_template != null)
                    {
                        GameObject.Destroy(_template.GameObject);
                        _template = null;
                    }
                }
            }
            else
            {
                //gb_InputManager.ActiveSceneView.Draw.Add(new gb_Draw3dGrid()
                //{
                //    Color = Color.red,
                //    Center = gridCenter,
                //    Normal = gridNormal
                //});
            }
        }

        protected override void OnDragStart(Rect dragRect)
        {
            _creatingBlock = true;
            _3d = gb_InputManager.ActiveSceneView.SceneAngle == gb_SceneViewAngle.ThreeDimensional;
            GetWorldHitPointAndNormal(dragRect.min, out _worldStart, out Vector3 hitNormal);
            _blockPlane = new Plane(hitNormal, _worldStart);
            _worldStart = _blockPlane.ClosestPointOnPlane(_worldStart.Snap(gb_Settings.Instance.SnapSize));
        }

        protected override void OnDrag(Rect dragRect)
        {
            _blockRect = dragRect;
            var ray = gb_InputManager.ActiveSceneView.ScreenToRay(Input.mousePosition);
            if (_blockPlane.Raycast(ray, out float enter))
            {
                _worldCurrent = ray.GetPoint(enter);
            }
            var snapAxis = new Vector3(_blockPlane.normal.x == 0 ? 1 : 0, _blockPlane.normal.y == 0 ? 1 : 0, _blockPlane.normal.z == 0 ? 1 : 0);
            _worldCurrent = _worldCurrent.Snap(snapAxis, gb_Settings.Instance.SnapSize);
            _worldCurrent += _blockPlane.normal * gb_Settings.Instance.SnapSize;

            if (_template != null)
            {
                GameObject.Destroy(_template.GameObject);
                _template = null;
            }

            if (_3d)
            {
                _template = CreateBlock(_worldStart, _worldCurrent);
            }
            else
            {
                _template = CreateBlock(gb_InputManager.ActiveSceneView.SceneToWorld(dragRect.min), gb_InputManager.ActiveSceneView.SceneToWorld(dragRect.max));
            }
            
            _template.GameObject.GetComponent<Collider>().enabled = false;
        }

        protected override void OnDragEnd(Rect dragRect)
        {
            if (_creatingBlock)
            {
                _creatingBlock = false;
                _template.GameObject.GetComponent<Collider>().enabled = true;
                _template = null;
            }
        }

        private void GetWorldHitPointAndNormal(Vector2 sceneScreenPos, out Vector3 point, out Vector3 normal)
        {
            var ray = gb_InputManager.ActiveSceneView.SceneToRay(sceneScreenPos);
            if(Physics.Raycast(ray, out RaycastHit hit))
            {
                point = hit.point;
                normal = hit.normal;
            }
            else
            {
                point = ray.origin + ray.direction * 15f;
                normal = Vector3.up;
            }
        }

        private gb_Mesh CreateBlock(Vector3 min, Vector3 max)
        {
            for (int i = 0; i < 3; i++)
            {
                if (Mathf.Approximately(max[i], min[i]))
                {
                    max[i] += gb_Settings.Instance.SnapSize;
                }
            }

            var position = (min + max) / 2;
            var obj = new gb_MeshShapeCube()
            {
                Min = min,
                Max = max
            }.Generate();

            obj.transform.position = position;
            obj.GetComponent<Renderer>().material = _defaultMaterial;
            obj.AddComponent<MeshCollider>();

            var result = gb_Map.ActiveMap.Create<gb_Mesh>(obj);
            result.Save();
            return result;
        }

    }
}

