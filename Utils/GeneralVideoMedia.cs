/* GeneralVideoMedia.cs : Representation of a Media and Everything that can be modified within it, including the video footage
 * Author : Demomaker
 * Version : 1.0
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace DVL
{
    class GeneralVideoMedia
    {
        public int MediaId;
        public int InterfaceRowId;
        public MediaElement MediaElement;
        public Label Title;
        public Label Description;
        public Label VideoWidth;
        public Label VideoHeight;
        public TextBox FileLocation;
        public TextBox TitleTextBox;
        public TextBox DescriptionTextBox;
        public TextBox VideoWidthTextBox;
        public TextBox VideoHeightTextBox;
        public Image Image;
        public string Filename;
        public bool play;
        public bool videoDisplayed = false;
        public Button DeleteButton;
        public int ScrollPositionChange = 0;

        public void CreateDeleteButton()
        {
            DeleteButton = new Button();
            Image img = new Image();
            img.Source = new BitmapImage(new Uri(Utils.DELETE_BUTTON_IMAGE_LOCATION, UriKind.Relative));

            img.Margin = new Thickness(-8,0,0,0);
            StackPanel stackPnl = new StackPanel();
            stackPnl.Orientation = Orientation.Horizontal;
            stackPnl.Children.Add(img);
            stackPnl.HorizontalAlignment = HorizontalAlignment.Left;
            stackPnl.VerticalAlignment = VerticalAlignment.Center;

            DeleteButton.Content = stackPnl;
            DeleteButton.Margin = new Thickness(50);
        }


        public string GetTitleString()
        {
            return Title.Content.ToString();
        }

        public string GetDescriptionString()
        {
            return Description.Content.ToString();
        }

        public void UpdateTitleLabelWithTextbox()
        {
            Title.Content = TitleTextBox.Text.ToString();
        }

        public void UpdateDescriptionLablWithTextbox()
        {
            Description.Content = DescriptionTextBox.Text.ToString();
        }

        public void SetVideoWidth(double Width)
        {
            MediaElement.Width = Width;
        }

        public void SetVideoHeight(double Height)
        {
            MediaElement.Height = Height;
        }

        public string GetFileLocation()
        {
            return FileLocation.Text.ToString();
        }
    }
}
