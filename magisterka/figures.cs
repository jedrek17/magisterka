using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Drawing;
using System.IO;
namespace magisterka
{
    class Figures
    {
        Mat src;
        Mat grayOrg;
        Mat grayThresh;
        Mat grayAfterTresh;
        Mat grayCut;
        Mat outputImg;
        Mat digitImage; // obrazek zawierajacy wyciety prostokat z licznikiem
        Mat[] digitImagesList = new Mat[5];
        List<int[,]> orgDigital; // tablica zawierajace wzory cyferek

        int threshold = 150;
        public Figures(Bitmap bmpSrc, int _window, double _sigma, double _dp, double _minDist, int _minRad, int _maxRad, double _param1 = 100, double _param2 = 100, int _threshold = 100, int mode = 1)
        {
            threshold = _threshold;
            orgDigital = loadDigitaFromImage();
            src = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmpSrc);
            grayThresh = new Mat();
            grayAfterTresh = new Mat();
            outputImg = new Mat();
            grayOrg = new Mat();
            grayCut = new Mat();
            //Mat mask = new Mat(src.Size(), OpenCvSharp.MatType.CV_8UC1, new Scalar(0,0,0));
            Mat mask = new Mat(src.Size(), OpenCvSharp.MatType.CV_8UC1);
            mask.SetTo(new Scalar(0, 0, 0));
            Cv2.CvtColor(src, grayOrg, OpenCvSharp.ColorConversionCodes.BGR2GRAY);
            grayOrg.CopyTo(grayThresh);
            grayOrg.CopyTo(grayAfterTresh);
            //OpenCvSharp.Size size = new OpenCvSharp.Size(320, 240);
            //Cv2.Resize(gray, gray, size, 0, 0, InterpolationFlags.Linear);
            OpenCvSharp.Size window = new OpenCvSharp.Size(_window, _window);
            Cv2.GaussianBlur(grayThresh, grayThresh, window, _sigma);

            // dp, minDist, par1, par2, minRadius, maxRadius
            // dp 2, minDist 500, par1 100, par2 100, minrad 100, maxrad 0
            CircleSegment[] circles = Cv2.HoughCircles(grayThresh, HoughMethods.Gradient, _dp, _minDist, _param1, _param2, _minRad, _maxRad); // tylko Gradient dziala 
            float maxRadius = 0;
            Point2f maxCenter = new Point2f();
            foreach (CircleSegment circle in circles)
            {
                if (circle.Radius > maxRadius) { maxRadius = circle.Radius; maxCenter = circle.Center; }
                Scalar color = new Scalar(255, 255, 255);
                Cv2.Circle(mask, circle.Center, (int)circle.Radius, color, -1); // rysujemy wypelniony okrag
            }

            grayOrg.CopyTo(grayCut, mask);
            grayOrg = grayCut;
            switch (mode)
            {
                case 0:
                    findRect1(maxRadius, maxCenter);
                    break;
                case 1:
                    findRect2(maxRadius, maxCenter);
                    break;
                case 2:
                    findRectT(maxRadius, maxCenter);
                    break;
            }
            
            //
            //
        }

        private void findRect2(float radius, Point2f center)
        {
            OpenCvSharp.Cv2.Threshold(grayOrg, grayThresh, getThresh(), 255, OpenCvSharp.ThresholdTypes.Binary);
            LineSegmentPoint[] lines = Cv2.HoughLinesP(grayThresh, 1, Math.PI / 90, 80, 30, 0.01);
            OpenCvSharp.RNG rng = new RNG();
            Scalar color = new Scalar(rng.Uniform(0, 255), rng.Uniform(0, 0), rng.Uniform(0, 0));
            foreach (var line in lines)
            {
                Cv2.Line(src, line.P1, line.P2, color);
            }
            outputImg = src;
        }

