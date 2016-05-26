﻿// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this 
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

namespace ServiceStack.Documentation.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text.RegularExpressions;
    using DataAnnotations;
    using Extensions;
    using Models;
    using Models.Postman;
    using Text;

    public class ApiSpecPostmanService : IService
    {
        // TODO Set as property in feature?
        public Dictionary<string, string> FriendlyTypeNames = new Dictionary<string, string>
        {
            {"Int32", "int"},
            {"Int64", "long"},
            {"Boolean", "bool"},
            {"String", "string"},
            {"Double", "double"},
            {"Single", "float"},
        };

        // TODO Need to be able to set which Headers to add
        // TODO Auth
        // TODO Have this use the same filtering logic as ApiSpecService to get a subset of data
        // TODO Take filter of Verb(s) to use/ignore?
        // TODO Have a parameter that can be set of whether to set the query string on all verbs or just GET
        [AddHeader(ContentType = MimeTypes.Json)]
        public object Get(PostmanRequest request)
        {
            // Get the documentation object
            var documentation = ApiDocumentationFilter.GetApiDocumentation(request);

            // TODO Look at the cookies that are in the current postman plugin

            // TODO Use SS AutoMapping for this?
            // Convert apiDocumentation to postman spec
            var collection = new PostmanSpecCollection();

            var collectionId = Guid.NewGuid().ToString();
            collection.Id = collectionId;
            collection.Name = documentation.Title;
            collection.Description = documentation.Description;
            collection.Timestamp = DateTime.UtcNow.ToUnixTimeMs();

            collection.Requests = GetRequests(documentation, collectionId).ToArray();

            return collection;
        }

        private IEnumerable<PostmanSpecRequest> GetRequests(ApiDocumentation documentation, string collectionId)
        {
            // Iterate over all resources
            foreach (var resource in documentation.Resources)
            {
                var contentType = GetContentTypes(resource);

                var data = GetPostmanSpecData(resource);

                // Get any pathVariables that are present (variable place holders in route)
                var pathVariables = resource.RelativePath.HasPathParams();
                string relativePath = resource.RelativePath;
                foreach (var match in pathVariables)
                {
                    // Replace any matched routes with :name and remove the {} from around them
                    relativePath = relativePath.Replace($"{{{match}}}", $":{match}");
                }               
                
                // Iterate through every verb of every resource. Generate a collection request per verb
                foreach (var verb in resource.Verbs)
                {
                    var hasRequestBody = verb.HasRequestBody();
                    string verbPath = relativePath;
                    if (!hasRequestBody)
                        verbPath = ProcessQueryStringParams(data, pathVariables, relativePath);

                    var request = new PostmanSpecRequest
                    {
                        Id = Guid.NewGuid().ToString(),
                        Url = documentation.ApiBaseUrl.CombineWith(verbPath),
                        Method = verb,
                        Time = DateTime.UtcNow.ToUnixTimeMs(),
                        Name = resource.Title,
                        Description = resource.Description,
                        CollectionId = collectionId,
                        Headers = $"Accept: {contentType}"
                    };

                    if (hasRequestBody)
                    {
                        request.Data = data;
                        request.PathVariables = null;
                    }
                    else
                    {
                        request.Data = null;
                        request.PathVariables =
                            data.Where(t => pathVariables.Contains(t.Key, StringComparer.OrdinalIgnoreCase))
                                .ToDictionary(k => k.Key, v => v.Value);
                    }

                    yield return request;
                }
            }
        }

        private static string GetContentTypes(ApiResourceDocumentation resource)
        {
            // TODO Tighten up the logic used here
            var contentType = resource.ContentTypes.Contains(MimeTypes.Json)
                                  ? MimeTypes.Json
                                  : resource.ContentTypes.First();
            return contentType;
        }

        private static string ProcessQueryStringParams(List<PostmanSpecData> data, List<string> pathVariables, string relativePath)
        {
            // TODO Make nicer attempts at querystring values. String if string, number if int etc

            var queryParams = data.Where(d => !pathVariables.Contains(d.Key, StringComparer.OrdinalIgnoreCase));
            return queryParams.Aggregate(relativePath, (current, queryParam) => current.AddQueryParam(queryParam.Key, queryParam.Value));
        }

        private List<PostmanSpecData> GetPostmanSpecData(ApiResourceDocumentation resource)
        {
            int count = 0;
            var data = resource.Properties.Select(r =>
                                                  new PostmanSpecData
                                                  {
                                                      Enabled = true,
                                                      Key = r.Title,
                                                      Type = FriendlyTypeNames.SafeGet(r.ClrType.Name, r.ClrType.Name),
                                                      Value = $"val-{++count}"
                                                  }).ToList();
            return data;
        }
    }

    public static class PostmanSpecExtensions
    {
        // Regex to get any 
        private static readonly Regex pathVariableRegex = new Regex("\\{([A-Za-z0-9-_]+)\\}");
        
        // TODO Handle wildcards
        public static List<string> HasPathParams(this string path)
        {
            var matches = pathVariableRegex.Matches(path);

            var output = new List<string>();
            foreach (Match match in matches)
            {
                if (!match.Success)
                    continue;
                
                output.Add(match.Groups[1].Value);
            }

            return output.Count > 0 ? output : Enumerable.Empty<string>().ToList();
        }
    }

    [Route(Constants.PostmanSpecUri)]
    [Exclude(Feature.Metadata | Feature.ServiceDiscovery)]
    public class PostmanRequest : IReturn<PostmanSpecCollection>, IFilterableSpecRequest
    {
        public string[] DtoName { get; set; }
        public string Category { get; set; }
        public string[] Tags { get; set; }
    }

    public class PostmanResponse
    {
        public PostmanSpecCollection Collection { get; set; }
    }
}
