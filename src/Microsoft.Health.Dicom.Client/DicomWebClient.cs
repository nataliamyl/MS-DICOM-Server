﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using EnsureThat;
using FellowOakDicom;
using Microsoft.Health.FellowOakDicom.Serialization;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;
using MediaTypeHeaderValue = Microsoft.Net.Http.Headers.MediaTypeHeaderValue;
using NameValueHeaderValue = System.Net.Http.Headers.NameValueHeaderValue;

namespace Microsoft.Health.Dicom.Client;

public partial class DicomWebClient : IDicomWebClient
{
    private readonly string _apiVersion;
    internal static readonly JsonSerializerOptions JsonSerializerOptions = CreateJsonSerializerOptions();

    /// <summary>
    /// New instance of DicomWebClient to talk to the server
    /// </summary>
    /// <param name="httpClient">HttpClient</param>
    /// <param name="apiVersion">Pin the DicomWebClient to a specific server API version.</param>
    public DicomWebClient(HttpClient httpClient, string apiVersion = DicomApiVersions.V1)
    {
        EnsureArg.IsNotNull(httpClient, nameof(httpClient));

        HttpClient = httpClient;
        _apiVersion = apiVersion;
        GetMemoryStream = () => new MemoryStream();
    }

    public HttpClient HttpClient { get; }

    /// <summary>
    /// Func used to obtain a <see cref="MemoryStream" />. The default value returns a new memory stream.
    /// </summary>
    /// <remarks>
    /// This can be used in conjunction with a memory stream pool.
    /// </remarks>
    public Func<MemoryStream> GetMemoryStream { get; set; }

