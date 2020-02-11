using ImageProcessor;
using ImageProcessor.Imaging.Formats;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;

namespace WebDiskApplication.Areas.WebDisk.Manage.Utils
{
    public class CreatePreview
    {
        public static string CheckFileMap(string IsPath, string FileID)
        {
            string IS_Ext = System.IO.Path.GetExtension(IsPath);

            if (IS_Ext.Contains("jpg") || IS_Ext.Contains("png") || IS_Ext.Contains("gif") || IS_Ext.Contains("bmp"))
            {
                return CreateThumbnail(IsPath, FileID);
            }
            else
            {
                return "123113211231 == 구현하지 않았다.";
            }
        }

        public static string CreateThumbnail(string IsPath, string FileID)
        {
            HttpContext _context = HttpContext.Current;

            string ThumbSavePath = string.Format(@"{0}\Thumbnail\{1}\t.png", _context.Server.MapPath("createpreview"), FileID);
            string rv = ThumbSavePath;
            if (!System.IO.File.Exists(IsPath)) { rv = "오류: 파일이 없는디유?"; }
            //var UplaodFile = Image.FromFile(System.Web.Hosting.HostingEnvironment.MapPath(IsPath));
            byte[] photoBytes = File.ReadAllBytes(IsPath); // change imagePath with a valid image path
            // Format is automatically detected though can be changed.
            ISupportedImageFormat format = new JpegFormat { Quality = 70 };
            Size size = new Size(150, 0);
            using (MemoryStream inStream = new MemoryStream(photoBytes))
            {
                using (MemoryStream outStream = new MemoryStream())
                {
                    // Initialize the ImageFactory using the overload to preserve EXIF metadata.
                    using (ImageFactory imageFactory = new ImageFactory(preserveExifData: true))
                    {
                        // Load, resize, set the format and quality and save an image.
                        imageFactory.Load(inStream)
                                    .Resize(size)
                                    .Format(format)
                                    .Save(ThumbSavePath);
                    }
                    // Do something with the stream.
                }
            }

            return rv;
        }
    }
}