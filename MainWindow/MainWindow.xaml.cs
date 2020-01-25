using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using System.Data.SqlClient;
using System.Data;
using System.Data.SQLite;
using System.Text.RegularExpressions;
using System.IO;
using System.Reflection;
using System.Resources;

namespace DVL
{
    /// <summary>
    /// Interaction Logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private List<GeneralVideoMedia> GeneralVideoMedias = new List<GeneralVideoMedia>();
        private System.IO.FileInfo[] files = new System.IO.FileInfo[4];
        private const int MEDIA_WIDTH = 256;
        private const int MEDIA_HEIGHT = 144;
        private const int GENERAL_MEDIA_OFFSET = 50;
        private const int FIRST_ROW_HEIGHT = 20 + 12;
        private const int FIRST_COLUMN_WIDTH = MEDIA_WIDTH + 12;
        private const int DpiX = 96;
        private const string APP_NAME = "Demomaker's Video Library";
        private DispatcherTimer time = new DispatcherTimer();
        private int fontSize = 20;
        private static SolidColorBrush brush = new SolidColorBrush();
        private SQLiteConnection m_dbConnection = new SQLiteConnection("Data Source=Medias.sqlite;Version=3;");
        private double listDetailsWidth = 0;
        private bool videoInFullScreen = false;
        string title = "Video Library";

