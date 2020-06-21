
using Emgu.CV;
using Emgu.CV.Face;
using Emgu.CV.Structure;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks.Sources;

namespace Ffmpeg.FaceRecognition
{
    //https://docs.opencv.org/2.4/modules/contrib/doc/facerec/facerec_tutorial.html
    //https://csharp.hotexamples.com/examples/Emgu.CV/EigenObjectRecognizer/-/php-eigenobjectrecognizer-class-examples.html
    public class FaceDetection
    {
        public class Reusult
        {
            public Image<Bgr, Byte> Face { get; set; }
            public Rectangle Position { get; set; }
            public FaceRecognizer.PredictionResult PredictionResult { get; set; }
        }

        //http://www.emgu.com/wiki/index.php/Camera_Capture_in_7_lines_of_code
        string _pathFileImgOrigin;
        Image<Bgr, Byte> _imgOrigin;
        string _imgOriginFileExt;

        public Image<Bgr, Byte> ImageInput { get { return _imgOrigin; } }

        public FaceDetection WithInputFile(string fileImg)
        {
            _pathFileImgOrigin = fileImg;
            _imgOrigin = new Image<Bgr, byte>(_pathFileImgOrigin);
            _imgOriginFileExt = new FileInfo(_pathFileImgOrigin).Extension.Trim('.');
            return this;
        }

