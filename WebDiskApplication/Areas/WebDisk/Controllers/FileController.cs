using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web;
using WebDiskApplication.EFDB;
using WebDiskApplication.Areas.WebDisk.Models;
using WebDiskApplication.Areas.WebDisk.Manage.Variables;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace WebDiskApplication.Areas.WebDisk.Controllers
{
    public class FileController : ApiController
    {

        //파일 업로드
        [Route("api/disk/file")]
        [HttpPost]
        public IHttpActionResult UploadFile()
        {
            using (var db = new WebDiskDBEntities())
            {
                if (HttpContext.Current.Request.Files.Count > 0) //파일개수가 0이상이면 
                {
                    HttpFileCollection files = HttpContext.Current.Request.Files;
                    for (int i = 0; i < files.Count; i++)
                    {
                        var newFile = new FileManage();
                        HttpPostedFile file = files[i];
                        if (file.ContentLength > 0)//업로드한 파일의 크기를 가져옴 
                        {
                            string forderId = HttpContext.Current.Request.Form["folderId"]; 
                            var parentFolder = db.FolderManage.Where(x => x.FolderId == forderId).SingleOrDefault();
                            string fileName = Path.GetFileName(file.FileName);
                            string serverPath = @"C:\WebDisk";
                            string userId = "kg93s4";
                            string realPath = parentFolder.RealPath;
                            string fullPath = Path.Combine(serverPath, userId, realPath);
                            if (!Directory.Exists(fullPath))
                            {
                                Directory.CreateDirectory(fullPath);
                            }

                            file.SaveAs(Path.Combine(fullPath, fileName));

                            newFile.FileId = GenerateUniqueID.FileID();
                            newFile.FolderId = forderId;
                            newFile.CreatedDate = DateTime.Now;
                            newFile.LastModified = DateTime.Now;
                            newFile.LastAccessed = DateTime.Now;
                            newFile.Type = "file";
                            newFile.Starred = false;
                            newFile.Trashed = false;
                            newFile.OwnerId = userId;
                            newFile.FileName = Path.GetFileNameWithoutExtension(file.FileName);
                            newFile.FileExtension = Path.GetExtension(file.FileName).TrimStart('.');
                            newFile.FileSize = file.ContentLength;
                            newFile.RealPath = realPath;
                            newFile.ServerPath = serverPath;

                            #region 업로드된 파일의 마인 타입을 지정한다.
                            Manage.Enums.MimeType isMimeType = Manage.Enums.MimeType.Unknown;
                            switch (newFile.FileExtension.ToLower())
                            {
                                case "jpg":
                                case "png":
                                case "gif":
                                case "bmp":
                                    isMimeType = Manage.Enums.MimeType.Image;
                                    break;
                                case "doc":
                                case "docx":
                                case "xls":
                                case "xlsx":
                                case "pdf":
                                    isMimeType = Manage.Enums.MimeType.Document;
                                    break;
                            }
                            newFile.MimeType = (byte)isMimeType;
                            #endregion

                            db.FileManage.Add(newFile);
                            db.SaveChanges();


                            #region 업로드된 파일의 썸네일 생성하기
                            string rv_FilePreview = Manage.Utils.CreatePreview.CheckFileMap(Path.Combine(fullPath, fileName), newFile.FileId);
                            if (rv_FilePreview.Contains("오류"))
                            {
                                throw new Exception(rv_FilePreview + " << 오류발생했다!!!!");
                                //1: 원래 생겨야하는 썸네일 등의 이미지를 '파일 썸네일 생성 오류안내' 이미지로 대체한다.
                                //2: 여기에 후속 대체 기능을 추가해야 한다.
                            }
                            else
                            {
                                
                            }
                            #endregion
                        }
                    }
                }
            }
            return Ok();
        }

        //파일 삭제하기
        [Route("api/disk/file/trash/{fileId}")]
        [HttpPut]
        public IHttpActionResult ChangeTrashedStatus(string fileId, [FromBody] bool trashed)
        {
            FileManage file = null;
            using (var db = new WebDiskDBEntities())
            {
                file = db.FileManage.Where(x => x.FileId == fileId).SingleOrDefault();

                if (file != null)
                {
                    file.Trashed = trashed;
                    if (file.Starred == true) file.Starred = false;
                    file.LastModified = DateTime.Now;
                    db.SaveChanges();

                }
                else
                {
                    return Ok(new { msg = "No corresponding items exist." });
                }

            }

            return Ok(file);
        }

        //파일 복원
        [Route("api/disk/file/recovery/{fileId}")]
        [HttpPut]
        public IHttpActionResult RecoverFile(string fileId)
        {
            FileManage file = null;
            using (var db = new WebDiskDBEntities())
            {
                file = db.FileManage.Where(x => x.FileId == fileId).SingleOrDefault();

                if (file != null)
                {
                    file.Trashed = false;
                    file.LastModified = DateTime.Now;

                    db.SaveChanges();
                }
                else
                {
                    return Ok(new { msg = "No corresponding items exist." });
                }

            }

            return Ok(file);
        }


        /// <summary>
        /// 사용자가 휴지통에 있는 파일을 수동으로 영구 삭제한다. 
        /// </summary>
        /// <param name="fileId">삭제하려는 파일 아이디</param>
        /// <returns></returns>
        [Route("api/disk/file/{fileId}")]
        [HttpDelete]
        public IHttpActionResult DeleteFileForever(string fileId)
        {
            FileManage deletedFile = null;
            using (var db = new WebDiskDBEntities())
            {
                deletedFile = db.FileManage.Where(x => x.Trashed == true && x.FileId == fileId).SingleOrDefault();

                string fileName = deletedFile.FileName + '.' + deletedFile.FileExtension;
                string filePath = Path.Combine(deletedFile.ServerPath, deletedFile.OwnerId, deletedFile.RealPath, fileName);

                if (deletedFile != null)
                {
                    if (File.Exists(filePath))
                    {
                        File.Delete(filePath);

                    }
                    db.FileManage.Remove(deletedFile);
                    db.SaveChanges();

                }
                else
                {
                    return Ok(new { msg = "No corresponding items exist" });
                }

            }
            return Ok(deletedFile);
        }

        //파일 이름 변경하기
        [Route("api/disk/file/rename/{fileId}")]
        [HttpPut]
        public IHttpActionResult RenameFile(string fileId, [FromBody]string newFileName)
        {
            FileManage renamedFile = null;
            using (var db = new WebDiskDBEntities())
            {
                renamedFile = db.FileManage.Where(x => x.FileId == fileId).SingleOrDefault();

                //실제 파일 이름도 변경하기

                string folderPath = Path.Combine(renamedFile.ServerPath, renamedFile.OwnerId, renamedFile.RealPath);
                string sourceFilePath = Path.Combine(folderPath, renamedFile.FileName + "." + renamedFile.FileExtension);
                string targetFilePath = Path.Combine(folderPath, newFileName + "." + renamedFile.FileExtension);


                if (renamedFile != null)
                {
                    if (!File.Exists(targetFilePath))
                    {
                        File.Move(sourceFilePath, targetFilePath);
                    }
                    //[질문]다운로드는 변경된 파일명으로 되나 파일은 그대로 남아 있음 소문자 대문자 어떻게하지?
                    renamedFile.FileName = newFileName;
                    renamedFile.LastModified = DateTime.Now;
                    db.SaveChanges();

                }
                else
                {
                    return Ok(new { msg = "No corresponding items exist." });
                }


            }
            return Ok(renamedFile);
        }


        //파일 영구 자동 삭제 
        [HttpDelete]
        public IHttpActionResult DeleteDiskFileAuto()
        {
            return Ok();
        }

        //파일 중요처리하기
        [Route("api/disk/file/starred/{fileId}")]
        [HttpPut]
        public IHttpActionResult MoveFileToStarred(string fileId, [FromBody] bool starred)
        {
            FileManage currentFile = null;
            using (var db = new WebDiskDBEntities())
            {
                currentFile = db.FileManage.Where(x => x.FileId == fileId).SingleOrDefault();

                if (currentFile != null)
                {
                    currentFile.Starred = starred;
                    currentFile.LastModified = DateTime.Now;
                    db.SaveChanges();
                }
                else
                {
                    return Ok(new { msg = "No corresponding items exist." });
                }

            }

            return Ok(currentFile);
        }

        //파일 이동
        [Route("api/disk/file/move/{fileId}")]
        [HttpPut]
        public IHttpActionResult MoveFileToTargetFolder(string fileId, [FromBody] string targetFolderId)
        {
            FileManage sourceFile = null;
            using (var db = new WebDiskDBEntities())
            {
                sourceFile = db.FileManage.Where(x => x.FileId == fileId).SingleOrDefault();

                if (sourceFile != null)
                {
                    FolderManage targetFolder = db.FolderManage.Where(x => x.FolderId == targetFolderId).SingleOrDefault();
                    string fullFileName = sourceFile.FileName + "." + sourceFile.FileExtension;

                    string commonPath = Path.Combine(sourceFile.ServerPath, sourceFile.OwnerId);

                    string sourceFilePath = Path.Combine(commonPath, sourceFile.RealPath, fullFileName);
                    string targetFilePath = Path.Combine(commonPath, targetFolder.RealPath, fullFileName);

                    if (sourceFilePath != targetFilePath)
                    {
                        if (File.Exists(targetFilePath)) //만약 타겟 폴더에 해당 파일이 있으면
                        {
                            File.Delete(sourceFilePath); //기존 폴더에 있는 파일은 지운다.
                        }
                        else
                        {
                            File.Move(sourceFilePath, targetFilePath); //만약에 타겟폴더에 해당 파일이 없으면
                            sourceFile.FolderId = targetFolderId;
                            sourceFile.RealPath = targetFolder.RealPath;
                            db.SaveChanges();
                        }
                    }

                }
                else
                {
                    return Ok(new { msg = "No corresponding items exist." });
                }
            }
            return Ok(sourceFile);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="fileId"></param>
        /// <returns></returns>

        [Route("api/disk/file/download/{fileId}")]
        [HttpGet]
        public HttpResponseMessage DownloadFile(string fileId)
        {
            using (var db = new WebDiskDBEntities())
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);

                FileManage file = db.FileManage.Where(x => x.FileId == fileId).SingleOrDefault();
                string fileName = file.FileName + '.' + file.FileExtension;
                string filePath = Path.Combine(file.ServerPath, file.OwnerId, file.RealPath, file.FileName + '.' + file.FileExtension);

                if (!File.Exists(filePath))
                {
                    //Throw 404 (Not Found) exception if File not found.
                    response.StatusCode = HttpStatusCode.NotFound;
                    response.ReasonPhrase = string.Format("File not found: {0} .", fileName);
                    throw new HttpResponseException(response);
                }

                byte[] bytes = File.ReadAllBytes(filePath);


                response.Content = new ByteArrayContent(bytes);

                response.Content.Headers.ContentLength = bytes.LongLength;

                response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                response.Content.Headers.ContentDisposition.FileName = fileName;

                response.Content.Headers.ContentType = new MediaTypeHeaderValue(MimeMapping.GetMimeMapping(fileName));
                return response;
            }

        }



        //최근 파일 가져오기
        [Route("api/disk/files/recent")]
        [HttpGet]
        public IHttpActionResult GetRecentFiles()
        {
            RecentFiles recentFiles = new RecentFiles();
            using (var db = new WebDiskDBEntities())
            {

                List<FileManage> allFilesThisYear = db.FileManage.Where(x => (x.LastModified.Value.Year == DateTime.Now.Year || x.LastAccessed.Value.Year == DateTime.Now.Year) && x.Trashed == false).ToList();
                List<FileManage> todayFiles = new List<FileManage>();
                List<FileManage> lastSevenDaysFiles = new List<FileManage>();
                List<FileManage> lastThirtyDaysFiles = new List<FileManage>();
                List<FileManage> lastSixMonthFiles = new List<FileManage>();
                List<FileManage> lastOneYearFiles = new List<FileManage>();
                List<FileManage> beforeFiles = new List<FileManage>();

                foreach (var file in allFilesThisYear.ToList())
                {
                    DateTime Today = DateTime.Now.Date;
                    DateTime lastModifiedDate = file.LastModified.Value.Date;
                    DateTime lastAccessedDate = file.LastAccessed.Value.Date;
                    /*
                     오늘(년, 월, 일이 같아야한다)
                     */
                    if (lastModifiedDate == Today || lastAccessedDate == Today)
                    {
                        todayFiles.Add(file);
                        allFilesThisYear.Remove(file);
                        continue;
                    }

                    /*
                     지난7일
                     일주일 전 ~ 어제
                     */
                    if ((lastModifiedDate < Today && lastModifiedDate >= DateTime.Now.AddDays(-7).Date) ||
                        (lastAccessedDate < Today && lastAccessedDate >= DateTime.Now.AddDays(-7).Date))
                    {
                        lastSevenDaysFiles.Add(file);
                        allFilesThisYear.Remove(file);
                        continue;
                    }

                    /*
                     최근 30일
                     30일 전 ~ 어제
                     */
                    if ((lastModifiedDate < Today && lastModifiedDate >= DateTime.Now.AddDays(-30).Date) ||
                        (lastAccessedDate < Today && lastAccessedDate >= DateTime.Now.AddDays(-30).Date))
                    {
                        lastThirtyDaysFiles.Add(file);
                        allFilesThisYear.Remove(file);
                        continue;
                    }


                    //최근 6개월
                    if ((lastModifiedDate < Today && lastModifiedDate >= DateTime.Now.AddMonths(-6).Date) ||
                        (lastAccessedDate < Today && lastAccessedDate >= DateTime.Now.AddMonths(-6).Date))
                    {
                        lastSixMonthFiles.Add(file);
                        allFilesThisYear.Remove(file);
                        continue;
                    }

                    //최근 1년 

                    if ((lastModifiedDate < Today && lastModifiedDate >= DateTime.Now.AddYears(-1).Date) ||
                        (lastAccessedDate < Today && lastAccessedDate >= DateTime.Now.AddYears(-1).Date))
                    {
                        lastOneYearFiles.Add(file);
                        allFilesThisYear.Remove(file);
                        continue;
                    }

                    // 그 이전 

                    beforeFiles.Add(file);


                }
                recentFiles.Today = todayFiles;
                recentFiles.LastSevenDays = lastSevenDaysFiles;
                recentFiles.LastThirtyDays = lastThirtyDaysFiles;
                recentFiles.LastSixMonth = lastSixMonthFiles;
                recentFiles.LastOneYear = lastOneYearFiles;
                recentFiles.Before = beforeFiles;

            }

            return Ok(recentFiles);

        }



        //파일 마임타입별 파일 가져오기
        [Route("api/disk/files/mimetype/{mimeType}")]
        [HttpGet]
        public IHttpActionResult GetRecentFiles(string mimeType)
        {
            byte RR_MimeType = (byte)Manage.Variables.GenerateUniqueID.MyFilesMimeType(mimeType);
            List<FileManage> mimetypeFiles = null;
            using (var db = new WebDiskDBEntities())
            {
                mimetypeFiles = db.FileManage.Where(x => x.MimeType == RR_MimeType).ToList();

                #region 만약 MimeType 을 사용하지않고, 직접 FileExtention 으로 검색 조건을 지정할 시 코드의 예제입니다.
                if (false)
                {
                    IQueryable<FileManage> queryList = null;
                    queryList = db.FileManage.Where(x => x.FileExtension == "jpg");
                    queryList = db.FileManage.Where(x => x.FileExtension == "png");
                    queryList = db.FileManage.Where(x => x.FileExtension == "gif");
                    queryList = db.FileManage.Where(x => x.FileExtension == "bmp");
                    mimetypeFiles = queryList.ToList();
                }
                #endregion
            }
            return Ok(mimetypeFiles);
        }
    }
}
