using System;
using System.Collections.Generic;
using Xamarin.Forms;
using Xamarin.Forms.ARToolkit;

namespace GolfCourses
{
    public class App : Application
    {
        public App()
        {
            var content = new ContentPage
            {
                Title = "Famous Golf Courses",
                Content = new AugmentedRealityView
                {
                    Camera = CameraOptions.Rear,
                    ItemsSource = new List<PointOfInterest>
	                {
	                    new PointOfInterest{
	                        Name = "Augusta National",
	                        Latitude = 33.50653,
	                        Longitude = -82.01935
	                    },
	                    new PointOfInterest{
	                        Name = "Pebble Beach",
	                        Latitude = 3.567598,
	                        Longitude = -121.9406033
	                    },
	                    new PointOfInterest{
	                        Name = "Pinehurst #2",
	                        Latitude = 35.18965,
	                        Longitude = -79.46780
	                    },
	                    new PointOfInterest{
	                        Name = "St. Andrews",
	                        Latitude = 56.34328,
	                        Longitude = -2.80206
	                    }
	                },
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    VerticalOptions = LayoutOptions.FillAndExpand
                }
            };

            MainPage = new NavigationPage(content);
        }
    }
}
