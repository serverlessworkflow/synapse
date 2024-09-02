// Copyright © 2024-Present The Synapse Authors
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

using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Synapse.Api.Application;

/// <summary>
/// Expose static methods used to help manage the key used for signing service account JwTs
/// </summary>
public static class ServiceAccountSigningKey
{

    const string KeyDirectory = "keys";
    const string PrivateKeyFile = "service_account_private.pem";
    const string PublicKeyFile = "service_account_public.pem";

    /// <summary>
    /// Creates the Service Account JWT signing key if it does not exist
    /// </summary>
    public static void Initialize()
    {
        var keyPath = Path.Combine(AppContext.BaseDirectory, KeyDirectory);
        if (!Directory.Exists(keyPath)) Directory.CreateDirectory(keyPath);
        var privateKeyPath = Path.Combine(keyPath, PrivateKeyFile);
        var publicKeyPath = Path.Combine(keyPath, PublicKeyFile);
        if (!File.Exists(privateKeyPath) || !File.Exists(publicKeyPath)) GenerateKeyPair(privateKeyPath, publicKeyPath);
    }

    /// <summary>
    /// Loads the private service account signing key
    /// </summary>
    /// <returns>The private service account signing key</returns>
    public static SigningCredentials LoadPrivateKey()
    {
        var keyPath = Path.Combine(AppContext.BaseDirectory, KeyDirectory);
        if (!Directory.Exists(keyPath)) Directory.CreateDirectory(keyPath);
        var privateKeyPath = Path.Combine(keyPath, PrivateKeyFile);
        var privateKey = File.ReadAllText(privateKeyPath)
            .Replace("-----BEGIN PRIVATE KEY-----\n", string.Empty)
            .Replace("-----END PRIVATE KEY-----", string.Empty)
            .Replace("\n", string.Empty);
        var privateKeyBytes = Convert.FromBase64String(privateKey);
        var rsa = RSA.Create();
        rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
        return new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256);
    }

    /// <summary>
    /// Loads the public service account signing key
    /// </summary>
    /// <returns>The public service account signing key</returns>
    public static SecurityKey LoadPublicKey()
    {
        var keyPath = Path.Combine(AppContext.BaseDirectory, KeyDirectory);
        if (!Directory.Exists(keyPath)) Directory.CreateDirectory(keyPath);
        var publicKeyPath = Path.Combine(keyPath, PublicKeyFile);
        var publicKey = File.ReadAllText(publicKeyPath)
            .Replace("-----BEGIN PUBLIC KEY-----\n", string.Empty)
            .Replace("-----END PUBLIC KEY-----", string.Empty)
            .Replace("\n", string.Empty);
        var publicKeyBytes = Convert.FromBase64String(publicKey);
        var rsa = RSA.Create();
        rsa.ImportRSAPublicKey(publicKeyBytes, out _);
        return new RsaSecurityKey(rsa);
    }

    static void GenerateKeyPair(string privateKeyPath, string publicKeyPath)
    {
        using var rsa = RSA.Create(2048);
        var privateKey = ExportPrivateKey(rsa);
        File.WriteAllText(privateKeyPath, privateKey);
        var publicKey = ExportPublicKey(rsa);
        File.WriteAllText(publicKeyPath, publicKey);
    }

    static string ExportPrivateKey(RSA rsa)
    {
        var privateKeyBytes = rsa.ExportRSAPrivateKey();
        return $"-----BEGIN PRIVATE KEY-----\n{Convert.ToBase64String(privateKeyBytes, Base64FormattingOptions.InsertLineBreaks)}\n-----END PRIVATE KEY-----";
    }

    static string ExportPublicKey(RSA rsa)
    {
        var publicKeyBytes = rsa.ExportRSAPublicKey();
        return $"-----BEGIN PUBLIC KEY-----\n{Convert.ToBase64String(publicKeyBytes, Base64FormattingOptions.InsertLineBreaks)}\n-----END PUBLIC KEY-----";
    }

}
