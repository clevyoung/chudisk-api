using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using WebDiskApplication.EFDB;
using WebDiskApplication.Areas.WebDisk.Models;
using WebDiskApplication.Areas.WebDisk.Manage.Variables;
using System.IO;
using System.Net.Http;
using System.Net;
using System.Net.Http.Headers;

namespace WebDiskApplication.Areas.WebDisk.Controllers
{
    public class FolderController : ApiController
    {
        [Route("api/disk/folder/{folderId}")]
        [HttpGet]
        public IHttpActionResult GetDisk(string folderId)
        {
            List<FolderManage> folderList = null;
            List<FileManage> fileList = null;
            Disk disk = new Disk();

            using (var db = new WebDiskDBEntities())
            {
                folderList = db.FolderManage.Where(x => x.ParentId == folderId && x.Trashed == false).OrderByDescending(o => o.CreatedDate).ToList();
                fileList = db.FileManage.Where(x => x.FolderId == folderId && x.Trashed == false).OrderByDescending(o => o.CreatedDate).ToList();
                disk.Folders = folderList;
                disk.Files = fileList;
            }

            return Ok(disk);
        }

        //폴더 생성
        [Route("api/disk/folder")]
        [HttpPost]
        public IHttpActionResult CreateFolder()
        {
            FolderManage newFolder = new FolderManage();
            using (var db = new WebDiskDBEntities())
            {
                HttpContext context = HttpContext.Current;
                newFolder.FolderId = GenerateUniqueID.FolderID();
                string folderName = context.Request.Form["folderName"];
                string parentId = context.Request.Form["parentId"];
                newFolder.ParentId = parentId;
                var parentFolder = db.FolderManage.Where(x => x.FolderId == parentId).SingleOrDefault();
                newFolder.ServerPath = @"C:\WebDisk";
                newFolder.RealPath = Path.Combine(parentFolder.RealPath, folderName);
                newFolder.CreatedDate = DateTime.Now;
                newFolder.LastAccessed = DateTime.Now;
                newFolder.LastModified = DateTime.Now;
                newFolder.FolderName = folderName;
                newFolder.Starred = false;
                newFolder.Trashed = false;
                newFolder.OwnerId = "kg93s4";

                string fullpath = Path.Combine(newFolder.ServerPath, newFolder.OwnerId, newFolder.RealPath);
                if (!Directory.Exists(fullpath))
                {
                    Directory.CreateDirectory(fullpath);

                }

                db.FolderManage.Add(newFolder);
                db.SaveChanges();
            }
            return Ok(newFolder);
        }

        //폴더 탐색
        [Route("api/disk/folderTreePath/{folderId?}")]
        [HttpGet]
        public IHttpActionResult GetFolderTreePath(string folderId = null)
        {
            FolderTree folderTree = new FolderTree();

            using (var db = new WebDiskDBEntities())
            {
                List<FolderPath> folderPathList = new List<FolderPath>();

                //서브 폴더
                List<FolderManage> subFolders = db.FolderManage.Where(x => x.ParentId == folderId && x.Trashed == false).ToList();

                foreach (var subForder in subFolders)
                {
                    FolderPath subFolderPath = new FolderPath()
                    {
                        Path = subForder.RealPath,
                        FolderId = subForder.FolderId,
                        FolderName = subForder.FolderName,
                        OwnerId = subForder.OwnerId,
                        Subfoldercnt = db.FolderManage.Where(x => x.ParentId == subForder.FolderId).Count()
                    };

                    folderPathList.Add(subFolderPath);

                }

                folderTree.FolderPath = folderPathList;
            }

            return Ok(folderTree);

        }

        //폴더 path얻기
        [Route("api/disk/folderPath/{folderId}")]
        [HttpGet]
        public IHttpActionResult GetFolderPath(string folderId)
        {
            List<FolderInfo> forderPath = getPath(folderId);
            forderPath.Reverse();
            return Ok(forderPath);
        }

        public List<FolderInfo> getPath(string folderId, List<FolderInfo> routes = null)
        {
            FolderManage currentFolder = null;
            FolderManage parentFolder = null;
            List<FolderInfo> routeList;
            if (routes != null)
            {
                routeList = routes.ToList();
            }
            else
            {
                routeList = new List<FolderInfo>();
            }

            using (var db = new WebDiskDBEntities())
            {

                currentFolder = db.FolderManage.Where(x => x.FolderId == folderId).SingleOrDefault();

                if (routeList.Count == 0)
                {
                    routeList.Add(new FolderInfo
                    {
                        folderId = currentFolder.FolderId,
                        folderName = currentFolder.FolderName
                    });
                }
                parentFolder = db.FolderManage.Where(x => x.FolderId == currentFolder.ParentId).SingleOrDefault();

                if (parentFolder == null)
                {
                    return routeList;
                }

                routeList.Add(new FolderInfo
                {
                    folderId = parentFolder.FolderId,
                    folderName = parentFolder.FolderName
                });

            };

            return getPath(parentFolder.FolderId, routeList);
        }


