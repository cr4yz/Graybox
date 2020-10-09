
namespace Graybox
{
    public class gb_Settings : gb_Singleton<gb_Settings>
    {
        public float UnitScale => .0254f;

        public float ConvertTo(float unityUnits)
        {
            return unityUnits / UnitScale;
        }

        public float ConvertFrom(float scaledUnits)
        {
            return scaledUnits * UnitScale;
        }

    }
}

