using Lagrange.Codec;
using Lagrange.Core.Utility;

namespace Lagrange.Core.Test.Codec;

[TestFixture]
public class VideoTest
{
    private byte[] _video;
    
    [SetUp]
    public void Setup()
    {
        _video = File.ReadAllBytes("/Users/wenxuanlin/Downloads/28426634695-1-192.mp4");
    }
    
    [Test]
    public void Test()
    {
        var size = VideoCodec.GetSize(_video);
        var firstFrame = VideoCodec.FirstFrame(_video);
        File.WriteAllBytes("/Users/wenxuanlin/Downloads/firstFrame.jpg", firstFrame);

        var format = ImageHelper.Resolve(firstFrame, out var frameSize);
        
        Assert.Multiple(() =>
        {
            Assert.That(size, Is.Not.Default);
            Assert.That(firstFrame, Is.Not.Null);
            Assert.That(format, Is.EqualTo(ImageFormat.Jpeg2000));
            Assert.That(size.Height, Is.EqualTo(frameSize.X));
            Assert.That(size.Width, Is.EqualTo(frameSize.Y));
        });
        Assert.Pass();
    }
}