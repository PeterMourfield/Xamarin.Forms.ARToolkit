using System.Collections.Generic;

namespace Xamarin.Forms.ARToolkit
{
    public class AugmentedRealityView : View
	{
		public static readonly BindableProperty ItemsProperty =
		  BindableProperty.Create("ItemsSource", 
                                  typeof(IEnumerable<PointOfInterest>), 
                                  typeof(AugmentedRealityView), 
                                  new List<PointOfInterest>());

		public IEnumerable<PointOfInterest> ItemsSource
		{
			get { return (IEnumerable<PointOfInterest>)GetValue(ItemsProperty); }
			set { SetValue(ItemsProperty, value); }
		}

		public static readonly BindableProperty CameraProperty = 
            BindableProperty.Create("Camera",
                                    typeof(CameraOptions),
                                    typeof(AugmentedRealityView),
                                    CameraOptions.Rear);

		public CameraOptions Camera
		{
			get { return (CameraOptions)GetValue(CameraProperty); }
			set { SetValue(CameraProperty, value); }
		}

	}

	public enum CameraOptions
	{
		Rear,
		Front
	}
}
