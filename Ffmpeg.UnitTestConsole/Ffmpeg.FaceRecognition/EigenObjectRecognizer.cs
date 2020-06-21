using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using Emgu.CV.Structure;

namespace Emgu.CV
{

    //https://sourceforge.net/projects/libface/
    //https://stackoverflow.com/questions/2837523/using-eigenobjectrecognizer
    /// <summary>
    /// An object recognizer using PCA (Principle Components Analysis)
    /// </summary>
    [Serializable]
    public class EigenObjectRecognizer

    {
        private Image<Gray, Single>[] _eigenImages;
        private Image<Gray, Single> _avgImage;
        private Matrix<float>[] _eigenValues;
        private string[] _labels;
        private double _eigenDistanceThreshold;

        /// <summary>
        /// Get the eigen vectors that form the eigen space
        /// </summary>
        /// <remarks>The set method is primary used for deserialization, do not attemps to set it unless you know what you are doing</remarks>
        public Image<Gray, Single>[] EigenImages
        {
            get { return _eigenImages; }
            set { _eigenImages = value; }
        }

        /// <summary>
        /// Get or set the labels for the corresponding training image
        /// </summary>
        public String[] Labels
        {
            get { return _labels; }
            set { _labels = value; }
        }

        /// <summary>
        /// Get or set the eigen distance threshold.
        /// The smaller the number, the more likely an examined image will be treated as unrecognized object. 
        /// Set it to a huge number (e.g. 5000) and the recognizer will always treated the examined image as one of the known object. 
        /// </summary>
        public double EigenDistanceThreshold
        {
            get { return _eigenDistanceThreshold; }
            set { _eigenDistanceThreshold = value; }
        }

        /// <summary>
        /// Get the average Image. 
        /// </summary>
        /// <remarks>The set method is primary used for deserialization, do not attemps to set it unless you know what you are doing</remarks>
        public Image<Gray, Single> AverageImage
        {
            get { return _avgImage; }
            set { _avgImage = value; }
        }

        /// <summary>
        /// Get the eigen values of each of the training image
        /// </summary>
        /// <remarks>The set method is primary used for deserialization, do not attemps to set it unless you know what you are doing</remarks>
        public Matrix<float>[] EigenValues
        {
            get { return _eigenValues; }
            set { _eigenValues = value; }
        }

        private EigenObjectRecognizer()
        {
        }


        /// <summary>
        /// Create an object recognizer using the specific tranning data and parameters, it will always return the most similar object
        /// </summary>
        /// <param name="images">The images used for training, each of them should be the same size. It's recommended the images are histogram normalized</param>
        /// <param name="termCrit">The criteria for recognizer training</param>
        public EigenObjectRecognizer(Image<Gray, Byte>[] images, ref MCvTermCriteria termCrit)
           : this(images, GenerateLabels(images.Length), ref termCrit)
        {
        }

        private static String[] GenerateLabels(int size)
        {
            String[] labels = new string[size];
            for (int i = 0; i < size; i++)
                labels[i] = i.ToString();
            return labels;
        }

        /// <summary>
        /// Create an object recognizer using the specific tranning data and parameters, it will always return the most similar object
        /// </summary>
        /// <param name="images">The images used for training, each of them should be the same size. It's recommended the images are histogram normalized</param>
        /// <param name="labels">The labels corresponding to the images</param>
        /// <param name="termCrit">The criteria for recognizer training</param>
        public EigenObjectRecognizer(Image<Gray, Byte>[] images, String[] labels, ref MCvTermCriteria termCrit)
           : this(images, labels, 0, ref termCrit)
        {
        }

