using System;
using Xamarin.Forms;

namespace SteganographyJr.CustomControls
{
   public delegate void CheckedEventHandler(object sender, EventArgs e);

   public class RadioButton : Image
   {
      public static BindableProperty CheckedImageSourceProperty =
         BindableProperty.Create("CheckedImageSource", typeof(string), typeof(RadioButton), string.Empty);
      public static BindableProperty UnCheckedImageSourceProperty =
         BindableProperty.Create("UnCheckedImageSource", typeof(string), typeof(RadioButton), string.Empty);
      public static BindableProperty ValueProperty =
         BindableProperty.Create("Value", typeof(bool), typeof(RadioButton), false);

      public string CheckedImageSource
      {
         get { return (string)GetValue(CheckedImageSourceProperty); }
         set { SetValue(CheckedImageSourceProperty, value); }
      }

      public string UnCheckedImageSource
      {
         get { return (string)GetValue(UnCheckedImageSourceProperty); }
         set { SetValue(UnCheckedImageSourceProperty, value); }
      }

      public bool Value
      {
         get { return (bool)GetValue(ValueProperty); }
         set { SetValue(ValueProperty, value); }
      }

      public event EventHandler<ToggledEventArgs> ValueChanged;

      void OnChecked(ToggledEventArgs e)
      {
         if (ValueChanged != null)
            ValueChanged(this, e);
      }

      public RadioButton()
      {
         var tap = new TapGestureRecognizer();
         tap.Tapped += (sender, e) =>
         {
            Value = !Value;
            OnChecked(new ToggledEventArgs(Value));
         };
         GestureRecognizers.Add(tap);
      }

      protected override void OnPropertyChanged(string propertyName = null)
      {
         base.OnPropertyChanged(propertyName);
         if (propertyName == CheckedImageSourceProperty.PropertyName)
         {
            if (Value)
               Source = CheckedImageSource;
         }
         if (propertyName == UnCheckedImageSourceProperty.PropertyName)
         {
            if (!Value)
               Source = UnCheckedImageSource;
         }
         if (propertyName == ValueProperty.PropertyName)
         {
            if (Value)
               Source = CheckedImageSource;
            else
               Source = UnCheckedImageSource;
         }
      }

   }
}

/* Created By: Carlos Campos
MIT License
Copyright (c) [year] [fullname]
Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE. */
