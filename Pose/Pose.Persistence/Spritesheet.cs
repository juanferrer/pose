using System.Collections.Generic;

namespace Pose.Persistence
{
    public class Spritesheet
    {
        public uint Width { get; set; }
        public uint Height { get; set; }
        public List<Sprite> Sprites { get; set; }
    }

    public class Sprite
    {
        public string Key { get; set; }
        public uint X { get; set; }
        public uint Y { get; set; }
        public uint Width { get; set; }
        public uint Height { get; set; }
    }
}