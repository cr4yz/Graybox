using UnityEngine;

namespace Graybox
{
    public class gb_Material
    {

        public string ShaderName;
        public string TextureName;
        public string[] ShaderKeywords;

        public static implicit operator Material(gb_Material gbMat)
        {
            var mat = new Material(Shader.Find(gbMat.ShaderName));
            mat.mainTexture = !string.IsNullOrEmpty(gbMat.TextureName) ? Resources.Load<Texture>(gbMat.TextureName) : null;
            mat.shaderKeywords = gbMat.ShaderKeywords;
            return mat;
        }

        public static implicit operator gb_Material(Material mat)
        {
            return new gb_Material()
            {
                ShaderName = mat.shader.name,
                TextureName = mat.mainTexture ? mat.mainTexture.name : string.Empty,
                ShaderKeywords = mat.shaderKeywords
            };
        }

    }
}

