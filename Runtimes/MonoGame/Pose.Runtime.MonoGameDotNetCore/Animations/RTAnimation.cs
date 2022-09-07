namespace Pose.Runtime.MonoGameDotNetCore.Animations
{
    public class RTAnimation
    {
        public string Name { get; }
        public bool IsLoop { get; }
        public float Duration { get; } // length of animation in sec
        private float? _startGameTime; // animation was started on this gametime.
        private readonly RTPropertyAnimation[] _propertyAnimations;

        /// <param name="duration">Duration of animation in seconds</param>
        /// <param name="isLoop">Loop or stop at end of animation</param>
        internal RTAnimation(string name, float duration, bool isLoop, RTPropertyAnimation[] propertyAnimations)
        {
            Name = name;
            Duration = duration;
            IsLoop = isLoop;
            _propertyAnimations = propertyAnimations;
        }

        /// <summary>
        /// (Re)starts the animation at frame 0.
        /// </summary>
        /// <param name="gameTime">The current absolute time in seconds.</param>
        public void Start(float gameTime)
        {
            _startGameTime = gameTime;
            for (var i = 0; i < _propertyAnimations.Length; i++)
            {
                _propertyAnimations[i].Reset();
            }
        }

        /// <summary>
        /// Updates all animated properties for the given time. Optimized for forward playing, not random jumping through the animation.
        /// </summary>
        internal void PlayForwardTo(in float gameTime, RTNode[] nodes)
        {
            if (!_startGameTime.HasValue)
                return;

            var t = gameTime - _startGameTime.Value;
            if (IsLoop)
                t %= Duration;

            for (var i = 0; i < _propertyAnimations.Length; i++)
            {
                var propertyAnimation = _propertyAnimations[i];
                var newValue = propertyAnimation.PlayForwardTo(t);
                ref var node = ref nodes[propertyAnimation.NodeIdx];

                switch (propertyAnimation.NodeProperty)
                {
                    case NodeProperty.TranslationX:
                        node.AnimateTransformation.X = newValue;
                        break;
                    case NodeProperty.TranslationY:
                        node.AnimateTransformation.Y = newValue;
                        break;
                    case NodeProperty.RotationAngle:
                        node.AnimateTransformation.Angle = newValue;
                        break;
                    case NodeProperty.Visibility:
                        node.IsVisible = newValue != 0f;
                        break;
                    default:
                        throw new PoseNotSupportedException($"Animating of PropertyType \"{propertyAnimation.NodeProperty}\" is currently not supported.");
                }
            }
        }
    }
}
