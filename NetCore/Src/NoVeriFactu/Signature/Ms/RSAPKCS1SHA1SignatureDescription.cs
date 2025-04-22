// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Cryptography;

namespace VeriFactu.NoVeriFactu.Signature.Ms
{
    internal sealed class RSAPKCS1SHA1SignatureDescription : RSAPKCS1SignatureDescription
    {
        public RSAPKCS1SHA1SignatureDescription() : base("SHA1")
        {
        }

        public sealed override HashAlgorithm CreateDigest()
        {
            return SHA1.Create();
        }
    }
}
