// Copyright 2014 Sean Farrow
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace SeanFarrow.EventStoreExtensions.Runners
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Threading;

    using EventStore.ClientAPI.Embedded;
    using EventStore.Core;

    public class EventStoreRunner: IDisposable
    {
        private ClusterVNode _embeddednode; //The embedded server.
        
        private bool _disposed;

        private EventStoreRunnerOptions _runneroptions;

        private Process _fullserverprocess;
        
        public EventStoreRunner(EventStoreRunnerOptions options)
        {
            this._runneroptions = options;
            if (this._runneroptions.UseEmbeddedServer)
            {
                this.CreateEmbeddedServer();
            }
            else
            {
                this.CreateFullServer();
            }
        }

        private void CreateFullServer()
        {
            var thread = new Thread(this.StartEventStoreServer)
            {
                IsBackground = true
            };
            thread.Start();
        }

        private void StartEventStoreServer()
        {
            var eventStorePath = Path.Combine(Assembly.GetExecutingAssembly().GetExecutingFolder(), "EventStoreBinaries", "EventStore.ClusterNode.exe");
            if (File.Exists(eventStorePath))
            {
                throw new FileNotFoundException("The EventStore binaries are not in the project directory structure.");
            }
            var startInfo = new ProcessStartInfo
            {
                FileName = eventStorePath,
                WindowStyle = ProcessWindowStyle.Normal,
                ErrorDialog = true,
                LoadUserProfile = true,
                CreateNoWindow = false,
                UseShellExecute = false
            };
            
            string cmdline = null;
            if (this._runneroptions.DataDirectory.Any())
            {
                string combinedPath;
                if (Path.IsPathRooted(this._runneroptions.DataDirectory))
                {
                    combinedPath = this._runneroptions.DataDirectory;
                }
                else
                {
                    combinedPath = Path.Combine(Assembly.GetExecutingAssembly().GetExecutingFolder(), this._runneroptions.DataDirectory);
                }
                cmdline = String.Format("--db={0}", combinedPath);
            }
            if (!String.IsNullOrEmpty(cmdline))
            {
                startInfo.Arguments = cmdline;
            }
            
            try
            {
                this._fullserverprocess = new Process { StartInfo = startInfo };

                this._fullserverprocess.Start();
                this._fullserverprocess.WaitForExit();
            }
            catch
            {
                this._fullserverprocess.CloseMainWindow();
                this._fullserverprocess.Dispose();
            }
        }

        private void CreateEmbeddedServer()
        {
            EmbeddedVNodeBuilder builder;
            builder = EmbeddedVNodeBuilder.AsSingleNode();
            builder.RunProjections(ProjectionsMode.All);
            builder.OnDefaultEndpoints();
            if (this._runneroptions.RunInMemory)
            {
                builder.RunInMemory();
            }
            else
            {
                string combinedPath;
                if (Path.IsPathRooted(this._runneroptions.DataDirectory))
                {
                    combinedPath = this._runneroptions.DataDirectory;
                }
                else
                {
                    combinedPath = Path.Combine(Assembly.GetExecutingAssembly().GetExecutingFolder(), this._runneroptions.DataDirectory);
                }
                builder.RunOnDisk(combinedPath);
            }
            this._embeddednode = builder.Build();
            this._embeddednode.Start();
        }
            
        public void Dispose()
        {
            this.Dispose(true);
        GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (this._disposed)
            {
                if (disposing)
                {
                    if (this._runneroptions.UseEmbeddedServer)
                    {
                        this.DisposeEmbeddedServer();
                    }
                    else
                    {
                        this.DisposeFullServer();
                    }
                    this.PurgeDataFolder();            
                    this._disposed = true;
                }
            }
        }

        private void PurgeDataFolder()
        {
            if (this._runneroptions.PurgeData)
            {
                string combinedPath;
                if (Path.IsPathRooted(this._runneroptions.DataDirectory))
                {
                    combinedPath = this._runneroptions.DataDirectory;
                }
                else
                {
                    combinedPath = Path.Combine(Assembly.GetExecutingAssembly().GetExecutingFolder(), this._runneroptions.DataDirectory);
                }
Directory.Delete(combinedPath);
            }
        }
        
        private void DisposeFullServer()
        {
            this._fullserverprocess.Kill();
            this._fullserverprocess.Dispose();
            }

        private void DisposeEmbeddedServer()
        {
            this._embeddednode.Stop();
            this._embeddednode = null;
        }
    }
}