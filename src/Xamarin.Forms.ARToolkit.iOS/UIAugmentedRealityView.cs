using System.Collections.Generic;
using System.Drawing;
using CoreLocation;
using CoreMotion;
using Foundation;
using Plugin.Geolocator;
using Plugin.Geolocator.Abstractions;
using UIKit;
using System;
using System.Linq;
using AVFoundation;

namespace Xamarin.Forms.ARToolkit.iOS
{
    public class UIAugmentedRealityView : UIView
    {
        CMMotionManager motionManager;

        float[] projectionTransform;
        float[] cameraTransform;
        List<float[]> placesOfInterestCoordinates;
        AVCaptureVideoPreviewLayer previewLayer;
        CameraOptions cameraOptions;

        public AVCaptureSession CaptureSession { get; private set; }

        public bool IsPreviewing { get; set; }

        public List<UIPointOfInterest> PointsOfInterest
        {
            get;
            set;
        }

        public UIAugmentedRealityView(CameraOptions options)
        {
            cameraOptions = options;
            IsPreviewing = false;
            Initialize();

            projectionTransform = new float[16];
            MathHelpers.CreateProjectionMatrix(ref projectionTransform, (float)(UIScreen.MainScreen.Bounds.Size.Width * 1.0 / UIScreen.MainScreen.Bounds.Size.Height), 0.25f, 1000.0f);

            StartLocationListening();
            StartDeviceMotion();
        }

        void Initialize()
        {
            CaptureSession = new AVCaptureSession();
            previewLayer = new AVCaptureVideoPreviewLayer(CaptureSession)
            {
                Frame = Bounds,
                VideoGravity = AVLayerVideoGravity.ResizeAspectFill
            };

            var videoDevices = AVCaptureDevice.DevicesWithMediaType(AVMediaType.Video);
            var cameraPosition = (cameraOptions == CameraOptions.Front) ? AVCaptureDevicePosition.Front : AVCaptureDevicePosition.Back;
            var device = videoDevices.FirstOrDefault(d => d.Position == cameraPosition);

            if (device == null)
            {
                return;
            }

            NSError error;
            var input = new AVCaptureDeviceInput(device, out error);
            CaptureSession.AddInput(input);
            Layer.AddSublayer(previewLayer);
            CaptureSession.StartRunning();
            IsPreviewing = true;
        }

        async void StartLocationListening()
        {
            var locator = CrossGeolocator.Current;
            locator.DesiredAccuracy = 1;
            locator.PositionChanged += (sender, e) =>
            {
                UpdatePlacesOfInterestCoordinates(e.Position);
            };

            await locator.StartListeningAsync(1, 1, true);
        }

        async void StopLocationListening()
        {
            var locator = CrossGeolocator.Current;
            await locator.StopListeningAsync();
        }

        public void Stop()
        {
            StopLocationListening();
            StopCameraPreview();
            StopDeviceMotion();
        }

        public void UpdatePlacesOfInterestCoordinates(Position position)
        {
            double myX = 0.0, myY = 0.0, myZ = 0.0;
            MathHelpers.LatLonToEcef(position.Latitude, position.Longitude, 0.0, ref myX, ref myY, ref myZ);

            placesOfInterestCoordinates = new List<float[]>();

            foreach (var poi in PointsOfInterest)
            {
                double poiX = 0.0, poiY = 0.0, poiZ = 0.0, e = 0.0, n = 0.0, u = 0.0;
                MathHelpers.LatLonToEcef(poi.POI.Latitude, poi.POI.Longitude, poi.POI.Altitude, ref poiX, ref poiY, ref poiZ);
                MathHelpers.EcefToEnu(poi.POI.Latitude, poi.POI.Longitude, myX, myY, myZ, poiX, poiY, poiZ, ref e, ref n, ref u);

                var p = new float[4];
                p[0] = (float)n;
                p[1] = -(float)e;
                p[2] = 0.0f;
                p[3] = 1.0f;

                placesOfInterestCoordinates.Add(p);

                if (poi.View == null)
                {
                    poi.View = new UILabel
                    {
                        AdjustsFontSizeToFitWidth = false,
                        Opaque = false,
                        BackgroundColor = new UIColor(0.1f, 0.1f, 0.1f, 0.5f),
                        Center = new PointF(200.0f, 200.0f),
                        TextAlignment = UITextAlignment.Center,
                        TextColor = UIColor.White,
                        Lines = 0,
                        LineBreakMode = UILineBreakMode.WordWrap,
                        Hidden = true
                    };

                    AddSubview(poi.View);
                }

                CLLocation newLocation = new CLLocation(new CLLocationCoordinate2D(position.Latitude, position.Longitude),
                                                        position.Altitude, position.Accuracy, position.AltitudeAccuracy, NSDate.Now);
                var distance = newLocation.DistanceFrom(new CLLocation(poi.POI.Latitude, poi.POI.Longitude));
                if (distance > 1000)
                {
                    ((UILabel)poi.View).Text = string.Format("{0} - {1:F} km", poi.POI.Name, distance / 1000);
                }
                else
                {
                    ((UILabel)poi.View).Text = string.Format("{0} - {1:F} m", poi.POI.Name, distance);
                }
                var size = ((UILabel)poi.View).Text.StringSize(((UILabel)poi.View).Font);
                // var size = UIStringDrawing.StringSize(((UILabel)poi.View).Text, ((UILabel)poi.View).Font);
                ((UILabel)poi.View).Bounds = new CoreGraphics.CGRect(0.0f, 0.0f, size.Width, size.Height);
            }
        }