        //폴더 이름 변경하기

        [Route("api/disk/folder/rename/{folderId}")]
        [HttpPut]
        public IHttpActionResult RenameFolder(string folderId)
        {
            FolderManage renamedfolder = null;
            using (var db = new WebDiskDBEntities())
            {
                renamedfolder = db.FolderManage.Where(x => x.FolderId == folderId).SingleOrDefault();
                //실제 폴더 이름도 변경하기
                string sourceFolderPath = Path.Combine(renamedfolder.ServerPath, renamedfolder.OwnerId, renamedfolder.RealPath);
                string newFolderName = HttpContext.Current.Request.Form["folderName"];
                string parentPath = sourceFolderPath.Substring(0, sourceFolderPath.LastIndexOf('\\'));
                string targetFolderPath = Path.Combine(parentPath, newFolderName);

                if (renamedfolder != null)
                {
                    if (!Directory.Exists(targetFolderPath))
                    {
                        Directory.Move(sourceFolderPath, targetFolderPath);
                        renamedfolder.FolderName = HttpContext.Current.Request.Form["folderName"];
                        renamedfolder.LastModified = DateTime.Now;
                        db.SaveChanges();
                    }
                    else
                    {
                        return Ok(new { msg = "같은 이름의 폴더가 존재합니다." });
                    }


                }
                else
                {
                    return Ok(new { msg = "No corresponding items exist." });
                }
            }
            return Ok(renamedfolder);
        }


        /// <summary>
        /// 폴더를 복사 
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        [Route("api/disk/folder/copy/{folderId}")]
        [HttpPut, HttpPost]
        public IHttpActionResult CopyFolder(string folderId)
        {
            using (var db = new WebDiskDBEntities())
            {
                string rootId = db.FolderManage.Where(x => x.ParentId == null).SingleOrDefault().FolderId;

                if (folderId != rootId)
                {
                    string targetFolderId = HttpContext.Current.Request.Form["folderId"];
                    CopyFolder(folderId, targetFolderId);

                }
            }
            return Ok(new { msg = "OK" });
        }

        public void CopyFolder(string folderId, string targetFolderId)
        {
            using (var db = new WebDiskDBEntities())
            {
                FolderManage sourceFolder = db.FolderManage.Where(x => x.FolderId == folderId).SingleOrDefault();
                FolderManage targetFolder = db.FolderManage.Where(x => x.FolderId == targetFolderId).SingleOrDefault();

                if (string.IsNullOrEmpty(targetFolder.OwnerId)) { targetFolder.OwnerId = sourceFolder.OwnerId; }
                string sourPath = Path.Combine(sourceFolder.ServerPath, sourceFolder.OwnerId, sourceFolder.RealPath);
                string tarPath = Path.Combine(targetFolder.ServerPath, targetFolder.OwnerId, targetFolder.RealPath, sourceFolder.FolderName);

                if (!Directory.Exists(tarPath))
                {
                    Directory.CreateDirectory(tarPath); //희영 > 여행 > 베트남
                }

                FolderManage copiedFolder = new FolderManage()
                {
                    FolderId = GenerateUniqueID.FolderID(),
                    FolderName = sourceFolder.FolderName,
                    CreatedDate = DateTime.Now,
                    LastModified = DateTime.Now,
                    LastAccessed = DateTime.Now,
                    OwnerId = sourceFolder.OwnerId,
                    ParentId = targetFolder.FolderId,
                    Starred = false,
                    Trashed = false,
                    ServerPath = sourceFolder.ServerPath,
                    RealPath = Path.Combine(targetFolder.RealPath, sourceFolder.FolderName)
                };

                db.FolderManage.Add(copiedFolder);
                db.SaveChanges();

                #region 하위 파일 복사하기
                List<FileManage> subFiles = db.FileManage.Where(x => x.FolderId == folderId).ToList();

                foreach (var subFile in subFiles)
                {
                    string fileName = subFile.FileName + '.' + subFile.FileExtension;
                    string filePath = Path.Combine(subFile.ServerPath, subFile.OwnerId, subFile.RealPath, fileName);
                    string tarFilePath = Path.Combine(tarPath, fileName);
                    if (!File.Exists(tarFilePath))
                    {
                        File.Copy(filePath, tarFilePath);
                    }

                    FileManage copiedFile = new FileManage()
                    {
                        FileId = GenerateUniqueID.FileID(),
                        FileName = subFile.FileName,
                        FileExtension = subFile.FileExtension,
                        FileSize = subFile.FileSize,
                        CreatedDate = DateTime.Now,
                        LastModified = DateTime.Now,
                        LastAccessed = DateTime.Now,
                        OwnerId = subFile.OwnerId,
                        Starred = false,
                        Trashed = false,
                        FolderId = copiedFolder.FolderId,
                        RealPath = copiedFolder.RealPath,
                        ServerPath = subFile.ServerPath
                    };

                    db.FileManage.Add(copiedFile);
                    db.SaveChanges();
                }

                #endregion


                #region 하위 폴더 복사하기
                List<FolderManage> subFolders = db.FolderManage.Where(x => x.ParentId == folderId).ToList();
                foreach (var subFolder in subFolders)
                {
                    CopyFolder(subFolder.FolderId, copiedFolder.FolderId);
                }
                #endregion
            }
        }


