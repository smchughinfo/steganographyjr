using System;
using System.Collections.Generic;
using System.Text;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SteganographyJr.Forms.MarkupExtensions
{

    // TODO: WHAT IS THIS? can it be deleted?
    // copied from https://docs.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/images?tabs=vswin#embedded_images
    [ContentProperty(nameof(Source))]
    public class ImageResourceExtension : IMarkupExtension
    {
        public string Source { get; set; }

        public object ProvideValue(IServiceProvider serviceProvider)
        {
            if (Source == null)
            {
                return null;
            }

            // Do your translation lookup here, using whatever method you require
            var imageSource = ImageSource.FromResource(Source);

            return imageSource;
        }
    }
}