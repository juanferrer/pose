using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;

using Extensions.JSON;

using Pose.Domain.Animations;
using Pose.Domain.Editor;
using Pose.Framework.Messaging;

namespace Pose.Popups.ExportAnimations
{
    public class AnimationItem
    {
        public bool IsMarkedForExport { get; set; }
        public string AnimationName { get; set; }
        public ulong AnimationId { get; set; }
    }

    public class ExportAnimationsViewModel
        : ViewModel
    {
        private readonly IAnimationProducer _animationProducer;
        private readonly Editor _editor;
        private IEnumerable<Animation> _animations;
        private ObservableCollection<AnimationItem> _animationItems;
        private ExportAnimationsWindow _window;

        public ExportAnimationsViewModel(IAnimationProducer animationProducer, Editor editor)
        { 
            _animationProducer = animationProducer;
            _editor = editor;
        }

        public void Initialize()
        {
            var messageBus = new MessageBus();
            var document = _editor.CloneDocument(messageBus);
            _animations = document.GetAnimations();
            AnimationItems = new ObservableCollection<AnimationItem>(_animations.Select(a => new AnimationItem { AnimationName = a.Name, AnimationId = a.Id }));
        }

        public void ShowModal()
        {
            _window = new ExportAnimationsWindow
            {
                Owner = System.Windows.Application.Current.MainWindow,
                DataContext = this,
                WindowStartupLocation = WindowStartupLocation.CenterOwner,
                ResizeMode = ResizeMode.NoResize
            };
            var result = _window.ShowDialog();
            if (result == true)
            {
                ExportAnimations();
            }
        }

        private void ExportAnimations()
        {
            var animationsToExport = _animationItems.Where(a => a.IsMarkedForExport);
            _animationProducer.PrepareDocument();
            _animationProducer.ProduceAnimations(animationsToExport);
        }

        public ObservableCollection<AnimationItem> AnimationItems
        {
            get => _animationItems;
            set
            {
                if (value == _animationItems) return;
                _animationItems = value;
                OnPropertyChanged();
            }
        }
    }
}
