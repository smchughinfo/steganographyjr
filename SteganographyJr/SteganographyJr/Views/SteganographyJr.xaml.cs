using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteganographyJr.Views
{
	//[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SteganographyJr : ContentPage
	{
		public SteganographyJr ()
		{
			InitializeComponent ();
            var assembly = (typeof(SteganographyJr)).GetTypeInfo().Assembly;
            carrierImage.Source = ImageSource.FromResource("SteganographyJr.Images.pic.jpg", assembly);
        }
	}
}