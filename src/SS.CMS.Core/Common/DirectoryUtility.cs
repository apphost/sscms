﻿using System;
using System.Collections;
using SS.CMS.Core.Cache;
using SS.CMS.Core.Models;
using SS.CMS.Core.Settings;
using SS.CMS.Utils;
using AppContext = SS.CMS.Core.Settings.AppContext;

namespace SS.CMS.Core.Common
{
    public static class DirectoryUtility
    {
        public static void ChangeSiteDir(string parentPsPath, string oldPsDir, string newPsDir)
        {
            var oldPsPath = PathUtils.Combine(parentPsPath, oldPsDir);
            var newPsPath = PathUtils.Combine(parentPsPath, newPsDir);
            if (DirectoryUtils.IsDirectoryExists(newPsPath))
            {
                throw new ArgumentException("发布系统修改失败，发布路径文件夹已存在！");
            }
            if (DirectoryUtils.IsDirectoryExists(oldPsPath))
            {
                DirectoryUtils.MoveDirectory(oldPsPath, newPsPath, false);
            }
            else
            {
                DirectoryUtils.CreateDirectoryIfNotExists(newPsPath);
            }
        }

        public static void DeleteSiteFiles(SiteInfo siteInfo)
        {
            if (siteInfo == null) return;

            var sitePath = PathUtility.GetSitePath(siteInfo);

            if (siteInfo.Root)
            {
                var filePaths = DirectoryUtils.GetFilePaths(sitePath);
                foreach (var filePath in filePaths)
                {
                    var fileName = PathUtils.GetFileName(filePath);
                    if (!PathUtility.IsSystemFile(fileName))
                    {
                        FileUtils.DeleteFileIfExists(filePath);
                    }
                }

                var siteDirList = DataProvider.SiteDao.GetLowerSiteDirListThatNotIsRoot();

                var directoryPaths = DirectoryUtils.GetDirectoryPaths(sitePath);
                foreach (var subDirectoryPath in directoryPaths)
                {
                    var directoryName = PathUtils.GetDirectoryName(subDirectoryPath, false);
                    if (!AppContext.IsSystemDirectory(directoryName) && !siteDirList.Contains(directoryName.ToLower()))
                    {
                        DirectoryUtils.DeleteDirectoryIfExists(subDirectoryPath);
                    }
                }
            }
            else
            {
                var direcotryPath = sitePath;
                DirectoryUtils.DeleteDirectoryIfExists(direcotryPath);
            }
        }

        public static void ImportSiteFiles(SiteInfo siteInfo, string siteTemplatePath, bool isOverride)
        {
            var sitePath = PathUtility.GetSitePath(siteInfo);

            if (siteInfo.Root)
            {
                var filePaths = DirectoryUtils.GetFilePaths(siteTemplatePath);
                foreach (var filePath in filePaths)
                {
                    var fileName = PathUtils.GetFileName(filePath);
                    if (!PathUtility.IsSystemFile(fileName))
                    {
                        var destFilePath = PathUtils.Combine(sitePath, fileName);
                        FileUtils.MoveFile(filePath, destFilePath, isOverride);
                    }
                }

                var siteDirList = DataProvider.SiteDao.GetLowerSiteDirListThatNotIsRoot();

                var directoryPaths = DirectoryUtils.GetDirectoryPaths(siteTemplatePath);
                foreach (var subDirectoryPath in directoryPaths)
                {
                    var directoryName = PathUtils.GetDirectoryName(subDirectoryPath, false);
                    if (!AppContext.IsSystemDirectory(directoryName) && !siteDirList.Contains(directoryName.ToLower()))
                    {
                        var destDirectoryPath = PathUtils.Combine(sitePath, directoryName);
                        DirectoryUtils.MoveDirectory(subDirectoryPath, destDirectoryPath, isOverride);
                    }
                }
            }
            else
            {
                DirectoryUtils.MoveDirectory(siteTemplatePath, sitePath, isOverride);
            }
            var siteTemplateMetadataPath = PathUtils.Combine(sitePath, DirectoryUtils.SiteTemplates.SiteTemplateMetadata);
            DirectoryUtils.DeleteDirectoryIfExists(siteTemplateMetadataPath);
        }

