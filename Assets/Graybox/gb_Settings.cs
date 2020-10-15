
using UnityEngine;

namespace Graybox
{
    public class gb_Settings : gb_Singleton<gb_Settings>
    {
        public float UnitScale => .0254f;
        public float GridSize { get; set; } = 32;
        public float SnapSize => UnitScale * GridSize;
        public float RotationSnapSize => 5f;
        public float ScaleSnapSize => .1f;
        public float ExtrusionSize => 1f;
        public bool SnapToGrid => true;
        public Color ElementColor => Color.red;
        public Color ElementHoverColor => Color.yellow;
        public Color ElementSelectedColor => Color.green;

        public float ConvertTo(float unityUnits)
        {
            return unityUnits / UnitScale;
        }

        public float ConvertFrom(float scaledUnits)
        {
            return scaledUnits * UnitScale;
        }

        private void Update()
        {
            if (gb_Binds.JustDown(gb_Bind.IncreaseGrid))
            {
                GridSize /= 2f;
                GridSize = Mathf.Max(GridSize, 1);
            }

            if (gb_Binds.JustDown(gb_Bind.ReduceGrid))
            {
                GridSize *= 2f;
                GridSize = Mathf.Min(GridSize, 512);
            }
        }

    }
}

