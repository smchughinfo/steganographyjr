﻿
using SteganographyJr.DependencyService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteganographyJr.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SteganographyJr : ContentPage
	{
		public SteganographyJr ()
		{
			InitializeComponent ();
            var assembly = (typeof(SteganographyJr)).GetTypeInfo().Assembly;
            carrierImage.Source = ImageSource.FromResource("SteganographyJr.Images.default-carrier-image.jpg", assembly);

            chooseImageBtn.Clicked += ExecuteBtn_Clicked;
        }
        
        private async void ExecuteBtn_Clicked(object sender, EventArgs e)
        {
            chooseImageBtn.IsEnabled = false;

            Stream stream = await Xamarin.Forms.DependencyService.Get<IPicturePicker>().GetImageStreamAsync();

            if (stream != null)
            {
                carrierImage.Source = ImageSource.FromStream(() => stream);
            }
            chooseImageBtn.IsEnabled = true;
        }
    }
}