
using SteganographyJr.Services.DependencyService;
using SteganographyJr.DTOs;
using SteganographyJr.Interfaces;
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

            topSeperator.IsVisible = Device.RuntimePlatform != Device.iOS;

            MessagingCenter.Subscribe<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessage, (IViewModel, message) =>
            {
                // TODO: copy to clipboard
                DisplayAlert(message.Title, message.Message, message.CancelButtonText);
            });
        }
    }
}