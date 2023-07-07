using OpenCvSharp;

namespace ImageProcessor
{
    public static class xnp
    {
        //https://stackoverflow.com/questions/52016253/python-np-array-equivilent-in-opencv-opencvsharp
        public static Mat zeros_like(Mat a)
        {
            return new Mat(a.Size(), a.Type(), new Scalar(0));
        }
        public static Mat array(params byte[] array_like)
        {
            return Mat.FromArray(array_like);
        }

        public static Mat array(Vec3b[] array_like)
        {
            return Mat.FromArray(array_like);
        }

        public static Mat array(Vec2b[] array_like)
        {
            return Mat.FromArray(array_like);
        }

        public static Mat array(Vec4b[] array_like)
        {
            return Mat.FromArray(array_like);
        }
    }
}
