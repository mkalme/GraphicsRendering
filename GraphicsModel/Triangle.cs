
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

namespace GraphicsModel
{
    public class Triangle
    {
        public Cords Point1 { get; set; }
        public Cords Point2 { get; set; }
        public Cords Point3 { get; set; }
        public Color Color { get; set; }

        public Triangle() {
            Point1 = new Cords();
            Point2 = new Cords();
            Point3 = new Cords();
            Color = Color.LightGray;
        }
        public Triangle(Cords point1, Cords point2, Cords point3, Color color)
        {
            this.Point1 = point1;
            this.Point2 = point2;
            this.Point3 = point3;
            this.Color = color;
        }
    }
}