        private void findRectT(float radius, Point2f center)
        {
            //Mat threshold_output = new Mat(grayCut.Size(), OpenCvSharp.MatType.CV_8UC1);
            Mat[] contours;
            Mat hierarchy = new Mat();
            List<Rect> foundBoundingRectangles = new List<Rect>();
            /// Detect edges using Threshold
            OpenCvSharp.Cv2.Threshold(grayOrg, grayThresh, getThresh(), 255, OpenCvSharp.ThresholdTypes.Binary);

            OpenCvSharp.Cv2.FindContours(grayThresh, out contours, hierarchy, OpenCvSharp.RetrievalModes.Tree, OpenCvSharp.ContourApproximationModes.ApproxSimple);
            OpenCvSharp.RNG rng = new RNG();
            Scalar color = new Scalar(rng.Uniform(0, 0), rng.Uniform(0,0), rng.Uniform(0,255));
            foreach (var cont in contours)
            {
                Rect rect = Cv2.BoundingRect(cont);
                Cv2.Rectangle(src, rect, color);
            }
            outputImg = src;
        }

        private int getThresh()
        {
            return threshold; // tymczasowo
        }
        public void findRect1(float radius, Point2f center)
        {
              Mat threshold_output = new Mat(grayCut.Size(), OpenCvSharp.MatType.CV_8UC1);
              Mat[] contours;
              Mat hierarchy = new Mat();
              OpenCvSharp.Cv2.AdaptiveThreshold(grayOrg, grayThresh, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 17, 3);

              //Cv2.BitwiseNot(grayThresh.Clone(), grayThresh);

              //Mat kernel = Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Ellipse, new OpenCvSharp.Size(2, 2));

              //Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.Gradient, kernel);
              
              //Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.Close, kernel);
              //Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.Open, kernel);
              //Cv2.Dilate(grayThresh.Clone(), grayThresh, kernel, null, 1);

              //OpenCvSharp.Cv2.Threshold(grayOrg, grayThresh, getThresh(), 255, OpenCvSharp.ThresholdTypes.Binary);
              //Cv2.BitwiseNot(grayThresh.Clone(), grayThresh);
            grayThresh.CopyTo(threshold_output);

            /*
              Mat edges = new Mat(grayCut.Size(), OpenCvSharp.MatType.CV_8UC1);
              OpenCvSharp.Cv2.Canny(threshold_output, edges, 50, 150);
              int minLineLength = (int)radius/2;
              int maxLineGap = 5;
            
              var lines = Cv2.HoughLinesP(edges, 1, Math.PI / 180, 200, minLineLength, maxLineGap);
              foreach (var line in lines)
              {
                  Cv2.Line(src,line.P1, line.P2, new Scalar(0,255,0));
              }
            */
            
              OpenCvSharp.Cv2.FindContours(threshold_output, out contours, hierarchy, OpenCvSharp.RetrievalModes.Tree, OpenCvSharp.ContourApproximationModes.ApproxSimple);

              /// Find the rotated rectangles and ellipses for each contour
              List<RotatedRect> minRect = new List<RotatedRect>();//minRect(contours);
              List<RotatedRect> minEllipse = new List<RotatedRect>();//(contours.size());
              
            foreach(Mat m in contours){
                minRect.Add(OpenCvSharp.Cv2.MinAreaRect(m));
            }
            Mat drawing = Mat.Zeros(grayThresh.Size(), OpenCvSharp.MatType.CV_8UC3);
            RotatedRect rect2draw = new RotatedRect();
            double biggestRect = 0d;
            StreamWriter sw = new StreamWriter("rect.txt");
            foreach (RotatedRect m in minRect)
            {
                
                double stosunek = m.Size.Height / m.Size.Width;
                if(stosunek > 0.3 && stosunek < 0.4){
                    if (biggestRect < m.Size.Height * m.Size.Width)
                    {
                        biggestRect = m.Size.Height * m.Size.Width;
                        rect2draw = m;
                    }
                }
                else
                {
                    stosunek = m.Size.Width / m.Size.Height;
                    if (stosunek > 0.3 && stosunek < 0.4)
                    {
                        if (biggestRect < m.Size.Height * m.Size.Width)
                        {
                            biggestRect = m.Size.Height * m.Size.Width;
                            rect2draw = m;
                        }
                    }  
                }
                //drawRect(m);
                sw.WriteLine("w: " + m.Size.Width + "; h: " + m.Size.Height + "; s: " + stosunek);
            }
            sw.WriteLine("najlepszy: w;h:" + rect2draw.Size.Width + "; " + rect2draw.Size.Height);
            sw.Close();
              if (!rect2draw.Equals(new RotatedRect()))
              {
                  //drawRect(rect2draw);

                  float angle = rect2draw.Angle;
                  OpenCvSharp.Size2f rect_size = rect2draw.Size;
                  if (rect2draw.Angle < -45.0) {
                    angle += 90;
                    // swap width and height
                    //rect2draw.Size = new Size2f(rect_size.Height, rect_size.Width);
                  }
                  Mat rot_mat = OpenCvSharp.Cv2.GetRotationMatrix2D(rect2draw.Center, angle, 1.0);
                  digitImage = new Mat();
                  OpenCvSharp.Cv2.WarpAffine(src, digitImage, rot_mat, src.Size());

                  //rect2draw.Angle = 90;

                  digitImage = new Mat(digitImage, rect2draw.BoundingRect());

                  double h ;//= (int)(rect2draw.Size.Width / 5);
                  double w ;//= (int)rect2draw.Size.Height;
                  if (rect2draw.Size.Width > rect2draw.Size.Height)
                  {
                      w = (double)(rect2draw.Size.Width / 5);
                      h = (double)rect2draw.Size.Height;

                  }
                  else
                  {
                     h = (double)rect2draw.Size.Width;
                     w = (double)(rect2draw.Size.Height/5);

                  }

                  //drawRect(rect2draw);
                  
                  digitImagesList[0] = new Mat(digitImage, new Rect(new OpenCvSharp.Point(0, 0), new OpenCvSharp.Size(w, h))).Clone();
                  digitImagesList[1] = new Mat(digitImage, new Rect(new OpenCvSharp.Point(w, 0), new OpenCvSharp.Size(w, h))).Clone();
                  digitImagesList[2] = new Mat(digitImage, new Rect(new OpenCvSharp.Point(w * 2, 0), new OpenCvSharp.Size(w, h))).Clone();
                  digitImagesList[3] = new Mat(digitImage, new Rect(new OpenCvSharp.Point(w * 3, 0), new OpenCvSharp.Size(w, h))).Clone();
                  digitImagesList[4] = new Mat(digitImage, new Rect(new OpenCvSharp.Point(w * 4, 0), new OpenCvSharp.Size(w, h))).Clone();
                  
                  findDigitOnImg();

                  /*
                  Cv2.CvtColor(digitImagesList[0], digitImagesList[0], OpenCvSharp.ColorConversionCodes.BGR2GRAY);
                  Cv2.CvtColor(digitImagesList[1], digitImagesList[1], OpenCvSharp.ColorConversionCodes.BGR2GRAY);
                  Cv2.CvtColor(digitImagesList[2], digitImagesList[2], OpenCvSharp.ColorConversionCodes.BGR2GRAY);
                  Cv2.CvtColor(digitImagesList[3], digitImagesList[3], OpenCvSharp.ColorConversionCodes.BGR2GRAY);
                  Cv2.CvtColor(digitImagesList[4], digitImagesList[4], OpenCvSharp.ColorConversionCodes.BGR2GRAY);

                  OpenCvSharp.Cv2.Threshold(digitImagesList[0], digitImagesList[0], 75, 255, OpenCvSharp.ThresholdTypes.Binary);
                  OpenCvSharp.Cv2.Threshold(digitImagesList[1], digitImagesList[1], 75, 255, OpenCvSharp.ThresholdTypes.Binary);
                  OpenCvSharp.Cv2.Threshold(digitImagesList[2], digitImagesList[2], 75, 255, OpenCvSharp.ThresholdTypes.Binary);
                  OpenCvSharp.Cv2.Threshold(digitImagesList[3], digitImagesList[3], 75, 255, OpenCvSharp.ThresholdTypes.Binary);
                  OpenCvSharp.Cv2.Threshold(digitImagesList[4], digitImagesList[4], 75, 255, OpenCvSharp.ThresholdTypes.Binary);
                  */
              }
           // gray = drawing; // tylko tymczasowo
              outputImg = src;
        }