        public static void ChangeParentSite(int oldParentSiteId, int newParentSiteId, int siteId, string siteDir)
        {
            if (oldParentSiteId == newParentSiteId) return;

            string oldPsPath;
            if (oldParentSiteId != 0)
            {
                var oldSiteInfo = SiteManager.GetSiteInfo(oldParentSiteId);

                oldPsPath = PathUtils.Combine(PathUtility.GetSitePath(oldSiteInfo), siteDir);
            }
            else
            {
                var siteInfo = SiteManager.GetSiteInfo(siteId);
                oldPsPath = PathUtility.GetSitePath(siteInfo);
            }

            string newPsPath;
            if (newParentSiteId != 0)
            {
                var newSiteInfo = SiteManager.GetSiteInfo(newParentSiteId);

                newPsPath = PathUtils.Combine(PathUtility.GetSitePath(newSiteInfo), siteDir);
            }
            else
            {
                newPsPath = PathUtils.Combine(AppContext.WebRootPath, siteDir);
            }

            if (DirectoryUtils.IsDirectoryExists(newPsPath))
            {
                throw new ArgumentException("发布系统修改失败，发布路径文件夹已存在！");
            }
            if (DirectoryUtils.IsDirectoryExists(oldPsPath))
            {
                DirectoryUtils.MoveDirectory(oldPsPath, newPsPath, false);
            }
            else
            {
                DirectoryUtils.CreateDirectoryIfNotExists(newPsPath);
            }
        }

        public static void ChangeToHeadquarters(SiteInfo siteInfo, bool isMoveFiles)
        {
            if (siteInfo.Root == false)
            {
                var sitePath = PathUtility.GetSitePath(siteInfo);

                DataProvider.SiteDao.UpdateParentIdToZero(siteInfo.Id);

                siteInfo.Root = true;
                siteInfo.SiteDir = string.Empty;

                DataProvider.SiteDao.Update(siteInfo);
                if (isMoveFiles)
                {
                    DirectoryUtils.MoveDirectory(sitePath, AppContext.WebRootPath, false);
                    DirectoryUtils.DeleteDirectoryIfExists(sitePath);
                }
            }
        }

        public static void ChangeToSubSite(SiteInfo siteInfo, string psDir, ArrayList fileSystemNameArrayList)
        {
            if (siteInfo.Root)
            {
                siteInfo.Root = false;
                siteInfo.SiteDir = psDir.Trim();

                DataProvider.SiteDao.Update(siteInfo);

                var psPath = PathUtils.Combine(AppContext.WebRootPath, psDir);
                DirectoryUtils.CreateDirectoryIfNotExists(psPath);
                if (fileSystemNameArrayList != null && fileSystemNameArrayList.Count > 0)
                {
                    foreach (string fileSystemName in fileSystemNameArrayList)
                    {
                        var srcPath = PathUtils.Combine(AppContext.WebRootPath, fileSystemName);
                        if (DirectoryUtils.IsDirectoryExists(srcPath))
                        {
                            var destDirectoryPath = PathUtils.Combine(psPath, fileSystemName);
                            DirectoryUtils.CreateDirectoryIfNotExists(destDirectoryPath);
                            DirectoryUtils.MoveDirectory(srcPath, destDirectoryPath, false);
                            DirectoryUtils.DeleteDirectoryIfExists(srcPath);
                        }
                        else if (FileUtils.IsFileExists(srcPath))
                        {
                            FileUtils.CopyFile(srcPath, PathUtils.Combine(psPath, fileSystemName));
                            FileUtils.DeleteFileIfExists(srcPath);
                        }
                    }
                }
            }
        }
    }
}
