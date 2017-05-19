using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Drawing;

namespace magisterka
{
    class FindEllipses
    {
        Mat src;
        Mat gray;
        int thresh;
        public FindEllipses(Bitmap bmpSrc, int _thresh, int _window, double _sigma)
        {
            src = OpenCvSharp.Extensions.BitmapConverter.ToMat(bmpSrc);
            gray = new Mat();
            thresh = _thresh;

            Cv2.CvtColor(src, gray, OpenCvSharp.ColorConversionCodes.BGR2GRAY);
            OpenCvSharp.Size window = new OpenCvSharp.Size(_window, _window);
            Cv2.GaussianBlur(gray, gray, window, _sigma);
        }
        private int getThresh()
        {
            return thresh; // tymczasowo
        }
        public void find()
        {
              Mat threshold_output = new Mat(gray.Size(),OpenCvSharp.MatType.CV_8UC1);
              Mat[] contours;
              Mat hierarchy = new Mat();
              /// Detect edges using Threshold
              OpenCvSharp.Cv2.Threshold(gray, threshold_output, getThresh(), 255, OpenCvSharp.ThresholdTypes.Binary);
              //threshold_output.CopyTo(gray);
              //threshold( src, threshold_output, thresh, 255, OpenCvSharp.ThresholdTypes.Binary );
              /// Find contours
              //OpenCvSharp.Point point = new OpenCvSharp.Point(0, 0);
              //OpenCvSharp.Cv2.FindContours(threshold_output, contours, hierarchy, )
              OpenCvSharp.Cv2.FindContours(threshold_output, out contours, hierarchy, OpenCvSharp.RetrievalModes.Tree, OpenCvSharp.ContourApproximationModes.ApproxSimple);
              //findContours( threshold_output, contours, hierarchy, CV_RETR_TREE, CV_CHAIN_APPROX_SIMPLE, Point(0, 0) );

              /// Find the rotated rectangles and ellipses for each contour
              List<RotatedRect> minRect = new List<RotatedRect>();//minRect(contours);
              List<RotatedRect> minEllipse = new List<RotatedRect>();//(contours.size());
              
            foreach(Mat m in contours){
                minRect.Add(OpenCvSharp.Cv2.MinAreaRect(m));
                   if( m.Rows > 5 ){ 
                       minEllipse.Add(OpenCvSharp.Cv2.FitEllipse(m)); 
                   }
            }
            /*
              for( int i = 0; i < contours.size(); i++ )
                 { minRect[i] = minAreaRect( Mat(contours[i]) );
                   if( contours[i].size() > 5 )
                     { minEllipse[i] = fitEllipse( Mat(contours[i]) ); }
                 }
            */
              /// Draw contours + rotated rects + ellipses
              Mat drawing = Mat.Zeros(threshold_output.Size(), OpenCvSharp.MatType.CV_8UC3);
            foreach (RotatedRect m in minEllipse)
            {
                OpenCvSharp.RNG rng = new RNG();
                Scalar color = new Scalar(rng.Uniform(0, 255), rng.Uniform(0,0), rng.Uniform(0,0));
                //OpenCvSharp.Cv2.DrawContours(drawing, contours, licz, color, 1, OpenCvSharp.LineTypes.Link8, hierarchy, 0);
                //OpenCvSharp.Cv2.DrawContours()
                // ellipse
                OpenCvSharp.Cv2.Ellipse( drawing, m, color, 2, OpenCvSharp.LineTypes.Link8);
            }
            gray = drawing; // tylko tymczasowo
            /*
              for( int i = 0; i< contours.size(); i++ )
                 {
                   Scalar color = Scalar( rng.uniform(0, 255), rng.uniform(0,255), rng.uniform(0,255) );
                   // contour
                   drawContours( drawing, contours, i, color, 1, 8, vector<Vec4i>(), 0, Point() );
                   // ellipse
                   ellipse( drawing, minEllipse[i], color, 2, 8 );
                   // rotated rectangle
                   Point2f rect_points[4]; minRect[i].points( rect_points );
                   for( int j = 0; j < 4; j++ )
                      line( drawing, rect_points[j], rect_points[(j+1)%4], color, 1, 8 );
                 }
             */
        }
        public Bitmap getImg()
        {
            return OpenCvSharp.Extensions.BitmapConverter.ToBitmap(gray);
        }
    }
}
