using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;

using Agent.Sdk;

using Microsoft.VisualStudio.Services.Agent;
using Microsoft.VisualStudio.Services.Agent.Listener.Configuration;
using Microsoft.VisualStudio.Services.Agent.Util;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

using Newtonsoft.Json;

namespace Agent.Listener.Configuration
{

    [ServiceLocator(Default = typeof(PipelineOrgSettingsProvider))]
    public interface IOrgSettingsProvider : IAgentService
    {
        /// <summary>
        /// Gets pipeline organization settings
        /// </summary>
        /// <param name="context">Agent host contexts</param>
        /// <param name="traceWriter">Trace writer for output</param>
        /// <returns>List of organization settings</returns>
        /// <exception cref="VssUnauthorizedException">Thrown if token is not suitable for retriving organization settings</exception>
        /// <exception cref="InvalidOperationException">Thrown if agent is not configured</exception>
        public Task<PipelineSettings> GetPipelineOrgSettingsAsync(IHostContext context, ITraceWriter traceWriter, CancellationToken ctk = default);

        /// <summary>
        /// Gets organization settings by name
        /// </summary>
        /// <param name="context">Agent host contexts</param>
        /// <param name="settingsName">The name of the organization settings</param>
        /// <param name="traceWriter">Trace writer for output</param>
        /// <returns>The status of organization settings.</returns>
        /// <exception cref="VssUnauthorizedException">Thrown if token is not suitable for retriving organization settings</exception>
        /// <exception cref="InvalidOperationException">Thrown if agent is not configured</exception>
        public Task<bool?> GetPipelineOrgSettingsByNameAsync(IHostContext context, string settingsName, ITraceWriter traceWriter, CancellationToken ctk = default);

    }

    public class PipelineOrgSettingsProvider : AgentService, IOrgSettingsProvider
    {

        public async Task<PipelineSettings> GetPipelineOrgSettingsAsync(IHostContext context, ITraceWriter traceWriter, CancellationToken ctk = default)
        {
            traceWriter.Verbose(nameof(GetPipelineOrgSettingsAsync));

            var credMgr = context.GetService<ICredentialManager>();
            var configManager = context.GetService<IConfigurationManager>();

            VssCredentials creds = credMgr.LoadCredentials();
            ArgUtil.NotNull(creds, nameof(creds));

            AgentSettings settings = configManager.LoadSettings();
            using var vssConnection = VssUtil.CreateConnection(new Uri(settings.ServerUrl), creds, traceWriter);
            var client = vssConnection.GetClient<PipelineOrgSettingsHttpClient>();
            try
            {
                return await client.GetPipelineOrgSettingsAsync(ctk);
            }
            catch (VssServiceException e)
            {
                Trace.Warning("Unable to retrieve pipeline organization settings: " + e.ToString());
                return null;
            }
        }
        //There are some logics on false value as well, so it's not fine to return false value if settings is not exsist
        public async Task<bool?> GetPipelineOrgSettingsByNameAsync(IHostContext context, string settingsName, ITraceWriter traceWriter, CancellationToken ctk = default)
        {
            var pipelineSettings = await GetPipelineOrgSettingsAsync(context, traceWriter, ctk);
            if (pipelineSettings == null || pipelineSettings.DataProviders == null)
                return null;

            if (pipelineSettings.DataProviders.PipelineSettingsMap.TryGetValue(settingsName, out var result))
            {
                return result;
            }

            return null;
        }
    }

    public class PipelineOrgSettingsHttpClient : VssHttpClientBase
    {
        public PipelineOrgSettingsHttpClient(Uri baseUrl, VssCredentials credentials)
            : base(baseUrl, credentials)
        {
        }

        public PipelineOrgSettingsHttpClient(Uri baseUrl, VssCredentials credentials, VssHttpRequestSettings settings)
            : base(baseUrl, credentials, settings)
        {
        }

        public PipelineOrgSettingsHttpClient(Uri baseUrl, VssCredentials credentials, params DelegatingHandler[] handlers)
            : base(baseUrl, credentials, handlers)
        {
        }

        public PipelineOrgSettingsHttpClient(Uri baseUrl, VssCredentials credentials, VssHttpRequestSettings settings, params DelegatingHandler[] handlers)
            : base(baseUrl, credentials, settings, handlers)
        {
        }

        public PipelineOrgSettingsHttpClient(Uri baseUrl, HttpMessageHandler pipeline, bool disposeHandler)
            : base(baseUrl, pipeline, disposeHandler)
        {
        }



        //
        // Summary:
        //     [Preview API] Retrieve information about pipeline organization settings
        //
        // Parameters:
        //   name:
        //     The name of the organization settings in pipeline section
        //
        //   userState:
        //
        //   cancellationToken:
        //     The cancellation token to cancel operation.
        public async Task<PipelineSettings> GetPipelineOrgSettingsAsync(object userState = null, CancellationToken cancellationToken = default(CancellationToken))
        {

            HttpMethod method = new HttpMethod("POST");
            Guid locationId = new Guid("3353E165-A11E-43AA-9D88-14F2BB09B6D9");
            var request = new PipelineSettingsRequest() { ContributionIds = new string[] { "ms.vss-build-web.pipelines-org-settings-data-provider" } };
            using (HttpContent content = new ObjectContent<PipelineSettingsRequest>(request, new VssJsonMediaTypeFormatter(bypassSafeArrayWrapping: true)))
                return await SendAsync<PipelineSettings>(method, locationId, null, new ApiResourceVersion(7.2, 1), content, null, userState, cancellationToken).ConfigureAwait(continueOnCapturedContext: false);
        }
    }

    [DataContract]
    public class PipelineSettings
    {
        [DataMember(Name = "dataProviders")]
        public DataProviders DataProviders { get; set; }

        public PipelineSettings()
        {
            DataProviders = new DataProviders() { PipelineSettingsMap=new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase) };
        }
    }

    public class DataProviders
    {
        [JsonProperty("ms.vss-build-web.pipelines-org-settings-data-provider")]
        public Dictionary<string, bool> PipelineSettingsMap { get; set; }
    }

    public class PipelineSettingsRequest
    {
        public string[] ContributionIds { get; set; }
    }
}
