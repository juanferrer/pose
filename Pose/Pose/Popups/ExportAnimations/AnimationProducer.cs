using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Extensions.JSON;
using Pose.Domain.Animations;
using Pose.Domain.Documents;
using Pose.Domain.Editor;
using Pose.Domain.Nodes;
using Pose.Domain.Nodes.Properties;
using Pose.Framework.Messaging;
using Pose.SceneEditor;
using Pose.SceneEditor.Viewport;
using Exception = System.Exception;

namespace Pose.Popups.ExportAnimations
{
    public class AnimationProducer : IAnimationProducer
    {
        private readonly Editor _editor;
        private Document _document;

        public AnimationProducer(Editor editor)
        {
            _editor = editor;
        }

        public void PrepareDocument()
        {
            var messageBus = new MessageBus();
            _document = CloneDocument(messageBus);
        }

        public void ProduceAnimations(IEnumerable<AnimationItem> animationsToExport)
        {
            if (_document == null)
                throw new Exception("Call PrepareDocument() first");
            var animations = animationsToExport.Select(a => _document.GetAnimation(a.AnimationId)).ToList();
            var directory = Directory.GetParent(_document.Filename);
            foreach (var animation in animations)
            {
                File.WriteAllText($"{directory}/{animation.Name}.anim", animations.Serialize());
            }
        }

        private Document CloneDocument(IMessageBus messageBus)
        {
            return _editor.CloneDocument(messageBus);
        }
    }
}
