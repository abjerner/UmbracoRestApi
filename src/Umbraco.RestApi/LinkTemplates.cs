using Umbraco.RestApi.Routing;
using WebApi.Hal;

namespace Umbraco.RestApi
{
    internal static class LinkTemplates
    {
        public static int ApiVersion = 1;

        public static class Relations
        {
            public static string BaseUrl => string.Format("~/{0}/{1}/{2}", RouteConstants.GetRestRootPath(ApiVersion), RouteConstants.RelationsSegment, RouteConstants.PublishedSegment);

            public static Link Root => new Link("root", BaseUrl);

            public static Link Self => new Link("relation", string.Format("{0}/{{id}}", BaseUrl ));

            public static Link Children => new Link("relatedChildren", string.Format("{0}//children/{{id}}{{?relationType}}", BaseUrl));

            public static Link Parents => new Link("relatedParents", string.Format("{0}//parents/{{id}}{{?relationType}}", BaseUrl));
        }

        public static class PublishedContent
        {
            public static string BaseUrl => string.Format("~/{0}/{1}/{2}", RouteConstants.GetRestRootPath(ApiVersion), RouteConstants.ContentSegment, RouteConstants.PublishedSegment);

            public static Link Root => new Link("root", BaseUrl);

            public static Link Self => new Link("content", string.Format("{0}/{{id}}", BaseUrl));

            public static Link Parent => new Link("parent", string.Format("{0}/{{parentId}}", BaseUrl));

            public static Link MetaData => new Link("meta", string.Format("{0}/{{id}}/meta", BaseUrl));


            public static Link PagedChildren => new Link("children",
                string.Format("{0}/{{id}}/children{{?page,size,query}}", BaseUrl));

            public static Link PagedDescendants => new Link("descendants",
                string.Format("{0}/{{id}}/descendants{{?page,size,query}}", BaseUrl));

            public static Link PagedAncestors => new Link("ancestors",
                string.Format("{0}/{{id}}/ancestors{{?page,size,query}}", BaseUrl));


            public static Link Search => new Link("search", string.Format("{0}/search{{?page,size,query}}", BaseUrl));

            public static Link Query => new Link("query", string.Format("{0}/query/{{id}}{{?page,size,query}}", BaseUrl));

            public static Link Url => new Link("url", string.Format("{0}/url{{?url}}", BaseUrl));

            public static Link Tag => new Link("tag", string.Format("{0}/tag/{{tag}}{{?group}}", BaseUrl));
        }

        public static class Media
        {
            public static string BaseUrl => string.Format("~/{0}/{1}", RouteConstants.GetRestRootPath(ApiVersion), RouteConstants.MediaSegment);

            public static Link Root => new Link("root", BaseUrl);

            public static Link Self => new Link("content", string.Format("{0}/{{id}}", BaseUrl));

            public static Link Parent => new Link("parent", string.Format("{0}/{{parentId}}", BaseUrl));

            public static Link PagedChildren => new Link("children", string.Format("{0}/{{id}}/children{{?pageIndex,pageSize}}", BaseUrl));

            public static Link PagedDescendants => new Link("descendants",
                string.Format("{0}/{{id}}/descendants{{?pageIndex,pageSize}}", BaseUrl));

            public static Link MetaData => new Link("meta", string.Format("{0}/{{id}}/meta", BaseUrl));

            public static Link Search => new Link("search", string.Format("{0}/search{{?page,size,query}}", BaseUrl));

            public static Link Upload => new Link("upload", string.Format("{0}/{{id}}/upload{{?property}}", BaseUrl));
        }
            
        public static class Members
        {
            public static string BaseUrl => string.Format("~/{0}/{1}", RouteConstants.GetRestRootPath(ApiVersion), RouteConstants.MembersSegment);

            public static Link Root => new Link("root", string.Format("{0}{{?page,size,orderBy,direction,memberTypeAlias,filter}}", BaseUrl ));

            public static Link Self => new Link("member", string.Format("{0}/{{id}}", BaseUrl));

            public static Link MetaData => new Link("meta", string.Format("{0}/{{id}}/meta", BaseUrl));

            public static Link Search => new Link("search", string.Format("{0}/search{{?page,size,query}}", BaseUrl));

            public static Link Upload => new Link("upload", string.Format("{0}/{{id}}/upload{{?property}}", BaseUrl));
        }
        
        public static class Content
        {
            public static string BaseUrl => string.Format("~/{0}/{1}", RouteConstants.GetRestRootPath(ApiVersion), RouteConstants.ContentSegment);

            public static Link Root => new Link("root", BaseUrl);

            public static Link Self => new Link("content", string.Format("{0}/{{id}}", BaseUrl));

            public static Link Parent => new Link("parent", string.Format("{0}/{{parentId}}", BaseUrl));

            public static Link PagedChildren => new Link("children", string.Format("{0}/{{id}}/children{{?page,size}}", BaseUrl));

            public static Link PagedDescendants => new Link("descendants",
                string.Format("{0}/{{id}}/descendants{{?page,size}}", BaseUrl));

            public static Link PagedAncestors => new Link("descendants",
                string.Format("{0}/{{id}}/ancestors{{?page,size}}", BaseUrl));

            public static Link MetaData => new Link("meta", string.Format("{0}/{{id}}/meta", BaseUrl));

            public static Link Search => new Link("search", string.Format("{0}/search{{?page,size,query}}", BaseUrl));

            public static Link Upload => new Link("upload", string.Format("{0}/{{id}}/upload{{?property}}", BaseUrl));
        }
    }
}
