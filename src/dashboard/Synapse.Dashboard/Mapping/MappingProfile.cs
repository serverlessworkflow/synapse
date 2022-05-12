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
using AutoMapper;
using Neuroglia.Mapping;

namespace Synapse.Application.Mapping
{

    /// <summary>
    /// Represents the application mapping <see cref="Profile"/>
    /// </summary>
    public class MappingProfile
        : Profile
    {

        /// <summary>
        /// Initializes a new <see cref="MappingProfile"/>
        /// </summary>
        public MappingProfile()
        {
            this.AllowNullCollections = true;
            this.MappingConfigurationTypes = new List<Type>();
            this.Initialize();
        }

        /// <summary>
        /// Gets a <see cref="List{T}"/> containing the types of all existing <see cref="IMappingConfiguration"/>s
        /// </summary>
        protected List<Type> MappingConfigurationTypes { get; }

        /// <summary>
        /// Initializes the <see cref="MappingProfile"/>
        /// </summary>
        protected void Initialize()
        {
            foreach (Type mappingConfigurationType in this.GetType().Assembly
                .GetTypes()
                .Where(t => !t.IsAbstract && !t.IsInterface && t.IsClass && typeof(IMappingConfiguration).IsAssignableFrom(t)))
            {
                this.MappingConfigurationTypes.Add(mappingConfigurationType);
                this.ApplyConfiguration((IMappingConfiguration)Activator.CreateInstance(mappingConfigurationType, new object[] { })!);
            }
        }

    }

}