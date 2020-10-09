using Graybox.In;
using UnityEngine;

namespace Graybox
{
	public class gb_SceneCamera : MonoBehaviour
	{
		public bool lockRotation;
		public bool lockWasd;
		public bool enableDrag = true;
		public bool enableZoom = true;
		public float mainSpeed = 10f;
		public float shiftAdd = 20f;
		public float maxShift = 1000.0f;
		public float camSens = 0.15f;
		public bool rotateOnlyIfMousedown = true;

		private Vector3 _worldDrag;
		private Vector3 _lastDragPosition;
		private Vector3 _mouseDownPosition;
		private float _totalRun;
		private Camera _camera;

        private void Start()
        {
			_camera = GetComponent<Camera>();
		}

        private void Update()
		{
			if (enableDrag)
			{
                if (gb_Binds.JustDown(gb_Bind.PanCamera))
                {
					_lastDragPosition = Input.mousePosition;
					_worldDrag = _camera.ScreenToWorldPoint(Input.mousePosition);
				}
				else if (gb_Binds.IsDown(gb_Bind.PanCamera))
				{
					var worldDrag = _camera.ScreenToWorldPoint(Input.mousePosition);
					var worldMagnitude = (worldDrag - _worldDrag).magnitude;
					_worldDrag = worldDrag;

					var pos = Input.mousePosition;
					var delta = _lastDragPosition - pos;
					transform.position += transform.TransformDirection(delta).normalized * worldMagnitude;
					_lastDragPosition = pos;
				}
			}

			if (enableZoom)
			{
				if (Input.mouseScrollDelta.y != 0)
				{
					if (_camera.orthographic)
					{
						_camera.orthographicSize += -Input.mouseScrollDelta.y;
					}
					else
					{
						transform.position += -transform.forward * -Input.mouseScrollDelta.y * .5f;
					}
				}
			}

			if (gb_Binds.JustDown(gb_Bind.FreeLook))
			{
				_mouseDownPosition = Input.mousePosition;
			}

            if (!lockRotation && gb_Binds.IsDown(gb_Bind.FreeLook))
            {
				_mouseDownPosition = Input.mousePosition - _mouseDownPosition;
				_mouseDownPosition = new Vector3(-_mouseDownPosition.y * camSens, _mouseDownPosition.x * camSens, 0);
				_mouseDownPosition = new Vector3(transform.eulerAngles.x + _mouseDownPosition.x, transform.eulerAngles.y + _mouseDownPosition.y, 0);
				transform.eulerAngles = _mouseDownPosition;
				_mouseDownPosition = Input.mousePosition;
			}

            if (!lockWasd)
            {
				var velocity = GetInputDirection();
				if (gb_Binds.IsDown(gb_Bind.FreeLookAccelerate))
				{
					_totalRun = Mathf.Clamp(_totalRun + Time.deltaTime, 0, 6.5f);
					velocity *= _totalRun * shiftAdd;
					velocity = Vector3.ClampMagnitude(velocity, maxShift);
				}
				else
				{
					_totalRun = 0f;
					velocity *= mainSpeed;
				}

				transform.Translate(velocity * Time.deltaTime);
			}
		}

		private Vector3 GetInputDirection()
		{
			var result = new Vector3();
			if (gb_Binds.IsDown(gb_Bind.MoveForward))
			{
				result += new Vector3(0, 0, 1);
			}
			if (gb_Binds.IsDown(gb_Bind.MoveBack))
			{
				result += new Vector3(0, 0, -1);
			}
			if (gb_Binds.IsDown(gb_Bind.MoveLeft))
			{
				result += new Vector3(-1, 0, 0);
			}
			if (gb_Binds.IsDown(gb_Bind.MoveRight))
			{
				result += new Vector3(1, 0, 0);
			}
			return result;
		}
	}
}
