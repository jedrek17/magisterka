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
        Mat digitImageTh; // obrazek zawierajacy wyciety prostokat z licznikiem po sprogowaniu
        Mat[] digitImagesList = new Mat[5];
        List<int[,]> orgDigital; // tablica zawierajace wzory cyferek
        int[] result = new int[5];
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
              //Cv2.Sobel(grayOrg.Clone(), grayThresh, MatType.CV_8UC1, 1, 1, 3, 1, 0, BorderTypes.Isolated);
              //Cv2.Canny(grayOrg.Clone(), grayThresh, 150, 50);
              //OpenCvSharp.Cv2.AdaptiveThreshold(grayThresh, grayThresh, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 17, 1);
              OpenCvSharp.Cv2.Threshold(grayOrg, grayThresh, getThresh(), 255, OpenCvSharp.ThresholdTypes.Binary); // dla zdjec od kuby na ciemnym tle dziala przy progu 205
              //Cv2.BitwiseNot(grayThresh.Clone(), grayThresh);

              Mat kernel = Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Ellipse, new OpenCvSharp.Size(2, 2));
              Mat kernel2 = Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Ellipse, new OpenCvSharp.Size(2, 2));
              //Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.Gradient, kernel);
              
              //Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.Close, kernel);
              //Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.Open, kernel);
             // Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.ERODE, kernel);
             // Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.Close, kernel);
              //Cv2.Erode(grayThresh.Clone(), grayThresh, kernel, null, 1);

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

                  //OpenCvSharp.Cv2.AdaptiveThreshold(grayOrg, grayThresh, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 17, 3);
                 // kernel = Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Ellipse, new OpenCvSharp.Size(2, 2));
                  //kernel2 = Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Ellipse, new OpenCvSharp.Size(2, 2));
                  //Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.Gradient, kernel);

                  //Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.Close, kernel);
                  //Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.Open, kernel);
                  // Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.ERODE, kernel);
                  // Cv2.MorphologyEx(grayThresh.Clone(), grayThresh, OpenCvSharp.MorphTypes.Close, kernel);
                  //Cv2.Erode(grayThresh.Clone(), grayThresh, kernel, null, 1);

                  float angle = rect2draw.Angle;
                  OpenCvSharp.Size2f rect_size = rect2draw.Size;
                  if (rect_size.Width < rect_size.Height)
                  {
                      //angle = 90 + angle;
                      angle = 90 + angle;
                  }
                  else
                  {
                      //angle = -1*angle;
                      rect2draw.Size = new Size2f(rect_size.Height, rect_size.Width);
                  }
                 // if (rect2draw.Angle < -45.0) {
                    //angle += 90;
                    // swap width and height
                    //rect2draw.Size = new Size2f(rect_size.Height, rect_size.Width);
                  //}
                  Mat rot_mat = OpenCvSharp.Cv2.GetRotationMatrix2D(rect2draw.Center, angle, 1.0);
                  digitImage = new Mat();
                  OpenCvSharp.Cv2.WarpAffine(src, digitImage, rot_mat, src.Size());

                  OpenCvSharp.Cv2.WarpAffine(grayAfterTresh, grayAfterTresh, rot_mat, src.Size());
                  rect2draw.Angle = 90; // obracamy prostokat, aby potem wyciac z takim samym kontem jak obrazek
                  drawRect(grayAfterTresh, rect2draw);

                  

                  digitImage = new Mat(digitImage, rect2draw.BoundingRect());

                  Cv2.CvtColor(digitImage, digitImage, OpenCvSharp.ColorConversionCodes.BGR2GRAY);
                  /*
                  //OpenCvSharp.Cv2.AdaptiveThreshold(digitImage, digitImage, 255, AdaptiveThresholdTypes.GaussianC, ThresholdTypes.Binary, 17, 3);
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

                  
                  
                  digitImagesList[0] = new Mat(digitImage, new Rect(new OpenCvSharp.Point(0, 0), new OpenCvSharp.Size(w, h))).Clone();
                  digitImagesList[1] = new Mat(digitImage, new Rect(new OpenCvSharp.Point(w, 0), new OpenCvSharp.Size(w, h))).Clone();
                  digitImagesList[2] = new Mat(digitImage, new Rect(new OpenCvSharp.Point(w * 2, 0), new OpenCvSharp.Size(w, h))).Clone();
                  digitImagesList[3] = new Mat(digitImage, new Rect(new OpenCvSharp.Point(w * 3, 0), new OpenCvSharp.Size(w, h))).Clone();
                  digitImagesList[4] = new Mat(digitImage, new Rect(new OpenCvSharp.Point(w * 4, 0), new OpenCvSharp.Size(w, h))).Clone();
                  */
                  //result = findDigitOnImg();
                  
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
        private int[] findDigitOnImg(int prog){
            double[] wyniki = new double[10];
            int[] wynikiOst = new int[digitImagesList.Length];
            int i = 0;
            Mat digitTmp = new Mat();
           // Cv2.CvtColor(digitImage, digitImage, OpenCvSharp.ColorConversionCodes.BGR2GRAY);
            OpenCvSharp.Cv2.Threshold(digitImage.Clone(), digitTmp, prog, 255, OpenCvSharp.ThresholdTypes.Binary); // -70
            Mat kernel = Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Ellipse, new OpenCvSharp.Size(2, 2));
            Cv2.MorphologyEx(digitTmp.Clone(), digitTmp, OpenCvSharp.MorphTypes.ERODE, kernel);
            digitImageTh = digitTmp;
            int w = digitTmp.Width / 5;
            int h = digitTmp.Height;

            digitImagesList[0] = new Mat(digitTmp, new Rect(new OpenCvSharp.Point(0, 0), new OpenCvSharp.Size(w, h))).Clone();
            digitImagesList[1] = new Mat(digitTmp, new Rect(new OpenCvSharp.Point(w, 0), new OpenCvSharp.Size(w, h))).Clone();
            digitImagesList[2] = new Mat(digitTmp, new Rect(new OpenCvSharp.Point(w * 2, 0), new OpenCvSharp.Size(w, h))).Clone();
            digitImagesList[3] = new Mat(digitTmp, new Rect(new OpenCvSharp.Point(w * 3, 0), new OpenCvSharp.Size(w, h))).Clone();
            digitImagesList[4] = new Mat(digitTmp, new Rect(new OpenCvSharp.Point(w * 4, 0), new OpenCvSharp.Size(w, h))).Clone();

            foreach (Mat m in digitImagesList)
            {
                Mat[] contours;
                Mat hierarchy = new Mat();

                

                OpenCvSharp.Cv2.FindContours(m.Clone(), out contours, hierarchy, OpenCvSharp.RetrievalModes.Tree, OpenCvSharp.ContourApproximationModes.ApproxSimple);
                //OpenCvSharp.Cv2.MedianBlur(m.Clone(), m, 3);
                /// Find the rotated rectangles and ellipses for each contour
                List<RotatedRect> minRect = new List<RotatedRect>();//minRect(contours);
                contours.OrderBy(x => Cv2.BoundingRect(x).Size.Width * Cv2.BoundingRect(x).Size.Height);
                Rect r2 = new Rect();
                if (contours.Length > 1)
                {
                    r2 = Cv2.BoundingRect(contours[1]);
                   // Cv2.Rectangle(m, r2, new Scalar(0, 255, 0));
                }
                else
                {
                    r2 = Cv2.BoundingRect(contours[0]);
                    //Cv2.Rectangle(m, r2, new Scalar(0, 255, 0));
                }
                Mat digit = new Mat(m, r2).Clone();
                Cv2.Resize(digit, digit, new OpenCvSharp.Size(9, 18));
                //OpenCvSharp.Cv2.MedianBlur(digit.Clone(), digit, 3);
                //Mat kernel = Cv2.GetStructuringElement(OpenCvSharp.MorphShapes.Ellipse, new OpenCvSharp.Size(2, 2));
                //Cv2.MorphologyEx(digit.Clone(), digit, OpenCvSharp.MorphTypes.ERODE, kernel);
                
                digitImagesList[i] = digit;
                Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(digit);
                bmp.Save("tst.bmp");
                int[,] img = new int[9, 18];

                for (int k = 0; k < 9; k++)
                {
                    for (int j = 0; j < 18; j++)
                    {
                        Color oc = bmp.GetPixel(k, j);
                        img[k, j] = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                    }
                }
                for (int liczba = 0; liczba < 10; liczba++)
                {
                //    Mat imgTmp = new Mat("../../digital/" + liczba.ToString() + "v.jpg");
                    //Cv2.CvtColor(imgTmp, imgTmp, OpenCvSharp.ColorConversionCodes.BGR2GRAY);
                wyniki[liczba] = compareDigit(img, liczba); //compareDigit(digit, imgTmp);//
                }
                wynikiOst[i] = Array.IndexOf(wyniki, wyniki.Min());
                /*
                foreach (Mat mc in contours)
                {
                    
                    Rect r = Cv2.BoundingRect(mc);

                    if (biggestRect < r.Size.Height * r.Size.Width)
                    {
                        biggestRect = r.Size.Height * r.Size.Width;
                        
                        rect2draw = tmp;
                        tmp = r;
                        
                    }
                    
                    //minRect.Add(OpenCvSharp.Cv2.MinAreaRect(mc));
                    //drawRect(m, OpenCvSharp.Cv2.MinAreaRect(mc));
                   // Cv2.Rectangle(m, Cv2.BoundingRect(mc), new Scalar(0, 255, 0));
                }
                 */
                
                i++;
            }


            return wynikiOst;
        }
        /*
        private int[] findDigitOnImg()
        {
            int i = 0;
            List<int> scoreInDigit = new List<int>();
            int[] wyniki = new int[10];
            int[] wynikiOst = new int[digitImagesList.Length];

            foreach (Mat m in digitImagesList)
            {
                int imgWid = m.Width;
                int imgHei = m.Height;

                //thresh = cv2.threshold(warped, 0, 255,cv2.THRESH_BINARY_INV | cv2.THRESH_OTSU)[1]
                Mat thresh = m;//new Mat(m.Size(), OpenCvSharp.MatType.CV_8UC1);
                //Mat src = new Mat(m.Size(), OpenCvSharp.MatType.CV_8UC1);
                if (imgWid == 9 && imgHei == 18)
                {
                    // porownujemy oba obrazy
                }
                else if (imgWid < 9 || imgHei < 18)
                {
                    // skalujemy do rozmiaru 9x18 i porownujemy
                }
                else if (imgWid > 9 && imgHei > 18)
                {
                    for (int liczba = 0; liczba < 10; liczba++)
                    {
                        int wid = 9;
                        int hei = 18;

                        while (wid < (imgWid -4) && hei < (imgHei - 12))
                        {
                            int poczWid = imgWid / 2 - wid / 2 + 2;
                            int poczHei = imgHei / 2 - hei / 2 + 6;
                            //wycinamy interesujacy nas obszar
                            Mat tmp = new Mat(m, new Rect(new OpenCvSharp.Point(poczWid, poczHei), new OpenCvSharp.Size(wid, hei))).Clone();
                            // skalujemy go do wymiaru 8x18
                            Cv2.Resize(tmp, tmp, new OpenCvSharp.Size(9, 18));
                            // odczytujemy

                            Bitmap bmp = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(tmp);
                            bmp.Save("tst.bmp");
                            int[,] img = new int[9, 18];

                            for (int k = 0; k < 9; k++)
                            {
                                for (int j = 0; j < 18; j++)
                                {
                                    Color oc = bmp.GetPixel(k, j);
                                    img[k, j] = (int)((oc.R * 0.3) + (oc.G * 0.59) + (oc.B * 0.11));
                                }
                            }
                            scoreInDigit.Add(compareDigit(img, liczba));
                            // zwiekszamy rozmiar do wyciecia
                            wid++;
                            hei++;
                        }
                        scoreInDigit.Sort();
                        wyniki[liczba] = scoreInDigit[0];
                        scoreInDigit.Clear();
                    }

                    wynikiOst[i] = Array.IndexOf(wyniki, wyniki.Min());
                }
                // Wyszukiwanie cyfr na wycietym fragmencie prostokata
                //Mat[] contours;
                //Mat hierarchy = new Mat();
                //OpenCvSharp.Cv2.FindContours(thresh.Clone(), out contours, hierarchy, OpenCvSharp.RetrievalModes.List, OpenCvSharp.ContourApproximationModes.ApproxSimple);
               
               // foreach (Mat mc in contours)
                //{
                    //Cv2.Rectangle(thresh, Cv2.BoundingRect(mc), new Scalar(0, 255, 0));
                //    Mat tmp = new Mat();
                //    Cv2.ApproxPolyDP(mc,tmp, 1, false);
                   // Cv2.Rectangle(thresh, Cv2.BoundingRect(tmp), new Scalar(0, 255, 0));
                //}

                
                /// Find the rotated rectangles and ellipses for each contour
                //List<RotatedRect> minRect = new List<RotatedRect>();//minRect(contours);

               // foreach (Mat mc in contours)
               // {
              //      minRect.Add(OpenCvSharp.Cv2.MinAreaRect(mc));
              //  }
              //  RotatedRect rect2draw = new RotatedRect();

              //  foreach (var rect in minRect)
              //  {
                   // drawRect(thresh, rect);
              //  }
                
                //digitImagesList[i] = thresh;
                i++;
            }
            return wynikiOst;
        }
        */
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
        public Bitmap getDigitImageTh()
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(digitImageTh);
        }

        public Bitmap getDigitsImg(int number, int treshold)
        {
            Mat toRet = digitImagesList[number].Clone();
            //Cv2.CvtColor(toRet, toRet, OpenCvSharp.ColorConversionCodes.BGR2GRAY);
            //OpenCvSharp.Cv2.Threshold(toRet, toRet, treshold, 255, OpenCvSharp.ThresholdTypes.Binary);
            Cv2.Resize(toRet, toRet, new OpenCvSharp.Size(40, 96));
            digitImagesList[number] = toRet;
            //OpenCvSharp.Cv2.MedianBlur(toRet, toRet, 3);
           // readDigital(number);
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(toRet);
        }
        public string getResult(int prog)
        {
            result = findDigitOnImg(prog);
            string res = "";
            foreach (int r in result)
            {
                res += r.ToString();
            }
            return res;
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
            //compareDigit(img);

            return 0;
        }

        public double compareDigit(Mat img1, Mat img2)
        {
            return Cv2.Norm(img1, img2, NormTypes.Hamming2);
        }

        public int compareDigit(int[,] img, int liczba)
        {
            int imgw  = 40; //szerokosc obrazka
            int imgh = 96; // wysokosc obrazka
            int[] score = new int[10];
            int[,] imgTab;

            int tabHeight; // wysokosc cyferki domyslnej
            int tabWidth; // szerokosc cyerki domyslnej

            int diffHeight; // roznica wysokosci
            int diffWidth;// roznica szerokosci
            int scoreTmp = 0;

                imgTab = orgDigital[liczba];

                //Cv2.Compare(img,imgTab,CmpTypes.)
                //Cv2.Norm(new Mat(), new Mat());
            
                tabHeight = imgTab.GetLength(1);
                tabWidth = imgTab.GetLength(0);
                diffHeight = imgh - tabHeight;
                diffWidth = imgw - tabWidth;
                        for (int i = 0; i < tabWidth; i++)
                        {
                            for (int j = 0; j < tabHeight; j++)
                            {
                                int wynik = (img[i, j] - imgTab[i, j]);// *(Math.Abs(tabWidth / 2 - i) * Math.Abs(tabHeight / 2 - j));
                                scoreTmp += Math.Abs(wynik);
                            }
                        }           
            return scoreTmp;
        }
        public List<int[,]> loadDigitaFromImage(){
            
            int imgw = 40; //szerokosc obrazka
            int imgh = 96; // wysokosc obrazka
            int[,] imgTab;
            List<int[,]> img2 = new List<int[,]>();
            for (int k = 0; k < 10; k++) // potem zrobic k< 10
            {
                Bitmap img = new Bitmap("../../digital/"+k.ToString()+"v.jpg");
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
