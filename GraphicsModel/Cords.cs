using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsModel
{
    public class Cords
    {
        public float X { get; set; }
        public float Y { get; set; }
        public float Z { get; set; }

        public Cords() {
            X = 0;
            Y = 0;
            Z = 0;
        }

        public Cords(float x, float y, float z) {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }
    }
}
