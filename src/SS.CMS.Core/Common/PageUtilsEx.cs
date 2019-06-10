﻿using System;
using System.Collections.Specialized;
using System.Linq;
using SS.CMS.Core.Settings;
using SS.CMS.Utils;
using AppContext = SS.CMS.Core.Settings.AppContext;

namespace SS.CMS.Core.Common
{
    public static class PageUtilsEx
    {
        // 系统根目录访问地址


        public static string GetMainUrl(int siteId)
        {
            return AppContext.GetAdminUrl($"main.cshtml?siteId={siteId}");
        }



        public static string GetSiteFilesUrl(string relatedUrl)
        {
            return PageUtils.Combine(AppContext.ApplicationPath, DirectoryUtils.SiteFiles.DirectoryName, relatedUrl);
        }

        public static string GetTemporaryFilesUrl(string relatedUrl)
        {
            return PageUtils.Combine(AppContext.ApplicationPath, DirectoryUtils.SiteFiles.DirectoryName, DirectoryUtils.SiteFiles.TemporaryFiles, relatedUrl);
        }

        public static string GetSiteTemplatesUrl(string relatedUrl)
        {
            return PageUtils.Combine(AppContext.ApplicationPath, DirectoryUtils.SiteFiles.DirectoryName, DirectoryUtils.SiteTemplates.DirectoryName, relatedUrl);
        }

        public static string GetSiteTemplateMetadataUrl(string siteTemplateUrl, string relatedUrl)
        {
            return PageUtils.Combine(siteTemplateUrl, DirectoryUtils.SiteTemplates.SiteTemplateMetadata, relatedUrl);
        }

        public static string ParsePluginUrl(string pluginId, string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            if (PageUtils.IsProtocolUrl(url)) return url;

            if (StringUtils.StartsWith(url, "~/"))
            {
                return AppContext.GetRootUrl(url.Substring(1));
            }

            if (StringUtils.StartsWith(url, "@/"))
            {
                return AppContext.GetAdminUrl(url.Substring(1));
            }

            return GetSiteFilesUrl(PageUtils.Combine(DirectoryUtils.SiteFiles.Plugins, pluginId, url));
        }

        public static string GetSiteServerUrl(string className)
        {
            return AppContext.GetAdminUrl(className.ToCamelCase() + ".cshtml");
        }

        public static string GetSiteServerUrl(string className, NameValueCollection queryString)
        {
            return PageUtils.AddQueryString(AppContext.GetAdminUrl(className.ToCamelCase() + ".aspx"), queryString);
        }

        public static string GetPluginsUrl(string className)
        {
            return AppContext.GetAdminUrl(PageUtils.Combine("plugins", className.ToCamelCase() + ".cshtml"));
        }

        public static string GetPluginsUrl(string className, NameValueCollection queryString)
        {
            return PageUtils.AddQueryString(AppContext.GetAdminUrl(PageUtils.Combine("plugins", className.ToCamelCase() + ".aspx")), queryString);
        }

        public static string GetSettingsUrl(string className)
        {
            return AppContext.GetAdminUrl(PageUtils.Combine("settings", className.ToCamelCase() + ".cshtml"));
        }

        public static string GetSettingsUrl(string className, NameValueCollection queryString)
        {
            return PageUtils.AddQueryString(AppContext.GetAdminUrl(PageUtils.Combine("settings", className.ToCamelCase() + ".aspx")), queryString);
        }

        public static string GetCmsUrl(string pageName, int siteId, object param = null)
        {
            var url = AppContext.GetAdminUrl(PageUtils.Combine("cms", $"{pageName.ToCamelCase()}.cshtml?siteId={siteId}"));
            return param == null ? url : param.GetType().GetProperties().Aggregate(url, (current, p) => current + $"&{p.Name.ToCamelCase()}={p.GetValue(param)}");
        }

        public static string GetCmsUrl(int siteId, string className, NameValueCollection queryString)
        {
            queryString = queryString ?? new NameValueCollection();
            queryString.Remove("siteId");
            return PageUtils.AddQueryString(AppContext.GetAdminUrl($"cms/{className.ToCamelCase()}.aspx?siteId={siteId}"), queryString);
        }

        public static string GetCmsWebHandlerUrl(int siteId, string className, NameValueCollection queryString)
        {
            queryString = queryString ?? new NameValueCollection();
            queryString.Remove("siteId");
            return PageUtils.AddQueryString(AppContext.GetAdminUrl($"cms/{className.ToCamelCase()}.ashx?siteId={siteId}"), queryString);
        }

