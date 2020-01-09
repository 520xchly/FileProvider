using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthentication("Bearer").AddJwtBearer(option =>
            {
                option.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    ValidateLifetime = true,
                    //token接收者 可设置多个 为string[]
                    ValidAudience = "liuyan",
                    //token颁发者 可设置多个 为string[]
                    ValidIssuer = "liuyan",
                    //这里的key要进行加密，需要引用Microsoft.IdentityModel.Tokens
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("12er234rqwerwqerwer"))
                };
            });

            //目录浏览
            services.AddDirectoryBrowser();

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            //静态文件提供查看
            //缓存
            var cachePeriod = env.IsDevelopment() ? "600" : "604800";
            //文件映射 文件扩展名到 MIME 内容类型的映射
            var provider = new FileExtensionContentTypeProvider();
            // Add new mappings
            provider.Mappings[".myapp"] = "application/x-msdownload";
            provider.Mappings[".htm3"] = "text/html";
            provider.Mappings[".image"] = "image/png";
            // Replace an existing mapping
            provider.Mappings[".rtf"] = "application/x-msdownload";
            //不提供 Remove gif.
            provider.Mappings.Remove(".gif");
            app.UseStaticFiles(new StaticFileOptions
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), "statics")),
                RequestPath = "/staticfiles",
                //OnPrepareResponse = ctx =>
                //{
                //    // 客户端缓存10分钟
                //    ctx.Context.Response.Headers.Append("Cache-Control", $"public, max-age={cachePeriod}");
                //}
                ContentTypeProvider = provider
            });

            //开启目录浏览，如果要查看单独文件，还需上面开启静态文件提供查看
            app.UseDirectoryBrowser(new DirectoryBrowserOptions
            {
                FileProvider = new PhysicalFileProvider(
                    Path.Combine(Directory.GetCurrentDirectory(), "statics")),
                RequestPath = "/staticfiles"
            });

            app.UseAuthentication();
            app.UseHttpsRedirection();
            app.UseMvc(r =>
            {
                r.MapRoute("mvcview", "view/{controller}/{action}");
            });
        }
    }
}
