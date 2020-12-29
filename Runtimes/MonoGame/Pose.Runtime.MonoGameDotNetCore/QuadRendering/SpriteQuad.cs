﻿using Microsoft.Xna.Framework;

namespace Pose.Runtime.MonoGameDotNetCore.QuadRendering
{
    /// <summary>
    /// Represents 4 corner vertices and UV coords of a quad with a width, height symmetrically around the 0,0 point in the center.
    /// </summary>
    public class SpriteQuad
    {
        public string Key { get; }
        internal readonly Vector2[] Vertices, TextureCoords;

        public SpriteQuad(string key, uint width, uint height, Vector2 t0, Vector2 t1)
        {
            Key = key;
            Vertices = PrepareVertices(width, height);
            TextureCoords = PrepareTextureCoords(t0, t1);
        }

        private static Vector2[] PrepareTextureCoords(Vector2 t0, Vector2 t1)
        {
            var aT = t0;
            var bT = new Vector2(t1.X, t0.Y);
            var cT = t1;
            var dT = new Vector2(t0.X, t1.Y);

            return new[] {aT, bT, cT, dT};
        }

        private static Vector2[] PrepareVertices(uint width, uint height)
        {
            var halfWidth = width * 0.5f;
            var halfHeight = height * 0.5f;

            var a = new Vector2(-halfWidth, halfHeight);
            var b = new Vector2(halfWidth, halfHeight);
            var c = new Vector2(halfWidth, -halfHeight);
            var d = new Vector2(-halfWidth, -halfHeight);

            return new[]
            {
                a, b, c, d
            };
        }
    }
}
