using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphicsModel;
using System.Drawing;
using System.Diagnostics;
using System.Windows;
using System.Threading;

namespace RayTracing
{
    public class Render
    {
        public World World { get; set; }
        public Bitmap Image { get; set; }
        public Size FOV { get; set; }

        public int ImageWidth { get; set; }
        public int ImageHeight { get; set; }

        private Graphics graphics { get; set; }

        public Render(World world, Size resolution)
        {
            this.World = world;
            Image = new Bitmap(resolution.Width, resolution.Height);

            int FovWidth = 65;
            FOV = new Size(FovWidth, (int)((resolution.Height / (double)resolution.Width) * FovWidth));

            graphics = Graphics.FromImage(Image);

            ImageWidth = resolution.Width;
            ImageHeight = resolution.Height;
        }

        //Update
        public void Update(){
            graphics.Clear(Color.DimGray);

            ViewAngle[,] viewAngles = GetPerspectiveViewAngles();
            //HitRay(World.Camera.Location, GetRayAngleLine(World.Camera.Location, viewAngles[50, 35], 1));
            HitRay(World.Camera.Location, GetRayAngleLine(World.Camera.Location, viewAngles[53, 35], 1));
            //HitRay(World.Camera.Location, new Ray(World.Camera.Location, new Cords(8, 0, 0), Sub(World.Camera.Location, new Cords(8, 0, 0))));

            //Intersection intersection = new Intersection();

            //intersection.ObjectType = ObjectType.Sphere;
            //intersection.IntersectionPoint = new Cords(8, 0, 0);
            //intersection.Sphere = new Sphere(new Cords(10, 0, 0), 2, Color.DimGray);

            //ReflectionRay(intersection);

            //TraceRays();
        }
        public void UpdateWithThread(){
            graphics.Clear(Color.DimGray);

            TraceRaysThread();
        }

        //Trace Rays
        private void TraceRaysThread() {
            Color color = Color.DimGray;
            ViewAngle[,] allViewAngles = GetPerspectiveViewAngles();

            Color[,] pixelColors = new Color[ImageWidth, ImageHeight];

            int NumberOfThreads = 30;
            Thread[] threads = new Thread[NumberOfThreads + 1];

            for (int i = 0; i < threads.Length; i++) {
                int yStart = ((ImageHeight / threads.Length) * i);
                int yEnd = yStart + (ImageHeight / threads.Length);

                yEnd = yEnd > ImageHeight ? ImageHeight : yEnd;
                if (i == threads.Length - 1) {
                    yEnd = ImageHeight;
                }

                threads[i] = new Thread(() => {
                    for (int y = yStart; y < yEnd; y++) {
                        for (int x = 0; x < ImageWidth; x++) {
                            pixelColors[x, y] = HitRay(World.Camera.Location, GetRayAngleLine(World.Camera.Location, allViewAngles[x, y], 1));
                        }
                    }
                });
                threads[i].Start();
            }

            for (int i = 0; i < threads.Length; i++) {
                threads[i].Join();
            }

            for (int x = 0; x < pixelColors.GetLength(0); x++) {
                for (int y = 0; y < pixelColors.GetLength(1); y++) {
                    if (pixelColors[x, y] != Color.Transparent)
                    {
                        Image.SetPixel(x, y, pixelColors[x, y]);
                    }
                }
            }
        }
        private void TraceRays() {
            Color color;

            ViewAngle[,] allViewAngles = GetPerspectiveViewAngles();

            for (int x = 0; x < ImageWidth; x++){
                for (int y = 0; y < ImageHeight; y++){
                    color = HitRay(World.Camera.Location, GetRayAngleLine(World.Camera.Location, allViewAngles[x, y], 1));

                    if (color != Color.Transparent){
                        Image.SetPixel(x, y, color);
                    }
                }
            }
        }
        private Color HitRay(Cords originCords, Ray ray) {
            Color color = Color.Transparent;

            Intersection intersection = CastRay(ray);

            if (intersection.Intersect) {
                Color tempColor = Color.Transparent;

                if (intersection.ObjectType == ObjectType.Sphere) {
                    tempColor = intersection.Sphere.Color;
                } else if (intersection.ObjectType == ObjectType.Triangle) {
                    tempColor = intersection.Triangle.Color;
                }

                //Light
                float lumens = ShadowRay(intersection);
                float maxToIlluminate = 500;

                if (lumens > maxToIlluminate) {
                    //lumens = maxToIlluminate + (lumens - maxToIlluminate) / 5;
                    lumens = maxToIlluminate + (lumens - maxToIlluminate) / 10;
                    //lumens = maxToIlluminate;
                }

                float lowerBy = 255 - (255 / (maxToIlluminate / lumens));

                tempColor = DarkenColor(tempColor, lowerBy);

                color = tempColor;

                //Reflection
                color = ReflectionRay(intersection);
            }

            return color;
        }

