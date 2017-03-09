using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace Pankow_ColorConverter_Labs
{
    public partial class Form1 : Form
    {
        Bitmap image;
        Bitmap tmpImage;

        Stack<Bitmap> undoStack = new Stack<Bitmap>();

        bool tmpStack = false;

        bool tmpUndo = false;

        public Form1()
        {

            InitializeComponent();
        }

        private void открытьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            undoStack.Clear();
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filter = "image files|*.jpg;*.png";
            if (dialog.ShowDialog() == DialogResult.OK)
            {
                image = new Bitmap(dialog.FileName);
                pictureBox1.Image = image;
                tmpImage = image;
                pictureBox1.Refresh();
                this.отменаToolStripMenuItem.Visible = false;
                /* undoStack.Push(image);*/
            }

        }

        private void сохранитьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "image files|*.jpg;*.png";
            saveDialog.FileName = "Image";
            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                 pictureBox1.Image.Save(saveDialog.FileName, System.Drawing.Imaging.ImageFormat.Jpeg);
            }
                
        }


        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            Bitmap newImage = ((Filters)e.Argument).processImage((Bitmap)(pictureBox1.Image), backgroundWorker1);
            if (backgroundWorker1.CancellationPending != true)
            {
                image = newImage;
            }

        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            progressBar1.Value = e.ProgressPercentage;
        }

        private void backgroundWorker1_RunWorkerCompleted_1(object sender, RunWorkerCompletedEventArgs e)
        {
            
            if (!e.Cancelled)
            {
                tmpUndo = false;
                Bitmap tmpImage = (Bitmap)(pictureBox1.Image);
                pictureBox1.Image = image;
                if (tmpUndo == false)
                {
                    undoStack.Push(image);
                    this.отменаToolStripMenuItem.Visible = true;
                }
                else
                {
                    pictureBox1.Image = tmpImage;
                }
                pictureBox1.Refresh();

            }
            progressBar1.Value = 0;
            tmpStack = true;
        }
        
        private void button1_Click(object sender, EventArgs e)
        {
            tmpUndo = true;
            backgroundWorker1.CancelAsync();

        }

        private void инверсияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new InvertFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void переводВToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayScaleFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }


        private void сепияToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SepiaFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void увеличитьЯркостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BrightnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void серыйМирToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GrayWorldFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void линейноеРастяжениеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new LinearStretchingFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void волныToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new WavesFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void эффектСтеклаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GlassFilter();
            backgroundWorker1.RunWorkerAsync(filter);
            
        }

        private void медианныйФильтрToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MedianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void разымытиеToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new BlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void фильтрГауссаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new GaussianFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void резкостьToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new HarshnessFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void размытиеВДвиженииToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new MotionBlurFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }

        private void фильтрСобеляToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Filters filter = new SobelFilter();
            backgroundWorker1.RunWorkerAsync(filter);
        }


        private void отменаToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                Bitmap undoStackPop = undoStack.Pop();
                if (tmpStack == true)
                {
                    Bitmap tmpUndoStackPop = undoStack.Pop();
                    pictureBox1.Image = tmpUndoStackPop;
                }
                else
                {
                    pictureBox1.Image = undoStackPop;
                }
            }
            catch
            {
                this.отменаToolStripMenuItem.Visible = false;
                pictureBox1.Image = tmpImage;
            }
            pictureBox1.Refresh();
            tmpStack = false;
        }

        

        

       
















    }
}
