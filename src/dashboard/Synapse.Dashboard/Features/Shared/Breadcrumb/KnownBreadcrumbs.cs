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
        public static IEnumerable<IBreadcrumbItem> Home = new List<IBreadcrumbItem>() { new BreadcrumbItem("Home", "/", "bi-house") };
        public static IEnumerable<IBreadcrumbItem> Workflows = new List<IBreadcrumbItem>(Home) { new BreadcrumbItem("Workflows", "/workflows", "bi-gear") };
        public static IEnumerable<IBreadcrumbItem> CreateWorkflow = new List<IBreadcrumbItem>(Workflows) { new BreadcrumbItem("Create Workflow", "/workflows/new") };
        public static IEnumerable<IBreadcrumbItem> UploadWorkflow = new List<IBreadcrumbItem>(KnownBreadcrumbs.Workflows) { new BreadcrumbItem("Upload Workflow", "/workflows/upload") };
        public static IEnumerable<IBreadcrumbItem> WorkflowEditor = new List<IBreadcrumbItem>(Workflows) { new BreadcrumbItem("Workflow Editor", "/workflows/editor") };
        public static IEnumerable<IBreadcrumbItem> Correlations = new List<IBreadcrumbItem>(Home) { new BreadcrumbItem("Correlations", "/correlations", "bi-link-45deg") };
        public static IEnumerable<IBreadcrumbItem> Resources = new List<IBreadcrumbItem>(Home) { new BreadcrumbItem("Resources", "/resources", "bi-files") };
        public static IEnumerable<IBreadcrumbItem> FunctionDefinitionCollections = new List<IBreadcrumbItem>(Resources) { new BreadcrumbItem("Functions", "/resources/collections/functions", "bi-files") };
        public static IEnumerable<IBreadcrumbItem> EventDefinitionCollections = new List<IBreadcrumbItem>(Resources) { new BreadcrumbItem("Events", "/resources/collections/events", "bi-files") };
        public static IEnumerable<IBreadcrumbItem> AuthenticationDefinitionCollections = new List<IBreadcrumbItem>(Resources) { new BreadcrumbItem("Authentications", "/resources/collections/authentications", "bi-files") };
        public static IEnumerable<IBreadcrumbItem> CreateFunctionDefinitionCollection = new List<IBreadcrumbItem>(FunctionDefinitionCollections) { new BreadcrumbItem("Create Function Definition Collection", "/resources/collections/functions/new") };
        public static IEnumerable<IBreadcrumbItem> CreateEventDefinitionCollection = new List<IBreadcrumbItem>(FunctionDefinitionCollections) { new BreadcrumbItem("Create Event Definition Collection", "/resources/collections/events/new") };
        public static IEnumerable<IBreadcrumbItem> CreateAuthenticationDefinitionCollection = new List<IBreadcrumbItem>(FunctionDefinitionCollections) { new BreadcrumbItem("Create Authentication Definition Collection", "/resources/collections/authentications/new") };
        public static IEnumerable<IBreadcrumbItem> About = new List<IBreadcrumbItem>(Home) { new BreadcrumbItem("About", "/application/info", "bi-display") };
       
    }
}