        private Intersection CastRay(Ray ray) {
            List<Intersection> allIntersections = GetAllIntersections(ray, float.MaxValue).OrderBy(o => o.Distance).ToList();

            if (allIntersections.Count > 0){
                return allIntersections[0];
            }else {
                Intersection intersection = new Intersection();
                intersection.Intersect = false;

                return intersection;
            }
        }
        private float ShadowRay(Intersection intersection) {
            float lumens = 100;
            //float lumens = 20;

            List<LightSource> lightSources = CheckLightSources(intersection.IntersectionPoint);

            for (int i = 0; i < lightSources.Count; i++) {
                float distanceFromLightSource = DistanceBetween(intersection.IntersectionPoint, lightSources[i].Location);

                //Intensity
                float currentLumens = lightSources[i].Lumens / (distanceFromLightSource * distanceFromLightSource);

                //Angle
                float angle = 0;

                if (intersection.ObjectType == ObjectType.Sphere)
                {
                    float radius = intersection.Sphere.Radius;
                    float adjacent = distanceFromLightSource;
                    float diagonal = DistanceBetween(intersection.Sphere.CenterLocation, lightSources[i].Location);

                    angle = (float)(Math.Acos(
                        ((radius * radius) + (adjacent * adjacent) - (diagonal * diagonal)) /
                        (2 * radius * adjacent)) * (180 / Math.PI)) - 90;
                }
                else if (intersection.ObjectType == ObjectType.Triangle){
                    angle = RayTriangleIntersectionAngle(
                        intersection.Triangle,
                        new Ray(intersection.IntersectionPoint, lightSources[i].Location, Sub(intersection.IntersectionPoint, lightSources[i].Location))
                    );
                }
                currentLumens *= angle / 90;

                //Add
                lumens += currentLumens;
            }

            return lumens;
        }
        private Color ReflectionRay(Intersection intersection) {
            Color color = Color.Transparent;

            Ray R1 = new Ray(
                World.Camera.Location,
                intersection.IntersectionPoint,
                Sub(World.Camera.Location, intersection.IntersectionPoint)
            );

            Debug.WriteLine(R1.Origin.X + ", " + R1.Origin.Y + ", " + R1.Origin.Z + " < Origin");
            Debug.WriteLine(R1.Point2.X + ", " + R1.Point2.Y + ", " + R1.Point2.Z + " < Point2 A.K.A The intersection point");
            Debug.WriteLine(R1.DistanceVector.X + ", " + R1.DistanceVector.Y + ", " + R1.DistanceVector.Z + " < DistanceVector");

            Debug.WriteLine("================");

            if (intersection.ObjectType == ObjectType.Sphere) {
                Sphere sphere = intersection.Sphere;
                Cords point = intersection.IntersectionPoint;

                //Surface normal
                Cords N = new Cords(
                    (point.X - sphere.CenterLocation.X) / sphere.Radius,
                    (point.Y - sphere.CenterLocation.Y) / sphere.Radius,
                    (point.Z - sphere.CenterLocation.Z) / sphere.Radius
                );

                //Ray direction
                Cords Rr = Sub(R1.DistanceVector, Multiply(N, 2 * Dot(R1.DistanceVector, N)));

                Debug.WriteLine(N.X + ", " + N.Y + ", " + N.Z + " < Normal");

                Debug.WriteLine(Rr.X + ", " + Rr.Y + ", " + Rr.Z + " < Rr");
                Debug.WriteLine(2 * Dot(R1.DistanceVector, N) + " < Dot");
                Debug.WriteLine("=================");

                Ray rayRr = new Ray(intersection.IntersectionPoint, Rr, Sub(intersection.IntersectionPoint, Rr));

                Intersection reflectiveIntersection = CastRay(rayRr);

                if (reflectiveIntersection.Intersect) {
                    if (reflectiveIntersection.ObjectType == ObjectType.Sphere) {
                        color = reflectiveIntersection.Sphere.Color;

                        //Light
                        float lumens = ShadowRay(reflectiveIntersection);
                        float maxToIlluminate = 500;

                        if (lumens > maxToIlluminate)
                        {
                            //lumens = maxToIlluminate + (lumens - maxToIlluminate) / 5;
                            lumens = maxToIlluminate + (lumens - maxToIlluminate) / 10;
                            //lumens = maxToIlluminate;
                        }

                        float lowerBy = 255 - (255 / (maxToIlluminate / lumens));

                        color = DarkenColor(color, lowerBy);
                    } else if (reflectiveIntersection.ObjectType == ObjectType.Triangle) {
                        color = reflectiveIntersection.Triangle.Color;
                    }
                }

                Debug.WriteLine(rayRr.Origin.X + ", " + rayRr.Origin.Y + ", " + rayRr.Origin.Z + " < Origin A.K.A The intersection point");
                Debug.WriteLine(rayRr.Point2.X + ", " + rayRr.Point2.Y + ", " + rayRr.Point2.Z + " < Point2");
                Debug.WriteLine(rayRr.DistanceVector.X + ", " + rayRr.DistanceVector.Y + ", " + rayRr.DistanceVector.Z + " < DistanceVector");
            } else if (intersection.ObjectType == ObjectType.Triangle) {
                Triangle triangle = intersection.Triangle;

                //Surface normal
                Cords vector1 = Sub(triangle.Point2, triangle.Point1);
                Cords vector2 = Sub(triangle.Point3, triangle.Point1);

                Cords N = Cross(vector1, vector2);
            }

            return color;
        }

