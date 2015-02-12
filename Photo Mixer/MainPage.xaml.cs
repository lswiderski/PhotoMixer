using System.Linq;
using Photo_Mixer.Models;
using Photo_Mixer.Resources;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Tasks;
using Nokia.Graphics.Imaging;
using Nokia.InteropServices.WindowsRuntime;
using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;


namespace Photo_Mixer
{
    public partial class MainPage : PhoneApplicationPage
    {
        private PhotoChooserTask _task = new PhotoChooserTask();
        private PhotoChooserTask _task2 = new PhotoChooserTask();
        private SolidColorBrush _brush;
        private Polyline _polyline;
        private bool _processing;
        private bool _processingPending;
        private ApplicationBarIconButton _openButton;
        private ApplicationBarIconButton _open2Button;
        private ApplicationBarIconButton _undoButton;
        private ApplicationBarMenuItem _resetButton;
        private ApplicationBarIconButton _acceptButton;
        private ApplicationBarMenuItem _feedbackItem;
        private ApplicationBarMenuItem _aboutMenuItem;
        private PhotoResult _photoResult;
        private PhotoResult _photoResult2;
        private bool _manipulating;
        private bool selectedSecond = false;
        private bool selectedFirst = false;

        private bool Processing
        {
            get
            {
                return _processing;
            }

            set
            {
                if (_processing != value)
                {
                    _processing = value;

                    ProgressBar.IsIndeterminate = _processing;
                    ProgressBar.Visibility = _processing ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private bool AnnotationsDrawn
        {
            get
            {
                return AnnotationsCanvas.Children.Count > 0;
            }
        }

        private bool ForegroundAnnotationsDrawn
        {
            get
            {
                return AnnotationsCanvas.Children.Cast<Polyline>().Any(p => p.Stroke == Model.ForegroundBrush);
            }
        }

        private bool BackgroundAnnotationsDrawn
        {
            get
            {
                return AnnotationsCanvas.Children.Cast<Polyline>().Any(p => p.Stroke == Model.BackgroundBrush);
            }
        }
        private void feedbackItem_Click(object sender, EventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "Photo Mixer feedback";
            emailComposeTask.Body = "Feedback";
            emailComposeTask.To = "neufrin.feedback@outlook.com";

            emailComposeTask.Show();
        }
        // Constructor
        public MainPage()
        {
            InitializeComponent();

            ShellTile oTile = ShellTile.ActiveTiles.FirstOrDefault();


            if (oTile != null)
            {
                FlipTileData oFliptile = new FlipTileData();
                oFliptile.Title = "Photo Mixer";
                oFliptile.Count = 0;
                oFliptile.BackTitle = "Photo Mixer";

                oFliptile.BackContent = "MIX";
                oFliptile.WideBackContent = "Mix your Photo";

                oFliptile.SmallBackgroundImage = new Uri("Assets/Tiles/PM-Small.png", UriKind.Relative);
                oFliptile.BackgroundImage = new Uri("Assets/Tiles/PM-Medium.png", UriKind.Relative);
                oFliptile.WideBackgroundImage = new Uri("Assets/Tiles/PM-Wide.png", UriKind.Relative);

                oFliptile.BackBackgroundImage = new Uri("/Assets/Tiles/PM2-Medium.png", UriKind.Relative);
                oFliptile.WideBackBackgroundImage = new Uri("/Assets/Tiles/PM2-Wide.png", UriKind.Relative);
                oTile.Update(oFliptile);
            }

            CreateButtons();
            selectedSecond = false;
            selectedFirst = false;
            _task.ShowCamera = true;
            _task.Completed += PhotoChooserTask_Completed;
            _task2.ShowCamera = true;
            _task2.Completed += PhotoChooserTask_Completed2;

            OriginalImage.LayoutUpdated += OriginalImage_LayoutUpdated;
            GoogleAnalytics.EasyTracker.GetTracker().SendView("MainPage");
        }

        private void CreateButtons()
        {
            _openButton = new ApplicationBarIconButton
            {
                Text = "open 1st ",
                IconUri = new Uri("Assets/Icons/Folder.png", UriKind.Relative),
            };
            _open2Button = new ApplicationBarIconButton
            {
                Text = "open 2nd",
                IconUri = new Uri("Assets/Icons/Folder.png", UriKind.Relative),
            };

            _undoButton = new ApplicationBarIconButton
            {
                Text = "undo",
                IconUri = new Uri("Assets/Icons/Undo.png", UriKind.Relative),
            };

            _resetButton = new ApplicationBarMenuItem
            {
                Text = "reset",
            };

            _acceptButton = new ApplicationBarIconButton
            {
                Text = "accept",
                IconUri = new Uri("Assets/Icons/Check.png", UriKind.Relative),
            };

            _feedbackItem = new ApplicationBarMenuItem
            {
                Text = "feedback"
            };

            _aboutMenuItem = new ApplicationBarMenuItem
            {
                Text = "about"
            };

            _openButton.Click += OpenButton_Click;
            _open2Button.Click += Open2Button_Click;
            _undoButton.Click += UndoButton_Click;
            _resetButton.Click += ResetButton_Click;
            _acceptButton.Click += AcceptButton_Click;
            _feedbackItem.Click += feedbackItem_Click;
            _aboutMenuItem.Click += AboutMenuItem_Click;

            ApplicationBar.Buttons.Add(_openButton);
            ApplicationBar.Buttons.Add(_open2Button);
            ApplicationBar.Buttons.Add(_undoButton);
            
            ApplicationBar.Buttons.Add(_acceptButton);
            ApplicationBar.MenuItems.Add(_resetButton);
            ApplicationBar.MenuItems.Add(_feedbackItem);
            ApplicationBar.MenuItems.Add(_aboutMenuItem);
        }


        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }

        private void OriginalImage_LayoutUpdated(object sender, EventArgs e)
        {
            MaskImage.Width = OriginalImage.ActualWidth;
            MaskImage.Height = OriginalImage.ActualHeight;

            AnnotationsCanvas.Width = OriginalImage.ActualWidth;
            AnnotationsCanvas.Height = OriginalImage.ActualHeight;
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (_photoResult != null )
            {
                Model.OriginalImage = _photoResult.ChosenPhoto;

                _photoResult = null;

                AnnotationsCanvas.Children.Clear();
                selectedFirst = true;
                Model.Saved = false;
            }
            if (_photoResult2 != null)
            {
                 Model.OriginalImage2 = _photoResult2.ChosenPhoto;
                 selectedFirst = true;
                _photoResult2 = null;   
            }

            if (Model.OriginalImage2 != null)
            {
                //selectedSecond = true;
            }
            if (Model.OriginalImage != null)
            {
                if (_brush == null)
                {
                    _brush = Model.ForegroundBrush;
                }

                var originalBitmap = new BitmapImage
                {
                    DecodePixelWidth = (int)(480.0 * Application.Current.Host.Content.ScaleFactor / 100.0)
                };

                Model.OriginalImage.Position = 0;

                originalBitmap.SetSource(Model.OriginalImage);

                OriginalImage.Source = originalBitmap;

                AttemptUpdatePreviewAsync();
            }
            else
            {
                _brush = null;
            }

            AdaptButtonsToState();

            ManipulationArea.ManipulationStarted += AnnotationsCanvas_ManipulationStarted;
            ManipulationArea.ManipulationDelta += AnnotationsCanvas_ManipulationDelta;
            ManipulationArea.ManipulationCompleted += AnnotationsCanvas_ManipulationCompleted;
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (Processing && e.IsCancelable)
            {
                e.Cancel = true;
            }
            else
            {
                ManipulationArea.ManipulationStarted -= AnnotationsCanvas_ManipulationStarted;
                ManipulationArea.ManipulationDelta -= AnnotationsCanvas_ManipulationDelta;
                ManipulationArea.ManipulationCompleted -= AnnotationsCanvas_ManipulationCompleted;
            }

            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            MaskImage.Source = null;
            OriginalImage.Source = null;
            selectedSecond = false;
            selectedFirst = false;
        }

        private void AdaptButtonsToState()
        {
            _undoButton.IsEnabled = AnnotationsDrawn;
            _resetButton.IsEnabled = AnnotationsDrawn;
            _acceptButton.IsEnabled = ForegroundAnnotationsDrawn && BackgroundAnnotationsDrawn && selectedSecond && selectedFirst;

            if (Model.OriginalImage != null)
            {
                ForegroundButton.IsEnabled = true;
                BackgroundButton.IsEnabled = true;

                ForegroundButton.Background = _brush == Model.ForegroundBrush ? Model.ForegroundBrush : null;
                BackgroundButton.Background = _brush == Model.BackgroundBrush ? Model.BackgroundBrush : null;

                GuideTextBlock.Visibility = Visibility.Collapsed;
            }
            else
            {
                ForegroundButton.IsEnabled = false;
                BackgroundButton.IsEnabled = false;

                GuideTextBlock.Visibility = Visibility.Visible;
            }
        }

        private Point NearestPointInElement(double x, double y, FrameworkElement element)
        {
            var clampedX = Math.Min(Math.Max(0, x), element.ActualWidth);
            var clampedY = Math.Min(Math.Max(0, y), element.ActualHeight);

            return new Point(clampedX, clampedY);
        }

        private void AnnotationsCanvas_ManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            _manipulating = true;

            _polyline = new Polyline
            {
                Stroke = _brush,
                StrokeThickness = 6
            };

            var manipulationAreaDeltaX = ManipulationArea.Margin.Left;
            var manipulationAreaDeltaY = ManipulationArea.Margin.Top;

            var point = NearestPointInElement(e.ManipulationOrigin.X + manipulationAreaDeltaX, e.ManipulationOrigin.Y + manipulationAreaDeltaY, AnnotationsCanvas);

            _polyline.Points.Add(point);

            CurrentAnnotationCanvas.Children.Add(_polyline);
        }

        private void AnnotationsCanvas_ManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            var manipulationAreaDeltaX = ManipulationArea.Margin.Left;
            var manipulationAreaDeltaY = ManipulationArea.Margin.Top;

            var x = e.ManipulationOrigin.X - e.DeltaManipulation.Translation.X + manipulationAreaDeltaX;
            var y = e.ManipulationOrigin.Y - e.DeltaManipulation.Translation.Y + manipulationAreaDeltaY;

            var point = NearestPointInElement(x, y, AnnotationsCanvas);

            _polyline.Points.Add(point);
        }

