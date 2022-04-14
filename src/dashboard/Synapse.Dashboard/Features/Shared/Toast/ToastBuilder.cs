/*
 * Copyright © 2022-Present The Synapse Authors
 * <p>
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 * <p>
 * http://www.apache.org/licenses/LICENSE-2.0
 * <p>
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using Microsoft.AspNetCore.Components;

namespace Synapse.Dashboard
{
    public class ToastBuilder
        : IToastBuilder
    {

        protected Toast Toast { get; } = new();

        public virtual IToastBuilder WithLevel(LogLevel level)
        {
            this.Toast.Level = level;
            return this;
        }

        public virtual IToastBuilder WithLifetime(TimeSpan duration)
        {
            this.Toast.Duration = duration;
            return this;
        }

        public virtual IToastBuilder WithHeader(string header)
        {
            this.Toast.Header = (MarkupString)header;
            return this;
        }

        public virtual IToastBuilder WithoutHeader()
        {
            this.Toast.HasHeader = false;
            return this;
        }

        public virtual IToastBuilder WithBody(string body)
        {
            this.Toast.Body = (MarkupString)body;
            return this;
        }

        public virtual IToastBuilder OnHide(Action callback)
        {
            this.Toast.OnHideCallback = callback;
            return this;
        }

        public virtual Toast Build()
        {
            return this.Toast;
        }

    }

}