        private void findDigitOnImg()
        {
            int i = 0;
            foreach (Mat m in digitImagesList)
            {
                //thresh = cv2.threshold(warped, 0, 255,cv2.THRESH_BINARY_INV | cv2.THRESH_OTSU)[1]
                Mat thresh = new Mat(m.Size(), OpenCvSharp.MatType.CV_8UC1);
                Mat src = new Mat(m.Size(), OpenCvSharp.MatType.CV_8UC1);

                Cv2.CvtColor(m.Clone(), src, OpenCvSharp.ColorConversionCodes.BGR2GRAY);

                //Cv2.AdaptiveThreshold(src.Clone(), thresh, 255, OpenCvSharp.AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 3, 11);
                Cv2.Threshold(src.Clone(), thresh, 55, 255, OpenCvSharp.ThresholdTypes.Binary );
               // kernel = cv2.getStructuringElement(cv2.MORPH_ELLIPSE, (1, 5))
                Mat kernel = Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Cross, new OpenCvSharp.Size(2, 2));
                Mat kernel2 = Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Cross, new OpenCvSharp.Size(3, 3));
                //thresh = cv2.morphologyEx(thresh, cv2.MORPH_OPEN, kernel)
               // Cv2.MorphologyEx(thresh.Clone(), thresh, OpenCvSharp.MorphTypes.Open, kernel);
                Cv2.MorphologyEx(thresh.Clone(), thresh, OpenCvSharp.MorphTypes.ERODE, kernel);
                //Cv2.MorphologyEx(thresh.Clone(), thresh, OpenCvSharp.MorphTypes.Close, kernel);
                //Cv2.MorphologyEx(thresh.Clone(), thresh, OpenCvSharp.MorphTypes.Close, kernel2);