        /// <summary>
        /// Constructor
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            time.Start();
            time.Tick += new EventHandler(Main);
            time.Interval = new TimeSpan(1);
        }
        #region App Initialization
        private void OnApplicationLayoutGridLoaded(object sender, RoutedEventArgs e)
        {
            InitializeApplication();
        }

        private void InitializeApplication()
        {
            CreateTable();
            Utils.APP_LOCATION = System.IO.Path.GetDirectoryName(Assembly.GetEntryAssembly().Location);
            Utils.FILE_STORING_LOCATION = Utils.APP_LOCATION + Utils.FILE_STORING_LOCATION_FOLDER_NAME;
            System.IO.Directory.CreateDirectory(Utils.FILE_STORING_LOCATION);
            title = Utils.APP_LOCATION.Substring(0, Utils.APP_LOCATION.Length - 4);
            (Title as Label).Content = title;
            title = APP_NAME + " " + title;
            window.SetValue(TitleProperty, title);
            CreateNonMedias();
            deleteCol.MinWidth = MEDIA_HEIGHT + 4;
            deleteCol.MaxWidth = MEDIA_HEIGHT + 4;
            List<int> MediaIDs = new List<int>();
            string sql = "SELECT id_media from medias";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            using (var data = command.ExecuteReader())
            {
                while (data.Read())
                {
                    MediaIDs.Add(Int32.Parse(data.GetValue(0).ToString()));
                }
                data.Close();
            }
            foreach (int mediaID in MediaIDs)
            {
                sql = "SELECT filelocation from medias WHERE id_media = '" + mediaID + "'";
                command = new SQLiteCommand(sql, m_dbConnection);
                var data = command.ExecuteReader();
                string filelocation = data.GetValue(0).ToString();

                sql = "SELECT filename from medias WHERE id_media = '" + mediaID + "'";
                command = new SQLiteCommand(sql, m_dbConnection);
                data = command.ExecuteReader();
                string filename = data.GetValue(0).ToString();

                if (File.Exists(filelocation))
                {
                    GeneralVideoMedia generalVideoMedia = CreateMedia(filelocation);
                    generalVideoMedia.Filename = filename;
                    generalVideoMedia.MediaId = mediaID;

                    LoadInterfaceChanges(generalVideoMedia.MediaId, generalVideoMedia, filelocation);
                }
                else
                {
                    sql = "DELETE FROM medias WHERE filelocation = '" + filelocation + "'";
                    command = new SQLiteCommand(sql, m_dbConnection);
                    data = command.ExecuteReader();

                    if (File.Exists(System.IO.Path.GetFullPath("Media") + "/" + filename.Substring(0, filename.Length) + ".png"))
                    {
                        File.Delete(System.IO.Path.GetFullPath("Media") + "/" + filename.Substring(0, filename.Length) + ".png");
                    }
                }
            }
            CreateGridTopParts();

            MediaIDs.Clear();
        }

        private void CreateTable()
        {
            m_dbConnection.Open();
            string sqlcreate = "create table if not exists medias (id_media INTEGER PRIMARY KEY, filename varchar(5000), img_path varchar(5000), title varchar(5000), description varchar(5000), filelocation varchar(5000));";
            SQLiteCommand commandcreate = new SQLiteCommand(sqlcreate, m_dbConnection);
            commandcreate.ExecuteNonQuery();
        }


        /// <summary>
        /// Window Running Instance
        /// </summary>
        /// <param name="sender">Object that starts the instance</param>
        /// <param name="e">Event argument</param>
        private void Main(object sender, EventArgs e)
        {
            //Pour chaque MediaElement qui n'est pas nulle, changer son état de jouage dépendamment si on l'active ou non.
            for (int count = 0; count < GeneralVideoMedias.Count; count++)
            {
                if (GeneralVideoMedias[count] != null && GeneralVideoMedias[count].MediaElement != null)
                {
                    if (GeneralVideoMedias[count].play == true)
                    {
                        GeneralVideoMedias[count].MediaElement.Play();
                    }
                    else
                    {
                        GeneralVideoMedias[count].MediaElement.Pause();
                    }
                }
            }
        }



        private void CreateNonMedias()
        {
            listDetailsWidth = (double)window.Width / 2;
            topCol.MinWidth = 40;
            topCol.MaxWidth = 40;
            colOfVids.MinWidth = FIRST_COLUMN_WIDTH;
            colOfVids.MaxWidth = FIRST_COLUMN_WIDTH;
            titleCol.MinWidth = window.Width / 2;
            titleCol.MaxWidth = window.Width / 2;
            topRow.MinHeight = FIRST_ROW_HEIGHT;
            topRow.MaxHeight = FIRST_ROW_HEIGHT;
            ColumnDefinition columnDefinition = new ColumnDefinition();
            columnDefinition.Width = new GridLength(1, GridUnitType.Star);
            grid.ColumnDefinitions.Add(columnDefinition);
            ResetTables();
        }

        private void CreateGridTopParts()
        {
            Grid.SetRow(Title, 0);
            searchBox.VerticalAlignment = VerticalAlignment.Center;
            searchBox.HorizontalAlignment = HorizontalAlignment.Left;
            searchBoxHint.VerticalAlignment = VerticalAlignment.Center;
            searchBoxHint.HorizontalAlignment = HorizontalAlignment.Left;
            searchButton.Width = 40;
            searchButton.Height = 34;
            searchButton.Margin = new Thickness(searchBox.Width, 0, 0, 0);

            Grid.SetRow(searchBox, 0);
            Grid.SetRow(searchBoxHint, 0);
            Grid.SetRow(searchButton, 0);
            Grid.SetRow(upload, 0);
            Grid.SetColumn(Title, 1);
            Grid.SetColumn(searchBox, 2);
            Grid.SetColumn(searchBoxHint, 2);
            Grid.SetColumn(searchButton, 2);
            Grid.SetColumn(upload, 3);
        }
        #endregion
        #region Controls
        #region Scrolling
        public void OnScroll(object sender, MouseWheelEventArgs e)
        {
            if (e.Delta > 0)
            {
                ScrollUp();
            }

            else if (e.Delta < 0)
                ScrollDown();
        }

        private void ScrollDown()
        {
            if (!VideoPlaying())
            {
                /*
                 Scroll Algorithm :
                 Take information from 1st row (store it in variable)
                 For each Row, Put its information in the previous row
                 Then take information from 1st row and put it at last row
                 */

                
                GeneralVideoMedia firstRowMedia = new GeneralVideoMedia();
                firstRowMedia = GetGeneralVideoMediaWithScrollViewID(0);
                for (int i = 1; i < grid.RowDefinitions.Count() - 1; i++)
                {

                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).MediaElement, i);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).Image, i);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).Title, i);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).TitleTextBox, i);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).Description, i);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).DescriptionTextBox, i);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).FileLocation, i);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).DeleteButton, i);
                    GetGeneralVideoMediaWithScrollViewID(i).ScrollPositionChange += -1;
                }

                Grid.SetRow(firstRowMedia.MediaElement, grid.RowDefinitions.Count() - 1);
                Grid.SetRow(firstRowMedia.Image, grid.RowDefinitions.Count() - 1);
                Grid.SetRow(firstRowMedia.Title, grid.RowDefinitions.Count() - 1);
                Grid.SetRow(firstRowMedia.TitleTextBox, grid.RowDefinitions.Count() - 1);
                Grid.SetRow(firstRowMedia.Description, grid.RowDefinitions.Count() - 1);
                Grid.SetRow(firstRowMedia.DescriptionTextBox, grid.RowDefinitions.Count() - 1);
                Grid.SetRow(firstRowMedia.FileLocation, grid.RowDefinitions.Count() - 1);
                Grid.SetRow(firstRowMedia.DeleteButton, grid.RowDefinitions.Count() - 1);

                firstRowMedia.ScrollPositionChange += (grid.RowDefinitions.Count() - 2);

            }
        }


        private void ScrollUp()
        {
            if (!VideoPlaying())
            {

                GeneralVideoMedia lastRowMedia = new GeneralVideoMedia();
                lastRowMedia = GetGeneralVideoMediaWithScrollViewID(grid.RowDefinitions.Count() - 2);
                for (int i = grid.RowDefinitions.Count() - 3; i >= 0; i--)
                {

                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).MediaElement, i+2);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).Image, i+2);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).Title, i+2);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).TitleTextBox, i+2);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).Description, i+2);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).DescriptionTextBox, i+2);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).FileLocation, i+2);
                    Grid.SetRow(GetGeneralVideoMediaWithScrollViewID(i).DeleteButton, i+2);
                    GetGeneralVideoMediaWithScrollViewID(i).ScrollPositionChange += 1;
                }

                Grid.SetRow(lastRowMedia.MediaElement, 1);
                Grid.SetRow(lastRowMedia.Image, 1);
                Grid.SetRow(lastRowMedia.Title, 1);
                Grid.SetRow(lastRowMedia.TitleTextBox, 1);
                Grid.SetRow(lastRowMedia.Description, 1);
                Grid.SetRow(lastRowMedia.DescriptionTextBox, 1);
                Grid.SetRow(lastRowMedia.FileLocation, 1);
                Grid.SetRow(lastRowMedia.DeleteButton, 1);

                lastRowMedia.ScrollPositionChange += -(grid.RowDefinitions.Count() - 2);
            }
        }
        #endregion
        #region Enter
        private void EnterPress(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                TextBoxDoubleClicked(sender, e);
            }
        }
        #endregion
        #endregion
        #region Interface Actions

        /// <summary>
        /// When we upload a media
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnMediaUpload(object sender, RoutedEventArgs e)
        {
            bool TakeOffPlayingVideo = false;
            foreach (GeneralVideoMedia generalVideoMedia in GeneralVideoMedias)
            {
                if (generalVideoMedia.play == true)
                {
                    TakeOffPlayingVideo = true;
                }
            }
            if (TakeOffPlayingVideo)
                ResetVideoDisplayUsing(GeneralVideoMedias);
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.DefaultExt = ".mpeg";
            openFileDialog.Filter = "Video files (*.mp4, *.mpeg)|*.mp4;*.mpeg;";
            openFileDialog.ShowDialog();
            if (!System.IO.Directory.Exists(System.IO.Path.GetFullPath("Media") + @"\"))
                System.IO.Directory.CreateDirectory(System.IO.Path.GetFullPath("Media"));
            if (!(openFileDialog.FileName == null) && !(openFileDialog.FileName == "") && !(FileIsAlreadyInDatabase(openFileDialog.FileName)))
            {
                LoadNewMedia(openFileDialog.SafeFileName, openFileDialog.FileName);
            }
        }



        private void LoadInterfaceChanges(int mediaID, GeneralVideoMedia generalVideoMedia, string filelocation)
        {
            string filename = generalVideoMedia.Filename;
            int mediaId = generalVideoMedia.MediaId;
            ChangeMediaPlayProperties(generalVideoMedia.MediaElement, generalVideoMedia);
            generalVideoMedia.FileLocation = CreateFileLocationLabel(filelocation);
            generalVideoMedia.Title = CreateTitleLabel(filename, mediaId);
            generalVideoMedia.TitleTextBox = CreateTitleTextBox(filename, mediaId);
            generalVideoMedia.Description = CreateDescriptionLabel(filename, mediaId);
            generalVideoMedia.DescriptionTextBox = CreateDescriptionTextBox(filename, mediaId);
            generalVideoMedia.VideoWidth = CreateVideoWidthLabel(filename);
            generalVideoMedia.VideoWidthTextBox = CreateVideoWidthTextBox(filename);
            generalVideoMedia.VideoHeight = CreateVideoHeightLabel(filename);
            generalVideoMedia.VideoHeightTextBox = CreateVideoHeightTextBox(filename);
            generalVideoMedia.Image = CreateImage(generalVideoMedia.MediaElement, filename);
            generalVideoMedia.DeleteButton = CreateDeleteButton(generalVideoMedia);
            generalVideoMedia.ScrollPositionChange = 0;
            GeneralVideoMedias.Add(generalVideoMedia);
            AddGeneralVideoMediaInformationToDisplay(generalVideoMedia);

        }

        private void OnMediaElementDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.ClickCount >= 2)
            {
                if (!videoInFullScreen)
                {
                    RowDefinition rowDefinition = new RowDefinition();
                    rowDefinition.Height = new GridLength(1, GridUnitType.Star);
                    (sender as MediaElement).Margin = new Thickness(0, -4, -12, 0);
                    grid.RowDefinitions.Add(rowDefinition);
                    this.WindowStyle = WindowStyle.None;
                    this.WindowState = WindowState.Maximized;
                    Grid.SetRow((sender as MediaElement), 0);
                    Grid.SetColumn((sender as MediaElement), 0);
                    Grid.SetColumnSpan((sender as MediaElement), 10);
                    Grid.SetRowSpan((sender as MediaElement), 10);
                    (sender as MediaElement).Width = Window.GetWindow(this).Width;
                    (sender as MediaElement).Height = Window.GetWindow(this).Height;
                }
                else
                {
                    Grid.SetRow((sender as MediaElement), 2);
                    Grid.SetColumn((sender as MediaElement), 2);
                    Grid.SetColumnSpan((sender as MediaElement), 1);
                    Grid.SetRowSpan((sender as MediaElement), 1);
                    (sender as MediaElement).Width = 1280;
                    (sender as MediaElement).Height = 720;
                    this.WindowStyle = WindowStyle.SingleBorderWindow;
                    this.WindowState = WindowState.Normal;

                }
                videoInFullScreen = !videoInFullScreen;
            }
        }

        private void LabelClicked(object sender, RoutedEventArgs e)
        {
            if (sender is Label)
            {
                for (int count = 0; count < GeneralVideoMedias.Count; count++)
                {
                    if (GeneralVideoMedias[count].Title == sender as Label)
                    {
                        ReplaceLabelWithTextBox(GeneralVideoMedias[count].Title, GeneralVideoMedias[count].TitleTextBox);
                    }
                    else if (GeneralVideoMedias[count].Description == sender as Label)
                    {
                        ReplaceLabelWithTextBox(GeneralVideoMedias[count].Description, GeneralVideoMedias[count].DescriptionTextBox);
                    }
                    else if (GeneralVideoMedias[count].VideoWidth == sender as Label)
                    {
                        ReplaceLabelWithTextBox(GeneralVideoMedias[count].VideoWidth, GeneralVideoMedias[count].VideoWidthTextBox);
                    }
                    else if (GeneralVideoMedias[count].VideoHeight == sender as Label)
                    {
                        ReplaceLabelWithTextBox(GeneralVideoMedias[count].VideoHeight, GeneralVideoMedias[count].VideoHeightTextBox);
                    }
                }
            }
        }

        private void TextBoxDoubleClicked(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox)
            {
                if ((sender as TextBox).Visibility != Visibility.Hidden)
                {
                    for (int count = 0; count < GeneralVideoMedias.Count; count++)
                    {
                        if (GeneralVideoMedias[count].TitleTextBox == sender as TextBox)
                        {
                            ReplaceTextBoxWithLabel(GeneralVideoMedias[count].TitleTextBox, GeneralVideoMedias[count].Title);
                            UpdateTitleOfSpecificMedia(GeneralVideoMedias[count].MediaId, (sender as TextBox).Text);
                            GeneralVideoMedias[count].Title.Content = GetTitleOfMedia(GeneralVideoMedias[count].MediaId);
                        }
                        else if (GeneralVideoMedias[count].DescriptionTextBox == sender as TextBox)
                        {
                            ReplaceTextBoxWithLabel(GeneralVideoMedias[count].DescriptionTextBox, GeneralVideoMedias[count].Description);
                            UpdateDescripitionOfSpecificMedia(GeneralVideoMedias[count].MediaId, (sender as TextBox).Text);
                            GeneralVideoMedias[count].Description.Content = GetDescriptionOfMedia(GeneralVideoMedias[count].MediaId);
                        }
                        else if (GeneralVideoMedias[count].VideoWidthTextBox == sender as TextBox)
                        {
                            ReplaceTextBoxWithLabel(GeneralVideoMedias[count].VideoWidthTextBox, GeneralVideoMedias[count].VideoWidth);
                            GeneralVideoMedias[count].VideoWidth.Content = (sender as TextBox).Text;
                            double testDouble;
                            if (Double.TryParse((sender as TextBox).Text, out testDouble) == true)
                            {
                                grid.ColumnDefinitions[2].MinWidth = Double.Parse((sender as TextBox).Text);
                                grid.ColumnDefinitions[2].MaxWidth = Double.Parse((sender as TextBox).Text);
                                window.Width = Double.Parse((sender as TextBox).Text) + FIRST_COLUMN_WIDTH + GENERAL_MEDIA_OFFSET;
                                GeneralVideoMedias[count].SetVideoWidth(Double.Parse((sender as TextBox).Text));
                            }
                        }
                        else if (GeneralVideoMedias[count].VideoHeightTextBox == sender as TextBox)
                        {
                            ReplaceTextBoxWithLabel(GeneralVideoMedias[count].VideoHeightTextBox, GeneralVideoMedias[count].VideoHeight);
                            GeneralVideoMedias[count].VideoHeight.Content = (sender as TextBox).Text;
                            double testDouble;
                            if (Double.TryParse((sender as TextBox).Text, out testDouble) == true)
                            {
                                grid.RowDefinitions[2].MinHeight = Double.Parse((sender as TextBox).Text);
                                grid.RowDefinitions[2].MaxHeight = Double.Parse((sender as TextBox).Text);
                                window.Height = Double.Parse((sender as TextBox).Text) + (FIRST_ROW_HEIGHT) + 100 + GENERAL_MEDIA_OFFSET + MEDIA_HEIGHT;
                                GeneralVideoMedias[count].SetVideoHeight(Double.Parse((sender as TextBox).Text));
                            }
                        }
                    }
                }
            }
        }


        private void OnSearch(object sender, RoutedEventArgs e)
        {
            List<GeneralVideoMedia> foundGeneralVideoMedias = GetVideosSimilarToSearchTermIntoStringArray();
            ResetVideoDisplayUsing(foundGeneralVideoMedias);
        }


        private void TitleClicked(object sender, MouseButtonEventArgs e)
        {
            ResetVideoDisplayUsing(GeneralVideoMedias);
        }
        #endregion
        #region GeneralVideoMedia Creation
        private void LoadNewMedia(string fileName, string fileLocation)
        {
            System.IO.DirectoryInfo medias = new System.IO.DirectoryInfo(System.IO.Path.GetFullPath("Media"));

            AddMediaToTable(fileName, fileName.Substring(0, fileName.Length) + ".png", fileLocation);

            GeneralVideoMedia generalVideoMedia = CreateMedia(fileLocation);
            generalVideoMedia.Filename = fileName.Substring(0, fileName.Length);
            generalVideoMedia.MediaId = GetMostRecentlyInsertedMediaID();

            LoadInterfaceChanges(generalVideoMedia.MediaId, generalVideoMedia, fileLocation);
        }
        private GeneralVideoMedia CreateMedia(string filelocation)
        {
            GeneralVideoMedia generalVideoMedia = new GeneralVideoMedia();
            generalVideoMedia.play = false;
            generalVideoMedia.MediaElement = new MediaElement();
            generalVideoMedia.MediaElement.Name = "mediaElement";
            generalVideoMedia.MediaElement.Source = new Uri(filelocation, UriKind.Relative);
            generalVideoMedia.MediaElement.Margin = new Thickness(0, 0, 0, 0);
            generalVideoMedia.MediaElement.UnloadedBehavior = MediaState.Close;
            generalVideoMedia.MediaElement.RenderTransformOrigin = new Point(0.251, 0.47);
            generalVideoMedia.MediaElement.Stretch = Stretch.Uniform;
            generalVideoMedia.MediaElement.StretchDirection = StretchDirection.Both;
            generalVideoMedia.MediaElement.RenderSize = new Size(MEDIA_WIDTH, MEDIA_HEIGHT);
            generalVideoMedia.MediaElement.MouseDown += OnMediaElementDoubleClick;

            RowDefinition row = new RowDefinition();
            row.MaxHeight = MEDIA_HEIGHT + 24;
            row.MinHeight = MEDIA_HEIGHT + 24;
            grid.RowDefinitions.Add(row);

            generalVideoMedia.InterfaceRowId = (grid.RowDefinitions.IndexOf(row) - 1);

            return generalVideoMedia;
        }


        private Image CreateImage(MediaElement mediaElement, string filename)
        {
            if (!File.Exists(System.IO.Path.GetFullPath("Media") + "/" + filename.Substring(0, filename.Length) + ".png"))
                WriteToPng(mediaElement, filename.Substring(0, filename.Length) + ".png");

            //Console.WriteLine("Image Resource Exists? : " + ResourceExists(System.IO.Path.GetFullPath("Media") + "/" + filename.Substring(0, filename.Length - 4) + ".png"));
            BitmapImage bitmap = new BitmapImage(new Uri(System.IO.Path.GetFullPath("Media") + "/" + filename.Substring(0, filename.Length) + ".png", UriKind.Absolute));


            Image image = new Image();
            image.Source = bitmap;
            image.Width = MEDIA_WIDTH;
            image.Height = MEDIA_HEIGHT;
            image.RenderTransformOrigin = new Point(0.251, 0.47);
            image.Stretch = Stretch.Uniform;
            image.StretchDirection = StretchDirection.Both;
            image.Margin = new Thickness(12, 12, 0, 0);
            image.Visibility = Visibility.Visible;
            image.MouseDown += PlayVideo;

            return image;
        }


        private Button CreateDeleteButton(GeneralVideoMedia generalVideoMedia)
        {
            generalVideoMedia.CreateDeleteButton();
            generalVideoMedia.DeleteButton.Click += (sender, e) => DeleteMedia(generalVideoMedia);
            return generalVideoMedia.DeleteButton;
        }

        private TextBox CreateFileLocationLabel(string filelocation)
        {
            TextBox FileLocation = new TextBox();
            FileLocation.Text = filelocation;
            FileLocation.FontSize = fontSize;
            FileLocation.FontWeight = FontWeights.Bold;
            FileLocation.Margin = new Thickness(12, fontSize * 1.5, 0, 0);
            brush.Color = Colors.Black;
            FileLocation.Foreground = brush;
            FileLocation.VerticalAlignment = VerticalAlignment.Center;
            FileLocation.VerticalContentAlignment = VerticalAlignment.Top;
            FileLocation.HorizontalContentAlignment = HorizontalAlignment.Left;
            FileLocation.IsReadOnly = true;
            FileLocation.TextWrapping = TextWrapping.Wrap;
            FileLocation.BorderThickness = new Thickness(0, 0, 0, 0);
            FileLocation.Background = Brushes.Transparent;
            return FileLocation;
        }

        private Label CreateTitleLabel(string filename, int mediaID)
        {
            Label title = new Label();
            GetTitleLabelContentFromDatabase(title, mediaID);
            title.FontSize = fontSize;
            title.FontWeight = FontWeights.Bold;
            title.Margin = new Thickness(12, fontSize * 1.5, 0, 0);
            brush.Color = Colors.Black;
            title.Foreground = brush;
            title.VerticalAlignment = VerticalAlignment.Top;
            title.VerticalContentAlignment = VerticalAlignment.Top;
            title.HorizontalContentAlignment = HorizontalAlignment.Left;
            title.MouseDown += LabelClicked;
            return title;
        }

        private Label CreateVideoWidthLabel(string filename)
        {
            Label videoWidth = new Label();
            videoWidth.FontSize = fontSize;
            videoWidth.FontWeight = FontWeights.Bold;
            videoWidth.Margin = new Thickness(12, fontSize * 1.5, 0, 0);
            brush.Color = Colors.Black;
            videoWidth.VerticalAlignment = VerticalAlignment.Top;
            videoWidth.VerticalContentAlignment = VerticalAlignment.Top;
            videoWidth.HorizontalContentAlignment = HorizontalAlignment.Center;
            videoWidth.MouseDown += LabelClicked;
            return videoWidth;
        }

        private Label CreateVideoHeightLabel(string filename)
        {
            Label videoHeight = new Label();
            videoHeight.FontSize = fontSize;
            videoHeight.FontWeight = FontWeights.Bold;
            videoHeight.Margin = new Thickness(12, fontSize * 1.5, 0, 0);
            brush.Color = Colors.Black;
            videoHeight.Foreground = brush;
            videoHeight.VerticalAlignment = VerticalAlignment.Bottom;
            videoHeight.VerticalContentAlignment = VerticalAlignment.Top;
            videoHeight.HorizontalContentAlignment = HorizontalAlignment.Center;
            videoHeight.MouseDown += LabelClicked;
            return videoHeight;
        }

        private Label CreateDescriptionLabel(string filename, int mediaID)
        {
            Label description = new Label();
            GetDescriptionLabelContentFromDatabase(description, mediaID);

            if (description.Content == null || description.Content.ToString() == "")
                description.Content = "Default Description";

            description.FontSize = fontSize - 4;
            description.FontWeight = FontWeights.Bold;
            description.Margin = new Thickness(12, fontSize * 1.5, 0, 0);
            brush.Color = Colors.Black;
            description.Foreground = brush;
            description.VerticalAlignment = VerticalAlignment.Bottom;
            description.VerticalContentAlignment = VerticalAlignment.Top;
            description.HorizontalContentAlignment = HorizontalAlignment.Left;
            description.MouseDown += LabelClicked;
            return description;
        }

        private TextBox CreateTitleTextBox(string filename, int mediaID)
        {
            TextBox titleTextBox = new TextBox();
            Label title = new Label();
            GetTitleLabelContentFromDatabase(title, mediaID);
            GetTextBoxTextFromLabelContent(titleTextBox, title);
            titleTextBox.FontSize = fontSize;
            titleTextBox.FontWeight = FontWeights.Bold;
            titleTextBox.Margin = new Thickness(12, fontSize * 1.5, 0, 0);
            brush.Color = Colors.Black;
            titleTextBox.Foreground = brush;
            titleTextBox.VerticalAlignment = VerticalAlignment.Top;
            titleTextBox.VerticalContentAlignment = VerticalAlignment.Top;
            titleTextBox.HorizontalContentAlignment = HorizontalAlignment.Left;
            titleTextBox.Visibility = Visibility.Hidden;
            titleTextBox.LostFocus += TextBoxDoubleClicked;
            titleTextBox.KeyDown += EnterPress;
            return titleTextBox;
        }

        private TextBox CreateVideoWidthTextBox(string filename)
        {
            TextBox videoWidthTextBox = new TextBox();
            videoWidthTextBox.FontSize = fontSize;
            videoWidthTextBox.FontWeight = FontWeights.Bold;
            videoWidthTextBox.Margin = new Thickness(12, fontSize * 1.5, 0, 0);
            brush.Color = Colors.Black;
            videoWidthTextBox.Foreground = brush;
            videoWidthTextBox.VerticalAlignment = VerticalAlignment.Top;
            videoWidthTextBox.VerticalContentAlignment = VerticalAlignment.Top;
            videoWidthTextBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            videoWidthTextBox.Visibility = Visibility.Hidden;
            videoWidthTextBox.LostFocus += TextBoxDoubleClicked;
            videoWidthTextBox.KeyDown += EnterPress;
            return videoWidthTextBox;
        }

        private TextBox CreateVideoHeightTextBox(string filename)
        {
            TextBox videoHeightTextBox = new TextBox();
            videoHeightTextBox.FontSize = fontSize;
            videoHeightTextBox.FontWeight = FontWeights.Bold;
            videoHeightTextBox.Margin = new Thickness(12, fontSize * 1.5, 0, 0);
            brush.Color = Colors.Black;
            videoHeightTextBox.Foreground = brush;
            videoHeightTextBox.VerticalAlignment = VerticalAlignment.Bottom;
            videoHeightTextBox.VerticalContentAlignment = VerticalAlignment.Top;
            videoHeightTextBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            videoHeightTextBox.Visibility = Visibility.Hidden;
            videoHeightTextBox.LostFocus += TextBoxDoubleClicked;
            videoHeightTextBox.KeyDown += EnterPress;
            return videoHeightTextBox;
        }


        private TextBox CreateDescriptionTextBox(string filename, int mediaID)
        {
            TextBox descriptionTextBox = new TextBox();

            Label description = new Label();
            GetDescriptionLabelContentFromDatabase(description, mediaID);
            GetTextBoxTextFromLabelContent(descriptionTextBox, description);
            descriptionTextBox.FontSize = fontSize;
            descriptionTextBox.FontWeight = FontWeights.Bold;
            descriptionTextBox.Margin = new Thickness(12, fontSize * 1.5, 0, 0);
            brush.Color = Colors.Black;
            descriptionTextBox.Foreground = brush;
            descriptionTextBox.VerticalAlignment = VerticalAlignment.Bottom;
            descriptionTextBox.VerticalContentAlignment = VerticalAlignment.Top;
            descriptionTextBox.HorizontalContentAlignment = HorizontalAlignment.Left;
            descriptionTextBox.Visibility = Visibility.Hidden;
            descriptionTextBox.LostFocus += TextBoxDoubleClicked;
            descriptionTextBox.KeyDown += EnterPress;
            return descriptionTextBox;

        }
        #endregion
        #region GeneralVideoMedia Modifying Methods


        private void ChangeMediaPlayProperties(MediaElement media, GeneralVideoMedia generalVideoMedia)
        {
            media.Width = MEDIA_WIDTH;
            media.Height = MEDIA_HEIGHT;
            media.Position = new TimeSpan(0);
            media.Visibility = Visibility.Visible;
            media.LoadedBehavior = MediaState.Manual;
            media.Stop();
            media.MouseDown += (sender, e) => generalVideoMedia.play = !generalVideoMedia.play;
        }

        private void DeleteMedia(GeneralVideoMedia generalVideoMedia)
        {
            MessageBoxResult messageBoxResult = System.Windows.MessageBox.Show("Are you sure?", "Delete Confirmation", System.Windows.MessageBoxButton.YesNo);
            if (messageBoxResult == MessageBoxResult.Yes)
            {
                grid.Children.Remove(generalVideoMedia.Title);
                grid.Children.Remove(generalVideoMedia.TitleTextBox);

                grid.Children.Remove(generalVideoMedia.Description);
                grid.Children.Remove(generalVideoMedia.DescriptionTextBox);

                grid.Children.Remove(generalVideoMedia.MediaElement);
                grid.Children.Remove(generalVideoMedia.DeleteButton);
                grid.Children.Remove(generalVideoMedia.Image);
                grid.Children.Remove(generalVideoMedia.FileLocation);
                grid.RowDefinitions.RemoveAt(1);
                int scrollChange = 0;
                for (int i = (GeneralVideoMedias.Count - 1); i >= 0; i--)
                {
                    if (GeneralVideoMedias[i].InterfaceRowId > generalVideoMedia.InterfaceRowId)
                    {
                        GeneralVideoMedias[i].InterfaceRowId = (GeneralVideoMedias[i].InterfaceRowId - 1);

                        Grid.SetRow(GeneralVideoMedias[i].MediaElement, GeneralVideoMedias[i].InterfaceRowId + 1);
                        Grid.SetRow(GeneralVideoMedias[i].DeleteButton, GeneralVideoMedias[i].InterfaceRowId + 1);
                        Grid.SetRow(GeneralVideoMedias[i].Image, GeneralVideoMedias[i].InterfaceRowId + 1);
                        Grid.SetRow(GeneralVideoMedias[i].FileLocation, GeneralVideoMedias[i].InterfaceRowId + 1);
                        Grid.SetRow(GeneralVideoMedias[i].Title, GeneralVideoMedias[i].InterfaceRowId + 1);
                        Grid.SetRow(GeneralVideoMedias[i].TitleTextBox, GeneralVideoMedias[i].InterfaceRowId + 1);
                        Grid.SetRow(GeneralVideoMedias[i].Description, GeneralVideoMedias[i].InterfaceRowId + 1);
                        Grid.SetRow(GeneralVideoMedias[i].DescriptionTextBox, GeneralVideoMedias[i].InterfaceRowId + 1);

                    }
                    if (GeneralVideoMedias[i].InterfaceRowId + GeneralVideoMedias[i].ScrollPositionChange == 0)
                    {
                        scrollChange = GeneralVideoMedias[i].ScrollPositionChange;
                    }
                    GeneralVideoMedias[i].ScrollPositionChange = 0;

                }

                if (scrollChange > 0)
                    for (int i = 0; i < scrollChange; i++)
                    {
                        ScrollDown();
                    }
                else
                {
                    for (int i = 0; i < scrollChange; i++)
                    {
                        ScrollUp();
                    }
                }




                string sql = "DELETE FROM medias WHERE filelocation = '" + generalVideoMedia.GetFileLocation() + "'";
                SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
                var data = command.ExecuteReader();
                GeneralVideoMedias.Remove(generalVideoMedia);
                //OnSearch(this, null);

            }

        }


        public void WriteToPng(MediaElement element, string filename)
        {
            MediaPlayer media = new MediaPlayer { Volume = 0, ScrubbingEnabled = true };
            media.Open(element.Source);
            media.Pause();
            media.Position = TimeSpan.FromSeconds(1);
            //We need to give MediaPlayer some time to load. 
            //The efficiency of the MediaPlayer depends                 
            //upon the capabilities of the machine it is running on and 
            //would be different from time to time
            System.Threading.Thread.Sleep(2000);

            //MEDIA_WIDTH = thumbnail width, MEDIA_HEIGHT = thumbnail height and 96x96 = horizontal x vertical DPI
            //In a real application, you would not probably use hard coded values!
            RenderTargetBitmap rtb = new RenderTargetBitmap(MEDIA_WIDTH, MEDIA_HEIGHT, DpiX, DpiX, PixelFormats.Pbgra32);
            DrawingVisual dv = new DrawingVisual();
            using (DrawingContext dc = dv.RenderOpen())
            {
                dc.DrawVideo(media, new Rect(0, 0, MEDIA_WIDTH, MEDIA_HEIGHT));
            }
            rtb.Render(dv);
            Duration duration = media.NaturalDuration;
            int videoLength = 0;
            if (duration.HasTimeSpan)
            {
                videoLength = (int)duration.TimeSpan.TotalSeconds;
            }
            BitmapFrame frame = BitmapFrame.Create(rtb).GetCurrentValueAsFrozen() as BitmapFrame;
            BitmapEncoder encoder = new PngBitmapEncoder();
            encoder.Frames.Add(frame as BitmapFrame);
            using (var file = File.OpenWrite(System.IO.Path.GetFullPath("Media") + @"\" + filename))
            {
                encoder.Save(file);
            }
            media.Close();
        }
        #endregion
        #region Other GeneralVideoMedia Property-Using Methods
        private GeneralVideoMedia GetGeneralVideoMediaWithRowId(int rowID)
        {
            foreach (GeneralVideoMedia generalVideoMedia in GeneralVideoMedias)
            {
                if (generalVideoMedia.InterfaceRowId == rowID)
                {
                    return generalVideoMedia;
                }
            }
            return null;
        }

        private GeneralVideoMedia GetGeneralVideoMediaWithScrollViewID(int scrollviewID)
        {
            foreach (GeneralVideoMedia generalVideoMedia in GeneralVideoMedias)
            {
                if (generalVideoMedia.InterfaceRowId + generalVideoMedia.ScrollPositionChange == scrollviewID)
                {
                    return generalVideoMedia;
                }
            }
            return null;
        }
        private void GetTextBoxTextFromLabelContent(TextBox textBox, Label label)
        {
            textBox.Text = label.Content.ToString();
        }
        public void ReplaceLabelWithTextBox(Label label, TextBox textBox)
        {
            label.Visibility = Visibility.Hidden;
            textBox.Visibility = Visibility.Visible;
            textBox.Focus();
        }

        public void ReplaceTextBoxWithLabel(TextBox textBox, Label label)
        {
            label.Visibility = Visibility.Visible;
            textBox.Visibility = Visibility.Hidden;
        }
        #endregion
        #region GeneralVideoMedia Table Changing Methods
        private void ResetTables()
        {
            GeneralVideoMedias.Clear();
        }
        #endregion
        #region Video Playing Methods
        private bool VideoPlaying()
        {
            for (int i = 0; i < GeneralVideoMedias.Count; i++)
            {
                if (GeneralVideoMedias[i].videoDisplayed == true)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Change l'état de jouabilité d'un MediaElement lorsque ce MediaElement est cliqué
        /// </summary>
        /// <param name="sender">le MediaElement cliqué</param>
        /// <param name="e">L'argument de la souris qui a permis l'action</param>
        private void PlayVideo(object sender, MouseButtonEventArgs e)
        {
            deleteCol.MinWidth = 0;
            deleteCol.MaxWidth = 0;
            GeneralVideoMedia playedVideo = new GeneralVideoMedia();
            if (sender is Image)
            {
                for (int count = 0; count < GeneralVideoMedias.Count; count++)
                {
                    if (GeneralVideoMedias[count].Image == sender as Image)
                    {
                        playedVideo = GeneralVideoMedias[count];
                        GeneralVideoMedias[count].videoDisplayed = true;
                    }
                }
            }
            RemoveMediasFromList();

            int TitleRowHeight = 100;
            int Scale = 5;

            int EndMediaHeight;
            if (playedVideo.VideoHeight.Content == null || playedVideo.VideoHeight.Content.ToString() == "")
                EndMediaHeight = MEDIA_HEIGHT * Scale;
            else
                EndMediaHeight = Int32.Parse(playedVideo.VideoHeight.Content.ToString());
            int EndVideoWidth;
            if (playedVideo.VideoWidth.Content == null || playedVideo.VideoWidth.Content.ToString() == "")
                EndVideoWidth = MEDIA_WIDTH * Scale;
            else
                EndVideoWidth = Int32.Parse(playedVideo.VideoWidth.Content.ToString());

            int DescriptionRowHeight = MEDIA_HEIGHT;
            window.Width = EndVideoWidth + FIRST_COLUMN_WIDTH + GENERAL_MEDIA_OFFSET;
            window.Height = EndMediaHeight + (FIRST_ROW_HEIGHT) + TitleRowHeight + GENERAL_MEDIA_OFFSET + DescriptionRowHeight;

            RowDefinition TitleRow = new RowDefinition();
            TitleRow.MaxHeight = TitleRowHeight;
            TitleRow.MinHeight = TitleRowHeight;
            grid.RowDefinitions.Add(TitleRow);

            RowDefinition VideoRow = new RowDefinition();
            VideoRow.MaxHeight = EndMediaHeight;
            VideoRow.MinHeight = EndMediaHeight;
            grid.RowDefinitions.Add(VideoRow);


            RowDefinition LocationRow = new RowDefinition();
            LocationRow.MaxHeight = fontSize * 3.5;
            LocationRow.MinHeight = fontSize * 3.5;
            grid.RowDefinitions.Add(LocationRow);

            RowDefinition DescriptionRow = new RowDefinition();
            DescriptionRow.MaxHeight = DescriptionRowHeight;
            DescriptionRow.MinHeight = DescriptionRowHeight;
            grid.RowDefinitions.Add(DescriptionRow);


            grid.ColumnDefinitions[2].MinWidth = EndVideoWidth;
            grid.ColumnDefinitions[2].MaxWidth = EndVideoWidth;


            playedVideo.Title.HorizontalAlignment = HorizontalAlignment.Center;
            playedVideo.TitleTextBox.HorizontalAlignment = HorizontalAlignment.Center;
            playedVideo.Description.VerticalAlignment = VerticalAlignment.Top;
            playedVideo.DescriptionTextBox.VerticalAlignment = VerticalAlignment.Top;
            if (playedVideo.VideoWidth.Content == null || playedVideo.VideoWidth.Content.ToString() == "")
            {
                playedVideo.VideoWidth.Content = EndVideoWidth.ToString();
                playedVideo.VideoWidthTextBox.Text = EndVideoWidth.ToString();
            }
            if (playedVideo.VideoHeight.Content == null || playedVideo.VideoHeight.Content.ToString() == "")
            {
                playedVideo.VideoHeight.Content = EndMediaHeight.ToString();
                playedVideo.VideoHeightTextBox.Text = EndMediaHeight.ToString();
            }

            AddPlayedVideoToInterface(playedVideo);

            playedVideo.MediaElement.Width = EndVideoWidth;
            playedVideo.MediaElement.Height = EndMediaHeight;
            playedVideo.MediaElement.VerticalAlignment = VerticalAlignment.Top;
            playedVideo.MediaElement.HorizontalAlignment = HorizontalAlignment.Center;

            AddMediaElementToPlayedVideo(playedVideo.MediaElement);

            playedVideo.play = true;
            //}
        }

        private void RemoveMediasFromList()
        {
            foreach (GeneralVideoMedia media in GeneralVideoMedias)
            {
                grid.Children.Remove(media.MediaElement);
                grid.Children.Remove(media.Title);
                grid.Children.Remove(media.TitleTextBox);
                grid.Children.Remove(media.Description);
                grid.Children.Remove(media.DescriptionTextBox);
                grid.Children.Remove(media.VideoWidth);
                grid.Children.Remove(media.VideoWidthTextBox);
                grid.Children.Remove(media.VideoHeight);
                grid.Children.Remove(media.VideoHeightTextBox);
                grid.Children.Remove(media.Image);
                grid.Children.Remove(media.FileLocation);
            }
            while (grid.RowDefinitions.Count > 1)
                grid.RowDefinitions.Remove(grid.RowDefinitions[1]);
        }
        #region Add Played Video Information to Interface
        private void AddPlayedVideoToInterface(GeneralVideoMedia playedVideo)
        {
            AddTitleToPlayedVideo(playedVideo.Title);
            AddTitleTextBoxToPlayedVideo(playedVideo.TitleTextBox);
            AddDescriptionToPlayedVideo(playedVideo.Description);
            AddDescriptionTextBoxToPlayedVideo(playedVideo.DescriptionTextBox);
            AddVideoWidthToPlayedVideo(playedVideo.VideoWidth);
            AddVideoWidthTextBoxToPlayedVideo(playedVideo.VideoWidthTextBox);
            AddVideoHeightToPlayedVideo(playedVideo.VideoHeight);
            AddVideoHeightTextBoxToPlayedVideo(playedVideo.VideoHeightTextBox);
            AddFileLocationLabelToPlayedVideo(playedVideo.FileLocation);
        }

        private void AddVideoWidthToPlayedVideo(Label VideoWidth)
        {
            grid.Children.Add(VideoWidth);
            Grid.SetColumn(VideoWidth, 1);
            Grid.SetRow(VideoWidth, 2);
        }

        private void AddVideoWidthTextBoxToPlayedVideo(TextBox VideoWidthTextBox)
        {
            grid.Children.Add(VideoWidthTextBox);
            Grid.SetColumn(VideoWidthTextBox, 1);
            Grid.SetRow(VideoWidthTextBox, 2);
        }

        private void AddVideoHeightToPlayedVideo(Label VideoHeight)
        {
            grid.Children.Add(VideoHeight);
            Grid.SetColumn(VideoHeight, 1);
            Grid.SetRow(VideoHeight, 2);
        }

        private void AddVideoHeightTextBoxToPlayedVideo(TextBox VideoHeightTextBox)
        {
            grid.Children.Add(VideoHeightTextBox);
            Grid.SetColumn(VideoHeightTextBox, 1);
            Grid.SetRow(VideoHeightTextBox, 2);
        }

        private void AddTitleToPlayedVideo(Label Title)
        {
            grid.Children.Add(Title);
            Grid.SetColumn(Title, 2);
            Grid.SetColumnSpan(Title, 1);
            Grid.SetRow(Title, 1);
        }

        private void AddDescriptionToPlayedVideo(Label Description)
        {
            grid.Children.Add(Description);
            Grid.SetColumn(Description, 2);
            Grid.SetColumnSpan(Description, 1);
            Grid.SetRow(Description, 4);
        }

        private void AddTitleTextBoxToPlayedVideo(TextBox TitleTextBox)
        {
            grid.Children.Add(TitleTextBox);
            Grid.SetColumn(TitleTextBox, 2);
            Grid.SetColumnSpan(TitleTextBox, 1);
            Grid.SetRow(TitleTextBox, 1);
        }

        private void AddDescriptionTextBoxToPlayedVideo(TextBox DescriptionTextBox)
        {
            grid.Children.Add(DescriptionTextBox);
            Grid.SetColumn(DescriptionTextBox, 2);
            Grid.SetColumnSpan(DescriptionTextBox, 1);
            Grid.SetRow(DescriptionTextBox, 4);
        }

        private void AddMediaElementToPlayedVideo(MediaElement mediaElement)
        {
            grid.Children.Add(mediaElement);
            Grid.SetColumn(mediaElement, 2);
            Grid.SetColumnSpan(mediaElement, 1);
            Grid.SetRow(mediaElement, 2);
        }


        private void AddFileLocationLabelToPlayedVideo(TextBox fileLocation)
        {
            grid.Children.Add(fileLocation);
            Grid.SetColumn(fileLocation, 2);
            Grid.SetRow(fileLocation, 3);
        }
        #endregion
        #endregion
        #region Listing Methods
        #region Add Videos to List
        private void AddGeneralVideoMediaInformationToDisplay(GeneralVideoMedia generalVideoMedia)
        {
            AddImageToDisplay(generalVideoMedia.InterfaceRowId);
            AddTitleLabelToDisplay(generalVideoMedia.InterfaceRowId);
            AddDescriptionLabelToDisplay(generalVideoMedia.InterfaceRowId);
            AddTitleTextBoxToDisplay(generalVideoMedia.InterfaceRowId);
            AddDescriptionTextBoxToDisplay(generalVideoMedia.InterfaceRowId);
            AddFileLocationLabelToDisplay(generalVideoMedia.FileLocation, generalVideoMedia.InterfaceRowId);
            AddDeleteButtonToDisplay(generalVideoMedia.DeleteButton, generalVideoMedia.InterfaceRowId);
        }

        private void AddDeleteButtonToDisplay(Button deleteButton, int interfaceRowId)
        {
            grid.Children.Add(deleteButton);
            Grid.SetColumn(deleteButton, 3);
            Grid.SetRow(deleteButton, interfaceRowId + 1);
        }

        private void AddFileLocationLabelToDisplay(TextBox fileLocation, int rowID)
        {
            grid.Children.Add(fileLocation);
            Grid.SetColumn(fileLocation, 2);
            Grid.SetRow(fileLocation, rowID + 1);
            fileLocation.InvalidateMeasure();
            fileLocation.UpdateLayout();
        }

        private void AddImageToDisplay(int count)
        {
            grid.Children.Add(GetGeneralVideoMediaWithRowId(count).Image);
            Grid.SetColumn(GetGeneralVideoMediaWithRowId(count).Image, 1);
            Grid.SetRow(GetGeneralVideoMediaWithRowId(count).Image, count + 1);
            GetGeneralVideoMediaWithRowId(count).Image.InvalidateMeasure();
            GetGeneralVideoMediaWithRowId(count).Image.UpdateLayout();
            if (GetGeneralVideoMediaWithRowId(count).Image.Source == null) { Console.WriteLine("Invalid Source!"); }
            if (!GetGeneralVideoMediaWithRowId(count).Image.IsMeasureValid) { Console.WriteLine("Measure Invalid"); }
            if (!GetGeneralVideoMediaWithRowId(count).Image.IsArrangeValid) { Console.WriteLine("Arrangement Invalid"); }
            if (!GetGeneralVideoMediaWithRowId(count).Image.IsLoaded) { Console.WriteLine("Wrongly Loaded"); };
            if (!GetGeneralVideoMediaWithRowId(count).Image.IsVisible) { Console.WriteLine("Not visible!"); }
            if (!GetGeneralVideoMediaWithRowId(count).Image.IsEnabled) { Console.WriteLine("Not Enabled!"); }
        }

        private void AddTitleLabelToDisplay(int count)
        {
            grid.Children.Add(GetGeneralVideoMediaWithRowId(count).Title);
            Grid.SetColumn(GetGeneralVideoMediaWithRowId(count).Title, 2);
            Grid.SetRow(GetGeneralVideoMediaWithRowId(count).Title, count + 1);
        }

        private void AddDescriptionLabelToDisplay(int count)
        {
            grid.Children.Add(GetGeneralVideoMediaWithRowId(count).Description);
            Grid.SetColumn(GetGeneralVideoMediaWithRowId(count).Description, 2);
            Grid.SetRow(GetGeneralVideoMediaWithRowId(count).Description, count + 1);
        }

        private void AddTitleTextBoxToDisplay(int count)
        {
            grid.Children.Add(GetGeneralVideoMediaWithRowId(count).TitleTextBox);
            Grid.SetColumn(GetGeneralVideoMediaWithRowId(count).TitleTextBox, 2);
            Grid.SetRow(GetGeneralVideoMediaWithRowId(count).TitleTextBox, count + 1);
        }

        private void AddDescriptionTextBoxToDisplay(int count)
        {
            grid.Children.Add(GetGeneralVideoMediaWithRowId(count).DescriptionTextBox);
            Grid.SetColumn(GetGeneralVideoMediaWithRowId(count).DescriptionTextBox, 2);
            Grid.SetRow(GetGeneralVideoMediaWithRowId(count).DescriptionTextBox, count + 1);
        }
        #endregion

        private void RemovePlayingVideoForSearch()
        {
            bool TakeOffPlayingVideo = false;
            foreach (GeneralVideoMedia generalVideoMedia in GeneralVideoMedias)
            {
                if (generalVideoMedia.videoDisplayed)
                {
                    TakeOffPlayingVideo = true;
                }
            }
            if (TakeOffPlayingVideo)
            {
                grid.RowDefinitions.Remove(grid.RowDefinitions[3]);
                grid.RowDefinitions.Remove(grid.RowDefinitions[2]);
                grid.RowDefinitions.Remove(grid.RowDefinitions[1]);
                foreach (GeneralVideoMedia media in GeneralVideoMedias)
                {
                    media.play = false;
                    media.MediaElement.VerticalAlignment = VerticalAlignment.Center;
                    media.Title.HorizontalAlignment = HorizontalAlignment.Left;
                    media.TitleTextBox.HorizontalAlignment = HorizontalAlignment.Left;
                    media.Description.VerticalAlignment = VerticalAlignment.Bottom;
                    media.DescriptionTextBox.VerticalAlignment = VerticalAlignment.Bottom;
                    media.videoDisplayed = false;
                    grid.Children.Remove(media.MediaElement);
                }
            }
        }



        private void UpdateInterfaceWithVideosSimilarToSearchTermUsing(List<GeneralVideoMedia> foundGeneralVideoMedias)
        {
            for (int i = 0; i < foundGeneralVideoMedias.Count; i++)
            {
                if (foundGeneralVideoMedias[i] != null)
                {
                    RowDefinition videoInformationRow = new RowDefinition();
                    videoInformationRow.MaxHeight = MEDIA_HEIGHT + 24;
                    videoInformationRow.MinHeight = MEDIA_HEIGHT + 24;
                    grid.RowDefinitions.Add(videoInformationRow);

                    grid.Children.Add(foundGeneralVideoMedias[i].Image);
                    Grid.SetColumn(foundGeneralVideoMedias[i].Image, 1);
                    Grid.SetRow(foundGeneralVideoMedias[i].Image, i + 1);

                    grid.Children.Add(foundGeneralVideoMedias[i].Title);
                    Grid.SetColumn(foundGeneralVideoMedias[i].Title, 2);
                    Grid.SetRow(foundGeneralVideoMedias[i].Title, i + 1);


                    grid.Children.Add(foundGeneralVideoMedias[i].TitleTextBox);
                    Grid.SetColumn(foundGeneralVideoMedias[i].TitleTextBox, 2);
                    Grid.SetRow(foundGeneralVideoMedias[i].TitleTextBox, i + 1);


                    grid.Children.Add(foundGeneralVideoMedias[i].Description);
                    Grid.SetColumn(foundGeneralVideoMedias[i].Description, 2);
                    Grid.SetRow(foundGeneralVideoMedias[i].Description, i + 1);


                    grid.Children.Add(foundGeneralVideoMedias[i].DescriptionTextBox);
                    Grid.SetColumn(foundGeneralVideoMedias[i].DescriptionTextBox, 2);
                    Grid.SetRow(foundGeneralVideoMedias[i].DescriptionTextBox, i + 1);

                    grid.Children.Add(foundGeneralVideoMedias[i].FileLocation);
                    Grid.SetColumn(foundGeneralVideoMedias[i].FileLocation, 2);
                    Grid.SetRow(foundGeneralVideoMedias[i].FileLocation, i + 1);


                }
            }
        }
        private void ResetVideoDisplayUsing(List<GeneralVideoMedia> wantedGeneralVideoMedias)
        {
            RemovePlayingVideoForSearch();
            RemoveMediasFromList();

            grid.ColumnDefinitions[2].MinWidth = listDetailsWidth;
            grid.ColumnDefinitions[2].MaxWidth = listDetailsWidth;

            deleteCol.MinWidth = MEDIA_HEIGHT + 4;
            deleteCol.MaxWidth = MEDIA_HEIGHT + 4;

            window.Width = 1000.10;
            window.Height = 350;

            for (int i = 0; i < GeneralVideoMedias.Count(); i++)
            {
                GeneralVideoMedias[i].ScrollPositionChange = 0;
            }
            UpdateInterfaceWithVideosSimilarToSearchTermUsing(wantedGeneralVideoMedias);
        }
        #endregion
        #region Database-Using Methods
        private bool FileIsAlreadyInDatabase(string filelocation)
        {
            int numOfSameFiles = 0;
            string sql = "SELECT COUNT(*) from medias WHERE filelocation = '" + filelocation + "' ";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            using (var data = command.ExecuteReader())
            {
                while (data.Read())
                {
                    numOfSameFiles = Int32.Parse(data.GetValue(0).ToString());
                }
                data.Close();
            }
            return numOfSameFiles > 0;
        }
        private int GetMostRecentlyInsertedMediaID()
        {
            int mediaID = -1;
            string sql = "SELECT id_media FROM medias ORDER BY id_media DESC LIMIT 1";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            var data = command.ExecuteReader();
            mediaID = Int32.Parse(data.GetValue(0).ToString());
            return mediaID;
        }


        private void GetTitleLabelContentFromDatabase(Label Title, int fileID)
        {
            string sql = "SELECT title FROM medias WHERE id_media = '" + (fileID) + "';";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            using (var data = command.ExecuteReader())
            {
                while (data.Read())
                {
                    Title.Content = data.GetValue(0).ToString();
                }
                data.Close();
            }
        }

        private void GetDescriptionLabelContentFromDatabase(Label Description, int fileID)
        {
            string sql = "SELECT description FROM medias WHERE id_media = '" + (fileID) + "';";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            using (var data = command.ExecuteReader())
            {
                while (data.Read())
                {
                    Description.Content = data.GetValue(0).ToString();
                }
                data.Close();
            }
        }
        private List<GeneralVideoMedia> GetVideosSimilarToSearchTermIntoStringArray()
        {
            List<GeneralVideoMedia> tempGeneralVideoMedias = new List<GeneralVideoMedia>();
            string sql = "SELECT id_media from medias WHERE title LIKE '%" + searchBox.Text.Replace("'", "''") + "%'";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            using (var data = command.ExecuteReader())
            {
                while (data.Read())
                {
                    int mediaID = Int32.Parse(data.GetValue(0).ToString());
                    for (int i = 0; i < GeneralVideoMedias.Count; i++)
                    {
                        if (GeneralVideoMedias[i].MediaId == mediaID)
                        {
                            tempGeneralVideoMedias.Add(GeneralVideoMedias[i]);
                        }
                    }
                }
                data.Close();
            }
            return tempGeneralVideoMedias;
        }


        public void UpdateTitleOfSpecificMedia(int mediaId, string title)
        {
            string sqlupdate = "UPDATE medias " +
                "SET title = '" + title.Replace("'", "''") + "'" +
                " WHERE id_media = " + mediaId + " ";
            SQLiteCommand commandupdate = new SQLiteCommand(sqlupdate, m_dbConnection);
            commandupdate.ExecuteNonQuery();
        }

        public void UpdateDescripitionOfSpecificMedia(int mediaId, string description)
        {
            string sqlupdate = "UPDATE medias " +
                "SET description = '" + description.Replace("'", "''") + "'" +
                " WHERE id_media = " + mediaId + " ";
            SQLiteCommand commandupdate = new SQLiteCommand(sqlupdate, m_dbConnection);
            commandupdate.ExecuteNonQuery();
        }

        public string GetTitleOfMedia(int mediaId)
        {
            string title = "";
            string sql = "SELECT title from medias WHERE id_media = " + mediaId + " ";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            using (var data = command.ExecuteReader())
            {
                while (data.Read())
                {
                    title = data.GetValue(0).ToString();
                }
                data.Close();
            }
            return title;
        }

        public string GetDescriptionOfMedia(int mediaId)
        {
            string description = "";
            string sql = "SELECT description from medias WHERE id_media = " + mediaId + " ";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            using (var data = command.ExecuteReader())
            {
                while (data.Read())
                {
                    description = data.GetValue(0).ToString();
                }
                data.Close();
            }
            return description;
        }
        private void AddMediaToTable(string fileName, string imgPath, string fileLocation)
        {
            string sql = "INSERT INTO medias(id_media, filename, img_path, title, description, filelocation) VALUES(NULL, '" + fileName + "', '" + imgPath + "', '" + fileName.Substring(0, fileName.Length) + "', '" + "Default Description" + "', '" + fileLocation + "');";
            SQLiteCommand command = new SQLiteCommand(sql, m_dbConnection);
            int data = command.ExecuteNonQuery();

        }
        #endregion
    }
}