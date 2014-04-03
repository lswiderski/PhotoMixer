using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Windows.Storage.Streams;
using Photo_Mixer.Models;
using Microsoft.Phone.Info;
using Microsoft.Xna.Framework.Media;
using Nokia.Graphics.Imaging;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using Microsoft.Phone.Tasks;
namespace Photo_Mixer
{
    public partial class MixedPage : PhoneApplicationPage
    {
        private ApplicationBarIconButton _saveButton;

        private ApplicationBarMenuItem _feedbackItem;
        private ApplicationBarMenuItem _aboutMenuItem;

        private bool _processing;
        private bool _processingPending;
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

        public MixedPage()
        {
            InitializeComponent();

            CreateButtons();

        }
       
        private void CreateButtons()
        {
            _saveButton = new ApplicationBarIconButton
            {
                Text = "save",
                IconUri = new Uri("Assets/Icons/Save.png", UriKind.Relative),
            };


            _feedbackItem = new ApplicationBarMenuItem
            {
                Text = "feedback"
            };

            _aboutMenuItem = new ApplicationBarMenuItem
            {
                Text = "about"
            };

            _saveButton.Click += SaveButton_Click;
            _feedbackItem.Click += feedbackItem_Click;
            _aboutMenuItem.Click += AboutMenuItem_Click;

            ApplicationBar.Buttons.Add(_saveButton);
            ApplicationBar.MenuItems.Add(_feedbackItem);
            ApplicationBar.MenuItems.Add(_aboutMenuItem);


        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
           AttemptSave();
        }
        private void AboutMenuItem_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/About.xaml", UriKind.Relative));
        }


        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (Model.OriginalImage == null || Model.AnnotationsBitmap == null)
            {
                NavigationService.GoBack();
            }
            else
            {

               // AttemptUpdatePreviewAsync();
                AttemptUpdatePreviewAsync2();
            }
        }

        protected override void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            if (Processing && e.IsCancelable)
            {
                e.Cancel = true;
            }

            base.OnNavigatingFrom(e);
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            PreviewImage.Source = null;

        }
        private void feedbackItem_Click(object sender, EventArgs e)
        {
            EmailComposeTask emailComposeTask = new EmailComposeTask();

            emailComposeTask.Subject = "Photo Mixer feedback";
            emailComposeTask.Body = "Feedback";
            emailComposeTask.To = "l.swiderski@outlook.com";

            emailComposeTask.Show();
        }
        private async void AttemptUpdatePreviewAsync2()
        {
            if (!Processing)
            {
                Processing = true;

                Model.OriginalImage.Position = 0;


                using (var source = new StreamImageSource(Model.OriginalImage))
                using (var segmenter = new InteractiveForegroundSegmenter(source))
                using (var annotationsSource = new BitmapImageSource(Model.AnnotationsBitmap))
                {
                    segmenter.Quality = 0.5;
                    segmenter.Source = source;
                    segmenter.AnnotationsSource = annotationsSource;

                    var foregroundColor = Model.ForegroundBrush.Color;
                    var backgroundColor = Model.BackgroundBrush.Color;

                    segmenter.ForegroundColor = Windows.UI.Color.FromArgb(foregroundColor.A, foregroundColor.R, foregroundColor.G, foregroundColor.B);
                    segmenter.BackgroundColor = Windows.UI.Color.FromArgb(backgroundColor.A, backgroundColor.R, backgroundColor.G, backgroundColor.B);

                   
                    do
                    {
                        _processingPending = false;




                        var previewBitmap = new WriteableBitmap((int)Model.AnnotationsBitmap.Dimensions.Width, (int)Model.AnnotationsBitmap.Dimensions.Height);


                        using (var backgroundSource = new StreamImageSource(Model.OriginalImage2))

                        using (var filterEffect = new FilterEffect(backgroundSource))

                        using (var blendFilter = new BlendFilter(source))


                        using (var renderer = new WriteableBitmapRenderer(filterEffect, previewBitmap))
                        

                        {
                            
                            blendFilter.BlendFunction = BlendFunction.Normal;
                            blendFilter.MaskSource = segmenter;
                            filterEffect.Filters = new IFilter[] { blendFilter };
                            try
                            {
                                await renderer.RenderAsync();
                            }
                            catch
                            {
                            }
                            
                            var wb = previewBitmap;
                            var fileStream = new MemoryStream();
                            wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 100, 100);
                            fileStream.Seek(0, SeekOrigin.Begin);

                            var effect = new LensBlurEffect(new StreamImageSource(fileStream), new LensBlurPredefinedKernel(LensBlurPredefinedKernelShape.Circle, 10));
                            var renderer2 = new WriteableBitmapRenderer(effect, previewBitmap);
                            effect.KernelMap = segmenter;
                            try
                            {
                                await renderer2.RenderAsync();
                            }
                            catch
                            {
                            }

                            PreviewImage.Source = previewBitmap;


                            wb = previewBitmap;
                            fileStream = new MemoryStream();
                            wb.SaveJpeg(fileStream, wb.PixelWidth, wb.PixelHeight, 100, 100);
                            fileStream.Seek(0, SeekOrigin.Begin);

                           // var m = new MediaLibrary();
                           // m.SavePictureToCameraRoll("test", fileStream);
                            Model.MixedStream = fileStream;
                           // Model.MixedStream = ConvertToStream(previewBitmap);
                            previewBitmap.Invalidate();

                            

                        }

                    }
                    while (_processingPending);
                }


                Processing = false;
            }
            else
            {
               
                _processingPending = true;
            }
        }
        public static Stream ConvertToStream(WriteableBitmap writeableBitmap)
        {
            using (var ms = new MemoryStream())
            {
                writeableBitmap.SaveJpeg(ms, writeableBitmap.PixelWidth, writeableBitmap.PixelHeight, 0, 100);

                return ms;
            }
        }
        private async void AttemptSave()
        {
            if (!Processing)
            {
                Processing = true;

                GC.Collect();


                using (var library = new MediaLibrary())
                {

                    library.SavePicture("photo_" + DateTime.Now.Ticks, Model.MixedStream);

                    Model.Saved = true;
                    MessageBox.Show("the image has been saved");

                }

                Processing = false;
            }
        }

       
    }
}