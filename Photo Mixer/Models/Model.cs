
using Nokia.Graphics.Imaging;
using System.IO;
using System.Windows.Media;

namespace Photo_Mixer.Models
{
    public class Model
    {
        // TODO: Tombstoning support

        private static Stream _originalImageStream;
        private static Stream _originalImage2Stream;
        private static Stream _mixedStream;
        private static Stream _mixedStream2;
        private static Bitmap _annotationsBitmap;
        private static Bitmap _annotationsBitmap2;

        public static readonly SolidColorBrush ForegroundBrush = new SolidColorBrush(Colors.Red);
        public static readonly SolidColorBrush BackgroundBrush = new SolidColorBrush(Colors.Blue);

        public static Stream OriginalImage
        {
            get
            {
                return _originalImageStream;
            }

            set
            {
                if (_originalImageStream != value)
                {
                    if (_originalImageStream != null)
                    {
                        _originalImageStream.Close();
                    }

                    _originalImageStream = value;
                }
            }
        }
        public static Stream OriginalImage2
        {
            get
            {
                return _originalImage2Stream;
            }

            set
            {
                if (_originalImage2Stream != value)
                {
                    if (_originalImage2Stream != null)
                    {
                        _originalImage2Stream.Close();
                    }

                    _originalImage2Stream = value;
                }
            }
        }

        public static Bitmap AnnotationsBitmap
        {
            get
            {
                return _annotationsBitmap;
            }

            set
            {
                if (_annotationsBitmap != value)
                {
                    if (_annotationsBitmap != null)
                    {
                        _annotationsBitmap.Dispose();
                    }

                    _annotationsBitmap = value;
                }
            }
        }
        public static Bitmap AnnotationsBitmap2
        {
            get
            {
                return _annotationsBitmap2;
            }

            set
            {
                if (_annotationsBitmap2 != value)
                {
                    if (_annotationsBitmap2 != null)
                    {
                        _annotationsBitmap2.Dispose();
                    }

                    _annotationsBitmap2 = value;
                }
            }
        }
        public static Stream MixedStream
        {
            get
            {
                return _mixedStream;
            }

            set
            {
                if (_mixedStream != value)
                {
                    if (_mixedStream != null)
                    {
                        _mixedStream.Dispose();
                    }

                    _mixedStream = value;
                }
            }
        }
        public static Stream MixedStream2
        {
            get
            {
                return _mixedStream2;
            }

            set
            {
                if (_mixedStream2 != value)
                {
                    if (_mixedStream2 != null)
                    {
                        _mixedStream2.Dispose();
                    }

                    _mixedStream2 = value;
                }
            }
        }


        public static LensBlurPredefinedKernelShape KernelShape { get; set; }
        public static double KernelSize { get; set; }
        public static bool Saved { get; set; }
    }
}
