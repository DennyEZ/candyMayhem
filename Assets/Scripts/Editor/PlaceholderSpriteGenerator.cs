using UnityEngine;
using UnityEditor;
using Match3.Data;

namespace Match3.Editor
{
    /// <summary>
    /// Creates placeholder gem sprites for quick prototyping.
    /// </summary>
    public static class PlaceholderSpriteGenerator
    {
#if UNITY_EDITOR
        [MenuItem("Match3/Generate Placeholder Sprites")]
        public static void GenerateSprites()
        {
            // Create a simple colored circle texture
            int size = 128;
            var colors = new (string name, Color color)[]
            {
                ("Red", new Color(0.9f, 0.2f, 0.2f)),
                ("Blue", new Color(0.2f, 0.4f, 0.9f)),
                ("Green", new Color(0.2f, 0.8f, 0.3f)),
                ("Yellow", new Color(0.95f, 0.85f, 0.2f)),
                ("Purple", new Color(0.7f, 0.2f, 0.8f)),
                ("Orange", new Color(0.95f, 0.5f, 0.1f)),
            };
            
            // Ensure directory exists
            if (!AssetDatabase.IsValidFolder("Assets/Sprites/Gems"))
            {
                if (!AssetDatabase.IsValidFolder("Assets/Sprites"))
                {
                    AssetDatabase.CreateFolder("Assets", "Sprites");
                }
                AssetDatabase.CreateFolder("Assets/Sprites", "Gems");
            }
            
            foreach (var (name, color) in colors)
            {
                var texture = CreateCircleTexture(size, color);
                var path = $"Assets/Sprites/Gems/{name}.png";
                
                // Save texture as PNG
                var bytes = texture.EncodeToPNG();
                System.IO.File.WriteAllBytes(path, bytes);
                
                Object.DestroyImmediate(texture);
            }
            
            AssetDatabase.Refresh();
            
            // Set import settings
            foreach (var (name, _) in colors)
            {
                var path = $"Assets/Sprites/Gems/{name}.png";
                var importer = AssetImporter.GetAtPath(path) as TextureImporter;
                if (importer != null)
                {
                    importer.textureType = TextureImporterType.Sprite;
                    importer.spritePixelsPerUnit = 128;
                    importer.filterMode = FilterMode.Bilinear;
                    importer.SaveAndReimport();
                }
            }
            
            Debug.Log("✓ Generated 6 placeholder gem sprites in Assets/Sprites/Gems/");
        }
        
        private static Texture2D CreateCircleTexture(int size, Color color)
        {
            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            
            float center = size / 2f;
            float radius = size / 2f - 4;
            
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                    
                    if (distance < radius - 2)
                    {
                        // Inner color with gradient
                        float gradient = 1f - (distance / radius) * 0.3f;
                        texture.SetPixel(x, y, new Color(
                            color.r * gradient,
                            color.g * gradient,
                            color.b * gradient,
                            1f
                        ));
                    }
                    else if (distance < radius)
                    {
                        // Anti-aliased edge
                        float alpha = (radius - distance) / 2f;
                        texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
                    }
                    else
                    {
                        texture.SetPixel(x, y, Color.clear);
                    }
                }
            }
            
            texture.Apply();
            return texture;
        }
#endif
    }
}
