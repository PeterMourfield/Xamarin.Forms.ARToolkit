using System;
using UIKit;

namespace Xamarin.Forms.ARToolkit.iOS
{
	public sealed class UIPointOfInterest
	{
		public UIPointOfInterest(PointOfInterest poi)
		{
            this.POI = poi;
		}

        public PointOfInterest POI
        {
            get;
            private set;
        }

		public UIView View
		{
			get;
			set;
		}
	}
}
