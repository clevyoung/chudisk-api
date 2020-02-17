using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebDiskApplication.Manage.Utils;

namespace WebDiskApplication.Manage.Variables
{
    public class GenerateUniqueID
    {
        //사용자 id
        private static EFDB.WebDiskDBEntities db = new EFDB.WebDiskDBEntities();
        public static string UserID()
        {
            string newId = RandomUtil.MixedIntChar(6, true);

            if (db.Users.Where(x => x.UserId == newId).Count() == 0) { return newId; } else { return UserID(); }
        }

        //파일id

        public static string FileID()
        {
            string newId = RandomUtil.MixedIntChar(10, true);
            if (db.FileManage.Where(x => x.FileId == newId).Count() == 0) { return newId; } else { return FileID(); }
        }

        //폴더 id
        public static string FolderID()
        {
            string newId = RandomUtil.MixedIntChar(10, true);
            if (db.FolderManage.Where(x => x.FolderId == newId).Count() == 0) { return newId; } else { return FileID(); }
        }
        public static string MyFilesMimeType(byte MineTypeNum)
        {
            string rv = null;
            switch (MineTypeNum)
            {
                case (byte)Manage.Enums.MimeType.Audio: rv = "Audio"; break;
                case (byte)Manage.Enums.MimeType.Document: rv = "Document"; break;
                case (byte)Manage.Enums.MimeType.Image: rv = "Audio"; break;
                case (byte)Manage.Enums.MimeType.Unknown: rv = "Unknown"; break;
                case (byte)Manage.Enums.MimeType.Video: rv = "Video"; break;
                case (byte)Manage.Enums.MimeType.Zip: rv = "Zip"; break;
            }
            return rv;
        }


        public static Manage.Enums.MimeType MyFilesMimeType(string MineTypeNum)
        {
            Manage.Enums.MimeType rv = Enums.MimeType.Unknown;
            switch (MineTypeNum)
            {
                case "any": rv = Manage.Enums.MimeType.Unknown; break;
                case "documents": rv = Manage.Enums.MimeType.Document; break;
                case "images": rv = Manage.Enums.MimeType.Image; break;
                case "videos": rv = Manage.Enums.MimeType.Video; break;
                case "audios": rv = Manage.Enums.MimeType.Audio; break;
                case "zip": rv = Manage.Enums.MimeType.Zip; break;
            }
            return rv;
        }

        public static string MyFilesMimeType(Manage.Enums.MimeType mimeType)
        {
            string rv = null;
            switch (mimeType)
            {
                case Manage.Enums.MimeType.Audio: rv = "Audio"; break;
                case Manage.Enums.MimeType.Document: rv = "Document"; break;
                case Manage.Enums.MimeType.Image: rv = "Audio"; break;
                case Manage.Enums.MimeType.Unknown: rv = "Unknown"; break;
                case Manage.Enums.MimeType.Video: rv = "Video"; break;
                case Manage.Enums.MimeType.Zip: rv = "Zip"; break;
            }
            return rv;
        }
    }
}