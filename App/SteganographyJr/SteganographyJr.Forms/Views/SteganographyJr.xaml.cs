using SteganographyJr.Forms.DTOs;
using SteganographyJr.Forms.Interfaces;
using SteganographyJr.Forms.Services.DependencyService;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteganographyJr.Forms.Views
{
	[XamlCompilation(XamlCompilationOptions.Compile)]
	public partial class SteganographyJr : ContentPage
	{
		public SteganographyJr ()
		{
			InitializeComponent ();

            topSeperator.IsVisible = Device.RuntimePlatform == Device.UWP;

            MessagingCenter.Subscribe<IViewModel, AlertMessage>(this, StaticVariables.DisplayAlertMessage, (IViewModel, message) =>
            {
                // TODO: copy to clipboard
                DisplayAlert(message.Title, message.Message, message.CancelButtonText);
            });

            MessagingCenter.Subscribe<IFileIO, AlertMessage>(this, StaticVariables.DisplayAlertMessage, async (IViewModel, message) =>
            {
                await DisplayAlert(message.Title, message.Message, message.CancelButtonText);

                MessagingCenter.Send<object>(this, StaticVariables.AlertCompleteMessage);
            });
        }
    }
}