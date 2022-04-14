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
using System.ComponentModel;

namespace Synapse.Dashboard
{
    public class LayoutService
        : ILayoutService
    {

        public event PropertyChangedEventHandler? PropertyChanged;

        public RenderFragment? HeaderFragment => this.Header?.ChildContent;
        public RenderFragment? RightSidebarFragment => this.RightSidebar?.ChildContent;
        public RenderFragment? LeftSidebarFragment => this.LeftSidebar?.ChildContent;

        private AppHeader? _Header;
        public AppHeader? Header
        {
            get => this._Header;
            set
            {
                if (this._Header == value) return;
                this._Header = value;
                this.UpdateHeader();
            }
        }

        private AppRightSidebar? _RightSidebar;
        public AppRightSidebar? RightSidebar
        {
            get => this._RightSidebar;
            set
            {
                if (this._RightSidebar == value) return;
                this._RightSidebar = value;
                this.UpdateRightSidebar();
            }
        }

        private AppLeftSidebar? _LeftSidebar;
        public AppLeftSidebar? LeftSidebar
        {
            get => this._LeftSidebar;
            set
            {
                if (this._LeftSidebar == value) return;
                this._LeftSidebar = value;
                this.UpdateLeftSidebar();
            }
        }

        public void UpdateHeader()
        {
            if (this.PropertyChanged != null) 
            { 
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.Header)));
            }
        }

        public void UpdateRightSidebar()
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.RightSidebar)));
            }
        }

        public void UpdateLeftSidebar()
        {
            if (this.PropertyChanged != null)
            {
                this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(this.LeftSidebar)));
            }
        }
    }

}
