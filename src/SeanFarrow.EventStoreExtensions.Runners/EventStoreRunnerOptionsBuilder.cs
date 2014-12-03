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

    public class EventStoreRunnerOptionsBuilder
    {
        private bool _useembeddedserver;

        private bool _purgedata;
        private bool _runinmemory;
        private string _datadirectory;

        public EventStoreRunnerOptionsBuilder()
        {
            UseEmbeddedServer();
        }

        public EventStoreRunnerOptionsBuilder UseEmbeddedServer()
        {
            this._useembeddedserver = true;
            this._runinmemory = true;
            this._datadirectory = String.Empty;
            return this;
        }

        public EventStoreRunnerOptionsBuilder RunInMemory()
        {
            this._runinmemory = true;
            return this;
        }
        
        public EventStoreRunnerOptionsBuilder UseFullServer()
        {
            _useembeddedserver = false;
            _runinmemory = false;
            _datadirectory = string.Empty;
            return this;
        }

        public EventStoreRunnerOptionsBuilder UseDataDirectory(string directory)
        {
            this._datadirectory = directory;
            return this;
        }

        public EventStoreRunnerOptionsBuilder PurgeData()
        {
            this._purgedata = true;
            return this;
        }
        
        public EventStoreRunnerOptions Build()
        {
            return new EventStoreRunnerOptions(this._useembeddedserver, this._runinmemory, this._datadirectory, this._purgedata);
        }
        
        /// <summary>
        /// Convert the mutable <see cref="EventStoreRunnerOptionsBuilder"/> object to an immutable
        /// <see cref="EventStoreRunnerOptions"/> object.
        /// </summary>
        /// <param name="builder">The <see cref="EventStoreRunnerOptionsBuilder"/> to convert.</param>
        /// <returns>An immutable <see cref="EventStoreRunnerOptions"/> object with the values specified by the builder.</returns>
        public static implicit operator EventStoreRunnerOptions(EventStoreRunnerOptionsBuilder builder)
        {
            return builder.Build();
        }
    }
}