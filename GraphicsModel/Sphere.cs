using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GraphicsModel
{
    public class Sphere
    {
        public Cords CenterLocation { get; set; }
        public float Radius { get; set; }
        public Color Color { get; set; }

        public Sphere() {
            CenterLocation = new Cords();
            Radius = 0;
            Color = Color.Gray;
        }
        public Sphere(Cords centerLocation, float radius, Color color) {
            this.CenterLocation = centerLocation;
            this.Radius = radius;
            this.Color = color;
        }
    }
}
