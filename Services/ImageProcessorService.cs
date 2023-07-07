using OpenCvSharp;
using static System.Net.WebRequestMethods;
using System.Web;
using Microsoft.AspNetCore.Mvc;

namespace ImageProcessor.Services
{
    public class ImageProcessorService
    {
        private readonly IHostEnvironment _hostingEnvironment;
        private readonly IHttpContextAccessor _http;
        private readonly string _path;
        private readonly IUrlHelper _url;
        public ImageProcessorService(IHostEnvironment hostingEnvironment, IHttpContextAccessor http, IUrlHelper url)
        {
            _url = url;
            _http = http;
            _hostingEnvironment = hostingEnvironment;
            _path = Path.Combine(_hostingEnvironment.ContentRootPath, "wwwroot", "Content");
        }
        public string ImagemPros(IFormFile request)
        {

            var photo = Mat.FromStream(request.OpenReadStream(), ImreadModes.Unchanged);
            var name = Guid.NewGuid().ToString();

            using (var resources = new ResourcesTracker())
            {
                // 1 - Carregar imagem do pin
                Mat? alpha = null;
                var pinImg = resources.T(Cv2.ImRead("pinWhite.png", ImreadModes.Unchanged));
                if (pinImg.Channels() > 3)
                {
                    var newPin = pinImg.Split();
                    alpha = newPin.LastOrDefault();
                    Cv2.Merge(new Mat[] { newPin[0], newPin[1], newPin[2] }, pinImg);
                }
                // 2 - Carregar imagem a ser processada
                photo = ImgCrop(photo);
                var newPhoto = photo.Split();
                var photoArray = newPhoto.Length == 1 ? new Mat[] { newPhoto[0], newPhoto[0], newPhoto[0] }
                                                      : new Mat[] { newPhoto[0], newPhoto[1], newPhoto[2] };
                Cv2.Merge(photoArray, photo);
                // 3 - Separar área do conteudo
                var lower = Mat.FromArray(new byte[] { 0, 240, 0 });
                var upper = Mat.FromArray(new byte[] { 70, 255, 70 });
                var tresh = pinImg.InRange(lower, upper);
                var kernel = Cv2.GetStructuringElement(MorphShapes.Rect, new Size(6, 6));
                var morph = tresh.MorphologyEx(MorphTypes.Dilate, kernel);
                var mask = morph;
                Mat la = new();
                Cv2.Merge(new Mat[] { 255 - mask, 255 - mask, 255 - mask }, la);
                var imgx = pinImg.BitwiseAnd(la).ToMat();
                // 4 - redimensionar tamanho da imagem
                Resize(photo, pinImg.Size().Width);
                // 5 - cropar imagem dimensionada
                var cropMask = mask.SubMat(0, photo.Height, 0, photo.Width);
                Cv2.Merge(new Mat[] { cropMask, cropMask, cropMask }, cropMask);
                var cropped = cropMask.BitwiseAnd(photo).ToMat();
                // 6 - inserir imagem cropada na imagem do pin
                var expanded = cropped.CopyMakeBorder(0, pinImg.Height - cropped.Height, 0, 0, BorderTypes.Constant);
                // 7 - retirar fundo da imagem do pin
                if (alpha != null)
                {
                    Cv2.Merge(imgx.Split().Append(alpha).ToArray(), imgx);
                }
                else
                {
                    imgx = removeBG(imgx);
                }
                var result = removeBG(expanded).BitwiseOr(imgx).ToMat();
                Cv2.ImWrite($"{_path}/{name}.png", result);
                return ImageUrl($"{HttpUtility.UrlEncode(name)}.png");
            }
        }

        public void DeleteImage(string filename)
        {
            System.IO.File.Delete($"{_path}/{filename}");
        }
        #region Private Methods
        private string ImagePath(string filename) =>
           filename != null ?
               _url.Content($"~/content/{filename}") :
               null;
        private string ImageUrl(string filename)
        {
            var request = _http.HttpContext.Request;
            var baseAddress = $"{request.Scheme}://{request.Host}{request.PathBase}";
            if (filename?.StartsWith("http") != false)
            {
                return filename;
            }

            return $"{baseAddress}{ImagePath(filename)}";
        }
        private Mat removeBG(Mat img)
        {
            var tmp = img.CvtColor(ColorConversionCodes.BGR2GRAY);
            var alpha = tmp.Threshold(0, 255, ThresholdTypes.Binary);
            Mat[] layers = img.Split();
            Cv2.Merge(layers.Append(alpha).ToArray(), img);
            return img;
        }
        private void Resize(Mat img, int width)
        {
            float r = (float)width / (float)img.Width;
            Size dim = new(width, img.Height * r);
            Cv2.Resize(img, img, dim, 0, 0, InterpolationFlags.Linear);
        }
        private Mat ImgCrop(Mat img)
        {
            var cropped = img;
            if (img.Height > img.Width)
            {
                var minimum = (img.Height - img.Width) / 2;
                cropped = img.SubMat(minimum, img.Width + minimum, 0, img.Width);
            }
            else if (img.Height < img.Width)
            {
                var minimum = (img.Width - img.Height) / 2;
                cropped = img.SubMat(0, img.Height, minimum, img.Height + minimum);
            }
            return cropped;
        }
        #endregion
    }
}