        public List<KeyValuePair<Image<Bgr, byte>, Rectangle>> Detect(Image<Bgr, Byte> imgInput)
        {
            if (string.IsNullOrEmpty(_pathFileImgOrigin)) throw new Exception("No input file");

            Image<Gray, Byte> faceInput = imgInput.Convert<Gray, byte>();

            faceInput._EqualizeHist();

            List<KeyValuePair<Image<Bgr, byte>, Rectangle>> faces = new List<KeyValuePair<Image<Bgr, byte>, Rectangle>>();


            using (CascadeClassifier faceDetector = new CascadeClassifier(
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FaceTest/haarcascade_frontalface_default.xml")))
            {
                //Detect the faces  from the gray scale image and store the locations as rectangle                   
                Rectangle[] facesDetected = faceDetector.DetectMultiScale(faceInput, 1.05,2, new Size(20, 20));

                foreach (var r in facesDetected)
                {
                    faces.Add(new KeyValuePair<Image<Bgr, byte>, Rectangle>(imgInput.GetSubRect(r), r));
                }
            }

            List<KeyValuePair<Image<Bgr, byte>, Rectangle>> founds = new List<KeyValuePair<Image<Bgr, byte>, Rectangle>>();

            foreach (var f in faces)
            {
                using (CascadeClassifier eyeDetector = new CascadeClassifier(
           Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FaceTest/haarcascade_eye.xml")))
                {
                    Rectangle[] eyes = eyeDetector.DetectMultiScale(f.Key, 1.05, 2, new Size(20, 20));
                    if (eyes.Length > 0)
                    {
                        founds.Add(f);
                    }

                }
                //    using (CascadeClassifier eyeLeftDetector = new CascadeClassifier(
                //  Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FaceTest/haarcascade_lefteye_2splits.xml")))
                //    {
                //        using (CascadeClassifier eyeRightDetector = new CascadeClassifier(
                //Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FaceTest/haarcascade_righteye_2splits.xml")))
                //        {
                //            Rectangle[] eyeL = eyeLeftDetector.DetectMultiScale(f.Key, 1.1, 10, new Size(20, 20));
                //            Rectangle[] eyeR = eyeRightDetector.DetectMultiScale(f.Key, 1.1, 10, new Size(20, 20));
                //            if (eyeL.Length > 0|| eyeR.Length>0)
                //            {
                //                founds.Add(f);
                //            }
                //        }

                //    }
            }
            //if (founds.Count == 0) return faces;

            return founds;
        }
        public void SaveDrawDetected(string outputfile)
        {
            if (string.IsNullOrEmpty(_pathFileImgOrigin)) throw new Exception("No input file");

            var faces = Detect(_imgOrigin);
            foreach (var f in faces)
            {

                CircleF circle = new CircleF();
                float x = (int)(f.Value.X + (f.Value.Width / 2));
                float y = (int)(f.Value.Y + (f.Value.Height / 2));
                circle.Radius = f.Value.Width / 2;

                _imgOrigin.Draw(new CircleF(new PointF(x, y), circle.Radius), new Bgr(Color.Yellow), 3);
            }
            _imgOrigin.Save(outputfile);
        }
        public void SaveCropDetected()
        {
            if (string.IsNullOrEmpty(_pathFileImgOrigin)) throw new Exception("No input file");

            var faces = Detect(_imgOrigin);
            var imgs = new List<Image<Bgr, byte>>();

            foreach (var f in faces)
            {
                ///_imgOrigin.Draw(f, new Bgr(0, double.MaxValue, 0), 3);

                var frameMultiply = 2;

                var xf = f.Value.X - frameMultiply * f.Value.Width;
                xf = xf < 0 ? 1 : xf;
                var yf = f.Value.Y - frameMultiply * f.Value.Height;
                yf = yf < 0 ? 1 : yf;
                int wf = f.Value.Width * 2 * frameMultiply;
                int hf = f.Value.Height * 2 * frameMultiply;
                if (wf + xf > _imgOrigin.Width) wf = _imgOrigin.Width - 1;
                if (hf + hf > _imgOrigin.Height) hf = _imgOrigin.Height - 1;

                var frameCrop = new Rectangle(xf, yf, wf, hf);

                var temp = _imgOrigin.GetSubRect(f.Value);
                imgs.Add(temp);

            }

            for (int i1 = 0; i1 < imgs.Count; i1++)
            {
                Image<Bgr, byte> i = imgs[i1];

                i.Save($"{_pathFileImgOrigin}.{i1}.{_imgOriginFileExt}");
            }

        }
        public void SaveCropDetected(int width, int height)
        {
            if (string.IsNullOrEmpty(_pathFileImgOrigin)) throw new Exception("No input file");

            var faces = Detect(_imgOrigin);
            var imgs = new List<Image<Bgr, byte>>();
            var marginX = width / 3;
            var marginY = height / 3;
            foreach (var f in faces)
            {
                var xf = f.Value.X - marginX;
                var yf = f.Value.Y - marginY;
                xf = xf < 0 ? 1 : xf;
                yf = yf < 0 ? 1 : yf;

                width = width < f.Value.Width ? f.Value.Width : width;
                height = height < f.Value.Height ? f.Value.Height : height;

                width = width + marginX;
                height = height + marginY;

                width = width + xf > _imgOrigin.Width ? _imgOrigin.Width - xf : width;
                height = height + yf > _imgOrigin.Height ? _imgOrigin.Height - yf : height;

                var frameCrop = new Rectangle(xf, yf, width, height);

                var temp = _imgOrigin.GetSubRect(frameCrop);
                imgs.Add(temp);

            }

            for (int i1 = 0; i1 < imgs.Count; i1++)
            {
                Image<Bgr, byte> i = imgs[i1];

                i.Save($"{_pathFileImgOrigin}.{i1}.{_imgOriginFileExt}");
            }

        }

        /// <summary>
        /// the lowest Distance will be more accurate
        /// </summary>
        /// <param name="fileFaceToCompare"></param>
        /// <returns></returns>
        public List<Reusult> CompareTo(params string[] fileFaceToCompare)
        {
            if (string.IsNullOrEmpty(_pathFileImgOrigin)) throw new Exception("No input file");
            //https://www.codeproject.com/Articles/261550/EMGU-Multiple-Face-Recognition-using-PCA-and-Paral

            //Eigen face recognizer
            //Parameters:	
            //      num_components – The number of components (read: Eigenfaces) kept for this Prinicpal 
            //          Component Analysis. As a hint: There’s no rule how many components (read: Eigenfaces) 
            //          should be kept for good reconstruction capabilities. It is based on your input data, 
            //          so experiment with the number. Keeping 80 components should almost always be sufficient.
            //
            //      threshold – The threshold applied in the prediciton. This still has issues as it work inversly to LBH and Fisher Methods.
            //          if you use 0.0 recognizer.Predict will always return -1 or unknown if you use 5000 for example unknow won't be reconised.
            //          As in previous versions I ignore the built in threhold methods and allow a match to be found i.e. double.PositiveInfinity
            //          and then use the eigen distance threshold that is return to elliminate unknowns. 
            //
            //NOTE: The following causes the confusion, sinc two rules are used. 
            //--------------------------------------------------------------------------------------------------------------------------------------
            //Eigen Uses
            //          0 - X = unknown
            //          > X = Recognised
            //
            //Fisher and LBPH Use
            //          0 - X = Recognised
            //          > X = Unknown
            //
            // Where X = Threshold value
            var facesFromOrigin = Detect(_imgOrigin);

            if (facesFromOrigin.Count == 0)
            {
                throw new Exception($"Not found any faces in original to compare: {_pathFileImgOrigin}");
            }

            List<Image<Bgr, byte>> listInputToTrain = new List<Image<Bgr, byte>>();

            foreach (var f in fileFaceToCompare)
            {
                var imgInput = new Image<Bgr, byte>(f);
                var facesToCompare = Detect(imgInput);

                if (facesToCompare.Count == 1)
                {
                    var inputFace = imgInput.GetSubRect(facesToCompare[0].Value);
                    listInputToTrain.Add(inputFace);
                }
            }

            if (listInputToTrain.Count <= 0) throw new Exception("No face to compare, check your fileFaceToCompare");

            var maxWidth = listInputToTrain.Min(i => i.Width);
            var maxHeight = listInputToTrain.Min(i => i.Height);

            var minWOrigin = facesFromOrigin.Min(i => i.Key.Width);
            var minHOrigin = facesFromOrigin.Min(i => i.Key.Height);

            maxWidth = maxWidth > minWOrigin ? minWOrigin : maxWidth;
            maxHeight = maxHeight > minHOrigin ? minHOrigin : maxHeight;

            List<Image<Gray, byte>> listInputGrayToTrain = new List<Image<Gray, byte>>();

            foreach (var img in listInputToTrain)
            {
                var temp = img.Resize(maxWidth, maxHeight, Emgu.CV.CvEnum.Inter.Cubic)
                    .Convert<Gray, byte>();
                temp._EqualizeHist();

                listInputGrayToTrain.Add(temp);
            }

            Mat[] trainData = listInputGrayToTrain.Select(i => i.Mat.IsContinuous ? i.Mat : i.Mat.Clone()).ToArray();
            int[] trainLabel = listInputGrayToTrain.Select(i => 1).ToArray();

            FaceRecognizer eigenRecognizer = new EigenFaceRecognizer(80, double.PositiveInfinity);

            eigenRecognizer.Train(trainData, trainLabel);

            //var trainDataFisher = trainData.ToList();
            //trainDataFisher.AddRange(trainData);
            //FaceRecognizer fisherRecognizer = new FisherFaceRecognizer(0, 3500);//4000
            //fisherRecognizer.Train(trainDataFisher.ToArray(), trainDataFisher.Select(i => 1).ToArray());

            FaceRecognizer lbphRecognizer = new LBPHFaceRecognizer(1, 8, 8, 8, 100);//50
            lbphRecognizer.Train(trainData, trainLabel);

            listInputGrayToTrain[0].Save(_pathFileImgOrigin + ".inputTrainData." + _imgOriginFileExt);

            List<Reusult> resultFound = new List<Reusult>();

            List<FaceRecognizer.PredictionResult> predictR = new List<FaceRecognizer.PredictionResult>();

            List<FaceRecognizer.PredictionResult> allPredict = new List<FaceRecognizer.PredictionResult>();

            for (int i = 0; i < facesFromOrigin.Count; i++)
            {
                //Image<Bgr, byte> item = _imgOrigin.GetSubRect(facesFromOrigin[i].Value).Convert<Bgr, byte>();

                Image<Bgr, byte> item = facesFromOrigin[i].Key;

                var f = item.Resize(maxWidth, maxHeight, Emgu.CV.CvEnum.Inter.Cubic).Convert<Gray, byte>();

                f._EqualizeHist();

                f.Save(_pathFileImgOrigin + $".needDetect{i}." + _imgOriginFileExt);

                var fm = f.Mat.IsContinuous ? f.Mat : f.Mat.Clone();

                var r0 = eigenRecognizer.Predict(fm);
                //var r1 = fisherRecognizer.Predict(fm);
                var r2 = lbphRecognizer.Predict(fm);
                allPredict.Add(r0);
                allPredict.Add(r2);
                if (r0.Label == 1 && r2.Label == 1)
                {
                    resultFound.Add(new Reusult
                    {
                        Face = facesFromOrigin[i].Key,
                        Position = facesFromOrigin[i].Value,
                        PredictionResult = new FaceRecognizer.PredictionResult
                        {
                            Distance =  r2.Distance,
                            Label = r0.Label
                        }
                    });
                    predictR.Add(r0);
                    predictR.Add(r2);
                }

            }

            return resultFound;
        }
    }
}
/*        public bool CompareTo(string fileFaceToCompare)
        {
            using (CascadeClassifier faceDetector = new CascadeClassifier(
              Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FaceTest/haarcascade_frontalface_default.xml")))
            {

            }

            //https://www.codeproject.com/Articles/261550/EMGU-Multiple-Face-Recognition-using-PCA-and-Paral
            var imgInput = new Image<Bgr, byte>(fileFaceToCompare);

            var facesToCompare = Detect(imgInput);
            if (facesToCompare.Count != 1)
            {
                throw new Exception("There 2 or 0 face in fileFaceToCompare");
            }

            var inputFace = imgInput.GetSubRect(facesToCompare[0]);

            var inputFaceGray = inputFace.Convert<Gray, byte>();

            var facesFromOrigin = Detect(_imgOrigin);

            var facesFromOriginGray = new List<Image<Gray, byte>>();

            foreach (var f in facesFromOrigin)
            {
                facesFromOriginGray.Add(_imgOrigin.GetSubRect(f).Convert<Gray, byte>());
            }
            Image<Gray, Byte>[] trainingImages = new Image<Gray, Byte>[] { inputFaceGray };
            String[] labels = new String[] { "OK" };

            MCvTermCriteria termCrit = new MCvTermCriteria(16, 0.001);

            EigenObjectRecognizer recognizer = new EigenObjectRecognizer(trainingImages, labels, 5000, ref termCrit);

            foreach (var f in facesFromOriginGray)
            {
                string label = recognizer.Recognize(f);
                if (label.Equals("OK", StringComparison.OrdinalIgnoreCase)) return true;
            }

            return false;
        }
*/
