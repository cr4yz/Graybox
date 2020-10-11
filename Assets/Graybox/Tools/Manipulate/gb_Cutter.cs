using Graybox.In;
using System.Collections.Generic;
using UnityEngine;
using EzySlice;

namespace Graybox.Tools
{
    public class gb_Cutter : gb_Tool
    {
        public override string ToolName => "Cutter";

        protected override void OnDrag(Rect dragRect)
        {
            gb_InputManager.ActiveSceneView.Draw.Draw2dLine(dragRect.min, dragRect.max, 2f, Color.red);
        }

        protected override void OnDragEnd(Rect dragRect)
        {
            if (!Target)
            {
                return;
            }

            var startRay = gb_InputManager.ActiveSceneView.SceneToRay(dragRect.min);
            var endRay = gb_InputManager.ActiveSceneView.SceneToRay(dragRect.max);

            var start = startRay.GetPoint(gb_InputManager.ActiveSceneView.Camera.nearClipPlane);
            var end = endRay.GetPoint(gb_InputManager.ActiveSceneView.Camera.nearClipPlane);
            var depth = gb_InputManager.ActiveSceneView.Camera.transform.forward;

            Debug.DrawLine(startRay.origin, endRay.origin, Color.red, 10f);
            Debug.Log(startRay.origin + ":" + endRay.origin);

            var planeTangent = (end - start).normalized;
            if (planeTangent == Vector3.zero)
                planeTangent = Vector3.right;
            var normalVec = Vector3.Cross(depth, planeTangent);
            var transformedNormal = ((Vector3)(Target.localToWorldMatrix.transpose * normalVec)).normalized;

            var p = new EzySlice.Plane(Target.InverseTransformPoint(start), transformedNormal);
            var results = Target.gameObject.SliceInstantiate(p);
            if(results != null)
            {
                GameObject.Destroy(Target.gameObject);
                foreach (var r in results)
                {
                    gb_Map.ActiveMap.Create<gb_Mesh>(r).Save();
                }
            }
        }

    }
}

