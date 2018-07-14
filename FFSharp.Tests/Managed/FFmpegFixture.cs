using System.IO;

using NUnit.Framework;

namespace FFSharp.Managed
{
    [SetUpFixture]
    internal sealed class FFmpegFixture
    {
        const string C_RelLibRoot = @"../../../FFmpeg";

        [OneTimeSetUp]
        public void Init()
        {
            var assemblyDir = TestContext.CurrentContext.TestDirectory;
            var libRoot = Path.Combine(assemblyDir, C_RelLibRoot);

            Assert.That(Directory.Exists(libRoot));

            LibraryLoader.Init(libRoot);
        }

        [OneTimeTearDown]
        public void Deinit()
        {
            LibraryLoader.Cleanup();
        }
    }
}