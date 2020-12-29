﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Pose.Runtime.MonoGameDotNetCore.QuadRendering;
using Pose.Runtime.MonoGameDotNetCore.Skeletons;

namespace Pose.Runtime.MonoGameDotNetCore
{
    public class PoseRuntime
    : IDisposable
    {
        private readonly GpuMeshRenderer _gpuMeshRenderer;
        private readonly List<Skeleton> _skeletons;

        public PoseRuntime(GpuMeshRenderer gpuMeshRenderer)
        {
            _gpuMeshRenderer = gpuMeshRenderer;
            _skeletons = new List<Skeleton>();

            ViewTransform = Matrix.Identity;
        }

        /// <summary>
        /// Adds the skeleton to the runtime, the runtime will dispose the skeleton.
        /// </summary>
        public Skeleton AddSkeleton(SkeletonDefinition skeletonDefinition, Vector2 position, float depth, float angle)
        {
            var skeleton = skeletonDefinition.CreateInstance(position, depth, angle);
            _skeletons.Add(skeleton);
            return skeleton;
        }

        /// <summary>
        /// Draws all skeletons. PoseRuntime uses Y+ = up convention. Position (0,0) with view = Identity is the center of the screen. Z can be used for depth occlusion.
        /// </summary>
        /// <param name="gameTime">Gametime in seconds. Used to update the Pose animations.</param>
        public void Draw(float gameTime)
        {
            UpdateAnimations(gameTime);
            RenderSprites();
        }

        private void UpdateAnimations(float gameTimeSeconds)
        {
            var sw = Stopwatch.StartNew();
            if (UseMultiCore)
            {
                Parallel.ForEach(_skeletons, skeleton => { skeleton.Update(gameTimeSeconds); });
            }
            else
            {
                foreach (var skeleton in _skeletons)
                {
                    skeleton.Update(gameTimeSeconds);
                }
            }

            UpdateTime = sw.Elapsed.TotalMilliseconds;
        }

        private void RenderSprites()
        {
            _gpuMeshRenderer.ProjectionTransform = ProjectionTransform;
            _gpuMeshRenderer.ViewTransform = ViewTransform;

            var sw = Stopwatch.StartNew();
            
            foreach (var skeleton in _skeletons.OrderByDescending(s => s.Depth))
            {
                skeleton.Draw(_gpuMeshRenderer);
            }

            DrawTime = sw.Elapsed.TotalMilliseconds;
        }

        public void Dispose()
        {
            //_quadRenderer?.Dispose();
        }

        /// <summary>
        /// Prefab method to set ViewTransform and Projection to a default 2D orthographic camera at a certain position in the world. You must call this method each draw-frame if you use it.
        /// The camera's view has the same size as the MonoGame viewport (pixels) and has X+ pointing to the right and Y+ pointing up.
        /// Alternatively, if you want more control over ViewTransform and Projection matrices, set them directly using their properties and don't call this method.
        /// </summary>
        /// <param name="zoom">1 means: 1 world unit == 1 screen pixel, 2 means: 1 world unit == 2 pixels, ...</param>
        /// <param name="nearPlane">Z is used for sprite depth order. High Z is drawn behind low Z. Z must be between nearplane and farplane. Using a large plane range causes inaccuracies in Z ordering, so keep it close to what you need.</param>
        /// <param name="farPlane">Z is used for sprite depth order. High Z is drawn behind low Z. Z must be between nearplane and farplane. Using a large plane range causes inaccuracies in Z ordering, so keep it close to what you need.</param>
        public void SetCameraPosition(Vector2 position, float zoom = 1f, float nearPlane = 0f, float farPlane = 100f)
        {
            var viewport = _gpuMeshRenderer.GraphicsDevice.Viewport;
            var halfWidth = viewport.Width * 0.5f / zoom;
            var halfHeight = viewport.Height * 0.5f / zoom;
            ProjectionTransform = Matrix.CreateOrthographicOffCenter(-halfWidth, +halfWidth, -halfHeight, +halfHeight, nearPlane, farPlane);
            ViewTransform = Matrix.CreateTranslation(-position.X, -position.Y, -1f);
        }

        /// <summary>
        /// Set this to the view matrix of your camera, or call SetCameraPosition() instead for simple cases.
        /// </summary>
        public Matrix ViewTransform { get; set; }

        /// <summary>
        /// Set this to the projection matrix of your camera, or call SetCameraPosition() instead for simple cases.
        /// </summary>
        public Matrix ProjectionTransform { get; set; }

        /// <summary>
        /// Distributed animation calculations of all skeletons among all processors. Multi-core processing comes at an overhead cost so only enable it when you have many skeletons. Test the performance difference.
        /// </summary>
        public bool UseMultiCore { get; set; }

        /// <summary>
        /// The time (ms) it took to update all skeleton animations (the previous call to Draw()).
        /// </summary>
        public double UpdateTime { get; private set; }
        /// <summary>
        /// The time (ms) it took to draw all skeletons (the previous call to Draw()).
        /// </summary>
        public double DrawTime { get; private set; }
    }
}
