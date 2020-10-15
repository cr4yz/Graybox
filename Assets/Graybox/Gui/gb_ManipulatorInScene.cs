using Graybox.Tools;
using UnityEngine;

namespace Graybox.Gui
{
    public class gb_ManipulatorInScene : MonoBehaviour
    {

        public void SetModeVertices()
        {
            GameObject.FindObjectOfType<gb_Manipulator>(true).Mode = gb_ManipulatorMode.Vertices;
        }

        public void SetModeEdges()
        {
            GameObject.FindObjectOfType<gb_Manipulator>(true).Mode = gb_ManipulatorMode.Edges;
        }

        public void SetModeFaces()
        {
            GameObject.FindObjectOfType<gb_Manipulator>(true).Mode = gb_ManipulatorMode.Faces;
        }

        public void SetHandleMove()
        {
            GameObject.FindObjectOfType<gb_Manipulator>(true).Handle = gb_ManipulatorHandle.Move;
        }

        public void SetHandleRotate()
        {
            GameObject.FindObjectOfType<gb_Manipulator>(true).Handle = gb_ManipulatorHandle.Rotate;
        }

        public void SetHandleScale()
        {
            GameObject.FindObjectOfType<gb_Manipulator>(true).Handle = gb_ManipulatorHandle.Scale;
        }

    }
}