        private void AnnotationsCanvas_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            if (_polyline.Points.Count < 2)
            {
                CurrentAnnotationCanvas.Children.Clear();

                _manipulating = false;
            }
            else
            {
                CurrentAnnotationCanvas.Children.RemoveAt(CurrentAnnotationCanvas.Children.Count - 1);

                AnnotationsCanvas.Children.Add(_polyline);

                Model.Saved = false;

                AdaptButtonsToState();

                _manipulating = false;

                AttemptUpdatePreviewAsync();
            }

            _polyline = null;
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            _task.Show();
        }

        private void Open2Button_Click(object sender, EventArgs e)
        {
            _task2.Show();
        }


        private void PhotoChooserTask_Completed(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                _photoResult = e;
                selectedFirst = true;
            }
        }
        private void PhotoChooserTask_Completed2(object sender, PhotoResult e)
        {
            if (e.TaskResult == TaskResult.OK)
            {
                _photoResult2 = e;
                selectedSecond = true;
            }
        }

        private void UndoButton_Click(object sender, EventArgs e)
        {
            AnnotationsCanvas.Children.RemoveAt(AnnotationsCanvas.Children.Count - 1);

            Model.Saved = false;

            AdaptButtonsToState();

            AttemptUpdatePreviewAsync();
        }

        private void ResetButton_Click(object sender, EventArgs e)
        {
            AnnotationsCanvas.Children.Clear();

            Model.Saved = false;

            AdaptButtonsToState();

            AttemptUpdatePreviewAsync();
        }

        private void AcceptButton_Click(object sender, EventArgs e)
        {
            if (!Processing && !_manipulating && Model.AnnotationsBitmap != null)
            {
                NavigationService.Navigate(new Uri("/MixedPage.xaml", UriKind.Relative));
            }
        }

        private async void AttemptUpdatePreviewAsync()
        {
            if (!Processing)
            {
                Processing = true;

                do
                {
                    _processingPending = false;

                    if (Model.OriginalImage != null && ForegroundAnnotationsDrawn && BackgroundAnnotationsDrawn)
                    {
                        Model.OriginalImage.Position = 0;

                        var maskBitmap = new WriteableBitmap((int)AnnotationsCanvas.ActualWidth, (int)AnnotationsCanvas.ActualHeight);
                        var annotationsBitmap = new WriteableBitmap((int)AnnotationsCanvas.ActualWidth, (int)AnnotationsCanvas.ActualHeight);

                        annotationsBitmap.Render(AnnotationsCanvas, new ScaleTransform
                        {
                            ScaleX = 1,
                            ScaleY = 1
                        });

                        annotationsBitmap.Invalidate();

                        Model.OriginalImage.Position = 0;

                        using (var source = new StreamImageSource(Model.OriginalImage))
                        using (var segmenter = new InteractiveForegroundSegmenter(source))
                        using (var renderer = new WriteableBitmapRenderer(segmenter, maskBitmap))
                        using (var annotationsSource = new BitmapImageSource(annotationsBitmap.AsBitmap()))
                        {
                            var foregroundColor = Model.ForegroundBrush.Color;
                            var backgroundColor = Model.BackgroundBrush.Color;

                            segmenter.ForegroundColor = Windows.UI.Color.FromArgb(foregroundColor.A, foregroundColor.R, foregroundColor.G, foregroundColor.B);
                            segmenter.BackgroundColor = Windows.UI.Color.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);
                            segmenter.Quality = 0.5;
                            segmenter.AnnotationsSource = annotationsSource;

                            await renderer.RenderAsync();

                            MaskImage.Source = maskBitmap;

                            maskBitmap.Invalidate();

                            Model.AnnotationsBitmap = (Bitmap)annotationsBitmap.AsBitmap();
                        }
                    }
                    else
                    {
                        MaskImage.Source = null;
                    }
                }
                while (_processingPending && !_manipulating);

                Processing = false;
            }
            else
            {
                _processingPending = true;
            }
        }

        private void ForegroundButton_Click(object sender, RoutedEventArgs e)
        {
            _brush = Model.ForegroundBrush;

            AdaptButtonsToState();
        }

        private void BackgroundButton_Click(object sender, RoutedEventArgs e)
        {
            _brush = Model.BackgroundBrush;

            AdaptButtonsToState();
        }
       
    }
}