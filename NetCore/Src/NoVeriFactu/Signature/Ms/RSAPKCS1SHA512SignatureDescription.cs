// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Cryptography;

namespace VeriFactu.NoVeriFactu.Signature.Ms
{
    internal sealed class RSAPKCS1SHA512SignatureDescription : RSAPKCS1SignatureDescription
    {
        public RSAPKCS1SHA512SignatureDescription() : base("SHA512")
        {
        }

        public sealed override HashAlgorithm CreateDigest()
        {
            return SHA512.Create();
        }
    }
}