        //Projection
        private ViewAngle[,] GetPerspectiveViewAngles()
        {
            ViewAngle[,] allViewAngles = new ViewAngle[ImageWidth, ImageHeight];

            //Rectangle
            float[] FovRectangle = GetScreenFovSizeRectangle(FOV);

            //Get degrees
            for (int x = 0; x < allViewAngles.GetLength(0); x++) {
                for (int y = 0; y < allViewAngles.GetLength(1); y++) {
                    allViewAngles[x, y] = GetRayPxAnglePerspective(
                        x,
                        y,
                        World.Camera.ViewAngle,
                        FovRectangle[0],
                        FovRectangle[1]
                    );
                }
            }

            return allViewAngles;
        }
        private ViewAngle[,] GetFishEyeViewAngles() {
            ViewAngle[,] allViewAngles = new ViewAngle[ImageWidth, ImageHeight];

            //Get degrees
            for (int x = 0; x < ImageWidth; x++){
                for (int y = 0; y < ImageHeight; y++){
                    allViewAngles[x, y] = GetRayPxAngleFishEye(x, y, World.Camera.ViewAngle);
                }
            }

            return allViewAngles;
        }

        private ViewAngle GetRayPxAnglePerspective(int x, int y, ViewAngle middleAngle, float rectangleWidth, float rectangleHeight) {
            float px = rectangleWidth / ImageWidth;
            float halfPx = px / 2;

            //Get X
            float xHalfPixel = ImageWidth % 2 == 1 ? 0 : (halfPx);
            float xLength = (x * px) + xHalfPixel - (rectangleWidth / 2);

            //Get Y
            float yHalfPixel = ImageHeight % 2 == 0 ? (halfPx) : 0;
            float yLength = (rectangleHeight / 2) - (y * px) + yHalfPixel;

            //BOTH
            Cords origin = new Cords(0, 0, 0);

            Cords line = RotateVerticalLine(origin, new Cords(1, yLength, xLength), middleAngle.VerticalDegrees - 90);
                  line = RotateHorizontalLine(origin, line, middleAngle.HorizontalDegrees);

            float horizontalLineTilt = GetHorizontalTiltOfLine(origin, line);
            float verticalLineTilt =   GetVerticalTiltOfLine(origin, line);

            return new ViewAngle(horizontalLineTilt, verticalLineTilt);
        }
        private ViewAngle GetRayPxAngleFishEye(int x, int y, ViewAngle middleAngle)
        {
            float oneRayAngle = FOV.Width / (float)Image.Width;

            float horizontalDistanceAngle = (x - (Image.Width / (float)2)) * oneRayAngle;
            float horizontalAngle = middleAngle.HorizontalDegrees + horizontalDistanceAngle + (oneRayAngle / 2);

            float verticalDistanceAngle = (y - (Image.Height / (float)2)) * oneRayAngle;
            float verticalAngle = middleAngle.VerticalDegrees + verticalDistanceAngle + (oneRayAngle / 2);

            return new ViewAngle(ValidateDegrees(horizontalAngle), ValidateDegrees(verticalAngle));
        }

