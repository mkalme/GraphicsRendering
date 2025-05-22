using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphicsModel
{
    public class LightSource
    {
        public Cords Location { get; set; }
        public float Lumens { get; set; }

        public LightSource() {
            Location = new Cords();
            Lumens = 0;
        }
        public LightSource(Cords location, float lumens) {
            this.Location = location;
            this.Lumens = lumens;
        }
    }
}