        /// <summary>
        /// Create an object recognizer using the specific tranning data and parameters
        /// </summary>
        /// <param name="images">The images used for training, each of them should be the same size. It's recommended the images are histogram normalized</param>
        /// <param name="labels">The labels corresponding to the images</param>
        /// <param name="eigenDistanceThreshold">
        /// The eigen distance threshold, (0, ~1000].
        /// The smaller the number, the more likely an examined image will be treated as unrecognized object. 
        /// If the threshold is &lt; 0, the recognizer will always treated the examined image as one of the known object. 
        /// </param>
        /// <param name="termCrit">The criteria for recognizer training</param>
        public EigenObjectRecognizer(Image<Gray, Byte>[] images, String[] labels, double eigenDistanceThreshold, ref MCvTermCriteria termCrit)
        {
            Debug.Assert(images.Length == labels.Length, "The number of images should equals the number of labels");
            Debug.Assert(eigenDistanceThreshold >= 0.0, "Eigen-distance threshold should always >= 0.0");

            CalcEigenObjects(images, ref termCrit, out _eigenImages, out _avgImage);

            /*
            _avgImage.SerializationCompressionRatio = 9;

            foreach (Image<Gray, Single> img in _eigenImages)
                //Set the compression ration to best compression. The serialized object can therefore save spaces
                img.SerializationCompressionRatio = 9;
            */

            _eigenValues = Array.ConvertAll<Image<Gray, Byte>, Matrix<float>>(images,
                delegate (Image<Gray, Byte> img)
                {
                    return new Matrix<float>(EigenDecomposite(img, _eigenImages, _avgImage));
                });

            _labels = labels;

            _eigenDistanceThreshold = eigenDistanceThreshold;
        }

        #region static methods
        /// <summary>
        /// Caculate the eigen images for the specific traning image
        /// </summary>
        /// <param name="trainingImages">The images used for training </param>
        /// <param name="termCrit">The criteria for tranning</param>
        /// <param name="eigenImages">The resulting eigen images</param>
        /// <param name="avg">The resulting average image</param>
        public static void CalcEigenObjects(Image<Gray, Byte>[] trainingImages, ref MCvTermCriteria termCrit, out Image<Gray, Single>[] eigenImages, out Image<Gray, Single> avg)
        {
            int width = trainingImages[0].Width;
            int height = trainingImages[0].Height;

            IntPtr[] inObjs = Array.ConvertAll<Image<Gray, Byte>, IntPtr>(trainingImages, delegate (Image<Gray, Byte> img) { return img.Ptr; });

            //if (termCrit.max_iter <= 0 || termCrit.max_iter > trainingImages.Length)
            //   termCrit.max_iter = trainingImages.Length;

            //int maxEigenObjs = termCrit.max_iter;

            if (termCrit.MaxIter <= 0 || termCrit.MaxIter > trainingImages.Length)
                termCrit.MaxIter = trainingImages.Length;

            int maxEigenObjs = termCrit.MaxIter;

            #region initialize eigen images
            eigenImages = new Image<Gray, float>[maxEigenObjs];
            for (int i = 0; i < eigenImages.Length; i++)
                eigenImages[i] = new Image<Gray, float>(width, height);
            IntPtr[] eigObjs = Array.ConvertAll<Image<Gray, Single>, IntPtr>(eigenImages, delegate (Image<Gray, Single> img) { return img.Ptr; });
            #endregion

            avg = new Image<Gray, Single>(width, height);

            CvInvokeCvaux.cvCalcEigenObjects(
             inObjs,
             ref termCrit,
             eigObjs,
             null,
             avg.Ptr);
        }

        /// <summary>
        /// Decompose the image as eigen values, using the specific eigen vectors
        /// </summary>
        /// <param name="src">The image to be decomposed</param>
        /// <param name="eigenImages">The eigen images</param>
        /// <param name="avg">The average images</param>
        /// <returns>Eigen values of the decomposed image</returns>
        public static float[] EigenDecomposite(Image<Gray, Byte> src, Image<Gray, Single>[] eigenImages, Image<Gray, Single> avg)
        {
            return CvInvokeCvaux.cvEigenDecomposite(
                src.Ptr,
                Array.ConvertAll<Image<Gray, Single>, IntPtr>(eigenImages, delegate (Image<Gray, Single> img) { return img.Ptr; }),
                avg.Ptr);
        }
        #endregion