        public override void Draw(CoreGraphics.CGRect rect)
        {
            base.Draw(rect);

            previewLayer.Frame = rect;

            if (placesOfInterestCoordinates == null)
            {
                return;
            }

            var projectionCameraTransform = new float[16];
            MathHelpers.MultiplyMatrixAndMatrix(ref projectionCameraTransform, projectionTransform, cameraTransform);

            for (int i = 0; i < PointsOfInterest.Count; i++)
            {
                var poi = PointsOfInterest[i];

                var v = new float[4];
                MathHelpers.MultiplyMatrixAndVector(ref v, projectionCameraTransform, placesOfInterestCoordinates[i]);

                float x = (v[0] / v[3] + 1.0f) * 0.5f;
                float y = (v[1] / v[3] + 1.0f) * 0.5f;

                if (v[2] < 0.0f)
                {
                    poi.View.Center = new CoreGraphics.CGPoint(x * Bounds.Size.Width, Bounds.Size.Height - y * Bounds.Size.Height);
                    poi.View.Hidden = false;
                }
                else
                {
                    poi.View.Hidden = true;
                }
            }
        }

        void StartDeviceMotion()
        {
            motionManager = new CMMotionManager
            {
                ShowsDeviceMovementDisplay = true,
                DeviceMotionUpdateInterval = 1.0 / 60.0
            };
            motionManager.StartDeviceMotionUpdates(CMAttitudeReferenceFrame.XTrueNorthZVertical, NSOperationQueue.CurrentQueue, (motion, error) =>
            {
                if (motion != null)
                {
                    cameraTransform = new float[16];
                    TransformFromCMRotationMatrix(ref cameraTransform, motion.Attitude.RotationMatrix);
                    SetNeedsDisplay();
                }
            });
        }

        void UpdateDisplay()
        {
            CMDeviceMotion motion = motionManager.DeviceMotion;
            if (motion != null)
            {
                cameraTransform = new float[16];
                TransformFromCMRotationMatrix(ref cameraTransform, motion.Attitude.RotationMatrix);

                SetNeedsDisplay();
            }
        }

        void StopCameraPreview()
        {
            CaptureSession.StopRunning();
        }

        void StopDeviceMotion()
        {
            motionManager.StopDeviceMotionUpdates();
        }

        public static void TransformFromCMRotationMatrix(ref float[] mout, CMRotationMatrix m)
        {
            mout[0] = (float)m.m11;
            mout[1] = (float)m.m21;
            mout[2] = (float)m.m31;
            mout[3] = 0.0f;

            mout[4] = (float)m.m12;
            mout[5] = (float)m.m22;
            mout[6] = (float)m.m32;
            mout[7] = 0.0f;

            mout[8] = (float)m.m13;
            mout[9] = (float)m.m23;
            mout[10] = (float)m.m33;
            mout[11] = 0.0f;

            mout[12] = 0.0f;
            mout[13] = 0.0f;
            mout[14] = 0.0f;
            mout[15] = 1.0f;
        }
    }
}
