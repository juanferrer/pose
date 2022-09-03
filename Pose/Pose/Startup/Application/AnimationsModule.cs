using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pose.Domain.Editor;
using Pose.Framework.IoC;
using Pose.Popups.ExportAnimations;
using Pose.Popups.ExportSpritesheets;

namespace Pose.Startup.Application
{
    public class AnimationModule
    : IModule
    {
        public void Register(ServiceCollection serviceCollection, IConfigurationRoot configuration)
        {
            // for exporting animations to spritesheet.
            serviceCollection.AddWithFactory<ExportAnimationsViewModel>();
            serviceCollection.AddTransient<IAnimationProducer, AnimationProducer>();
        }
    }
}
