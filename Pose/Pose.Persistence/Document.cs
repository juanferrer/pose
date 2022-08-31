using System.Collections.Generic;

namespace Pose.Persistence
{
    public class Document
    {
        public string LastFilename { get; set; }
        public ulong IdSequence { get; set; }
        public string AssetFolder { get; set; }
        public DrawOrder DrawOrder { get; set; }
        public List<Node> Nodes { get; set; }
        public List<Animation> Animations { get; set; }
    }

    public class DrawOrder
    {
        public List<ulong> NodeIds { get; set; }
    }

    public class Node
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public ulong ParentId { get; set; }
        public Point Position { get; set; }
        public float Angle { get; set; }
        public NodeType Type { get; set; }
        public string SpriteKey { get; set; }
        public float BoneLength { get; set; }
        public bool IsVisible { get; set; }
    }

    public class Point
    {
        public float X { get; set; }
        public float Y { get; set; }
    }

    public class Animation
    {
        public ulong Id { get; set; }
        public string Name { get; set; }
        public int BeginFrame { get; set; }
        public int EndFrame { get; set; }
        public uint FramesPerSecond { get; set; }
        public bool IsLoop { get; set; }
        public List<NodeAnimation> NodeAnimations { get; set; }
    }

    public class NodeAnimation
    {
        public ulong NodeId { get; set; }
        public List<PropertyAnimation> PropertyAnimations { get; set; }
    }

    public class PropertyAnimation
    {
        public ulong Id { get; set; }
        public uint Property { get; set; }
        public uint Vertex { get; set; }
        public List<Key> Keys { get; set; }
    }

    public class Key
    {
        public ulong Id { get; set; }
        public int Frame { get; set; }
        public float Value { get; set; }
        public InterpolationType InterpolationType { get; set; }
        public BezierCurve Curve { get; set; }
    }

    public class BezierCurve
    {
        public Point P0 { get; set; }
        public Point P1 { get; set; }
        public Point P2 { get; set; }
        public Point P3 { get; set; }
    }

    #region Enums

    public enum NodeType
    {
        SpriteNode,
        BoneNode
    };


    public enum InterpolationType
    {
        Linear,
        Hold,
        Bezier
    }

    #endregion
}
