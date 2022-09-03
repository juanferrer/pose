﻿using System;
using System.IO;
using System.Reflection;
using System.Windows;
using Microsoft.Win32;
using Pose.Domain.Documents.Messages;
using Pose.Domain.Editor;
using Pose.Domain.Editor.Messages;
using Pose.Framework.Messaging;
using Pose.Framework.ViewModels;
using Pose.Panels.Animations;
using Pose.Panels.Assets;
using Pose.Panels.Dopesheet;
using Pose.Panels.DrawOrder;
using Pose.Panels.Hierarchy;
using Pose.Panels.History;
using Pose.Panels.ModeSwitching;
using Pose.Panels.Properties;
using Pose.Persistence.Editor;
using Pose.Popups.ExportSpritesheets;
using Pose.Popups.ExportAnimations;
using Pose.SceneEditor;
namespace Pose.Shell
{
    public class ShellViewModel
    : ViewModel
    {
        private readonly Func<ExportSpritesheetViewModel> _exportSpritesheetViewModelFactory;
        private readonly Func<ExportAnimationsViewModel> _exportAnimationsViewModelFactory;

        private readonly Editor _editor;
        private string _title;
        private Visibility _autoKeyVisibility;
        private double _dopesheetMaxHeight;
        private bool _dopesheetIsVisible;
        private bool _isAssetPanelVisible;
        private bool _isAnimationsPanelVisible;

        public ShellViewModel(
            Func<AssetPanelViewModel> assetPanelViewModelFactory,
            Func<HierarchyPanelViewModel> hierarchyPanelViewModelFactory,
            Func<DrawOrderPanelViewModel> drawOrderPanelViewModelFactory,
            Func<PropertiesPanelViewModel> propertyPanelViewModelFactory,
            Func<DopesheetPanelViewModel> dopesheetPanelViewModelFactory,
            Func<HistoryPanelViewModel> historyPanelViewModelFactory,
            Func<ModeSwitchPanelViewModel> modeSwitchPanelViewModelFactory,
            Func<AnimationsPanelViewModel> animationsPanelViewModelFactory,
            Func<ExportSpritesheetViewModel> exportSpritesheetViewModelFactory,
            Func<ExportAnimationsViewModel> exportAnimationsViewModelFactory,
            SceneEditorViewModel sceneEditorViewModel,
            Editor editor)
        {
            _exportSpritesheetViewModelFactory = exportSpritesheetViewModelFactory;
            _exportAnimationsViewModelFactory = exportAnimationsViewModelFactory;
            _editor = editor;

            Title = $"Pose {Assembly.GetEntryAssembly().GetName().Version}";
            Width = 1600;
            Height = 900;

            AssetPanel = assetPanelViewModelFactory.Invoke();
            HierarchyPanel = hierarchyPanelViewModelFactory.Invoke();
            DrawOrderPanel = drawOrderPanelViewModelFactory.Invoke();
            PropertyPanel = propertyPanelViewModelFactory.Invoke();
            DopesheetPanel = dopesheetPanelViewModelFactory.Invoke();
            HistoryPanel = historyPanelViewModelFactory.Invoke();
            ModeSwitchPanel = modeSwitchPanelViewModelFactory.Invoke();
            AnimationsPanel = animationsPanelViewModelFactory.Invoke();
            SceneEditor = sceneEditorViewModel;

            AutoKeyVisibility = Visibility.Hidden;

            OnEditorModeChanged(new EditorModeChanged(_editor.Mode));
            ConfigureMessageHandlers();
        }

        private void ConfigureMessageHandlers()
        {
            MessageBus.Default.Subscribe<UserInterfaceReady>(OnUserInterfaceReady);
            MessageBus.Default.Subscribe<AutoKeyToggled>(e =>
            {
                AutoKeyVisibility = e.IsAutoKeying ? Visibility.Visible : Visibility.Collapsed;
            });
            MessageBus.Default.Subscribe<DocumentLoaded>(msg => UpdateWindowTitle());
            MessageBus.Default.Subscribe<CurrentDocumentFilenameChanged>(msg => UpdateWindowTitle());
            MessageBus.Default.Subscribe<CurrentDocumentModified>(msg => UpdateWindowTitle());
            MessageBus.Default.Subscribe<CurrentDocumentSaved>(msg => UpdateWindowTitle());
            MessageBus.Default.Subscribe<EditorModeChanged>(OnEditorModeChanged);
        }

        private void OnEditorModeChanged(EditorModeChanged msg)
        {
            DopesheetMaxHeight = msg.Mode == EditorMode.Animate ? double.MaxValue : 0d;
            DopesheetIsVisible = msg.Mode == EditorMode.Animate;
            IsAssetPanelVisible = msg.Mode == EditorMode.Design;
            IsAnimationsPanelVisible = msg.Mode == EditorMode.Animate;
        }

        public void OnUserInterfaceReady(UserInterfaceReady msg)
        {
            _editor.CreateNewDocument();
        }

        public bool DoSaveWorkflow()
        {
            if (!_editor.CurrentDocument.HasFilename)
            {
                return DoSaveAsWorkflow();
            }
            Save();
            return true;
        }

        private void Save()
        {
            _editor.SaveDocument();
        }

        public bool DoSaveAsWorkflow()
        {
            var dialog = new SaveFileDialog
            {
                DefaultExt = ".pose",
                Filter = "Pose Documents (*.pose)|*.pose|All files (*.*)|*.*",
                FilterIndex = 0
            };
            if (!_editor.CurrentDocument.HasFilename)
            {
                dialog.InitialDirectory = Path.GetDirectoryName(_editor.CurrentDocument.Filename) ?? Environment.CurrentDirectory;
                dialog.FileName = Path.GetFileName(_editor.CurrentDocument.Filename);
            }

            if (dialog.ShowDialog(System.Windows.Application.Current.MainWindow) == true)
            {
                _editor.SetDocumentFilename(dialog.FileName);
                Save();
                return true;
            }

            return false; // user canceled
        }

        /// <summary>
        /// Checks if the current document can be closed. Asks for save if modified. Returns false if the user chooses to cancel the close.
        /// </summary>
        private bool DoCloseDocumentWorkflow()
        {
            if (_editor.CurrentDocument.IsModified)
            {
                var result = MessageBox.Show("Save your changes before closing?", "Close application",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                if (result == MessageBoxResult.Cancel)
                    return false;
                if (result == MessageBoxResult.Yes)
                {
                    if (!DoSaveWorkflow())
                        return false;
                }
            }

            return true;
        }

        public bool DoNewDocumentWorkflow()
        {
            if (!DoCloseDocumentWorkflow())
                return false;

            _editor.CreateNewDocument();
            return true;
        }

        public bool DoOpenDocumentWorkflow()
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".pose",
                Filter = "Pose Document (*.pose)|*.pose|All files (*.*)|*.*",
                FilterIndex = 0
            };

            if (dialog.ShowDialog(System.Windows.Application.Current.MainWindow) != true)
                return false;

            if (!DoCloseDocumentWorkflow())
                return false;

            var document = DocumentLoader.LoadFromFile(MessageBus.Default, dialog.FileName);
            _editor.LoadDocument(document);
            ValidateAssetFolder();
            return true;
        }

