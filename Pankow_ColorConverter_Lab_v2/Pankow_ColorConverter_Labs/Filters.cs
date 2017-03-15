using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.ComponentModel;
using System.IO;

namespace Pankow_ColorConverter_Labs
{
    abstract class Filters
    {
       

        protected bool tmpKludge = false;//нужно для оптимизации
        protected int R_ = 0; protected int G_ = 0; protected int B_ = 0;protected int counterI=0;protected int counterJ=0;//для "Серого мира"
        protected int Rmax = 0; protected int Gmax = 0; protected int Bmax = 0; protected int Rmin = 0; protected int Gmin = 0; protected int Bmin = 0;//для "Линейного растяжения" и "Мат.морфологии"
        protected Random rand = new Random(1/*int)DateTime.Now.Ticks*/);//Для "Стекла"
        protected int[,] StructuralElement; protected int MW = 0; protected int MH = 0; protected int radiusW=0; protected int radiusH=0; protected Bitmap tmpImage; protected bool tmpKludgeMatMorf = false; protected Bitmap tmpImageGrad; protected bool tmpKludgeMatMorfGradDil = false; protected bool tmpKludgeMatMorfGradEro = false;//Для мат морфологии
        protected abstract Color calculateNewPixelColor(Bitmap sourseImage, int x, int y);

