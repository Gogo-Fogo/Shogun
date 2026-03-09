#if UNITY_EDITOR
using UnityEditor;

namespace Shogun.Features.Characters
{
    internal sealed class CharacterSpriteImportPostprocessor : AssetPostprocessor
    {
        private void OnPreprocessTexture()
        {
            if (!CharacterSpriteImportPolicy.ShouldEnforce(assetPath))
                return;

            if (assetImporter is TextureImporter textureImporter)
            {
                CharacterSpriteImportPolicy.Apply(textureImporter);
            }
        }
    }
}
#endif