        private void ValidateAssetFolder()
        {
            if (_editor.CurrentDocument.PreviousSaveFilename != _editor.CurrentDocument.Filename)
            {
                // file was moved since last session.

                var previousAssetFolder = Path.GetFullPath(_editor.CurrentDocument.RelativeAssetFolder, Path.GetDirectoryName(_editor.CurrentDocument.PreviousSaveFilename));
                if (!Directory.Exists(_editor.CurrentDocument.AbsoluteAssetFolder))
                {
                    if (Directory.Exists(previousAssetFolder))
                    {
                        if (MessageBox.Show($"This file was moved but the used assets were not. I found them in the original location, and will change your assetfolder to point there:\n\n{previousAssetFolder}\n\nOk?",
                                "Asset folder link severed", MessageBoxButton.OKCancel, MessageBoxImage.Warning) == MessageBoxResult.OK)
                        {
                            _editor.SetDocumentAssetFolder(previousAssetFolder);
                        }
                    }
                    else
                    {
                        MessageBox.Show($"This file was moved and I can't find the asset folder. You will have to set the correct asset folder in the Assets panel.",
                            "Asset folder link severed", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    }
                }
            }
        }

        private void UpdateWindowTitle()
        {
            var version = Assembly.GetEntryAssembly().GetName().Version;
            var appName = $"Pose v{version.Major}.{version.Minor}";
            var documentName = _editor.CurrentDocument.HasFilename
                ? Path.GetFileName(_editor.CurrentDocument.Filename)
                : "<new document>";

            Title = _editor.CurrentDocument.IsModified
                ? $"*{documentName} - {appName}"
                : $"{documentName} - {appName}";
        }

        public void ExportSpritesheet()
        {
            var viewModel = _exportSpritesheetViewModelFactory.Invoke();
            viewModel.ShowModal();
        }

        public void ExportAnimations()
        {
            var viewModel = _exportAnimationsViewModelFactory.Invoke();
            viewModel.ShowModal();
        }

        public void ExitApplication()
        {
            if (!DoCloseDocumentWorkflow())
                return;

            IsExiting = true;
            System.Windows.Application.Current.Shutdown(0); // healthy shutdown
        }

        public bool CanUndo()
        {
            return _editor.CanUndo();
        }

        public void Undo()
        {
            _editor.Undo();
        }

        public bool CanRedo()
        {
            return _editor.CanRedo();
        }

        public void Redo()
        {
            _editor.Redo();
        }

        public HierarchyPanelViewModel HierarchyPanel { get; }

        public AssetPanelViewModel AssetPanel { get; }

        public DrawOrderPanelViewModel DrawOrderPanel { get; }

        public PropertiesPanelViewModel PropertyPanel { get; }

        public DopesheetPanelViewModel DopesheetPanel { get; }

        public AnimationsPanelViewModel AnimationsPanel { get; }

        public HistoryPanelViewModel HistoryPanel { get; }

        public ModeSwitchPanelViewModel ModeSwitchPanel { get; }

        public SceneEditorViewModel SceneEditor { get; set; }

        public bool IsAssetPanelVisible
        {
            get => _isAssetPanelVisible;
            set
            {
                if (_isAssetPanelVisible == value)
                    return;

                _isAssetPanelVisible = value;
                OnPropertyChanged();
            }
        }

        public bool IsAnimationsPanelVisible
        {
            get => _isAnimationsPanelVisible;
            set
            {
                if (_isAnimationsPanelVisible == value)
                    return;

                _isAnimationsPanelVisible = value;
                OnPropertyChanged();
            }
        }

        public double DopesheetMaxHeight
        {
            get => _dopesheetMaxHeight;
            set
            {
                if (_dopesheetMaxHeight == value)
                    return;

                _dopesheetMaxHeight = value;
                OnPropertyChanged();
            }
        }

        public bool DopesheetIsVisible
        {
            get => _dopesheetIsVisible;
            set
            {
                if (_dopesheetIsVisible == value)
                    return;

                _dopesheetIsVisible = value;
                OnPropertyChanged();
            }
        }

        public Visibility AutoKeyVisibility
        {
            get => _autoKeyVisibility;
            set
            {
                if (value == _autoKeyVisibility) return;
                _autoKeyVisibility = value;
                OnPropertyChanged();
            }
        }

        public string Title
        {
            get => _title;
            set
            {
                if (value == _title) return;
                _title = value;
                OnPropertyChanged();
            }
        }

        public double Width { get; set; }

        public double Height { get; set; }

        public bool IsExiting { get; set; }
    }
}
