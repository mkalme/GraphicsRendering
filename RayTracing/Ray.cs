using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphicsModel;

namespace RayTracing
{
    public class Ray
    {
        public Cords Origin { get; set; }
        public Cords Point2 { get; set; }
        public Cords DistanceVector { get; set; }

        public Ray()
        {
            Origin = new Cords();
            Point2 = new Cords();
            DistanceVector = new Cords();
        }
        public Ray(Cords origin, Cords point2, Cords distanceVector)
        {
            this.Origin = origin;
            this.Point2 = point2;
            this.DistanceVector = distanceVector;
        }
    }
}