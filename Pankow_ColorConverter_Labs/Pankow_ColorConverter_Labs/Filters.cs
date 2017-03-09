using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;

namespace Pankow_ColorConverter_Labs
{
    abstract class Filters
    {
        protected bool tmpKludge = false;

        protected int R_ = 0; protected int G_ = 0; protected int B_ = 0;protected int counterI=0;protected int counterJ=0;
        protected int Rmax = 0; protected int Gmax = 0; protected int Bmax = 0; protected int Rmin = 0; protected int Gmin = 0; protected int Bmin = 0;

        protected abstract Color calculateNewPixelColor(Bitmap sourceImage, int x, int y);



        public Bitmap processImage(Bitmap sourseImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourseImage.Width, sourseImage.Height);
            for (int i = 0; i < sourseImage.Width; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                {
                    return null;
                }
                for (int j = 0; j < sourseImage.Height; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourseImage, i, j));
                }
            }
            tmpKludge = false;
            return resultImage;
        }

        public int Clamp(int value, int min, int max)
        {
            if (value < min)
            {
                return min;
            }
            if (value > max)
            {
                return max;
            }
            return value;
        }


    }
    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    }

    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int tmpCol = Clamp(((int)(0.36 * sourceColor.R) + (int)(0.53 * sourceColor.G) + (int)(0.11 * sourceColor.B)), 0, 255);
            Color resultColor = Color.FromArgb(tmpCol, tmpCol, tmpCol);
            return resultColor;
        }
    }

    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int tmpCol = Clamp(((int)(0.36 * sourceColor.R) + (int)(0.53 * sourceColor.G) + (int)(0.11 * sourceColor.B)), 0, 255);
            float k = 30f;
            int tmpR = Clamp((int)(tmpCol + 2 * k), 0, 255);
            int tmpG = Clamp((int)(tmpCol + 0.5 * k), 0, 255);
            int tmpB = Clamp((int)(tmpCol - 1 * k), 0, 255);
            Color resultColor = Color.FromArgb(tmpR, tmpG, tmpB);
            return resultColor;
        }
    }

    class BrightnessFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            int tmpR = Clamp((int)(sourceColor.R + 50), 0, 255);
            int tmpG = Clamp((int)(sourceColor.G + 50), 0, 255);
            int tmpB = Clamp((int)(sourceColor.B + 50), 0, 255);
            Color resultColor = Color.FromArgb(tmpR, tmpG, tmpB);
            return resultColor;

        }
    }

    class GrayWorldFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            if (tmpKludge == false)
            {
                for (int i = 0; i < sourceImage.Width; i++)
                {
                    for (int j = 0; j < sourceImage.Height; j++)
                    {
                        R_ = (int)(sourceImage.GetPixel(i, j).R + R_);
                        G_ = (int)(sourceImage.GetPixel(i, j).G + G_);
                        B_ = (int)(sourceImage.GetPixel(i, j).B + B_);
                        counterJ++;
                    }
                    counterI++;
                }
                tmpKludge = true;
            }
            float averageR = (float)((float)R_ / (float)(counterI * counterJ));
            float averageG = (float)((float)G_ / (float)(counterI * counterJ));
            float averageB = (float)((float)B_ / (float)(counterI * counterJ));
            float Avg = (float)((averageR + averageG + averageB) / 3);
            int tmpR = Clamp((int)((int)(sourceColor.R) * ((float)(Avg) / (float)(averageR))), 0, 255);
            int tmpG = Clamp((int)((int)(sourceColor.G) * ((float)(Avg) / (float)(averageG))), 0, 255);
            int tmpB = Clamp((int)((int)(sourceColor.B) * ((float)(Avg) / (float)(averageB))), 0, 255);
            Color resultColor = Color.FromArgb(tmpR, tmpG, tmpB);
            return resultColor;
        }

    }

    class LinearStretchingFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Color sourceColor = sourceImage.GetPixel(x, y);
            if (tmpKludge == false)
            {
                MaxMinRGB(sourceImage);
                tmpKludge = true;
            }
            int tmpR = Clamp((int)((int)(sourceColor.R) - Rmin) * (255 / (Rmax - Rmin)) + sourceColor.R, 0, 255);
            int tmpG = Clamp((int)((int)(sourceColor.G) - Gmin) * (255 / (Gmax - Gmin)) + sourceColor.G, 0, 255);
            int tmpB = Clamp((int)((int)(sourceColor.B) - Bmin) * (255 / (Bmax - Bmin)) + sourceColor.B, 0, 255);
            Color resultColor = Color.FromArgb(tmpR , tmpG , tmpB );
            return resultColor;
        }

        public void MaxMinRGB(Bitmap sourceImage)
        {
            Rmax = (int)(sourceImage.GetPixel(0, 0).R);
            Gmax = (int)(sourceImage.GetPixel(0, 0).G);
            Bmax = (int)(sourceImage.GetPixel(0, 0).B);
            Rmin = (int)(sourceImage.GetPixel(0, 0).R);
            Gmin = (int)(sourceImage.GetPixel(0, 0).G);
            Bmin = (int)(sourceImage.GetPixel(0, 0).B);
            for (int i = 0; i < sourceImage.Width; i++)
            {
                for (int j = 0; j < sourceImage.Height; j++)
                {
                    if (Rmax < (int)(sourceImage.GetPixel(i, j).R))
                    {
                        Rmax = (int)(sourceImage.GetPixel(i, j).R);
                    }
                    if (Gmax < (int)(sourceImage.GetPixel(i, j).G))
                    {
                        Gmax = (int)(sourceImage.GetPixel(i, j).G);
                    }
                    if (Bmax < (int)(sourceImage.GetPixel(i, j).B))
                    {
                        Bmax = (int)(sourceImage.GetPixel(i, j).B);
                    }

                    if (Rmin > (int)(sourceImage.GetPixel(i, j).R))
                    {
                        Rmin = (int)(sourceImage.GetPixel(i, j).R);
                    }
                    if (Gmin > (int)(sourceImage.GetPixel(i, j).G))
                    {
                        Gmin = (int)(sourceImage.GetPixel(i, j).G);
                    }
                    if (Bmin > (int)(sourceImage.GetPixel(i, j).B))
                    {
                        Bmin = (int)(sourceImage.GetPixel(i, j).B);
                    }

                }
            }
        }

    }

    class WavesFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int tmpX = Clamp((int)(x - 20 * (float)(Math.Sin((2f * Math.PI * y) / 60f))), 0, sourceImage.Width - 1);
            Color resultColor = sourceImage.GetPixel(tmpX, y);
            return resultColor;
        }
    }

    class GlassFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            Random randX = new Random();
            int tmpX = Clamp((int)(x - 10 * (float)((float)(randX.Next(10)) / 10f - 0.5f)), 0, sourceImage.Width - 1);

            Random randY = new Random();
            int tmpY = Clamp((int)(y - 10 * (float)((float)(randY.Next(10)) / 10f - 0.5f)), 0, sourceImage.Height - 1);

            Color resultColor = sourceImage.GetPixel(tmpX, tmpY);
            return resultColor;
        }
    }

    class MedianFilter : Filters 
    {
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radius = 3;
            int pixelInspection = (radius * 2 + 1) * (radius * 2 + 1);
            int tmpI=0;
            int[] arrayR = new int[pixelInspection];
            int[] arrayG = new int[pixelInspection];
            int[] arrayB = new int[pixelInspection];
            for (int i = x - radius; i < x + radius; i++)
            {
                for (int j = y - radius; j < y + radius; j++)
                {
                    Color tmpPixel = sourceImage.GetPixel(Clamp(i, 0, (sourceImage.Width-1)), Clamp(j, 0, (sourceImage.Height-1)));
                    arrayR[tmpI] = (int)(tmpPixel.R);
                    int tmp = arrayR[tmpI];
                    arrayG[tmpI] = (int)(tmpPixel.G);
                    tmp = arrayG[tmpI];
                    arrayB[tmpI] = (int)(tmpPixel.B);
                    tmp = arrayB[tmpI];
                    tmpI++;
                }
            }
            QuickSort(arrayR, 0, pixelInspection - 1);
            QuickSort(arrayG, 0, pixelInspection - 1);
            QuickSort(arrayB, 0, pixelInspection - 1);

            int averageValue = (int)(pixelInspection / 2) + 1;

            int resultR = (int)arrayR[averageValue];
            int resultG = (int)arrayG[averageValue];
            int resultB = (int)arrayB[averageValue];
            Color resultColor = Color.FromArgb(resultR, resultG, resultB);
            return resultColor;
        }

        static void QuickSort(int[] sourceArray, int l, int r)
        {
            int temp;
            int x = sourceArray[l + (r - l) / 2];
            int i = l;
            int j = r;
            while (i <= j)
            {
                while (sourceArray[i] < x)
                {
                    i++;
                }
                while (sourceArray[j] > x) 
                {
                    j--;
                }
                if (i <= j)
                {
                    temp = sourceArray[i];
                    sourceArray[i] = sourceArray[j];
                    sourceArray[j] = temp;
                    i++;
                    j--;
                }
            }
            if (i < r)
                QuickSort(sourceArray, i, r);

            if (l < j)
                QuickSort(sourceArray, l, j);
        }
    }


    class MatrixFilter : Filters
    {
        protected float[,] kernel = null;
        protected MatrixFilter() { }
        public MatrixFilter(float[,] kernel)
        {
            this.kernel = kernel;
        }
        protected override Color calculateNewPixelColor(Bitmap sourceImage, int x, int y)
        {
            int radiusX = kernel.GetLength(0) / 2;
            int radiusY = kernel.GetLength(1) / 2;
            float resultR = 0;
            float resultG = 0;
            float resultB = 0;
            for (int l = -radiusY; l <= radiusY; l++)
            {
                for (int k = -radiusX; k <= radiusX; k++)
                {
                    int idX = Clamp(x + k, 0, sourceImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourceImage.Height - 1);
                    Color neighborColor = sourceImage.GetPixel(idX, idY);
                    resultR += neighborColor.R * kernel[k + radiusX, l + radiusY];
                    resultG += neighborColor.G * kernel[k + radiusX, l + radiusY];
                    resultB += neighborColor.B * kernel[k + radiusX, l + radiusY];
                }
            }
            return Color.FromArgb(Clamp((int)resultR, 0, 255), Clamp((int)resultG, 0, 255), Clamp((int)resultB, 0, 255));
        }
    }

    class BlurFilter : MatrixFilter
    {
        public BlurFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            for (int i = 0; i < sizeX; i++)
            {
                for (int j = 0; j < sizeY; j++)
                {
                    kernel[i, j] = 1.0f / (float)(sizeX * sizeY);
                }
            }
        }
    }

    class GaussianFilter : MatrixFilter
    {
        public void CreateGaussianKernel(int radius, float sigma)
        {
            int size = 2 * radius + 1;
            kernel = new float[size, size];
            float norm = 0;
            for (int i = -radius; i <= radius; i++)
            {
                for (int j = -radius; j <= radius; j++)
                {
                    kernel[i + radius, j + radius] = (float)(Math.Exp(-(i * i + j * j) / (sigma * sigma)));
                    norm += kernel[i + radius, j + radius];
                }
            }
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernel[i, j] /= norm;
                }
            }
        }
        public GaussianFilter()
        {
            CreateGaussianKernel(3, 2);
        }
    }

    class HarshnessFilter : MatrixFilter
    {
        public HarshnessFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            kernel[0, 0] = -1;
            kernel[0, 1] = -1;
            kernel[0, 2] = -1;
            kernel[1, 0] = -1;
            kernel[1, 1] = 9;
            kernel[1, 2] = -1;
            kernel[2, 0] = -1;
            kernel[2, 1] = -1;
            kernel[2, 2] = -1;

        }
    }

    class MotionBlurFilter : MatrixFilter
    {
        public void CreateMotionBlurKernel(int n)
        {
            kernel = new float[n, n];
            for (int i = 0; i < n; i++)
            {
                for (int j = 0; j < n; j++)
                {
                    if (i == j)
                    {
                        kernel[i, j] = 1.0f / (float)(n);
                    }
                    else
                    {
                        kernel[i, j] = 0f;
                    }
                }
            }
        }

        public MotionBlurFilter()
        {
            CreateMotionBlurKernel(5);
        }
    }

    class SobelFilter : MatrixFilter
    {
        public SobelFilter()
        {
            int sizeX = 3;
            int sizeY = 3;
            kernel = new float[sizeX, sizeY];
            kernel[0, 0] = -1;
            kernel[0, 1] = -2;
            kernel[0, 2] = -1;
            kernel[1, 0] = 0;
            kernel[1, 1] = 0;
            kernel[1, 2] = 1;
            kernel[2, 0] = 2;
            kernel[2, 1] = 1;
        }
    }
}