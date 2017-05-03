using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(Xamarin.Forms.ARToolkit.AugmentedRealityView), typeof(Xamarin.Forms.ARToolkit.iOS.UIAugmentedRealityRenderer))]

namespace Xamarin.Forms.ARToolkit.iOS
{
	public class UIAugmentedRealityRenderer : ViewRenderer<AugmentedRealityView, UIAugmentedRealityView>
	{
		UIAugmentedRealityView uiCameraPreview;

		protected override void OnElementChanged(ElementChangedEventArgs<AugmentedRealityView> e)
		{
			base.OnElementChanged(e);

			if (uiCameraPreview == null)
			{
				uiCameraPreview = new UIAugmentedRealityView(e.NewElement.Camera);
				SetNativeControl(uiCameraPreview);

				uiCameraPreview.PointsOfInterest = new List<UIPointOfInterest>();
				foreach (var poi in e.NewElement.ItemsSource)
				{
					uiCameraPreview.PointsOfInterest.Add(new UIPointOfInterest(poi));
				}
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				uiCameraPreview.Stop();
				uiCameraPreview.CaptureSession.Dispose();
				uiCameraPreview.Dispose();
				uiCameraPreview = null;
			}
			base.Dispose(disposing);
		}

        public static void Initialize()
        {
            
        }
   	}
}
