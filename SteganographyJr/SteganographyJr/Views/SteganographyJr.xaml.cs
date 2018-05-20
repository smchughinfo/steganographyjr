
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
        }
    }
}