        public Bitmap processImage(Bitmap sourseImage, BackgroundWorker worker)
        {
            Bitmap resultImage = new Bitmap(sourseImage.Width, sourseImage.Height);
            for (int i = 0; i < sourseImage.Width ; i++)
            {
                worker.ReportProgress((int)((float)i / resultImage.Width * 100));
                if (worker.CancellationPending)
                {
                    return null;
                }
                for (int j = 0; j < sourseImage.Height ; j++)
                {
                    resultImage.SetPixel(i, j, calculateNewPixelColor(sourseImage, i, j));
                }
            }
            tmpKludge = false;
            tmpKludgeMatMorf = false;
            tmpKludgeMatMorfGradDil = false;
            tmpKludgeMatMorfGradEro = false;
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

        public void GetStructuralElement()
        {
           /* StructuralElement = new int[3, 3];
            StructuralElement[0, 0] = 1;
            StructuralElement[0, 1] = 1;
            StructuralElement[0, 2] = 1;
            StructuralElement[1, 0] = 1;
            StructuralElement[1, 1] = 1;
            StructuralElement[1, 2] = 1;
            StructuralElement[2, 0] = 1;
            StructuralElement[2, 1] = 1;
            StructuralElement[2, 2] = 1;
            MW = StructuralElement.GetLength(0);
            MH = StructuralElement.GetLength(1);
            */
            /*
            StreamReader StructuralElementText = new StreamReader("StuctElem.txt");
            int sizeStirng = StructuralElementText.ReadToEnd().;
            StructuralElement = new int[(int)Math.Sqrt(MW), (int)Math.Sqrt(MH)];
            int i = 0;
            int j = 0;
            while (!StructuralElementText.EndOfStream)
            {
                StructuralElement[i, j] = Convert.ToInt16(StructuralElementText.ReadLine());
            }
            StructuralElementText.Close();
            */
            /*
            string[] lines = File.ReadAllLines(@"StuctElem.txt").ToArray();
            int sizeStirng = lines.Length;
            MW = (int)Math.Sqrt(sizeStirng);
            MH = (int)Math.Sqrt(sizeStirng);
            StructuralElement=new int[MW,MH];
           // int[] row = new int[sizeStirng];
           // for (int i = 0; i < sizeStirng; i++)
           // {
           //    row = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
           // }
            for (int i = 0; i < MW; i++)
            {
                int[] row = lines[i].Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(Int32.Parse).ToArray();
                for (int j = 0; j < MH; j++)
                {
                    StructuralElement[i, j] = row[j];
                }
            }*/
            string[] linesStructuralElement = File.ReadAllLines(@"StuctElem.txt");
            int sizeStirng = linesStructuralElement[1].Length;
            if(sizeStirng==0)
            {
                StructuralElement = new int[3, 3];
                StructuralElement[0, 0] = 1;
                StructuralElement[0, 1] = 1;
                StructuralElement[0, 2] = 1;
                StructuralElement[1, 0] = 1;
                StructuralElement[1, 1] = 1;
                StructuralElement[1, 2] = 1;
                StructuralElement[2, 0] = 1;
                StructuralElement[2, 1] = 1;
                StructuralElement[2, 2] = 1;
                MW = StructuralElement.GetLength(0);
                MH = StructuralElement.GetLength(1);
                goto A;
            }
            MW = ((int)(sizeStirng / 2)) + 1;
            MH = ((int)(sizeStirng / 2)) + 1;
            StructuralElement = new int[MW, MH];
            for (int i = 0; i < MW; i++)
            {
                string[] str = linesStructuralElement[i].Trim().Split(' ');
                for (int j = 0; j < MH; j++)
                {
                    StructuralElement[i,j] = Clamp(int.Parse(str[j].Trim()),0,1);
                }
            }
            A:
            radiusH = (int)(MH / 2);
            radiusW = (int)(MW / 2);
        }

        public void GetDilation(Bitmap sourseImage, int x, int y)
        {
            Rmax = 0;
            Gmax = 0;
            Bmax = 0;
            if (tmpKludge == false)
            {
                GetStructuralElement();
                tmpKludge = true;
            }
            for (int j = -radiusH; j < radiusH; j++)
            {
                for (int i = -radiusW; i < radiusW; i++)
                {
                    int xi = Clamp(x + i, 0, sourseImage.Width - 1);
                    int yj = Clamp(y + j, 0, sourseImage.Height - 1);
                    if ((StructuralElement[i + radiusW, j + radiusH] == 1) && ((int)sourseImage.GetPixel(xi, yj).R > Rmax))
                    {
                        Rmax = (int)sourseImage.GetPixel(xi, yj).R;
                    }
                    if ((StructuralElement[i + radiusW, j + radiusH] == 1) && ((int)sourseImage.GetPixel(xi, yj).G > Gmax))
                    {
                        Gmax = (int)sourseImage.GetPixel(xi, yj).G;
                    }
                    if ((StructuralElement[i + radiusW, j + radiusH] == 1) && ((int)sourseImage.GetPixel(xi, yj).B > Bmax))
                    {
                        Bmax = (int)sourseImage.GetPixel(xi, yj).B;
                    }
                }
            }
        }

        public void GetErosion(Bitmap sourseImage, int x, int y)
        {
            Rmin = 255;
            Gmin = 255;
            Bmin = 255;
            if (tmpKludge == false)
            {
                GetStructuralElement();
                tmpKludge = true;
            }
            for (int j = -radiusH; j < radiusH; j++)
            {
                for (int i = -radiusW; i < radiusW; i++)
                {
                    int xi = Clamp(x + i, 0, sourseImage.Width - 1);
                    int yj = Clamp(y + j, 0, sourseImage.Height - 1);
                    if ((StructuralElement[i + radiusW, j + radiusH] == 1) && ((int)sourseImage.GetPixel(xi, yj).R < Rmin))
                    {
                        Rmin = (int)sourseImage.GetPixel(xi, yj).R;
                    }
                    if ((StructuralElement[i + radiusW, j + radiusH] == 1) && ((int)sourseImage.GetPixel(xi, yj).G < Gmin))
                    {
                        Gmin = (int)sourseImage.GetPixel(xi, yj).G;
                    }
                    if ((StructuralElement[i + radiusW, j + radiusH] == 1) && ((int)sourseImage.GetPixel(xi, yj).B < Bmin))
                    {
                        Bmin = (int)sourseImage.GetPixel(xi, yj).B;
                    }
                }
            }
        }

        /*public void tmpProcessImage(Bitmap sourseImage)
        {
            tmpImage = new Bitmap(sourseImage.Width, sourseImage.Height);
            for (int i = 0; i < sourseImage.Width; i++)
            {
                for (int j = 0; j < sourseImage.Height; j++)
                {
                    tmpImage.SetPixel(i, j, calculateNewPixelColor(sourseImage, i, j));
                }
            }
        }
        */
    }