        //Intersections
        private List<Intersection> GetAllIntersections(Ray ray, float t)
        {
            List<Intersection> intersections = new List<Intersection>();

            for (int i = 0; i < World.Spheres.Count; i++){
                Intersection intersection = SphereIntersection(World.Spheres[i], ray, t);

                if (intersection.Intersect){
                    intersections.Add(intersection);
                }
            }

            for (int i = 0; i < World.Triangles.Count; i++){
                Intersection intersection = TriangleIntersection(World.Triangles[i], ray, t);

                if (intersection.Intersect){

                    intersections.Add(intersection);
                }
            }

            return intersections;
        }

        private Intersection SphereIntersection(Sphere sphere, Ray ray, float t) {
            Intersection intersection = new Intersection();

            //http://www.codeproject.com/Articles/19799/Simple-Ray-Tracing-in-C-Part-II-Triangles-Intersec

            float EPSILON = 0.00001f;

            double cx = sphere.CenterLocation.X;
            double cy = sphere.CenterLocation.Y;
            double cz = sphere.CenterLocation.Z;

            double px = ray.Origin.X; //Point
            double py = ray.Origin.Y;
            double pz = ray.Origin.Z;

            double vx = ray.Point2.X - px; //Vector
            double vy = ray.Point2.Y - py;
            double vz = ray.Point2.Z - pz;


            double A = (vx * vx) + (vy * vy) + (vz * vz);
            double B = 2.0 * (px * vx + py * vy + pz * vz - vx * cx - vy * cy - vz * cz);
            double C = (px * px) - 2 * px * cx + (cx * cx) +
                       (py * py) - 2 * py * cy + (cy * cy) +
                       (pz * pz) - 2 * pz * cz + (cz * cz) - (sphere.Radius * sphere.Radius);

            //Discriminant
            double D = (B * B) - 4 * A * C;

            if (D < 0) {//If doesn't intersect
                intersection.Intersect = false;

                return intersection;
            }

            double t1 = (-B - Math.Sqrt(D)) / (2.0 * A);

            if (t1 < -EPSILON || t1 > t){//If behind origin point or ahead of line point2
                intersection.Intersect = false;

                return intersection;
            }

            Cords intersection1 = new Cords((float)(ray.Origin.X * (1 - t1) + t1 * ray.Point2.X),
                                            (float)(ray.Origin.Y * (1 - t1) + t1 * ray.Point2.Y),
                                            (float)(ray.Origin.Z * (1 - t1) + t1 * ray.Point2.Z));

            intersection = new Intersection(
                sphere,
                intersection1,
                DistanceBetween(intersection1, ray.Origin),
                true
            );

            return intersection;
        }
        private Intersection TriangleIntersection(Triangle triangle, Ray ray, float t) {
            Intersection intersection = new Intersection();

            float EPSILON = 0.00001f;
            Cords v0v1 = Sub(triangle.Point2, triangle.Point1);
            Cords v0v2 = Sub(triangle.Point3, triangle.Point1);
            Cords pvec = Cross(ray.DistanceVector, v0v2);

            float det = Dot(v0v1, pvec);

            if (det < EPSILON && det > -EPSILON) {
                intersection.Intersect = false;

                return intersection;
            }

            float invDet = 1.0f / det;

            Cords tvec = Sub(ray.Origin, triangle.Point1);

            float u = Dot(tvec, pvec) * invDet;

            if (u < 0 || u > 1) {
                intersection.Intersect = false;

                return intersection;
            }

            Cords qvec = Cross(tvec, v0v1);
            float v = Dot(ray.DistanceVector, qvec) * invDet;
            if (v < 0 || u + v > 1) {
                intersection.Intersect = false;

                return intersection;
            }

            float t1 = Dot(v0v2, qvec) * invDet;

            if (t1 > EPSILON && t1 <= t){// ray intersection
                Cords intersection1 = Add(ray.Origin, Multiply(ray.DistanceVector, t1));

                intersection = new Intersection(
                    triangle,
                    intersection1,
                    DistanceBetween(intersection1, ray.Origin),
                    true
                );

                return intersection;
            }else{// This means that there is a line intersection but not a ray intersection.
                intersection.Intersect = false;

                return intersection;
            }
        }

