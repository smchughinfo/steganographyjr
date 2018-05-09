using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SteganographyJr
{
	public partial class MainPage : CarouselPage
	{
		public MainPage()
		{
         var steganographyJrContentPage = new XAML.SteganographyJr();
         var aboutContentPage = new XAML.About();

         Children.Add(steganographyJrContentPage);
         Children.Add(aboutContentPage);
      }
	}
}
