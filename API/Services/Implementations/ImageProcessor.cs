using API.Services.Interfaces;
using System.Drawing.Imaging;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace API.Services.Implementations
{
    public class ImageProcessor : IImageProcessor
    {
        public IEnumerable<IEnumerable<Bitmap?>> GetNumberCards(Bitmap image, int resizedSize)
        {
            var processedImage = PreprocessInputImage(image);
            processedImage.Save(@"C:\Users\kvaza\OneDrive\Рабочий стол\Практики\ДИПЛОМ\processed.jpg");

            var rows = GetBoundingRectangles(processedImage);

            var pictures = ExtractDigits(processedImage, rows, resizedSize);
            int o = 0;
            foreach ( var picture in pictures )
            {
                foreach ( var card in picture )
                {
                    card?.Save($@"C:\Users\kvaza\OneDrive\Рабочий стол\Практики\ДИПЛОМ\o-{++o}.jpg");
                }
            }

            using (Graphics graphics = Graphics.FromImage(image))
            {
                foreach (var orderedRectancles in rows)
                {
                    foreach (var rectangle in orderedRectancles)
                    {
                        graphics.DrawRectangle(new Pen(Color.Red, 2), rectangle);
                    }
                }
            }

            return pictures;
        }

        private Bitmap PreprocessInputImage(Bitmap image)
        {
            Bitmap proccessedImage = ApplySharpenFilter(image);

            proccessedImage = ApplyGaussianBlur(proccessedImage, 1.9);

            ApplyBrightnessThreshhold(proccessedImage);
            using (Graphics graphics = Graphics.FromImage(proccessedImage))
            {
                graphics.DrawRectangle(new Pen(Color.White, 7), 0, 0, proccessedImage.Width, proccessedImage.Height);
            }

            return proccessedImage;
        }

        private Bitmap ApplySharpenFilter(Bitmap image)
        {
            Bitmap sharpenedImage = new Bitmap(image.Width, image.Height);

            float[,] coreMatrix = new float[,]
            {
                { -1, -1, -1 },
                { -1, 11, -1 },
                { -1, -1, -1 }
            };

            int filterSize = 3;

            for (int x = 1; x < image.Width - 1; x++)
            {
                for (int y = 1; y < image.Height - 1; y++)
                {
                    float red = 0, green = 0, blue = 0;

                    for (int i = 0; i < filterSize; i++)
                    {
                        for (int j = 0; j < filterSize; j++)
                        {
                            Color pixel = image.GetPixel(x - 1 + i, y - 1 + j);
                            red += pixel.R * coreMatrix[i, j];
                            green += pixel.G * coreMatrix[i, j];
                            blue += pixel.B * coreMatrix[i, j];
                        }
                    }

                    red = Math.Min(Math.Max(red, 0), 255);
                    green = Math.Min(Math.Max(green, 0), 255);
                    blue = Math.Min(Math.Max(blue, 0), 255);

                    sharpenedImage.SetPixel(x, y, Color.FromArgb((int)red, (int)green, (int)blue));
                }
            }

            return sharpenedImage;
        }

        private Bitmap ApplyGaussianBlur(Bitmap image, double sigma)
        {
            int size = (int)Math.Ceiling(sigma) * 2 + 1;
            double[,] kernel = GenerateGaussianKernel(size, sigma);

            Bitmap result = new Bitmap(image.Width, image.Height);

            int border = size / 2;

            BitmapData srcData = image.LockBits(new Rectangle(0, 0, image.Width, image.Height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            BitmapData destData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb);

            unsafe
            {
                for (int y = border; y < image.Height - border; y++)
                {
                    byte* srcRow = (byte*)srcData.Scan0 + (y - border) * srcData.Stride;
                    byte* destRow = (byte*)destData.Scan0 + y * destData.Stride;

                    for (int x = border; x < image.Width - border; x++)
                    {
                        double sumR = 0, sumG = 0, sumB = 0;

                        for (int i = 0; i < size; i++)
                        {
                            byte* srcPixel = srcRow + (x - border) * 4 + i * srcData.Stride;
                            double weight = kernel[i, 0];

                            sumR += srcPixel[2] * weight;
                            sumG += srcPixel[1] * weight;
                            sumB += srcPixel[0] * weight;
                        }

                        destRow[x * 4] = (byte)sumB;
                        destRow[x * 4 + 1] = (byte)sumG;
                        destRow[x * 4 + 2] = (byte)sumR;
                        destRow[x * 4 + 3] = 255;
                    }
                }
            }

            image.UnlockBits(srcData);
            result.UnlockBits(destData);

            return result;
        }

        private double[,] GenerateGaussianKernel(int size, double sigma)
        {
            double[,] kernel = new double[size, 1];
            double sum = 0;

            int mid = size / 2;

            for (int i = 0; i < size; i++)
            {
                kernel[i, 0] = Math.Exp(-(i - mid) * (i - mid) / (2 * sigma * sigma));
                sum += kernel[i, 0];
            }

            for (int i = 0; i < size; i++)
            {
                kernel[i, 0] /= sum;
            }

            return kernel;
        }

        private void ApplyBrightnessThreshhold(Bitmap image)
        {
            for (int x = 0; x < image.Width; x++)
            {
                for (int y = 0; y < image.Height; y++)
                {
                    image.SetPixel(x, y, image.GetPixel(x, y).GetBrightness() > 0.9 ? Color.White : Color.Black);
                }
            }
        }

        private IEnumerable<IEnumerable<Rectangle>> GetBoundingRectangles(Bitmap image)
        {
            var boundingRectangles = FindBoundingBoxes(image);
            return OrderRectancles(boundingRectangles);
        }

        private List<Rectangle> FindBoundingBoxes(Bitmap image)
        {
            List<Rectangle> boundingRectangles = new List<Rectangle>();

            int width = image.Width;
            int height = image.Height;

            bool[,] visited = new bool[width, height];

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    if (IsMarkedPixel(image.GetPixel(x, y)) && !visited[x, y])
                    {
                        Rectangle rect = FindBoundingBox(image, x, y, visited);
                        if (rect.Height > 5 && rect.Width > 3)
                        {
                            boundingRectangles.Add(rect);
                        }
                    }
                }
            }

            return boundingRectangles;
        }

        private Rectangle FindBoundingBox(Bitmap image, int startX, int startY, bool[,] visited)
        {
            int left = int.MaxValue;
            int right = int.MinValue;
            int top = int.MaxValue;
            int bottom = int.MinValue;

            Queue<Point> queue = new Queue<Point>();
            queue.Enqueue(new Point(startX, startY));
            visited[startX, startY] = true;

            while (queue.Count > 0)
            {
                Point current = queue.Dequeue();

                if (current.X < left) left = current.X;
                if (current.X > right) right = current.X;
                if (current.Y < top) top = current.Y;
                if (current.Y > bottom) bottom = current.Y;

                var xCeiling = 1;
                var yCeiling = 1;

                if (right - left + 1 >= 3 && bottom - top + 1 >= 3)
                {
                    xCeiling = 5;
                    yCeiling = 5;
                }

                for (int dx = -xCeiling; dx <= xCeiling; dx++)
                {
                    for (int dy = -yCeiling; dy <= yCeiling; dy++)
                    {
                        int x = current.X + dx;
                        int y = current.Y + dy;

                        if (x >= 0 && x < image.Width && y >= 0 && y < image.Height &&
                            IsMarkedPixel(image.GetPixel(x, y)) && !visited[x, y])
                        {
                            queue.Enqueue(new Point(x, y));
                            visited[x, y] = true;
                        }
                    }
                }
            }

            return new Rectangle(left, top, right - left + 1, bottom - top + 1);
        }

        private static IEnumerable<IEnumerable<Rectangle>> OrderRectancles(IEnumerable<Rectangle> rectangles)
        {
            var row = rectangles.OrderBy(e => e.Top).FirstOrDefault();
            if (row == default)
            {
                return Enumerable.Empty<IEnumerable<Rectangle>>();
            }

            var rects = rectangles
                .Where(e => e.Top < row.Top + row.Height / 2 && e.Top + e.Height / 2 < row.Bottom)
                .OrderBy(e => e.Left)
                .ToList();

            return new List<IEnumerable<Rectangle>>() { rects }
                .Concat(OrderRectancles(rectangles.Except(rects).ToList()));
        }

        private bool IsMarkedPixel(Color color)
        {
            return color.GetBrightness() == 0;
        }

        private IEnumerable<IEnumerable<Bitmap?>> ExtractDigits(Bitmap processedImage, IEnumerable<IEnumerable<Rectangle>> rows, int resizedSize)
        {
            var result = new List<IEnumerable<Bitmap?>>();
            foreach (var orderedRectancles in rows)
            {
                int nextNumber = -1;
                var row = new List<Bitmap?>();

                foreach (var rectangle in orderedRectancles)
                {
                    var maxSide = (rectangle.Width >= rectangle.Height ? rectangle.Width : rectangle.Height);
                    var widthOffset = (int)Math.Ceiling((double)(maxSide - rectangle.Width) / 2);
                    var heightOffset = (int)Math.Ceiling((double)(maxSide - rectangle.Height) / 2);

                    using var map = new Bitmap(maxSide, maxSide);
                    for (int i = 0; i < map.Width; i++)
                    {
                        for (int j = 0; j < map.Height; j++)
                        {
                            var color = Color.White;

                            if (!(widthOffset > i
                                || widthOffset + rectangle.Width < i
                                || heightOffset > j
                                || heightOffset + rectangle.Height < j))
                            {
                                try
                                {
                                    color = processedImage.GetPixel(rectangle.Left - widthOffset + i, rectangle.Top - heightOffset + j);
                                }
                                catch (Exception) { }
                            }

                            map.SetPixel(i, j, color);
                        }
                    }

                    if (nextNumber != -1 && nextNumber < rectangle.Left)
                    {
                        row.Add(null);
                    }

                    row.Add(ResizeBitmap(map, resizedSize));

                    nextNumber = rectangle.Right + rectangle.Width * 2;
                    if (nextNumber < 20)
                    {
                        nextNumber = -1;
                    }
                }

                result.Add(row);
            }

            return result;
        }

        private static Bitmap ResizeBitmap(Bitmap originalBitmap, int newSide)
        {
            var toReturn = new Bitmap(newSide, newSide);

            using (var graphics = Graphics.FromImage(toReturn))
            using (var attributes = new ImageAttributes())
            {
                toReturn.SetResolution(originalBitmap.HorizontalResolution, originalBitmap.VerticalResolution);

                attributes.SetWrapMode(WrapMode.TileFlipXY);

                graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                graphics.PixelOffsetMode = PixelOffsetMode.Half;
                graphics.CompositingMode = CompositingMode.SourceCopy;
                graphics.DrawImage(originalBitmap,
                    Rectangle.FromLTRB(0, 0, newSide, newSide),
                    0,
                    0,
                    originalBitmap.Width,
                    originalBitmap.Height,
                    GraphicsUnit.Pixel,
                    attributes);
            }

            return toReturn;
        }
    }
}