        private List<LightSource> CheckLightSources(Cords point) {
            List<LightSource> lightSources = new List<LightSource>();

            for (int i = 0; i < World.LightSources.Count; i++) {
                LightSource lightSource = World.LightSources[i];

                Ray ray = new Ray(point, lightSource.Location, Sub(lightSource.Location, point));
                List<Intersection> intersections = GetAllIntersections(ray, 1);

                if (intersections.Count == 0){
                    lightSources.Add(lightSource);
                }
            }

            return lightSources;
        }

        //Scripts
        private float[] GetScreenFovSizeRectangle(Size fov) {
            float adjacentWidth = 1 / GetTriangleTan(90 - fov.Width / 2.0f) * 2;
            float adjacentHeight = fov.Height / (float)fov.Width * adjacentWidth;

            return new float[] {adjacentWidth, adjacentHeight};
        }

        public Ray GetRayAngleLine(Cords cords, ViewAngle viewAngle, float multiplyer) {
            Ray ray = new Ray();

            float xAngle = GetTriangleOpposite(90 - viewAngle.HorizontalDegrees);
            float yAngle = GetTriangleOpposite(90 - viewAngle.VerticalDegrees);
            float zAngle = GetTriangleOpposite(viewAngle.HorizontalDegrees);

            float verticalOpposite = GetTriangleOpposite(viewAngle.VerticalDegrees);

            xAngle *= verticalOpposite;
            zAngle *= verticalOpposite;


            ray.Origin = cords;
            ray.Point2 = new Cords(cords.X + xAngle * multiplyer, cords.Y + yAngle * multiplyer, cords.Z + zAngle * multiplyer);
            ray.DistanceVector = new Cords(xAngle * multiplyer, yAngle * multiplyer, zAngle * multiplyer);

            return ray;
        }

