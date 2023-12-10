using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Agent.Sdk;
using Agent.Sdk.Util;

namespace Microsoft.VisualStudio.Services.Agent.Util
{
    public class NodeJsUtil
    {
        public CancellationToken CancellationToken { protected get; set; }
        readonly Tracing Tracer;
        readonly IHostContext HostContext;

        public NodeJsUtil(IHostContext hostContext)
        {
            Tracer = hostContext.GetTrace(this.GetType().Name);
            HostContext = hostContext;
        }
        public async Task DownloadNodeRunnerAsync(CancellationToken cancellationToken)
        {

            if (File.Exists(Path.Combine(HostContext.GetDirectory(WellKnownDirectory.Externals), "node", "bin", $"node{IOUtil.ExeExtension}")))
            {
                Tracer.Info($"Node 6 runner already exist.");
                return;
            }

            string downloadUrl;
            string urlFileName;

            if (PlatformUtil.RunningOnWindows)
            {
                urlFileName = $"node-v6-latest-win-{VarUtil.OSArchitecture}";
            }
            else
            {
                urlFileName = $"node-v6-latest-{VarUtil.OS}-{VarUtil.OSArchitecture}";
            }

            if (PlatformUtil.HostOS == PlatformUtil.OS.OSX && PlatformUtil.HostArchitecture == System.Runtime.InteropServices.Architecture.X86)
            {
                urlFileName = $"node-v6-latest-linux-x86";
            }

            urlFileName = urlFileName.ToLower();

            downloadUrl = $"https://vstsagenttools.blob.core.windows.net/tools/nodejs/deprecated/{urlFileName}.zip".ToLower();

            Tracer.Info($"Downloading Node 6 runner from: {downloadUrl}");

            string externalsFolder = HostContext.GetDirectory(WellKnownDirectory.Externals);
            string filePath = Path.Combine(externalsFolder, $"node_{DateTime.Now.ToFileTime()}.zip");

            var timeoutSeconds = 600;

            using (var downloadTimeout = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken))
            {
                // Set a timeout because sometimes stuff gets stuck.
                downloadTimeout.CancelAfter(TimeSpan.FromSeconds(timeoutSeconds));

                try
                {
                    Tracer.Info($"Download Node 6 runner: begin download");

                    using (var handler = HostContext.CreateHttpClientHandler())
                    {
                        handler.CheckCertificateRevocationList = true;
                        using (var httpClient = new HttpClient(handler))
                        using (var fs = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize: 4096, useAsync: true))
                        using (var result = await httpClient.GetStreamAsync(downloadUrl))
                        {
                            //81920 is the default used by System.IO.Stream.CopyTo and is under the large object heap threshold (85k).
                            await result.CopyToAsync(fs, 81920, downloadTimeout.Token);
                            await fs.FlushAsync(downloadTimeout.Token);
                        }

                        Tracer.Info($"Download Node 6 runner: finished download");

                        Tracer.Info($"Extracting downloaded archive into externals folder");

                        ZipFile.ExtractToDirectory(filePath, externalsFolder);

                        Tracer.Info($"Move node binary into relevant folder");

                        Directory.Move(Path.Combine(externalsFolder, urlFileName, "node"), Path.Combine(externalsFolder, "node"));

                        Tracer.Info($"Finished getting Node 6 runner at: {externalsFolder}.");
                    }
                }
                catch (OperationCanceledException) when (downloadTimeout.IsCancellationRequested)
                {
                    Tracer.Info($"Node 6 runner download has been canceled.");
                    throw;
                }
                catch (SocketException ex)
                {
                    ExceptionsUtil.HandleSocketException(ex, downloadUrl, Tracer.Warning);
                }
                catch (Exception ex)
                {
                    if (downloadTimeout.Token.IsCancellationRequested)
                    {
                        Tracer.Warning($"Node 6 runner download has timed out after {timeoutSeconds} seconds");
                    }

                    Tracer.Warning($"Failed to get package '{filePath}' from '{downloadUrl}'. Exception {ex}");
                }

                finally
                {
                    try
                    {
                        IOUtil.DeleteDirectory(Path.Combine(externalsFolder, urlFileName), cancellationToken);
                        // delete .zip file
                        if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
                        {
                            Tracer.Verbose("Deleting Node 6 runner package zip: {0}", filePath);
                            IOUtil.DeleteFile(filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        //it is not critical if we fail to delete the .zip file
                        Tracer.Warning("Failed to delete Node 6 runner package zip '{0}'. Exception: {1}", filePath, ex);
                    }
                }
            }
        }
    }
}
