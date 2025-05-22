using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Drawing;

namespace GraphicsModel
{
    public class World
    {
        public List<Sphere> Spheres { get; set; }
        public List<Triangle> Triangles { get; set; }
        public List<LightSource> LightSources { get; set; }
        public Camera Camera { get; set; }

        public World() {
            Spheres = new List<Sphere>();
            Triangles = new List<Triangle>();
            LightSources = new List<LightSource>();
            Camera = new Camera();
        }
        public World(List<Sphere> spheres, List<Triangle> triangles, List<LightSource> lightSources, Camera camera) {
            this.Spheres = spheres;
            this.Triangles = triangles;
            this.LightSources = lightSources;
            this.Camera = camera;
        }

        public static World FromFile(string path) {
            World world = new World();

            XmlDocument document = new XmlDocument();
            document.LoadXml(File.ReadAllText(path));

            //Get triangles
            XmlNodeList allTriangleNodes = document.SelectNodes("/world/triangle");
            for (int i = 0; i < allTriangleNodes.Count; i++) {
                Cords point1 = GetNodeCords(allTriangleNodes[i].SelectSingleNode("point1"));
                Cords point2 = GetNodeCords(allTriangleNodes[i].SelectSingleNode("point2"));
                Cords point3 = GetNodeCords(allTriangleNodes[i].SelectSingleNode("point3"));

                Color color = GetNodeColor(allTriangleNodes[i].SelectSingleNode("color"));

                Triangle triangle = new Triangle(point1, point2, point3, color);
                world.Triangles.Add(triangle);
            }

            //Get spheres
            XmlNodeList allSphereNodes = document.SelectNodes("/world/sphere");
            for (int i = 0; i < allSphereNodes.Count; i++){
                Cords centerLocation = GetNodeCords(allSphereNodes[i].SelectSingleNode("center-location"));

                float radius = float.Parse(allSphereNodes[i].SelectSingleNode("radius").Attributes["length"].Value.ToString());

                Color color = GetNodeColor(allSphereNodes[i].SelectSingleNode("color"));

                Sphere sphere = new Sphere(centerLocation, radius, color);
                world.Spheres.Add(sphere);
            }

            //Get lightsources
            XmlNodeList allLightsourceNodes = document.SelectNodes("/world/light-source");
            for (int i = 0; i < allLightsourceNodes.Count; i++) {
                Cords centerLocation = GetNodeCords(allLightsourceNodes[i].SelectSingleNode("location"));
                float lumens = float.Parse(allLightsourceNodes[i].SelectSingleNode("intensity").Attributes["lumens"].Value.ToString());

                LightSource lightSource = new LightSource(centerLocation, lumens);
                world.LightSources.Add(lightSource);
            }

            //Get camera
            Cords location = GetNodeCords(document.SelectSingleNode("/world/camera/location"));
            float horizontal = float.Parse(document.SelectSingleNode("/world/camera/view-angle").Attributes["horizontal"].Value.ToString());
            float vertical = float.Parse(document.SelectSingleNode("/world/camera/view-angle").Attributes["vertical"].Value.ToString());

            Camera camera = new Camera(location, new ViewAngle(horizontal, vertical));
            world.Camera = camera;

            return world;
        }

        private static Cords GetNodeCords(XmlNode node) {
            return new Cords(float.Parse(node.Attributes["x"].Value.ToString()), float.Parse(node.Attributes["y"].Value.ToString()), float.Parse(node.Attributes["z"].Value.ToString()));
        }
        private static Color GetNodeColor(XmlNode node) {
            return ColorTranslator.FromHtml(node.Attributes["hex"].Value.ToString());
        }
    }
}
