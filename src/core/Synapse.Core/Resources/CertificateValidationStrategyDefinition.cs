﻿// Copyright © 2024-Present The Synapse Authors
//
// Licensed under the Apache License, Version 2.0 (the "License"),
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

namespace Synapse.Resources;

/// <summary>
/// Represents the configuration of a certificate validation strategy
/// </summary>
[DataContract]
public record CertificateValidationStrategyDefinition
{

    /// <summary>
    /// Gets/sets a boolean indicating whether or not to validate certificates when performing requests
    /// </summary>
    [DataMember(Order = 1, Name = "validate"), JsonPropertyOrder(1), JsonPropertyName("validate"), YamlMember(Order = 1, Alias = "validate")]
    public virtual bool? Validate { get; set; }

}