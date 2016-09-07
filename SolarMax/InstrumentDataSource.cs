using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SolarMax
{
    internal class InstrumentDataSource
    {
        public Controller Controller { get; private set; }
        public Physics Physics { get; private set; }
        public Camera Camera { get; private set; }
        public Projector Projector { get; private set; }
        public Vector CameraPosition { get; set; }
        public Vector CameraView { get; set; }
        public Vector CameraUp { get; set; }
        public double CameraZoom { get; set; }
        public QSize ScreenSize { get; set; }

        public InstrumentDataSource(Controller Controller, Physics Physics, Camera Camera, Projector Projector)
        {
            this.Controller = Controller;
            this.Camera = Camera;
            this.Physics = Physics;
            this.Projector = Projector;
        }

        public void Update()
        {
            ScreenSize = Controller.ScreenSize;

            CameraPosition = Camera.Position;
            CameraView = Projector.PanView;
            CameraUp = Projector.PanUp;
            CameraZoom = Camera.Zoom;
        }
    }
}
