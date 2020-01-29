using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using System.Web;
using WebDiskApplication.EFDB;
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
                            string forderId = HttpContext.Current.Request.Form["folderId"]; var parentFolder = db.FolderManage.Where(x => x.FolderId == forderId).SingleOrDefault();
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
                            newFile.Starred = false;
                            newFile.Trashed = false;
                            newFile.OwnerId = userId;
                            newFile.FileName = Path.GetFileNameWithoutExtension(file.FileName);
                            newFile.FileExtension = Path.GetExtension(file.FileName).TrimStart('.');
                            newFile.FileSize = file.ContentLength;
                            newFile.RealPath = realPath;
                            newFile.ServerPath = serverPath;

                            db.FileManage.Add(newFile);
                            db.SaveChanges();


                        }

                    }

                }
            }

            return Ok();
        }

        //파일 삭제하기
        [Route("api/disk/file/trash/{fileId}")]
        [HttpPut]
        public IHttpActionResult MoveFileToTrash(string fileId)
        {
            FileManage file = null;
            using (var db = new WebDiskDBEntities())
            {
                file = db.FileManage.Where(x => x.FileId == fileId).SingleOrDefault();

                if (file != null)
                {
                    file.Trashed = true;
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
        public IHttpActionResult RenameFile(string fileId)
        {
            FileManage renamedFile = null;
            using (var db = new WebDiskDBEntities())
            {
                renamedFile = db.FileManage.Where(x => x.FileId == fileId).SingleOrDefault();

                //실제 파일 이름도 변경하기

                string folderPath = Path.Combine(renamedFile.ServerPath, renamedFile.OwnerId, renamedFile.RealPath);
                string newFileName = HttpContext.Current.Request.Form["fileName"];
                string sourceFilePath = Path.Combine(folderPath, renamedFile.FileName + "." + renamedFile.FileExtension);
                string targetFilePath = Path.Combine(folderPath, newFileName + "." + renamedFile.FileExtension);


                if (renamedFile != null)
                {
                    if (!File.Exists(targetFilePath))
                    {
                        File.Move(sourceFilePath, targetFilePath);
                    }
                    else
                    {
                        File.Delete(sourceFilePath);
                    }
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
        public IHttpActionResult MoveFileToStarred(string fileId)
        {
            FileManage currentFile = null;
            using (var db = new WebDiskDBEntities())
            {
                currentFile = db.FileManage.Where(x => x.FileId == fileId).SingleOrDefault();

                if (currentFile != null)
                {
                    currentFile.Starred = true;
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

        //파일 중요처리 해제하기
        //파일 중요처리하기
        [Route("api/disk/file/cancel/starred/{fileId}")]
        [HttpPut]
        public IHttpActionResult MoveStarredFileToBack(string fileId)
        {
            FileManage currentFile = null;
            using (var db = new WebDiskDBEntities())
            {
                currentFile = db.FileManage.Where(x => x.FileId == fileId).SingleOrDefault();

                if (currentFile != null)
                {
                    currentFile.Starred = false;
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
        public IHttpActionResult MoveFileToTargetFolder(string fileId)
        {
            FileManage sourceFile = null;
            using (var db = new WebDiskDBEntities())
            {
                string targetFolderId = HttpContext.Current.Request.Form["folderId"];
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
            using(var db = new WebDiskDBEntities())
            {
                List<FolderManage> allFoldersThisYear = db.FolderManage.Where(x => x.LastModified.Value.Year == DateTime.Now.Year || x.LastAccessed.Value.Year == DateTime.Now.Year).ToList();
                List<FolderManage> todayFolders = null;

                foreach(var folders in allFoldersThisYear.ToList())
                {
                    //오늘
                    if(folders.LastModified.Value.Day == DateTime.Now.Day)
                    {
                        todayFolders.Add(folders);
                        allFoldersThisYear.Remove(folders);
                    }

                    //이번주인 것을 어떻게 판단할지


                    //




                    //이번주

                    //이번달

                    //올해

                    //이전 
                }
            }


            
            return Ok();

        }

        //중요 파일 가져오기
        [Route("api/disk/files/starred")]
        [HttpGet]
        public IHttpActionResult GetStarredFiles()
        {
            List<FileManage> starredFiles = null;
            using (var db = new WebDiskDBEntities())
            {
                starredFiles = db.FileManage.Where(x => x.Starred == true).ToList();
            }
            return Ok(starredFiles);
        }

        //휴지통 파일 가져오기
        [Route("api/disk/files/deleted")]
        [HttpGet]
        public IHttpActionResult GetDeletedFiles()
        {
            List<FileManage> deletedFiles = null;

            using (var db = new WebDiskDBEntities())
            {
                deletedFiles = db.FileManage.Where(x => x.Trashed == true).ToList(); //null이랑 count체크 
            }
            return Ok(deletedFiles);
        }





    }
}
