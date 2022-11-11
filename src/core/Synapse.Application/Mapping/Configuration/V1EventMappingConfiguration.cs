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
using AutoMapper;

namespace Synapse.Application.Mapping.Configuration
{

    internal class V1EventMappingConfiguration
        : IMappingConfiguration<CloudEvent, Integration.Models.V1Event>,
        IMappingConfiguration<V1Event, Integration.Models.V1Event>,
        IMappingConfiguration<Integration.Models.V1Event, V1Event>
    {

        void IMappingConfiguration<CloudEvent, Integration.Models.V1Event>.Configure(IMappingExpression<CloudEvent, Integration.Models.V1Event> mapping)
        {
            mapping.ForMember(e => e.SpecVersion, options => options.MapFrom(ce => ce.SpecVersion.VersionId));
        }

        void IMappingConfiguration<V1Event, Integration.Models.V1Event>.Configure(IMappingExpression<V1Event, Integration.Models.V1Event> mapping)
        {
            mapping.ForMember(e => e.Attributes, options => options.Ignore());
            mapping.ForMember(e => e.ContextAttributes, options => options.Ignore());
        }

        void IMappingConfiguration<Integration.Models.V1Event, V1Event>.Configure(IMappingExpression<Integration.Models.V1Event, V1Event> mapping)
        {
            mapping.ForMember(e => e.Attributes, options => options.Ignore());
        }

    }
}
