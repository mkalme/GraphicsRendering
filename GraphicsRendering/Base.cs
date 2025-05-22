using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using RayTracing;
using GraphicsModel;
using System.Diagnostics;
using System.Threading;

namespace GraphicsRendering
{
    public partial class Base : Form
    {
        public static World World;
        public static Render Render;

        private static BackgroundWorker BackgroundWorker;

        public Base()
        {
            InitializeComponent();
        }

        private void Base_Load(object sender, EventArgs e)
        {
            SetupWorld();
            SetupRender();

            UpdateGraphics(false);
            UpdatePictureBox();

            //Test();

            //SetupBackgroundWorker();
        }
        private void Test() {
            TimeSpan timeSpan1 = new TimeSpan();
            DateTime time1 = DateTime.Now;

            for (int i = 0; i < 100; i++)
            {
                UpdateGraphics(false);
            }
            timeSpan1 = DateTime.Now - time1;
            Debug.WriteLine(timeSpan1.TotalSeconds + " seconds");
        }

        //Setup
        private void SetupWorld() {
            //World = World.FromFile(@"D:\User\Darbvisma\Scenes\Current\Scene4.xml");
            //World = World.FromFile(@"D:\User\Darbvisma\Scenes\Current\Scene3.xml");
            //World = World.FromFile(@"D:\User\Darbvisma\Scenes\Current\Scene2.xml");
            //World = World.FromFile(@"D:\User\Darbvisma\Scenes\Current\Scene1.xml");
            World = World.FromFile(@"D:\User\Darbvisma\Word Documents\Scenes\Original\World1.xml");
            //World = World.FromFile(@"D:\User\Darbvisma\Scenes\Original\World.xml");
            //World = World.FromFile(@"D:\User\Darbvisma\Scenes\Original\WorldTriangles1.xml");
        }
        private void SetupRender() {
            //Size resolution = new Size(1920, 1080);
            //Size resolution = new Size(1280, 720);
            //Size resolution = new Size(320, 180);
            //Size resolution = new Size(1200, 1200);
            //Size resolution = new Size(934, 700);
            Size resolution = new Size(93, 70);
            //Size resolution = new Size(300, 300);

            Render = new Render(World, resolution);
        }

        private void SetupBackgroundWorker(){
            BackgroundWorker = new BackgroundWorker();

            BackgroundWorker.DoWork += BackgroundWorker_DoWork;
            BackgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            BackgroundWorker.WorkerReportsProgress = true;

            BackgroundWorker.RunWorkerAsync();
        }

        //Update
        private void UpdateGraphics(bool threading) {
            if (threading){
                Render.UpdateWithThread();
            }else {
                Render.Update();
            }
        }
        private void UpdatePictureBox() {
            int width = (int)((Render.ImageWidth / (double)Render.ImageHeight) * pictureBox1.Height);
            int height = pictureBox1.Height;

            if (width > pictureBox1.Width){
                width = pictureBox1.Width;
                height = (int)((Render.ImageHeight / (double)Render.ImageWidth) * width);
            }

            if (width > 0 && height > 0){
                pictureBox1.Image = ResizeBitmap(Render.Image, width, height);
            }
        }
        private Bitmap ResizeBitmap(Bitmap sourceBMP, int width, int height)
        {
            Bitmap result = new Bitmap(width, height);
            using (Graphics g = Graphics.FromImage(result))
            {
                g.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
                g.DrawImage(sourceBMP, 0, 0, width, height);
            }
            return result;
        }

        //Timer
        static float place = 1; //max = 360
        static bool canUpdate = true;

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            for (int i = 0; i < 10000; i++){
                while (!canUpdate) {
                }

                place = Render.ValidateDegrees(place);

                update2();

                UpdateGraphics(true);

                place += 1;

                canUpdate = false;
                BackgroundWorker.ReportProgress(i);
            }
        }
        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            UpdatePictureBox();
            canUpdate = true;
        }

        private void update1() {
            //Get Location
            Ray line = Render.GetRayAngleLine(
                new Cords(5, 0, 0),
                new ViewAngle(Render.ValidateDegrees(place + 180), 90),
                5
            );

            Cords location = line.Point2;
            ViewAngle viewAngle = new ViewAngle(Render.ValidateDegrees(place), 90);

            World.Camera = new Camera(location, viewAngle);
        }
        private void update2() {
            World.LightSources[0].Location.X += 0.30f;
        }
    }
}
