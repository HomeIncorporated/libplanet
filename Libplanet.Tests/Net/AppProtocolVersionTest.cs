using System;
using System.Collections.Immutable;
using Bencodex.Types;
using Libplanet.Crypto;
using Libplanet.Net;
using Xunit;

namespace Libplanet.Tests.Net
{
    public class AppProtocolVersionTest
    {
        [Fact]
        public void Sign()
        {
            var signer = new PrivateKey();
            PublicKey otherParty = new PrivateKey().PublicKey;
            var claim = new AppProtocolVersion(signer, 1, null);
            Assert.Equal(1, claim.Version);
            Assert.Null(claim.Extra);
            Assert.True(claim.Verify(signer.PublicKey));
            Assert.False(claim.Verify(otherParty));

            var claimWithExtra = new AppProtocolVersion(signer, 2, (Bencodex.Types.Text)"extra");
            Assert.Equal(2, claimWithExtra.Version);
            Assert.Equal((Bencodex.Types.Text)"extra", claimWithExtra.Extra);
            Assert.True(claimWithExtra.Verify(signer.PublicKey));
            Assert.False(claimWithExtra.Verify(otherParty));

            ArgumentNullException exception =
                Assert.Throws<ArgumentNullException>(() => new AppProtocolVersion(null, 1));
            Assert.Equal("signer", exception.ParamName);
        }

        [Fact]
        public void Verify()
        {
            var signer = new PrivateKey(new byte[]
            {
                0x45, 0x7a, 0xfa, 0x94, 0x17, 0x78, 0x6e, 0x0c, 0xff, 0x4b, 0xa2,
                0x5b, 0x35, 0x95, 0xe1, 0xfb, 0x2a, 0x54, 0x39, 0xf9, 0x0e, 0xd2,
                0x9d, 0x39, 0xdf, 0x54, 0x57, 0x9b, 0x13, 0xea, 0x7c, 0x0f,
            });
            PublicKey signerPublicKey = signer.PublicKey;
            var otherParty = new PrivateKey();
            PublicKey otherPartyPublicKey = otherParty.PublicKey;

            var validClaim = new AppProtocolVersion(
                version: 1,
                extra: null,
                signer: signerPublicKey.ToAddress(),
                signature: new byte[]
                {
                    0x30, 0x45, 0x02, 0x21, 0x00, 0x89, 0x95, 0x9c, 0x59, 0x25, 0x83, 0x4e,
                    0xbc, 0x45, 0x59, 0xd7, 0x9b, 0xca, 0x82, 0x4a, 0x69, 0x20, 0xe5, 0x18,
                    0xf0, 0xc5, 0xad, 0xe2, 0xb9, 0xa3, 0xa3, 0xb3, 0x29, 0xbb, 0xa3, 0x3d,
                    0xd8, 0x02, 0x20, 0x1d, 0xcb, 0x88, 0xa1, 0x3a, 0x3c, 0x19, 0x2d, 0xe1,
                    0x9e, 0x39, 0xf6, 0x58, 0x05, 0xd4, 0x06, 0xbf, 0xb2, 0x93, 0xd1, 0x64,
                    0x85, 0x75, 0xa8, 0xa2, 0xcb, 0x9f, 0x95, 0xd9, 0x90, 0xb9, 0x51,
                }.ToImmutableArray()
            );
            Assert.True(validClaim.Verify(signerPublicKey));
            Assert.False(validClaim.Verify(otherPartyPublicKey));

            // A signature is no more valid for a different version.
            var invalidVersionClaim = new AppProtocolVersion(
                version: validClaim.Version + 1,
                extra: validClaim.Extra,
                signer: validClaim.Signer,
                signature: validClaim.Signature
            );
            Assert.False(invalidVersionClaim.Verify(signerPublicKey));
            Assert.False(invalidVersionClaim.Verify(otherPartyPublicKey));

            // A signature is no more valid for a different extra data.
            var invalidExtraClaim = new AppProtocolVersion(
                version: validClaim.Version,
                extra: (Bencodex.Types.Text)"invalid extra",
                signer: validClaim.Signer,
                signature: validClaim.Signature
            );
            Assert.False(invalidExtraClaim.Verify(signerPublicKey));
            Assert.False(invalidExtraClaim.Verify(otherPartyPublicKey));

            // If a signer field does not correspond to an actual private key which signed
            // a signature a claim is invalid even if a signature in itself is valid.
            var invalidSigner = new AppProtocolVersion(
                version: validClaim.Version,
                extra: validClaim.Extra,
                signer: otherPartyPublicKey.ToAddress(),
                signature: validClaim.Signature
            );
            Assert.False(invalidSigner.Verify(signerPublicKey));
            Assert.False(invalidSigner.Verify(otherPartyPublicKey));
        }

        [Fact]
        public void String()
        {
            var signer = new PrivateKey();
            var claim = new AppProtocolVersion(signer, 123);
            Assert.Equal("123", claim.ToString());

            var claimWithExtra = new AppProtocolVersion(signer, 456, (Bencodex.Types.Text)"extra");
            Assert.Equal("456 (Bencodex.Types.Text \"extra\")", claimWithExtra.ToString());
        }
    }
}
