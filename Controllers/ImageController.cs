using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Rewrite;
using OpenCvSharp;
using System.Net.NetworkInformation;
using System.Resources;
using System.Security.Cryptography;
using System.Threading;
using Numpy;
using static System.Net.Mime.MediaTypeNames;
using static System.Net.WebRequestMethods;
using System.Text.Encodings.Web;
using System.Web;
using ImageProcessor.Services;

namespace ImageProcessor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private readonly ImageProcessorService _imageSevice;
        public ImageController(ImageProcessorService imageSevice)
        {
            _imageSevice = imageSevice;
        }
        [HttpPost]
        public IActionResult UploadImage(IFormFile photo) => (photo != null && photo.Length > 0) ? Ok(_imageSevice.ImagemPros(photo)) : BadRequest("Nenhuma imagem foi enviada.");

        [HttpDelete("{filename}")]
        public IActionResult DeleteImage(string filename) {
            try
            {
                _imageSevice.DeleteImage(filename);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        } 


    }

}
