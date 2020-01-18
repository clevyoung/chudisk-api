using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebDiskApplication.EFDB;

namespace WebDiskApplication.Areas.WebDisk.Models
{
    public class Disk
    {
        public List<FolderManage> Folders { get; set; }
        public List<FileManage> Files { get; set; }

    }

    public class FolderTree
    {
        public List<FolderPath> FolderPath { get; set; }
    }
  
    public class FolderPath
    {
        public string Path { get; set; }
        public string FolderId { get; set; }
        public string FolderName { get; set; }
        public int Subfoldercnt { get; set; }
        public string OwnerId { get; set; }
    }

    public class FolderInfo
    {
        public string folderId { get; set; }
        public string folderName { get; set; }
    }




    
}