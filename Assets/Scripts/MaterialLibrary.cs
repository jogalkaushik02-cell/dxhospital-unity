using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Caches runtime materials so we don't allocate one per primitive.
/// </summary>
public static class MaterialLibrary
{
    static readonly Dictionary<Color, Material> Cache = new Dictionary<Color, Material>();

    public static Material Solid(Color color)
    {
        if (Cache.TryGetValue(color, out var m) && m != null) return m;
        m = new Material(Shader.Find("Standard"));
        m.color = color;
        m.SetFloat("_Glossiness", 0.15f);
        Cache[color] = m;
        return m;
    }
}
