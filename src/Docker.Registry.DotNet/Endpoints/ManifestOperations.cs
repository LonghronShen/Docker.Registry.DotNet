﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Docker.Registry.DotNet.Models;

namespace Docker.Registry.DotNet.Endpoints
{
    using System.Runtime.Serialization;
    using Newtonsoft.Json;

    internal class ManifestOperations : IManifestOperations
    {
        private readonly NetworkClient _client;

        public ManifestOperations(NetworkClient client)
        {
            _client = client;
        }

        public async Task<GetImageManifestResult> GetManifestAsync(string name, string reference, CancellationToken cancellationToken = new CancellationToken())
        {
            var headers = new Dictionary<string, string>
            {
                { "Accept", $"{ManifestMediaTypes.ManifestSchema1}, {ManifestMediaTypes.ManifestSchema2}, {ManifestMediaTypes.ManifestList}, {ManifestMediaTypes.ManifestSchema1Signed}"   }
            };

            var response = await _client.MakeRequestAsync(cancellationToken, HttpMethod.Get, $"v2/{name}/manifests/{reference}", null, headers).ConfigureAwait(false);

            string contentType = GetContentType(response.GetHeader("ContentType"), response.Body);

            switch (contentType)
            {
                case ManifestMediaTypes.ManifestSchema1:
                case ManifestMediaTypes.ManifestSchema1Signed:
                    return new GetImageManifestResult(contentType,
                        _client.JsonSerializer.DeserializeObject<ImageManifest2_1>(response.Body), response.Body)
                    {
                        DockerContentDigest = response.GetHeader("Docker-Content-Digest"),
                        Etag = response.GetHeader("Etag")
                    };

                case ManifestMediaTypes.ManifestSchema2:
                    return new GetImageManifestResult(contentType, _client.JsonSerializer.DeserializeObject<ImageManifest2_2>(response.Body), response.Body);

                case ManifestMediaTypes.ManifestList:
                    return new GetImageManifestResult(contentType, _client.JsonSerializer.DeserializeObject<ManifestList>(response.Body), response.Body);

                default:
                    throw new Exception($"Unexpectd ContentType '{contentType}'.");
            }
        }

        private string GetContentType(string contentTypeHeader, string manifest)
        {
            if (!string.IsNullOrWhiteSpace(contentTypeHeader))
                return contentTypeHeader;

            var check = JsonConvert.DeserializeObject<SchemaCheck>(manifest);

            if (check.SchemaVersion == null)
                return ManifestMediaTypes.ManifestSchema1;

            if (check.SchemaVersion.Value == 2)
                return ManifestMediaTypes.ManifestSchema2;

            throw new Exception($"Unable to determine schema type from version {check.SchemaVersion}");
        }

        private class SchemaCheck
        {
            /// <summary>
            /// This field specifies the image manifest schema version as an integer.
            /// </summary>
            [DataMember(Name = "schemaVersion")]
            public int? SchemaVersion { get; set; }
        }

        public Task PutManifestAsync(string name, string reference, ImageManifest manifest,
            CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public Task<bool> DoesManifestExistAsync(string name, string reference, CancellationToken cancellation = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public async Task DeleteManifestAsync(string name, string reference,
            CancellationToken cannCancellationToken = new CancellationToken())
        {
            string path = $"v2/{name}/manfiests/{reference}";

            await _client.MakeRequestAsync(cannCancellationToken, HttpMethod.Delete, path);
        }

        public async Task<string> GetManifestRawAsync(string name, string reference, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>
            {
                { "Accept", $"{ManifestMediaTypes.ManifestSchema1}, {ManifestMediaTypes.ManifestSchema2}, {ManifestMediaTypes.ManifestList}, {ManifestMediaTypes.ManifestSchema1Signed}"   }
            };

            var response = await _client.MakeRequestAsync(cancellationToken, HttpMethod.Get, $"v2/{name}/manifests/{reference}", null, headers).ConfigureAwait(false);

            return response.Body;
        }
    }
}