        /// <summary>
        /// 폴더 이동 메소드
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        [Route("api/disk/folder/move/{folderId}")]
        [HttpPut]
        public IHttpActionResult MoveFolder(string folderId)
        {
            using (var db = new WebDiskDBEntities())
            {
                string rootId = db.FolderManage.Where(x => x.ParentId == null).SingleOrDefault().FolderId;

                if (folderId != rootId)
                {
                    string targetFolderId = HttpContext.Current.Request.Form["folderId"];

                    FolderManage sourceFolder = db.FolderManage.Where(x => x.FolderId == folderId).SingleOrDefault();
                    FolderManage targetFolder = db.FolderManage.Where(x => x.FolderId == targetFolderId).SingleOrDefault();

                    if (string.IsNullOrEmpty(targetFolder.OwnerId)) { targetFolder.OwnerId = sourceFolder.OwnerId; }
                    string sourPath = Path.Combine(sourceFolder.ServerPath, sourceFolder.OwnerId, sourceFolder.RealPath);
                    string tarPath = Path.Combine(targetFolder.ServerPath, targetFolder.OwnerId, targetFolder.RealPath, sourceFolder.FolderName); //루트로 옮길경우 여기서 에러남 

                    if (!Directory.Exists(tarPath))
                    {
                        Directory.CreateDirectory(tarPath);
                    }

                    MoveFolder(folderId, tarPath);

                    Directory.Delete(sourPath);
                    sourceFolder.RealPath = Path.Combine(targetFolder.RealPath, sourceFolder.FolderName);
                    sourceFolder.ParentId = targetFolder.FolderId;
                    db.SaveChanges();

                }
            }

            return Ok(new { msg = "OK" });
        }


        /// <summary>
        /// 폴더 이동 재귀 메소드 호출자는 MoveFolder
        /// </summary>
        /// <param name="folderId">이동하려는 폴더의 아이디</param>
        /// <param name="targetPath">타겟폴더의 패스 </param>
        public void MoveFolder(string folderId, string targetPath)
        {
            using (var db = new WebDiskDBEntities())
            {
                #region 하위 파일 이동하기
                List<FileManage> subFiles = db.FileManage.Where(x => x.FolderId == folderId).ToList();
                foreach (var file in subFiles)
                {
                    string fileName = file.FileName + '.' + file.FileExtension;
                    string filePath = Path.Combine(file.ServerPath, file.OwnerId, file.RealPath, fileName);

                    File.Move(filePath, Path.Combine(targetPath, fileName));
                    string realPath = targetPath.Replace(Path.Combine(file.ServerPath, file.OwnerId), "").TrimStart('\\');
                    file.RealPath = realPath;
                    db.SaveChanges();
                }
                #endregion

                #region 하위 폴더 이동하기
                List<FolderManage> subFolders = db.FolderManage.Where(x => x.ParentId == folderId).ToList();
                foreach (var folder in subFolders)
                {
                    string sourPath = Path.Combine(folder.ServerPath, folder.OwnerId, folder.RealPath);
                    string tarPath = Path.Combine(targetPath, folder.FolderName);

                    if (!Directory.Exists(tarPath))
                    {
                        Directory.CreateDirectory(tarPath);
                    }
                    MoveFolder(folder.FolderId, tarPath);

                    Directory.Delete(sourPath);
                    string realPath = targetPath.Replace(Path.Combine(folder.ServerPath, folder.OwnerId), "").TrimStart('\\');
                    folder.RealPath = Path.Combine(realPath, folder.FolderName);
                    db.SaveChanges();
                }
                #endregion
            }
        }

