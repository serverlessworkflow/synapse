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
using Synapse.Integration.Models;

namespace Synapse.Application.Mapping.Configuration
{
    internal class CloudEventMappingConfiguration
        : IMappingConfiguration<CloudEvent, V1CloudEventDto>
    {
        void IMappingConfiguration<CloudEvent, V1CloudEventDto>.Configure(IMappingExpression<CloudEvent, V1CloudEventDto> mapping)
        {
            mapping.ForMember(m => m.Data, optuions => optuions.Ignore());
        }

    }

}