        private float GetTriangleOpposite(float degreeBH) {
            return (float)Math.Sin(Math.PI * degreeBH / 180.0);
        }
        private float GetTriangleTan(float degreeBH) {
            return (float)Math.Tan(Math.PI * degreeBH / 180.0);
        }

        public float ValidateDegrees(float degrees)
        {
            if (degrees < 0){
                return 360 + degrees % 360;
            }else if (degrees >= 360){
                return degrees % 360;
            }

            return degrees;
        }
        private static Color DarkenColor(Color color, float amount){
            float multiplyBy = (255 - amount) / 255;

            float red = color.R;
            float green = color.G;
            float blue = color.B;

            red *= multiplyBy;
            green *= multiplyBy;
            blue *= multiplyBy;

            red = red < 0 ? 0 : red;
            green = green < 0 ? 0 : green;
            blue = blue < 0 ? 0 : blue;

            red = red > 255 ? 255 : red;
            green = green > 255 ? 255 : green;
            blue = blue > 255 ? 255 : blue;

            return Color.FromArgb(color.A, (int)red, (int)green, (int)blue);
        }

        private float RayTriangleIntersectionAngle(Triangle triangle, Ray ray) {
            float angle = 90;

            ////Plane equation
            Cords vector1 = Sub(triangle.Point3, triangle.Point1);
            Cords vector2 = Sub(triangle.Point2, triangle.Point1);

            Cords cross_product = Cross(vector1, vector2);

            float a = cross_product.X;
            float b = cross_product.Y;
            float c = cross_product.Z;
            float d = (a * triangle.Point1.X - b * triangle.Point1.Y - c * triangle.Point1.Z);

            //Angle
            double numerator = Math.Abs((float)(a * ray.DistanceVector.X + b * ray.DistanceVector.Y + c * ray.DistanceVector.Z));
            double denominator = Math.Sqrt((float)(a * a + b * b + c * c)) * Math.Sqrt((float)(ray.DistanceVector.X * ray.DistanceVector.X + ray.DistanceVector.Y * ray.DistanceVector.Y + ray.DistanceVector.Z * ray.DistanceVector.Z));

            angle = (float)(Math.Asin(numerator / denominator) * (180 / Math.PI));

            return angle;
        }

        //Projection
        private Cords RotateHorizontalLine(Cords origin, Cords point2, float horizontalAngle){
            //Get angles
            float firstAngle = (float)(Math.Atan((point2.X - origin.X) / (point2.Z - origin.Z)) * (180 / Math.PI));
            float secondAngle = horizontalAngle - firstAngle;

            float radius = (float)Math.Sqrt(((point2.X - origin.X) * (point2.X - origin.X)) + ((point2.Z - origin.Z) * (point2.Z - origin.Z)));

            float zPercentage = 0;
            float xPercentage = 0;

            if (point2.Z < origin.Z){//If left side
                secondAngle *= -1;

                zPercentage = -(float)Math.Cos(Math.PI * (secondAngle) / 180.0) * radius;
                xPercentage = -(float)Math.Cos(Math.PI * (90 - secondAngle) / 180.0) * radius;
            }
            else{//If right side
                zPercentage = (float)Math.Cos(Math.PI * (secondAngle) / 180.0) * radius;
                xPercentage = -(float)Math.Cos(Math.PI * (90 - secondAngle) / 180.0) * radius;
            }

            return new Cords(origin.X + xPercentage, point2.Y, origin.Z + zPercentage);
        }
        private Cords RotateVerticalLine(Cords origin, Cords point2, float verticalAngle){
            //Get angles
            float firstAngle = (float)(Math.Atan((point2.Y - origin.Y) / (point2.X - origin.X)) * (180 / Math.PI));
            float secondAngle = verticalAngle - firstAngle;

            float radius = (float)Math.Sqrt(((point2.X - origin.X) * (point2.X - origin.X)) + ((point2.Y - origin.Y) * (point2.Y - origin.Y)));

            float xPercentage = (float)Math.Cos(Math.PI * (secondAngle) / 180.0) * radius;
            float yPercentage = -(float)Math.Cos(Math.PI * (90 - secondAngle) / 180.0) * radius;

            return new Cords(origin.X + xPercentage, origin.Y + yPercentage, point2.Z);
        }