        /// <summary>
        /// 해당 폴더 중요처리(바로가기 만들기)
        /// </summary>
        /// <param name="folderId">해당 폴더 아이디</param>
        /// <returns></returns>
        [Route("api/disk/folder/starred/{folderId}")]
        [HttpPut]
        public IHttpActionResult MoveFolderToStarred(string folderId)
        {
            FolderManage starredFolder = null;
            using (var db = new WebDiskDBEntities())
            {
                string rootId = db.FolderManage.Where(x => x.ParentId == null).SingleOrDefault().FolderId;

                #region 루트 폴더는 상태 처리 예외
                if (folderId != rootId)
                {
                    starredFolder = db.FolderManage.Where(x => x.FolderId == folderId).SingleOrDefault();

                    starredFolder.Starred = true;
                    db.SaveChanges();
                }
                #endregion
            }
            return Ok(starredFolder);
        }


        /// <summary>
        /// 폴더 중요처리 해제하기
        /// </summary>
        /// <param name="folderId">해당 폴더 아이디</param>
        /// <returns></returns>
        [Route("api/disk/folder/cancelstarred/{folderId}")]
        [HttpPut]
        public IHttpActionResult MoveStarredFolderToBack(string folderId)
        {
            FolderManage unStarredFolder = null;
            using (var db = new WebDiskDBEntities())
            {
                string rootId = db.FolderManage.Where(x => x.ParentId == null).SingleOrDefault().FolderId;

                #region 루트 폴더는 상태 처리 예외
                if (folderId != rootId)
                {
                    unStarredFolder = db.FolderManage.Where(x => x.FolderId == folderId).SingleOrDefault();

                    unStarredFolder.Starred = false;
                    db.SaveChanges();
                }
                #endregion

            }

            return Ok(unStarredFolder);
        }


        /// <summary>
        /// 휴지통 상태 처리
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        [Route("api/disk/folder/trash/{folderId}")]
        [HttpPut]
        public IHttpActionResult MoveFolderToTrash(string folderId)
        {
            using (var db = new WebDiskDBEntities())
            {
                #region 루트 폴더는 상태 처리 예외
                string rootId = db.FolderManage.Where(x => x.ParentId == null).SingleOrDefault().FolderId;

                if (folderId != rootId)
                {
                    ChangeTrashStatus(folderId, true);
                }

                #endregion
            }
            return Ok();
        }

        /// <summary>
        /// 휴지통 상태 복원
        /// </summary>
        /// <param name="folderId"></param>
        /// <returns></returns>
        [Route("api/disk/folder/recovery/{folderId}")]
        [HttpPut]
        public IHttpActionResult RecoverFolder(string folderId)
        {
            using (var db = new WebDiskDBEntities())
            {
                string rootId = db.FolderManage.Where(x => x.ParentId == string.Empty).SingleOrDefault().FolderId;

                if (folderId != rootId)
                {
                    ChangeTrashStatus(folderId, false);
                }


            }
            return Ok();
        }

        /// <summary>
        /// 휴지통 상태 처리 재귀 메소드
        /// </summary>
        /// <param name="folderId"></param>
        /// <param name="isBool"></param>

        public void ChangeTrashStatus(string folderId, Boolean isBool)
        {
            using (var db = new WebDiskDBEntities())
            {
                List<FolderManage> subfolders = db.FolderManage.Where(x => x.ParentId == folderId).ToList();
                List<FileManage> subFiles = db.FileManage.Where(x => x.FolderId == folderId).ToList();
                FolderManage currentFolder = db.FolderManage.Where(x => x.FolderId == folderId).SingleOrDefault();

                if (currentFolder != null)
                {
                    //하위 파일
                    for (int i = 0; i < subFiles.Count; i++)
                    {
                        if (subFiles[i] != null)
                        {
                            if (isBool == true) subFiles[i].Starred = false;

                            subFiles[i].Trashed = isBool;
                            subFiles[i].LastModified = DateTime.Now;
                            db.SaveChanges();
                        }

                    }

                    //하위 폴더
                    for (int i = 0; i < subfolders.Count; i++)
                    {
                        if (subfolders[i] != null)
                        {
                            ChangeTrashStatus(subfolders[i].FolderId, isBool);
                        }
                    }

                    if (isBool == true) currentFolder.Starred = false;
                    currentFolder.Trashed = isBool;
                    currentFolder.LastModified = DateTime.Now;
                    db.SaveChanges();
                }

            }
        }


