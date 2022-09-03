using System.Collections.Generic;

using Pose.Domain.Animations;

namespace Pose.Popups.ExportAnimations
{
    public interface IAnimationProducer
    {
        void PrepareDocument();
        void ProduceAnimations(IEnumerable<AnimationItem> animationsToExport);
    }
}