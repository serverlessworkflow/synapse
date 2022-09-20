/*
 * Copyright © 2022-Present The Synapse Authors
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 * http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

namespace Synapse.Dashboard
{
    public static class KnownBreadcrumbs
    {
        public static IEnumerable<IBreadcrumbItem> Home = new List<IBreadcrumbItem>() { new BreadcrumbItem("Home", "/", "bi-clipboard") };
        public static IEnumerable<IBreadcrumbItem> Workflows = new List<IBreadcrumbItem>(Home) { new BreadcrumbItem("Workflows", "/workflows", "bi-gear") };
        public static IEnumerable<IBreadcrumbItem> CreateWorkflow = new List<IBreadcrumbItem>(Workflows) { new BreadcrumbItem("Create Workflow", "/workflows/new") };
        public static IEnumerable<IBreadcrumbItem> UploadWorkflow = new List<IBreadcrumbItem>(KnownBreadcrumbs.Workflows) { new BreadcrumbItem("Upload Workflow", "/workflows/upload") };
        public static IEnumerable<IBreadcrumbItem> WorkflowEditor = new List<IBreadcrumbItem>(Workflows) { new BreadcrumbItem("Workflow Editor", "/workflows/editor") };
        public static IEnumerable<IBreadcrumbItem> Correlations = new List<IBreadcrumbItem>(Home) { new BreadcrumbItem("Correlations", "/correlations", "bi-link-45deg") };
        public static IEnumerable<IBreadcrumbItem> System = new List<IBreadcrumbItem>(Home) { new BreadcrumbItem("System", "/system/info", "bi-display") };
       
    }
}