    class InvertFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            Color sourceColor = sourseImage.GetPixel(x, y);
            Color resultColor = Color.FromArgb(255 - sourceColor.R, 255 - sourceColor.G, 255 - sourceColor.B);
            return resultColor;
        }
    }

    class GrayScaleFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            Color sourceColor = sourseImage.GetPixel(x, y);
            int tmpCol = Clamp(((int)(0.36 * sourceColor.R) + (int)(0.53 * sourceColor.G) + (int)(0.11 * sourceColor.B)), 0, 255);
            Color resultColor = Color.FromArgb(tmpCol, tmpCol, tmpCol);
            return resultColor;
        }
    }

    class SepiaFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            Color sourceColor = sourseImage.GetPixel(x, y);
            int tmpCol = Clamp(((int)(0.36 * sourceColor.R) + (int)(0.53 * sourceColor.G) + (int)(0.11 * sourceColor.B)), 0, 255);
            float k = 30f;//1-ч.б;100-очень ярко
            int tmpR = Clamp((int)(tmpCol + 2 * k), 0, 255);
            int tmpG = Clamp((int)(tmpCol + 0.5 * k), 0, 255);
            int tmpB = Clamp((int)(tmpCol - 1 * k), 0, 255);
            Color resultColor = Color.FromArgb(tmpR, tmpG, tmpB);
            return resultColor;
        }
    }

    class BrightnessFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            Color sourceColor = sourseImage.GetPixel(x, y);
            int tmpR = Clamp((int)(sourceColor.R + 50), 0, 255);
            int tmpG = Clamp((int)(sourceColor.G + 50), 0, 255);
            int tmpB = Clamp((int)(sourceColor.B + 50), 0, 255);
            Color resultColor = Color.FromArgb(tmpR, tmpG, tmpB);
            return resultColor;

        }
    }

    class GrayWorldFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            Color sourceColor = sourseImage.GetPixel(x, y);
            if (tmpKludge == false)
            {
                for (int i = 0; i < sourseImage.Width; i++)
                {
                    for (int j = 0; j < sourseImage.Height; j++)
                    {
                        R_ = (int)(sourseImage.GetPixel(i, j).R + R_);
                        G_ = (int)(sourseImage.GetPixel(i, j).G + G_);
                        B_ = (int)(sourseImage.GetPixel(i, j).B + B_);
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
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            Color sourceColor = sourseImage.GetPixel(x, y);
            if (tmpKludge == false)
            {
                MaxMinRGB(sourseImage);
                tmpKludge = true;
            }
            int tmpR = Clamp((int)((int)(sourceColor.R) - Rmin) * (255 / (Rmax - Rmin)) + sourceColor.R, 0, 255);
            int tmpG = Clamp((int)((int)(sourceColor.G) - Gmin) * (255 / (Gmax - Gmin)) + sourceColor.G, 0, 255);
            int tmpB = Clamp((int)((int)(sourceColor.B) - Bmin) * (255 / (Bmax - Bmin)) + sourceColor.B, 0, 255);
            Color resultColor = Color.FromArgb(tmpR , tmpG , tmpB );
            return resultColor;
        }

        public void MaxMinRGB(Bitmap sourseImage)
        {
            Rmax = (int)(sourseImage.GetPixel(0, 0).R);
            Gmax = (int)(sourseImage.GetPixel(0, 0).G);
            Bmax = (int)(sourseImage.GetPixel(0, 0).B);
            Rmin = (int)(sourseImage.GetPixel(0, 0).R);
            Gmin = (int)(sourseImage.GetPixel(0, 0).G);
            Bmin = (int)(sourseImage.GetPixel(0, 0).B);
            for (int i = 0; i < sourseImage.Width; i++)
            {
                for (int j = 0; j < sourseImage.Height; j++)
                {
                    if (Rmax < (int)(sourseImage.GetPixel(i, j).R))
                    {
                        Rmax = (int)(sourseImage.GetPixel(i, j).R);
                    }
                    if (Gmax < (int)(sourseImage.GetPixel(i, j).G))
                    {
                        Gmax = (int)(sourseImage.GetPixel(i, j).G);
                    }
                    if (Bmax < (int)(sourseImage.GetPixel(i, j).B))
                    {
                        Bmax = (int)(sourseImage.GetPixel(i, j).B);
                    }

                    if (Rmin > (int)(sourseImage.GetPixel(i, j).R))
                    {
                        Rmin = (int)(sourseImage.GetPixel(i, j).R);
                    }
                    if (Gmin > (int)(sourseImage.GetPixel(i, j).G))
                    {
                        Gmin = (int)(sourseImage.GetPixel(i, j).G);
                    }
                    if (Bmin > (int)(sourseImage.GetPixel(i, j).B))
                    {
                        Bmin = (int)(sourseImage.GetPixel(i, j).B);
                    }

                }
            }
        }

    }

    class WavesFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            int tmpX = Clamp((int)(x - 20 * (float)(Math.Sin((2f * Math.PI * y) / 60f))), 0, sourseImage.Width - 1);
            Color resultColor = sourseImage.GetPixel(tmpX, y);
            return resultColor;
        }
    }

    class GlassFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
           /* Random randX = new Random((int)DateTime.Now.Ticks);
              float tmpRandX = (float)(rand.Next(10));
              float tmpX_2 = (float)((float)(randX.Next(10)) / 10f - 0.5f);
              int tmpX_1 = (int)(x - 10 * (float)((float)(randX.Next(10)) / 10f - 0.5f));

              Random randY = new Random(int)DateTime.Now.Ticks);
              float tmpY_2 = (float)((float)(randY.Next(10)) / 10f - 0.5f);
              int tmpY_1 = (int)(y - 10 * (float)((float)(randY.Next(10)) / 10f - 0.5f));*/
            int tmpX = Clamp((int)(x - 10 * (float)((float)(rand.Next(10)) / 10f - 0.5f)), 0, sourseImage.Width - 1);
            int tmpY = Clamp((int)(y - 10 * (float)((float)(rand.Next(10)) / 10f - 0.5f)), 0, sourseImage.Height - 1);

            Color resultColor = sourseImage.GetPixel(tmpX, tmpY);
            return resultColor;
        }
    }

    class MedianFilter : Filters 
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
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
                    Color tmpPixel = sourseImage.GetPixel(Clamp(i, 0, (sourseImage.Width-1)), Clamp(j, 0, (sourseImage.Height-1)));
                    arrayR[tmpI] = (int)(tmpPixel.R);
                    int tmp = arrayR[tmpI];
                    arrayG[tmpI] = (int)(tmpPixel.G);
                    tmp = arrayG[tmpI];
                    arrayB[tmpI] = (int)(tmpPixel.B);
                    tmp = arrayB[tmpI];
                    tmpI++;
                }
            }

           Array.Sort(arrayR);
           Array.Sort(arrayG);
           Array.Sort(arrayB);

            int averageValue = (int)(pixelInspection / 2) + 1;

            int resultR = (int)arrayR[averageValue];
            int resultG = (int)arrayG[averageValue];
            int resultB = (int)arrayB[averageValue];
            Color resultColor = Color.FromArgb(resultR, resultG, resultB);
            return resultColor;
        }
    }

    class KekFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            int tmpX = x;
            int seredX = (int)(sourseImage.Width / 2);
            if (tmpX > seredX)
            {
                tmpX = Clamp(seredX - (tmpX - seredX), 0, sourseImage.Width - 1);
            }
            Color resultColor = sourseImage.GetPixel(tmpX, y);
            return resultColor;

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
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
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
                    int idX = Clamp(x + k, 0, sourseImage.Width - 1);
                    int idY = Clamp(y + l, 0, sourseImage.Height - 1);
                    Color neighborColor = sourseImage.GetPixel(idX, idY);
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
    
    class DilationFilter:Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            GetDilation(sourseImage, x, y);
            Color resultColor = Color.FromArgb(Rmax, Gmax, Bmax);
            return resultColor;
        }
    }

    class ErosionFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            GetErosion(sourseImage, x, y);
            Color resultColor = Color.FromArgb(Rmin, Gmin, Bmin);
            return resultColor;
        }
    }

    class OpeningFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            
            if (tmpKludgeMatMorf == false)
            {
                tmpImage = new Bitmap(sourseImage.Width, sourseImage.Height);
                for (int i = 0; i < tmpImage.Width; i++)
                {
                    for (int j = 0; j < tmpImage.Height ; j++)
                    {
                        GetErosion(sourseImage, i, j);
                        tmpImage.SetPixel(i, j, Color.FromArgb(Rmin, Gmin, Bmin));
                    }
                }
                tmpKludgeMatMorf = true;
            }
            GetDilation(tmpImage, x, y);
            Color resultColor = Color.FromArgb(Rmax, Gmax, Bmax);
            return resultColor;
        }
    }

    class ClosingFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {

            if (tmpKludgeMatMorf == false)
            {
                tmpImage = new Bitmap(sourseImage.Width, sourseImage.Height);
                for (int i = 0; i < tmpImage.Width; i++)
                {
                    for (int j = 0; j < tmpImage.Height; j++)
                    {
                        GetDilation(sourseImage, i, j);
                        tmpImage.SetPixel(i, j, Color.FromArgb(Rmax, Gmax, Bmax));
                    }
                }
                tmpKludgeMatMorf = true;
            }
            GetErosion(tmpImage, x, y);
            Color resultColor = Color.FromArgb(Rmin, Gmin, Bmin);
            return resultColor;
        }
    }

    class GradFilter : Filters
    {
        protected override Color calculateNewPixelColor(Bitmap sourseImage, int x, int y)
        {
            
            if (tmpKludgeMatMorfGradDil == false)
            {
                tmpImage = new Bitmap(sourseImage.Width, sourseImage.Height);
                for (int i = 0; i < tmpImage.Width; i++)
                {
                    for (int j = 0; j < tmpImage.Height; j++)
                    {
                        GetDilation(sourseImage, i, j);
                        tmpImage.SetPixel(i, j, Color.FromArgb(Rmax, Gmax, Bmax));
                    }
                }
                tmpKludgeMatMorfGradDil = true;
            }
            if (tmpKludgeMatMorfGradEro == false)
            {
                tmpImageGrad = new Bitmap(sourseImage.Width, sourseImage.Height);
                for (int i = 0; i < tmpImageGrad.Width; i++)
                {
                    for (int j = 0; j < tmpImageGrad.Height; j++)
                    {
                        GetErosion(sourseImage, i, j);
                        tmpImageGrad.SetPixel(i, j, Color.FromArgb(Rmin, Gmin, Bmin));
                    }
                }
                tmpKludgeMatMorfGradEro = true;
            }
            Color resultColor = Color.FromArgb(((int)tmpImage.GetPixel(x, y).R - ((int)tmpImageGrad.GetPixel(x, y).R)), ((int)tmpImage.GetPixel(x, y).G - ((int)tmpImageGrad.GetPixel(x, y).G)), ((int)tmpImage.GetPixel(x, y).B - ((int)tmpImageGrad.GetPixel(x, y).B)));
            /*if (((int)tmpImage.GetPixel(x, y).R == ((int)tmpImageGrad.GetPixel(x, y).R)) && ((int)tmpImage.GetPixel(x, y).G == ((int)tmpImageGrad.GetPixel(x, y).G)) && ((int)tmpImage.GetPixel(x, y).B == ((int)tmpImageGrad.GetPixel(x, y).B)))
            {
                resultColor = Color.FromArgb(0 , 0 , 0);
            }
            else
            {
                resultColor = tmpImage.GetPixel(x, y);
            }*/
            return resultColor;
           
        }
    }


   
}