
using SteganographyJr.DependencyService;
using SteganographyJr.Models;
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
            carrierImage.Source = ImageSource.FromResource("SteganographyJr.Images.default-carrier-image.png", assembly);

            chooseImageBtn.Clicked += ExecuteBtn_Clicked;
            chooseFileBtn.Clicked += ChooseFileBtn_Clicked;
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

        private async void ChooseFileBtn_Clicked(object sender, EventArgs e)
        {
            chooseFileBtn.IsEnabled = false;

            StreamWithPath streamWithPath = await Xamarin.Forms.DependencyService.Get<IFilePicker>().GetSteamWithPathAsync();

            if(streamWithPath.Stream != null)
            {
                chooseFileEntry.Text = streamWithPath.Path;
            }
            chooseFileBtn.IsEnabled = true;
        }
    }
}