    private static async Task<T> ValueFactory<T>(HttpContent content)
    {
        string contentText = await content.ReadAsStringAsync().ConfigureAwait(false);
        return JsonSerializer.Deserialize<T>(contentText, JsonSerializerOptions);
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "Callers will dispose of the StreamContent")]
    private static MultipartContent ConvertStreamsToMultipartContent(IEnumerable<Stream> streams)
    {
        var multiContent = new MultipartContent("related");

        multiContent.Headers.ContentType.Parameters.Add(new NameValueHeaderValue("type", $"\"{DicomWebConstants.MediaTypeApplicationDicom.MediaType}\""));

        foreach (Stream stream in streams)
        {
            var streamContent = new StreamContent(stream);
            streamContent.Headers.ContentType = DicomWebConstants.MediaTypeApplicationDicom;
            multiContent.Add(streamContent);
        }

        return multiContent;
    }

    private static StreamContent ConvertStreamToStreamContent(Stream stream)
    {
        var streamContent = new StreamContent(stream);

        streamContent.Headers.ContentType = DicomWebConstants.MediaTypeApplicationDicom;

        return streamContent;
    }

    private static string GetQueryParamUriString(string queryString)
    {
        return string.IsNullOrWhiteSpace(queryString) == true ? string.Empty : "?" + queryString;
    }

    private static string CreateAcceptHeader(MediaTypeWithQualityHeaderValue mediaTypeHeader, string dicomTransferSyntax)
    {
        string transferSyntaxHeader = dicomTransferSyntax == null ? string.Empty : $";{DicomWebConstants.TransferSyntaxHeaderName}=\"{dicomTransferSyntax}\"";

        return $"{mediaTypeHeader}{transferSyntaxHeader}";
    }

    private static MediaTypeWithQualityHeaderValue CreateMultipartMediaTypeHeader(string contentType)
    {
        var multipartHeader = new MediaTypeWithQualityHeaderValue(DicomWebConstants.MultipartRelatedMediaType);
        var contentHeader = new NameValueHeaderValue("type", "\"" + contentType + "\"");

        multipartHeader.Parameters.Add(contentHeader);
        return multipartHeader;
    }

    private Uri GenerateRequestUri(string relativePath, string partitionName = default)
    {
        var uriString = "/" + _apiVersion;

        if (!string.IsNullOrEmpty(partitionName))
        {
            uriString += string.Format(DicomWebConstants.BasePartitionUriFormat, partitionName);
        }

        uriString += relativePath;

        return new Uri(uriString, UriKind.Relative);
    }

    private Uri GenerateStoreRequestUri(string partitionName = default, string studyInstanceUid = default)
    {
        if (string.IsNullOrEmpty(studyInstanceUid))
        {
            return GenerateRequestUri(DicomWebConstants.StudiesUriString, partitionName);
        }

        return GenerateRequestUri(string.Format(DicomWebConstants.BaseStudyUriFormat, studyInstanceUid), partitionName);
    }

    private Uri GenerateWorkitemAddRequestUri(string workitemUid, string partitionName = default)
    {
        return GenerateRequestUri(string.Format(DicomWebConstants.AddWorkitemUriFormat, workitemUid), partitionName);
    }

    private Uri GenerateWorkitemCancelRequestUri(string workitemUid, string partitionName = default)
    {
        return GenerateRequestUri(string.Format(DicomWebConstants.CancelWorkitemUriFormat, workitemUid), partitionName);
    }

    private Uri GenerateWorkitemRetrieveRequestUri(string workitemUid, string partitionName = default)
    {
        return GenerateRequestUri(string.Format(DicomWebConstants.BaseWorkitemUriFormat, workitemUid), partitionName);
    }

    private Uri GenerateWorkitemUpdateRequestUri(string workitemUid, string transactionUid, string partitionName = default)
    {
        return GenerateRequestUri(string.Format(DicomWebConstants.UpdateWorkitemUriFormat, workitemUid, transactionUid), partitionName);
    }

    private async IAsyncEnumerable<Stream> ReadMultipartResponseAsStreamsAsync(HttpContent httpContent, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        EnsureArg.IsNotNull(httpContent, nameof(httpContent));

        await using Stream stream = await httpContent.ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);
        stream.Seek(0, SeekOrigin.Begin);
        MultipartSection part;

        var media = MediaTypeHeaderValue.Parse(httpContent.Headers.ContentType.ToString());
        var multipartReader = new MultipartReader(HeaderUtilities.RemoveQuotes(media.Boundary).Value, stream, 100);

        while ((part = await multipartReader.ReadNextSectionAsync(cancellationToken).ConfigureAwait(false)) != null)
        {
            MemoryStream memoryStream = GetMemoryStream();
            await part.Body.CopyToAsync(memoryStream, cancellationToken).ConfigureAwait(false);
            memoryStream.Seek(0, SeekOrigin.Begin);

            yield return memoryStream;
        }
    }

    private static async Task EnsureSuccessStatusCodeAsync(
        HttpResponseMessage response,
        Func<HttpStatusCode, HttpResponseHeaders, HttpContentHeaders, string, bool> additionalFailureInspector = null)
    {
        if (!response.IsSuccessStatusCode)
        {
            HttpStatusCode statusCode = response.StatusCode;
            HttpResponseHeaders responseHeaders = response.Headers;
            HttpContentHeaders contentHeaders = response.Content?.Headers;
            string responseBody = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            Exception exception = null;

            try
            {
                bool handled = additionalFailureInspector?.Invoke(statusCode, responseHeaders, contentHeaders, responseBody) ?? false;

                if (!handled)
                {
                    throw new DicomWebException(statusCode, responseHeaders, contentHeaders, responseBody);
                }
            }
            catch (Exception ex)
            {
                exception = ex;
            }
            finally
            {
                // If we are throwing exception, then we can close the response because we have already read the body.
                if (exception != null)
                {
                    response.Dispose();
                }
            }

            if (exception != null)
            {
                throw exception;
            }
        }
    }

    private async IAsyncEnumerable<DicomFile> ReadMultipartResponseAsDicomFileAsync(HttpContent content, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        await foreach (Stream stream in ReadMultipartResponseAsStreamsAsync(content, cancellationToken).ConfigureAwait(false))
        {
            yield return await DicomFile.OpenAsync(stream).ConfigureAwait(false);
        }
    }

    private static async IAsyncEnumerable<T> DeserializeAsAsyncEnumerable<T>(HttpContent content)
    {
        string contentText = await content.ReadAsStringAsync().ConfigureAwait(false);

        if (string.IsNullOrEmpty(contentText))
        {
            yield break;
        }

        foreach (T item in JsonSerializer.Deserialize<IReadOnlyList<T>>(contentText, JsonSerializerOptions))
        {
            yield return item;
        }
    }

    private static JsonSerializerOptions CreateJsonSerializerOptions()
    {
        var options = new JsonSerializerOptions
        {
            AllowTrailingCommas = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            DictionaryKeyPolicy = JsonNamingPolicy.CamelCase,
            Encoder = null,
            IgnoreReadOnlyFields = false,
            IgnoreReadOnlyProperties = false,
            IncludeFields = false,
            MaxDepth = 0, // 0 indicates the max depth of 64
            NumberHandling = JsonNumberHandling.Strict,
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            ReadCommentHandling = JsonCommentHandling.Skip,
            WriteIndented = false,
        };

        options.Converters.Add(new JsonStringEnumConverter());
        options.Converters.Add(new DicomJsonConverter(writeTagsAsKeywords: true, autoValidate: false));

        return options;
    }
}
