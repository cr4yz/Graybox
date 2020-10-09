using Graybox.Utility;
using UnityEngine;
using UnityEngine.ProBuilder;

namespace Graybox.Tools 
{
    public class gb_MeshShapeCube : gb_MeshShape
    {

        public Vector3 Min;
        public Vector3 Max;

		public override GameObject Generate()
        {
			return ShapeGenerator.GenerateCube(PivotLocation.Center, (Max - Min).Absolute()).gameObject;
		}

    }
}


