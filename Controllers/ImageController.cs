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
using Microsoft.Extensions.Logging;

namespace ImageProcessor.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ImageController : ControllerBase
    {
        private ILogger<ImageController> _logger;
        private readonly ImageProcessorService _imageSevice;
        public ImageController(ImageProcessorService imageSevice, ILogger<ImageController> logger)
        {
            _logger = logger;
            _imageSevice = imageSevice;
        }
        [HttpPost]
        public IActionResult UploadImage(IFormFile photo)
        {
            if (photo == null && photo.Length == 0)
                BadRequest("Nenhuma imagem foi enviada.");

            try
            {
                _logger.LogInformation($"<--- Inicio do processamento da imagem --->");
                var url = _imageSevice.ImagemPros(photo);
                _logger.LogInformation($"<--- Imagem processada com sucesso url: {url} --->");
                return Ok(url);
            }
            catch (Exception e)
            {
                _logger.LogError($"Messagem de erro: {e.Message}");
                _logger.LogError($"Stacktrace do erro: {e.StackTrace}");
                return BadRequest(e.Message);
            }           
        }
            
            
            

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
