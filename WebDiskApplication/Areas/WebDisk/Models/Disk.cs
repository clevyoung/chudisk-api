using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebDiskApplication.EFDB;

namespace WebDiskApplication.Areas.WebDisk.Models
{
    public class Disk
    {
        public string FolderId { get; set; }
        public Folder Folder { get; set;}
    }

    public class Folder
    {
        public List<FolderManage> subFolders { get; set; }
        public List<FileManage> subFiles { get; set; }
    }

    public class FolderTree
    {
        public string Path { get; set; }
        public string FolderId { get; set; }
        public string FolderName { get; set; }
        public int Subfoldercnt { get; set; }
        public List<FolderTree> children { get; set; }
    }


    public class FolderInfo
    {
        public string folderId { get; set; }
        public string folderName { get; set; }
    }

    public class RecentFiles
    {
        public List<FileManage> Today { get; set; }
        public List<FileManage> LastSevenDays { get; set; }
        public List<FileManage> LastThirtyDays { get; set; }
        public List<FileManage> LastSixMonth { get; set; }
        public List<FileManage> LastOneYear { get; set; }
        public List<FileManage> Before { get; set; }
    }




    
}