                // Wyszukiwanie cyfr na wycietym fragmencie prostokata
                Mat[] contours;
                Mat hierarchy = new Mat();
                OpenCvSharp.Cv2.FindContours(thresh.Clone(), out contours, hierarchy, OpenCvSharp.RetrievalModes.List, OpenCvSharp.ContourApproximationModes.ApproxSimple);
               
                //
                
                //for (int j = 0; j < contours.Length; j++)
                //{
                   // contours_poly[j] = Cv2.ApproxPolyDP(contours[j], 3, true);
                    //approxPolyDP(Mat(contours[j]), contours_poly[j], 3, true);
                   // boundRect[i] = boundingRect(Mat(contours_poly[j]));
                //}
                foreach (Mat mc in contours)
                {
                    Cv2.Rectangle(thresh, Cv2.BoundingRect(mc), new Scalar(0, 255, 0));
                }

                /*
                /// Find the rotated rectangles and ellipses for each contour
                List<RotatedRect> minRect = new List<RotatedRect>();//minRect(contours);

                foreach (Mat mc in contours)
                {
                    minRect.Add(OpenCvSharp.Cv2.MinAreaRect(mc));
                }
                RotatedRect rect2draw = new RotatedRect();

                foreach (var rect in minRect)
                {
                   // drawRect(thresh, rect);
                }
                */
                digitImagesList[i] = thresh;
                i++;
            }
        }

        private void drawRect(RotatedRect rect){
            OpenCvSharp.RNG rng = new RNG();
                    Scalar color = new Scalar(rng.Uniform(0, 0), rng.Uniform(0, 0), rng.Uniform(255, 255));

                    Point2f[] rect_points = rect.Points();
                    for (int j = 0; j < 4; j++) { 
                            //Cv2.Line( gray, rect_points[j], rect_points[(j+1)%4], color);
                            Cv2.Line( src, rect_points[j], rect_points[(j + 1) % 4], color);
                    }
        }
        private void drawRect(Mat img, RotatedRect rect)
        {
            OpenCvSharp.RNG rng = new RNG();
            Scalar color = new Scalar(rng.Uniform(0, 0), rng.Uniform(0, 0), rng.Uniform(255, 255));

            Point2f[] rect_points = rect.Points();
            for (int j = 0; j < 4; j++)
            {
                //Cv2.Line( gray, rect_points[j], rect_points[(j+1)%4], color);
                Cv2.Line(img, rect_points[j], rect_points[(j + 1) % 4], color);
            }
        }
        public Bitmap getImg(){
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(outputImg);
        }
        public Bitmap getImgGrayThresh()
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(grayThresh);
        }
        public Bitmap getImgAfterThreshold()
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(grayAfterTresh);
        }
        public Bitmap getImgGrayOrg()
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(grayOrg);
        }
        public Bitmap getDigitImg()
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(digitImage);
        }
        public Bitmap getDigitsImg(int number, int treshold)
        {
            Mat toRet = digitImagesList[number].Clone();
            //Cv2.CvtColor(toRet, toRet, OpenCvSharp.ColorConversionCodes.BGR2GRAY);
            //OpenCvSharp.Cv2.Threshold(toRet, toRet, treshold, 255, OpenCvSharp.ThresholdTypes.Binary);
            Cv2.Resize(toRet, toRet, new OpenCvSharp.Size(40, 96));
            digitImagesList[number] = toRet;
            //OpenCvSharp.Cv2.MedianBlur(toRet, toRet, 3);
            readDigital(number);
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(toRet);
        }
        public int readDigital(int number)
        {
            int imgw  = 40; //szerokosc obrazka
            int imgh = 96; // wysokosc obrazka

            int newW = 4; // szerokosc okna
            int newH = newW; // wysokosc okna
            
            Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(digitImagesList[number]);
            int[,] img = new int[imgw, imgh];

            for (int i = 0; i < imgw; i++)
            {
                for (int j = 0; j < imgh; j++)
                {
                    Color oc = bmp.GetPixel(i, j);
                    img[i, j] = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                }
            }
            compareDigit(img);

            return 0;
        }
        public int compareDigit(int[,] img)
        {
            int imgw  = 40; //szerokosc obrazka
            int imgh = 96; // wysokosc obrazka
            int[] score = new int[10];
            int[,] imgTab;

            int tabHeight; // wysokosc cyferki domyslnej
            int tabWidth; // szerokosc cyerki domyslnej

            int diffHeight; // roznica wysokosci
            int diffWidth;// roznica szerokosci
            int bestScore = 999999999;
            int scoreTmp = 0;
            for (int k = 0; k < 10; k++) // potem zrobic k< 10
            {
                imgTab = orgDigital[k];

                tabHeight = imgTab.GetLength(1);
                tabWidth = imgTab.GetLength(0);
                diffHeight = imgh - tabHeight;
                diffWidth = imgw - tabWidth;
                bestScore = 999999999;
                for (int a = 0; a < diffWidth; a++)
                {
                    for (int b = 0; b < diffHeight; b++)
                    {
                        for (int i = 0; i < tabWidth; i++)
                        {
                            for (int j = 0; j < tabHeight; j++)
                            {
                                int wynik = (img[i + a, j + b] - imgTab[i, j]) * (Math.Abs(tabWidth / 2 - i) * Math.Abs(tabHeight / 2 - j));

                                scoreTmp += Math.Abs(wynik);
                            }
                        }
                        if (bestScore > scoreTmp) bestScore = scoreTmp;
                        scoreTmp = 0;
                    }
                }
                score[k] = bestScore;

            }
            return 0;
        }
        public List<int[,]> loadDigitaFromImage(){
            
            int imgw = 40; //szerokosc obrazka
            int imgh = 96; // wysokosc obrazka
            int[,] imgTab;
            List<int[,]> img2 = new List<int[,]>();
            for (int k = 0; k < 10; k++) // potem zrobic k< 10
            {
                Bitmap img = new Bitmap("../../digital/"+k.ToString()+".jpg");
                imgTab = new int[img.Width, img.Height];

                for (int i = 0; i < img.Width; i++)
                {
                    for (int j = 0; j < img.Height; j++)
                    {
                        Color oc = img.GetPixel(i, j);
                        imgTab[i, j] = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                    }
                }
                img2.Add(imgTab);
            }
            return img2;
        }
    }
}
