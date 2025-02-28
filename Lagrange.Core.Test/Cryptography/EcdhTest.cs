using Lagrange.Core.Utility.Cryptography;

namespace Lagrange.Core.Test.Cryptography;

[TestFixture]
[Parallelizable]
public class EcdhTest
{
    private EcdhProvider _alice;
    private EcdhProvider _bob;

    private EcdhProvider _customSecret;
    
    [SetUp]
    public void Setup()
    {
        _alice = new EcdhProvider(EllipticCurve.Prime256V1);
        _bob = new EcdhProvider(EllipticCurve.Prime256V1);

        var secret = _alice.PackSecret();
        _customSecret = new EcdhProvider(EllipticCurve.Prime256V1, secret); // same secret should generate same public key
    }
    
    [Test]
    public void Test()
    {
        var alicePubCompressed = _alice.PackPublic(true);
        var bobPubCompressed = _bob.PackPublic(true);
        
        var aliceSharedPacked = _alice.KeyExchange(bobPubCompressed, true);
        var bobSharedPacked = _bob.KeyExchange(alicePubCompressed, true);
        
        var alicePub = _alice.PackPublic(false);
        var bobPub = _bob.PackPublic(false);
        
        var aliceShared = _alice.KeyExchange(bobPub, true);
        var bobShared = _bob.KeyExchange(alicePub, true);
        
        var aliceSecret = _alice.PackSecret();
        var bobSecret = _bob.PackSecret();
        
        Assert.Multiple(() =>
        {
            Assert.That(aliceShared, Is.EqualTo(bobShared)); // Equality check
            Assert.That(aliceSharedPacked, Is.EqualTo(bobSharedPacked)); // Equality check
            Assert.That(aliceSecret, Is.Not.EqualTo(bobSecret)); // Uniqueness check
            Assert.That(aliceSecret, Has.Length.EqualTo(aliceSecret[3] + 4)); // Length check
            Assert.That(bobSecret, Has.Length.EqualTo(bobSecret[3] + 4)); // Length check
        });
        Assert.Pass();
    }
    
    [Test]
    public void TestCustomSecret()
    {
        var customPub = _customSecret.PackPublic(false);
        var alicePub = _alice.PackPublic(false);
        
        Assert.That(customPub, Is.EqualTo(alicePub));
    }
}