        public static string GetAjaxUrl(string className, NameValueCollection queryString)
        {
            return PageUtils.AddQueryString(AppContext.GetAdminUrl(PageUtils.Combine("ajax", className.ToLower() + ".aspx")), queryString);
        }



        public static string GetRootUrlByPhysicalPath(string physicalPath)
        {
            var requestPath = PathUtils.GetPathDifference(AppContext.WebRootPath, physicalPath);
            requestPath = requestPath.Replace(PathUtils.SeparatorChar, PageUtils.SeparatorChar);
            return AppContext.GetRootUrl(requestPath);
        }

        public static string GetLoadingUrl(string url)
        {
            return AppContext.GetAdminUrl($"loading.aspx?redirectUrl={AppContext.Encrypt(url)}");
        }

        public static string ParseNavigationUrl(string url)
        {
            if (string.IsNullOrEmpty(url)) return string.Empty;

            url = url.StartsWith("~") ? PageUtils.Combine(AppContext.ApplicationPath, url.Substring(1)) : url;
            url = url.Replace(PathUtils.SeparatorChar, PageUtils.SeparatorChar);
            return url;
        }

        public static string AddProtocolToUrl(string url)
        {
            return AddProtocolToUrl(url, string.Empty);
        }

        public static string AddProtocolToUrl(string url, string host)
        {
            if (url == PageUtils.UnclickedUrl)
            {
                return url;
            }
            var retval = string.Empty;

            if (!string.IsNullOrEmpty(url))
            {
                url = url.Trim();
                if (PageUtils.IsProtocolUrl(url))
                {
                    retval = url;
                }
                else
                {
                    retval = url.StartsWith("/") ? host.TrimEnd('/') + url : host + url;
                }
            }
            return retval;
        }

        public static string GetUrlWithReturnUrl(string pageUrl, string returnUrl)
        {
            var retval = pageUrl;
            returnUrl = $"ReturnUrl={returnUrl}";
            if (pageUrl.IndexOf("?", StringComparison.Ordinal) != -1)
            {
                if (pageUrl.EndsWith("&"))
                {
                    retval += returnUrl;
                }
                else
                {
                    retval += "&" + returnUrl;
                }
            }
            else
            {
                retval += "?" + returnUrl;
            }
            return ParseNavigationUrl(retval);
        }

        public static string GetUrlByBaseUrl(string rawUrl, string baseUrl)
        {
            var url = string.Empty;
            if (!string.IsNullOrEmpty(rawUrl))
            {
                rawUrl = rawUrl.Trim().TrimEnd('#');
            }
            if (!string.IsNullOrEmpty(baseUrl))
            {
                baseUrl = baseUrl.Trim();
            }
            if (!string.IsNullOrEmpty(rawUrl))
            {
                rawUrl = rawUrl.Trim();
                if (PageUtils.IsProtocolUrl(rawUrl))
                {
                    url = rawUrl;
                }
                else if (rawUrl.StartsWith("/"))
                {
                    var domain = PageUtils.GetUrlWithoutPathInfo(baseUrl);
                    url = domain + rawUrl;
                }
                else if (rawUrl.StartsWith("../"))
                {
                    var count = StringUtils.GetStartCount("../", rawUrl);
                    rawUrl = rawUrl.Remove(0, 3 * count);
                    baseUrl = PageUtils.GetUrlWithoutFileName(baseUrl).TrimEnd('/');
                    baseUrl = PageUtils.RemoveProtocolFromUrl(baseUrl);
                    for (var i = 0; i < count; i++)
                    {
                        var j = baseUrl.LastIndexOf('/');
                        if (j != -1)
                        {
                            baseUrl = StringUtils.Remove(baseUrl, j);
                        }
                        else
                        {
                            break;
                        }
                    }
                    url = PageUtils.Combine(AddProtocolToUrl(baseUrl), rawUrl);
                }
                else
                {
                    if (baseUrl != null && baseUrl.EndsWith("/"))
                    {
                        url = baseUrl + rawUrl;
                    }
                    else
                    {
                        var urlWithoutFileName = PageUtils.GetUrlWithoutFileName(baseUrl);
                        if (!urlWithoutFileName.EndsWith("/"))
                        {
                            urlWithoutFileName += "/";
                        }
                        url = urlWithoutFileName + rawUrl;
                    }
                }
            }
            return url;
        }

        public static string ParseConfigRootUrl(string url)
        {
            return ParseNavigationUrl(url);
        }
    }
}