        private float GetHorizontalTiltOfLine(Cords origin, Cords point2) {
            float horizontalDistance = (float)Math.Sqrt(
                ((origin.X - point2.X) * (origin.X - point2.X)) + ((origin.Z - point2.Z) * (origin.Z - point2.Z))
            );

            if (point2.Z < origin.Z) {//If in left side
                float thirdLength = (float)Math.Sqrt(
                    ((origin.X - point2.X + 1) * (origin.X - point2.X + 1)) + ((origin.Z - point2.Z) * (origin.Z - point2.Z))
                );

                return 180 - (float)(Math.Acos((1 + horizontalDistance * horizontalDistance - thirdLength * thirdLength) /
                                               (2 * 1 * horizontalDistance)) * (180 / Math.PI)) + 180;
            } else if (point2.Z == origin.Z) {//If middle
                if (point2.X >= origin.X){
                    return 0;
                }else {
                    return 180;
                }
            } else{ //if right side 
                float thirdLength = (float)Math.Sqrt(
                    ((origin.X - point2.X + 1) * (origin.X - point2.X + 1)) + ((origin.Z - point2.Z) * (origin.Z - point2.Z))
                );

                return (float)(Math.Acos((1 + horizontalDistance * horizontalDistance - thirdLength * thirdLength) /
                                               (2 * 1 * horizontalDistance)) * (180 / Math.PI));
            }
        }
        private float GetVerticalTiltOfLine(Cords origin, Cords point2) {
            //Get horizontal distance
            float horizontalDistance = (float)Math.Sqrt(((origin.X - point2.X) * (origin.X - point2.X)) + ((origin.Z - point2.Z) * (origin.Z - point2.Z)));

            //Get vertical distance
            float verticalDistance = point2.Y - origin.Y;

            //Angle
            float angle = (float)(Math.Atan(verticalDistance / horizontalDistance) * (180 / Math.PI));

            return 90 - angle;
        }

        //Vector functions
        private Cords MultiplyVector(Cords point1, Cords point2) {
            float x = point1.X * point2.X;
            float y = point1.Y * point2.Y;
            float z = point1.Z * point2.Z;

            return new Cords(x, y, z);
        }
        private Cords Multiply(Cords point1, float value) {
            float x = point1.X * value;
            float y = point1.Y * value;
            float z = point1.Z * value;

            return new Cords(x, y, z);
        }
        private Cords Cross(Cords point1, Cords point2){
            float x = point1.Y * point2.Z - point1.Z * point2.Y;
            float y = point1.Z * point2.X - point1.X * point2.Z;
            float z = point1.X * point2.Y - point1.Y * point2.X;

            return new Cords(x, y, z);
        }
        private Cords Sub(Cords point1, Cords point2) {
            float x = point1.X - point2.X;
            float y = point1.Y - point2.Y;
            float z = point1.Z - point2.Z;

            return new Cords(x, y, z);
        }
        private Cords Add(Cords point1, Cords point2) {
            float x = point1.X + point2.X;
            float y = point1.Y + point2.Y;
            float z = point1.Z + point2.Z;

            return new Cords(x, y, z);
        }
        private float Dot(Cords point1, Cords point2) {
            float dotX = point1.X * point2.X;
            float dotY = point1.Y * point2.Y;
            float dotZ = point1.Z * point2.Z;

            return dotX + dotY + dotZ;
        }

        private float DistanceBetween(Cords point1, Cords point2){
            float length = (point1.X - point2.X) * (point1.X - point2.X) + ((point1.Z - point2.Z) * (point1.Z - point2.Z));

            return (float)Math.Sqrt(((point1.Y - point2.Y) * (point1.Y - point2.Y)) + length);
        }
    }
}