        /// <summary>
        /// Given the eigen value, reconstruct the projected image
        /// </summary>
        /// <param name="eigenValue">The eigen values</param>
        /// <returns>The projected image</returns>
        public Image<Gray, Byte> EigenProjection(float[] eigenValue)
        {
            Image<Gray, Byte> res = new Image<Gray, byte>(_avgImage.Width, _avgImage.Height);
            CvInvokeCvaux.cvEigenProjection(
             Array.ConvertAll<Image<Gray, Single>, IntPtr>(_eigenImages, delegate (Image<Gray, Single> img) { return img.Ptr; }),
             eigenValue,
             _avgImage.Ptr,
             res.Ptr);
            return res;
        }

        /// <summary>
        /// Get the Euclidean eigen-distance between <paramref name="image"/> and every other image in the database
        /// </summary>
        /// <param name="image">The image to be compared from the training images</param>
        /// <returns>An array of eigen distance from every image in the training images</returns>
        public float[] GetEigenDistances(Image<Gray, Byte> image)
        {
            using (Matrix<float> eigenValue = new Matrix<float>(EigenDecomposite(image, _eigenImages, _avgImage)))
                return Array.ConvertAll<Matrix<float>, float>(_eigenValues,
                    delegate (Matrix<float> eigenValueI)
                    {
                        return (float)CvInvokeCvaux.cvNorm(eigenValue.Ptr, eigenValueI.Ptr, Emgu.CV.CvEnum.NORM_TYPE.CV_L2, IntPtr.Zero);
                    });
        }

        /// <summary>
        /// Given the <paramref name="image"/> to be examined, find in the database the most similar object, return the index and the eigen distance
        /// </summary>
        /// <param name="image">The image to be searched from the database</param>
        /// <param name="index">The index of the most similar object</param>
        /// <param name="eigenDistance">The eigen distance of the most similar object</param>
        /// <param name="label">The label of the specific image</param>
        public void FindMostSimilarObject(Image<Gray, Byte> image, out int index, out float eigenDistance, out String label)
        {
            float[] dist = GetEigenDistances(image);

            index = 0;
            eigenDistance = dist[0];
            for (int i = 1; i < dist.Length; i++)
            {
                if (dist[i] < eigenDistance)
                {
                    index = i;
                    eigenDistance = dist[i];
                }
            }
            label = Labels[index];
        }

        /// <summary>
        /// Try to recognize the image and return its label
        /// </summary>
        /// <param name="image">The image to be recognized</param>
        /// <returns>
        /// String.Empty, if not recognized;
        /// Label of the corresponding image, otherwise
        /// </returns>
        public String Recognize(Image<Gray, Byte> image)
        {
            int index;
            float eigenDistance;
            String label;
            FindMostSimilarObject(image, out index, out eigenDistance, out label);

            return (_eigenDistanceThreshold <= 0 || eigenDistance < _eigenDistanceThreshold) ? _labels[index] : String.Empty;
        }



    }

    public partial class CvInvoke1
    {

        /// <summary>
        /// The file name of the cxcore library
        /// </summary>
        public const string CXCORE_LIBRARY = "cxcore210";
        /// <summary>
        /// The file name of the cv library
        /// </summary>
        public const string CV_LIBRARY = "cv210";
        /// <summary>
        /// The file name of the highgui library
        /// </summary>
        public const string HIGHGUI_LIBRARY = "highgui210";
        /// <summary>
        /// The file name of the cvaux library
        /// </summary>
        public const string CVAUX_LIBRARY = "cvaux210";
        /// <summary>
        /// The file name of the cvextern library
        /// </summary>
        public const string EXTERN_LIBRARY = "cvextern210";