        /// <summary>
        /// 폴더를 압축해서 zip파일로 다운로드 하도록 하는 메소드
        /// </summary>
        /// <param name="folderId">압축하려는 폴더 아이디</param>
        /// <returns></returns>
        [Route("api/disk/folder/download/{folderId}")]
        [HttpGet]
        public HttpResponseMessage DownloadFolder(string folderId)
        {
            using (var db = new WebDiskDBEntities())
            {
                HttpResponseMessage response = Request.CreateResponse(HttpStatusCode.OK);
                string zipName = db.FolderManage.Where(x => x.FolderId == folderId).SingleOrDefault().FolderName;
                #region Ionic라이브러리로 파일 압축하기
                using (var zip = new Ionic.Zip.ZipFile())
                {
                    List<string> fileList = GetFileList(folderId, new List<string>());

                    foreach (var file in fileList)
                    {
                        try
                        {
                            #region 파일을 읽어와서 zip entry에 추가
                            //todo : file 이란 변수가 실제로 파일인것만 stream으로 생성하여 압축 entry 에 추가하기..

                            //Bytes 배열로  생성
                            byte[] bytes_file = File.ReadAllBytes(file);

                           #region 해당 폴더 경로만 얻기
                            int index = file.IndexOf(zipName); //해당 폴더 인덱스 
                            string parsedPath = file.Substring(0, index); //인덱스까지 문자열을 자른다.
                            string result = file.Replace(parsedPath, ""); //파싱한 문자열을 제거한다

                            //시스템의 기본 인코딩 타입으로 읽어서
                            byte[] __filename_bytec = System.Text.Encoding.Default.GetBytes(result);
                            
                            // IBM437로 변환해 준다.
                            string IS_FileName = System.Text.Encoding.GetEncoding("IBM437").GetString(__filename_bytec);
                            zip.AddEntry(IS_FileName, bytes_file);
                            #endregion
                            #endregion

                        }
                        catch
                        {
                            return new HttpResponseMessage()
                            {
                                StatusCode = HttpStatusCode.OK,
                                Content = new StringContent("오류발생 : " + file, System.Text.Encoding.UTF8)
                            };
                        }
                    }
                    #region 메모리스트림에 zip 파일 저장하기 zip파일로 리턴하기
                    using (MemoryStream memoryStream = new MemoryStream())
                    {
                        zip.Save(memoryStream);
                        response.Content = new ByteArrayContent(memoryStream.ToArray());
                        response.Content.Headers.ContentLength = memoryStream.ToArray().LongLength;

                        response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment");
                        response.Content.Headers.ContentDisposition.FileName = $"{zipName}_{DateTime.Now.ToString("yyyyMMdd_HHmmss")}.zip";
                        response.Content.Headers.ContentType = new MediaTypeHeaderValue("application/zip");
                    }
                    #endregion

                }
                #endregion


                return response;
            }
        }


        /// <summary>
        /// 해당 폴더의 하위 폴더와 파일 path얻기
        /// </summary>
        /// <param name="folderId">해당 폴더 아이디</param>
        /// <param name="fileList">파일 path가 담겨져 있는 string 리스트</param>
        /// <returns></returns>
        public List<string> GetFileList(string folderId, List<string> fileList)
        {

            using (var db = new WebDiskDBEntities())
            {
                FolderManage folder = db.FolderManage.Where(x => x.FolderId == folderId).SingleOrDefault();

                string sourcePath = Path.Combine(folder.ServerPath, folder.OwnerId, folder.RealPath);
                string folderName = folder.FolderName;

                #region 하위 파일 path 추가
                List<FileManage> subFiles = db.FileManage.Where(x => x.FolderId == folderId).ToList();

                foreach (var subFile in subFiles)
                {
                    string subFileName = subFile.FileName + "." + subFile.FileExtension;
                    string subFilePath = Path.Combine(subFile.ServerPath, subFile.OwnerId, subFile.RealPath, subFileName);

                    fileList.Add(subFilePath);
                }
                #endregion

                #region 하위 폴더 path 추가
                List<FolderManage> subFolders = db.FolderManage.Where(x => x.ParentId == folderId).ToList();
                foreach (var subFolder in subFolders)
                {
                    GetFileList(subFolder.FolderId, fileList);

                }
                #endregion

                //if (!Directory.Exists(sourcePath))
                //{
                //    response.StatusCode = HttpStatusCode.NotFound;
                //    response.ReasonPhrase = string.Format("Folder not found: {0} .", folderName);
                //    throw new HttpResponseException(response);
                //}
            }


            return fileList;
        }

