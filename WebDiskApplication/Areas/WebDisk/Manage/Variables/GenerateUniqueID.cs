using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebDiskApplication.Areas.WebDisk.Manage.Utils;

namespace WebDiskApplication.Areas.WebDisk.Manage.Variables
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


    }
}