using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsModel
{
    public class Camera
    {
        public Cords Location { get; set; }
        public ViewAngle ViewAngle { get; set; }

        public Camera() {
            Location = new Cords();
            ViewAngle = new ViewAngle();
        }
        public Camera(Cords location, ViewAngle viewAngle) {
            this.Location = location;
            this.ViewAngle = viewAngle;
        }
    }
}
