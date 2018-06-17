using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SteganographyJr.Forms
{
	public partial class MainPage : TabbedPage
	{
		public MainPage()
		{
            InitializeComponent();

            Children.Add(new Views.SteganographyJr() { Title = "Encoder" });
            Children.Add(new Views.About() { Title = "About" });
        }
	}
}
