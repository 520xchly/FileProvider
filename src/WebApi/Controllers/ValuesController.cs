using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Hosting.Internal;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Controllers
{
    [Route("api/values")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private IHostingEnvironment _hostingEnvironment;
        public ValuesController(IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
        }

        // GET api/values
        [HttpGet("get")]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1", "value2" };
        }

        [HttpGet("token")]
        public ActionResult<string> Token()
        {
            return CreateToken(new Claim[] {
                new Claim("SelfUserId","2019"),
                new Claim("SelfUserName","liuyan2019ps"),
                new Claim("ValidTime","2019/11/26 12:40:00")
            });
        }

        /// <summary>
        /// 相当于静态资源授权，在configure中不提供静态资源文件查看，这里也可进行权限返回
        /// 可根据参数进行组装路径
        /// </summary>
        /// <returns></returns>
        [Authorize]
        [HttpGet("getimg")]
        public IActionResult GetImg()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(),
                            "statics", "image_gif.gif");

            return PhysicalFile(file, "image/svg+xml");
        }

        /// <summary>
        /// 直接文件返回，不经过静态资源处理
        /// </summary>
        /// <returns></returns>
        [HttpGet("getmyimage")]
        public IActionResult GetMyImage()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(),
                            "statics", "image_gif.gif");

            return PhysicalFile(file, "image/svg+xm");
        }

        /// <summary>
        /// base64图片返回
        /// </summary>
        /// <returns></returns>
        [HttpGet("getimg64")]
        public ActionResult<string> GetBase64Img()
        {
            var file = Path.Combine(Directory.GetCurrentDirectory(),
                            "statics", "test.png");
            BufferedStream stream = new BufferedStream(new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read));
            MemoryStream memoryStream = new MemoryStream();
            int b = 0;
            while (b != -1)
            {
                b = stream.ReadByte();
                memoryStream.WriteByte((byte)b);
            }
            memoryStream.Close();
            stream.Close();
            string res = $"data:image/gif;base64,{Convert.ToBase64String(memoryStream.ToArray())}";
            return res;
        }

        /// <summary>
        /// 多文件上传
        /// </summary>
        /// <param name="files">与表单的name一致</param>
        /// <param name="file2">与表单的name一致</param>
        /// <param name="filename">与表单的name一致</param>
        /// <returns></returns>
        [HttpPost("upload")]
        public ActionResult<string> Upload(List<IFormFile> files, IFormFile file2, [FromForm]string filename)
        {
            string webRootPath = _hostingEnvironment.WebRootPath; //null
            string contentRootPath = _hostingEnvironment.ContentRootPath; //src/webapi
            string path = Directory.GetCurrentDirectory(); //src/webapi
            //以file2单文件保存为例
            string fileExt = Path.GetExtension(file2.FileName); //文件扩展名，含“.”
            long fileLength = file2.Length; //字节
            string filepath = path + "\\upload\\";
            if (!Directory.Exists(filepath))
            {
                Directory.CreateDirectory(filepath);
            }
            //无法使用空格和特殊字符
            string newname = "2020-01-09-12-00-00文件" + fileExt;
            try
            {
                using (FileStream fs = System.IO.File.Create(filepath + newname))
                {
                    file2.CopyTo(fs);
                    fs.Flush();
                }
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
            }
            return HttpContext.Request.Query["guid"][0];
        }

        /// <summary>
        /// 下载文件 文件流的方式输出
        /// 可增加参数下载不同的文件
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        [HttpGet("downloadfile")]
        public IActionResult DownLoad(string fileName)
        {
            string path = Directory.GetCurrentDirectory(); //src/webapi
            string filePath = path + "/statics/test.png";
            var stream = System.IO.File.OpenRead(path + "/statics/test.png");
            string fileExt = Path.GetExtension(filePath);
            //获取文件的ContentType
            var provider = new FileExtensionContentTypeProvider();
            var memi = provider.Mappings[fileExt];
            return File(stream, memi, Path.GetFileName(filePath));
        }

        private string CreateToken(Claim[] claims)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("12er234rqwerwqerwer"));
            var credis = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var token = new JwtSecurityToken(
                issuer: "liuyan",
                audience: "liuyan",
                claims: claims,
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddMinutes(1),
                signingCredentials: credis
                );
            return "Bearer " + new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
