using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsModel
{
    public class ViewAngle
    {
        public float HorizontalDegrees { get; set; } // 0 Degrees = +X, 180 Degrees = -X, 90 Degrees = +Z, 270 Degrees = -Z 
        public float VerticalDegrees { get; set; }   // 0 Degrees = +Y, 90  Degrees = -Y

        public ViewAngle() {
            HorizontalDegrees = 0;
            VerticalDegrees = 90;
        }
        public ViewAngle(float horizontalDegrees, float verticalDegrees) {
            this.HorizontalDegrees = horizontalDegrees;
            this.VerticalDegrees = verticalDegrees;
        }
    }
}