        /// <summary>
        /// 사용자가 휴지통에서 폴더를 영구 삭제
        /// </summary>
        /// <param name="folderId">삭제하려는 폴더 아이디</param>
        /// <returns></returns>
        [Route("api/disk/folder/{folderId}")]
        [HttpDelete]
        public IHttpActionResult DeleteFolderForever(string folderId)
        {
            using (var db = new WebDiskDBEntities())
            {
                #region 폴더 삭제 재귀함수 호출

                DeleteFolder(folderId);
                #endregion
            }

            return Ok(new { msg = "OK" });
        }

        /// <summary>
        /// 폴더의 folderId를 Parameter로 받아 폴더 및 하위 폴더와 파일을 찾아 DB에서 삭제한다.
        /// 호출자는 DeleteFolderForever, DeleteFolderAutoForever 메소드
        /// </summary>
        /// <param name="folderId">해당 폴더 아이디</param>
        public void DeleteFolder(string folderId)
        {
            using (var db = new WebDiskDBEntities())
            {
                List<FolderManage> subfolders = db.FolderManage.Where(x => x.Trashed == true && x.ParentId == folderId).ToList();
                List<FileManage> subFiles = db.FileManage.Where(x => x.Trashed == true && x.FolderId == folderId).ToList();
                FolderManage currentFolder = db.FolderManage.Where(x => x.Trashed == true && x.FolderId == folderId).SingleOrDefault();
                string rootId = db.FolderManage.Where(x => x.ParentId == null).SingleOrDefault().FolderId;

                if (currentFolder != null && rootId != currentFolder.FolderId)
                {
                    string folderPath = Path.Combine(currentFolder.ServerPath, currentFolder.OwnerId, currentFolder.RealPath);

                    #region 하위 파일 삭제
                    //하위 파일

                    for (int i = 0; i < subFiles.Count; i++)
                    {
                        if (subFiles[i] != null)
                        {
                            string fileName = subFiles[i].FileName + '.' + subFiles[i].FileExtension;
                            string filePath = Path.Combine(subFiles[i].ServerPath, subFiles[i].OwnerId, subFiles[i].RealPath, fileName);
                            if (File.Exists(filePath))
                            {
                                File.Delete(filePath);
                            }
                            db.FileManage.Remove(subFiles[i]);
                            db.SaveChanges();
                        }
                    }

                    #endregion
                    #region 하위 폴더 삭제하기
                    for (int i = 0; i < subfolders.Count; i++)
                    {
                        if (subfolders[i] != null)
                        {
                            //재귀함수 호출
                            DeleteFolder(subfolders[i].FolderId);

                        }
                    }
                    #endregion

                    #region 현재 폴더 삭제하기
                    Directory.Delete(folderPath);
                    db.FolderManage.Remove(currentFolder);
                    db.SaveChanges();
                    #endregion
                }

            }
        }

        [Route("api/disk/folder/autoDelete")]
        [HttpDelete]
        public IHttpActionResult DeleteFolderAutoForever()
        {
            using (var db = new WebDiskDBEntities())
            {
                #region 삭제 대상 폴더 얻기

                //스케줄러가 이 API를 호출한다.

                //휴지통에 있는 파일과 폴더 중에서 t상태가 trashed이고 마지막으로 수정한 날짜에서 두달되는 날인 파일과 폴더를 가져온다.

                List<FolderManage> deletedFolders = db.FolderManage.Where(x => x.Trashed == true && x.LastModified.Value.AddMonths(2) == DateTime.Today).ToList();
                List<FileManage> deletedFiles = db.FileManage.Where(x => x.Trashed == true && x.LastModified.Value.AddMonths(2) == DateTime.Today).ToList();

                //파일이나 폴더를 완전 삭제한다.
                foreach (var folder in deletedFolders)
                {
                    DeleteFolder(folder.FolderId);
                }

                //foreach(var file in deletedFiles)
                //{

                //}
                #endregion
            }

            return Ok(new { msg = "OK" });
        }

    }
}
