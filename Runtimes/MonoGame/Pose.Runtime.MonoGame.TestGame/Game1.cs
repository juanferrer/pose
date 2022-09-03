using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Pose.Runtime.MonoGameDotNetCore;
using Pose.Runtime.MonoGameDotNetCore.Rendering;
using Pose.Runtime.MonoGameDotNetCore.Skeletons;
using SharpDX.Mathematics.Interop;

namespace Pose.Runtime.MonoGame.TestGame
{
    public class Game1 : Game
    {
        private PoseRuntime _poseRuntime;
        private readonly GraphicsDeviceManager _graphicsDeviceManager;
        private float _cameraZoom;
        private Texture2D _squareTexture;
        private SpriteBatch _spriteBatch;

        // performance tracking
        private int frameCount = 0;
        private float _averageUpdate = 0;
        private float _averageDraw = 0;

        public Game1()
        {
            _graphicsDeviceManager = new GraphicsDeviceManager(this)
            {
                PreferredBackBufferWidth = 1920,
                PreferredBackBufferHeight = 1080
            };
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            IsFixedTimeStep = true;
        }

        protected override void LoadContent()
        {
            // The PoseRuntime is a light manager class for high performance 2D rendering. It supports rendering Pose animations, but also your custom (not-made-with-Pose) entities your game needs to render.
            // The runtime can therefore serve as the only renderer for your 2D game.

            // Note: the PoseRuntime is optional: you can also just load Pose skeletons and draw them through your own rendering code.
            //       When calling Skeleton.Draw() you need to supply an ICpuMeshRenderer which will receive a single Mesh from the Skeleton
            //       containing an array of vertices, indices and a texture, so all you need to do is send it to the gpu.

            // To use PoseRuntime:

            // 1. new PoseRuntime(GraphicsDeviceManager)
            // 2. load a Pose SkeletonDefinition from file or MonoGame's content pipeline
            // 3. create a Skeleton instance of the SkeletonDefinition.
            // 4. PoseRuntime.Add(mySkeleton)
            // 5. each frame: PoseRuntime.Draw().

            _poseRuntime = new PoseRuntime(_graphicsDeviceManager)
            {
                UseMultiCore = true
            };
            
            var skeletonDefinition = SkeletonDefinition.LoadFromFiles(GraphicsDevice, "../../../../../../pose/pose/assets/poser/poser"); // this points to the original 'poser' sample files in git so we don't need to copy them over each time it changes.

            // use this following variant to load via MonoGame's content pipeline 
            // note: in the MG pipeline tool: add the .png just like any texture, add the .sheet and .pose files with Build Action 'Copy'
            // var skeletonDefinition = Content.LoadPoseSkeletonDefinition("poser");

            // DEMO 1 -----------
            CreateDemo1(skeletonDefinition);
            // ----

            // DEMO 2 ------------------
            //CreateDemo2(skeletonDefinition); // don't forget setting UseMultiCore = true in the PoseRuntime.
            // ----
            
            StartAnimations("Run"); // the animationname is the one assigned to the animation in Pose Editor
        }

        private void CreateDemo1(SkeletonDefinition skeletonDefinition)
        {
            _cameraZoom = 1f;
            _spriteBatch = new SpriteBatch(GraphicsDevice);
            //_squareTexture = new Texture2D(GraphicsDevice, 500, 500);
            _squareTexture = Content.Load<Texture2D>("poser");

            // shows four running guys with different depths to show layering of sprites.
            // the second is most in front, 1 and 3 are behind that, and 4th is furthest
            // TopLeft
            _poseRuntime.Add(skeletonDefinition.CreateInstance(new Vector2(-200, -200), 1, 45));
            // Top Right
            _poseRuntime.Add(skeletonDefinition.CreateInstance(new Vector2(200, -200), 1, 0));
            // Centre
            _poseRuntime.Add(skeletonDefinition.CreateInstance(new Vector2(0, 0), 0, 0));
            // BottomLeft
            _poseRuntime.Add(skeletonDefinition.CreateInstance(new Vector2(-200, 200), 2, 0));
            // BottomRight
            _poseRuntime.Add(skeletonDefinition.CreateInstance(new Vector2(200, 200), 1, 0));
        }

        private void CreateDemo2(SkeletonDefinition skeletonDefinition)
        {
            // this shows a lot of running guys in a grid with random rotations. It's to test performance and try out potential improvements of the runtime logic.
            // for detailed performance measuring, I suggest using JetBrains' profiler in Line-by-Line mode.

            _cameraZoom = 0.3f;
            var r = new Random();
            const int count = 5000;
            var horizCount = (int) (MathF.Sqrt(count) * 1.3f);
            var vertCount = count / horizCount + 1;
            const int distance = 100;
            for (var i = 0; i < count; i++)
            {
                var position = new Vector2(i % horizCount - horizCount / 2, i / horizCount - vertCount / 2) * distance;
                _poseRuntime.Add(skeletonDefinition.CreateInstance(position, 0, (float) r.NextDouble() * 6.283f));
            }
        }

        private void StartAnimations(string animationName)
        {
            var t = 0;
            var offset = 0f;
            foreach (var skeleton in _poseRuntime.Entities.OfType<Skeleton>())
            {
                skeleton.StartAnimation(animationName, t - offset);
                offset += 0.097f; // add diversity in the animation's starttime
            }
        }

        protected override void UnloadContent()
        {
            _poseRuntime.Dispose();
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin();
            //_spriteBatch.Draw(_squareTexture, Vector2.Zero, Color.White);
            _spriteBatch.End();


            //_poseRuntime.SetCameraPosition(Vector2.Zero, _cameraZoom);
            var viewport = _graphicsDeviceManager.GraphicsDevice.Viewport;
            var halfWidth = viewport.Width * 0.5f / _cameraZoom;
            var halfHeight = viewport.Height * 0.5f / _cameraZoom;
            var position = new Vector2(-halfWidth, -halfHeight);
            /**
            var left = -halfWidth;
            var right = +halfWidth;
            var bottom = -halfHeight;
            var top = halfHeight;
            /**/

            /**/
            var left = 0;
            var right = viewport.Width;
            var bottom = viewport.Height;
            var top = 0;
            /**/

            //_poseRuntime.ProjectionTransform = Matrix.CreateOrthographicOffCenter(left, right, bottom, top, 0, 100);
            _poseRuntime.ProjectionTransform = Matrix.CreateOrthographic(viewport.Width, viewport.Height, 0, 100);
            _poseRuntime.ViewTransform = Matrix.CreateTranslation(position.X, -position.Y, -1f);

            _poseRuntime.Draw((float)gameTime.TotalGameTime.TotalSeconds);

            MeasurePerformance();
        }

        private void MeasurePerformance()
        {
            _averageUpdate += ((float) _poseRuntime.UpdateTime - _averageUpdate) * 0.1f;
            _averageDraw += ((float) _poseRuntime.DrawTime - _averageDraw) * 0.1f;
            if (frameCount++ % 60 == 0)
            {
                Debug.WriteLine($"U = {_averageUpdate:0.0}ms    D = {_averageDraw:0.0}ms");
            }
        }
    }
}
