using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphicsModel;

namespace RayTracing
{
    class Intersection
    {
        public Sphere Sphere { get; set; }
        public Triangle Triangle { get; set; }
        public Cords IntersectionPoint { get; set; }
        public float Distance { get; set; }
        public bool Intersect { get; set; }
        public ObjectType ObjectType {get;set;}

        public Intersection() {
            Sphere = new Sphere();
            Triangle = new Triangle();
            IntersectionPoint = new Cords();
            Distance = 0;
            Intersect = false;
            ObjectType = ObjectType.Triangle;
        }
        public Intersection(object object1, Cords intersectionPoint, float distance, bool intersect) {
            if (object1.GetType().Equals(typeof(Sphere))) {
                this.Sphere = (Sphere)object1;
                this.ObjectType = ObjectType.Sphere;
            } else if (object1.GetType().Equals(typeof(Triangle))) {
                this.Triangle = (Triangle)object1;
                this.ObjectType = ObjectType.Triangle;
            }

            this.IntersectionPoint = intersectionPoint;
            this.Distance = distance;
            this.Intersect = intersect;
        }
    }
}