        /// <summary>
        /// Returns the calculated norm. The multiple-channel array are treated as single-channel, that is, the results for all channels are combined. 
        /// </summary>
        /// <param name="arr1">The first source image</param>
        /// <param name="arr2">The second source image. If it is IntPtr.Zero, the absolute norm of arr1 is calculated, otherwise absolute or relative norm of arr1-arr2 is calculated</param>
        /// <param name="normType">Type of norm</param>
        /// <param name="mask">The optional operation mask</param>
        /// <returns>The calculated norm</returns>
        [DllImport(CXCORE_LIBRARY)]
        public static extern double cvNorm(
            IntPtr arr1,
            IntPtr arr2,
            Emgu.CV.CvEnum.NORM_TYPE normType,
            IntPtr mask);
    }

}
namespace Emgu.CV.CvEnum
{


    /// <summary>
    /// IO type for eigen object related functions
    /// </summary>
    public enum EIGOBJ_TYPE
    {
        /// <summary>
        /// No callback
        /// </summary>
        CV_EIGOBJ_NO_CALLBACK = 0,
        /// <summary>
        /// input callback
        /// </summary>
        CV_EIGOBJ_INPUT_CALLBACK = 1,
        /// <summary>
        /// output callback
        /// </summary>
        CV_EIGOBJ_OUTPUT_CALLBACK = 2,
        /// <summary>
        /// both callback
        /// </summary>
        CV_EIGOBJ_BOTH_CALLBACK = 3
    }


    /// <summary>
    /// Type for cvNorm
    /// </summary>
    [Flags]
    public enum NORM_TYPE
    {
        /// <summary>
        /// if arr2 is NULL, norm = ||arr1||_C = max_I abs(arr1(I));
        /// if arr2 is not NULL, norm = ||arr1-arr2||_C = max_I abs(arr1(I)-arr2(I))
        /// </summary>
        CV_C = 1,
        /// <summary>
        /// if arr2 is NULL, norm = ||arr1||_L1 = sum_I abs(arr1(I));
        /// if arr2 is not NULL, norm = ||arr1-arr2||_L1 = sum_I abs(arr1(I)-arr2(I))
        /// </summary>
        CV_L1 = 2,
        /// <summary>
        /// if arr2 is NULL, norm = ||arr1||_L2 = sqrt( sum_I arr1(I)^2);
        /// if arr2 is not NULL, norm = ||arr1-arr2||_L2 = sqrt( sum_I (arr1(I)-arr2(I))^2 )
        /// </summary>
        CV_L2 = 4,
        /// <summary>
        /// 
        /// </summary>
        CV_NORM_MASK = 7,
        /// <summary>
        /// It is used in combination with either CV_C, CV_L1 or CV_L2
        /// </summary>
        CV_RELATIVE = 8,
        /// <summary>
        /// It is used in combination with either CV_C, CV_L1 or CV_L2
        /// </summary>
        CV_DIFF = 16,
        /// <summary>
        /// 
        /// </summary>
        CV_MINMAX = 32,
        /// <summary>
        /// 
        /// </summary>
        CV_DIFF_C = (CV_DIFF | CV_C),
        /// <summary>
        /// 
        /// </summary>
        CV_DIFF_L1 = (CV_DIFF | CV_L1),
        /// <summary>
        /// 
        /// </summary>
        CV_DIFF_L2 = (CV_DIFF | CV_L2),
        /// <summary>
        /// norm = ||arr1-arr2||_C/||arr2||_C
        /// </summary>
        CV_RELATIVE_C = (CV_RELATIVE | CV_C),
        /// <summary>
        /// norm = ||arr1-arr2||_L1/||arr2||_L1
        /// </summary>
        CV_RELATIVE_L1 = (CV_RELATIVE | CV_L1),
        /// <summary>
        /// norm = ||arr1-arr2||_L2/||arr2||_L2
        /// </summary>
        CV_RELATIVE_L2 = (CV_RELATIVE | CV_L2)
    }

}