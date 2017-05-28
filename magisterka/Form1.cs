using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace magisterka
{
    public partial class Form1 : Form
    {
        Bitmap orgBmp;
        Figures fig;

        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
             try
            {
                openFileDialog1.Filter = "Image Files(*.jpg; *.jpeg; *.gif; *.bmp)|*.jpg; *.jpeg; *.gif; *.bmp";
                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    orgBmp = new Bitmap(openFileDialog1.FileName);
                    // 640x480
                    float scaleW, scaleH;
                    if(orgBmp.Size.Width > 640 && orgBmp.Size.Height > 480){
                        scaleW = (float)640.0 / (float)orgBmp.Size.Width;
                        scaleH = (float)480.0 / (float)orgBmp.Size.Height;
                        if (scaleW < scaleH)
                        {
                            Bitmap resized = new Bitmap(orgBmp, new Size((int)(orgBmp.Size.Width * scaleW), (int)(orgBmp.Size.Height * scaleW)));
                            orgBmp = resized;
                        }
                        else
                        {
                            Bitmap resized = new Bitmap(orgBmp, new Size((int)(orgBmp.Size.Width * scaleH), (int)(orgBmp.Size.Height * scaleH)));
                            orgBmp = resized;
                        }

                    } else if(orgBmp.Size.Width > 640){
                        scaleW = 640 / orgBmp.Size.Width;
                        Bitmap resized = new Bitmap(orgBmp, new Size((int)(orgBmp.Size.Width * scaleW), (int)(orgBmp.Size.Height * scaleW)));
                        orgBmp = resized;
                    }
                    else if (orgBmp.Size.Height > 480)
                    {
                        scaleH = 480 / orgBmp.Size.Height;
                        Bitmap resized = new Bitmap(orgBmp, new Size((int)(orgBmp.Size.Width * scaleH), (int)(orgBmp.Size.Height * scaleH)));
                        orgBmp = resized;
                    }
                    
                    pictureBoxOryginal.Image = orgBmp;
                    //pictureBoxTransform.Image = orgBmp;
                }
            } catch (Exception)
             {
                 throw new ApplicationException("Failed loading image");
             }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            fig = new Figures(orgBmp, (int)windowNUM.Value, (double)sigmaNUM.Value, (double)dpNUM.Value, (double)minDistNUM.Value, (int)minRadNUM.Value, (int)maxRadNUM.Value, (double)param1NUM.Value, (double)param2NUM.Value, (int)numericUpDownThreshRect.Value, (int)numericUpDownMode.Value);
            pictureBoxTransform.Image = fig.getImg();
            pictureBoxGray.Image = fig.getImgGrayOrg();
            pictureBoxThresh.Image = fig.getImgGrayThresh();
            pictureBoxAfterTreshold.Image = fig.getImgAfterThreshold();
            labelResult.Text = fig.getResult();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            FindEllipses elips = new FindEllipses(orgBmp, Convert.ToInt32(numericUpDown1.Value), (int)windowNUM2.Value, (double)sigmaNUM2.Value);
            elips.find();
            pictureBoxTransform.Image = elips.getImg();
        }

        private void button4_Click(object sender, EventArgs e) // wydzielanie cyferek
        {
            int tresh = Convert.ToInt32(textBoxTresholdDigit.Text);
            pictureBoxDigit.Image = fig.getDigitImg();
            pictureBox1.Image = fig.getDigitsImg(0, tresh);
            pictureBox2.Image = fig.getDigitsImg(1, tresh);
            pictureBox3.Image = fig.getDigitsImg(2, tresh);
            pictureBox4.Image = fig.getDigitsImg(3, tresh);
            pictureBox5.Image = fig.getDigitsImg(4, tresh);
        }

    }
}
