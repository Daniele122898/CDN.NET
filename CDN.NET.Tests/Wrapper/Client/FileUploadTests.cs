using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CDN.NET.Wrapper.Client;
using CDN.NET.Wrapper.Models;
using NUnit.Framework;

namespace CDN.NET.Tests.Wrapper.Client
{
    public class FileUploadTests
    {
        private CdnClient _client;
        
        [OneTimeSetUp]
        public async Task CreateAndAuthenticate()
        {
            _client = new CdnClient(Constants.BaseUrl);
            var resp = await _client.Login("daniele", "123456");
            Assert.IsNotEmpty(resp.Token);
        }

        [OneTimeTearDown]
        public void DestroyClient()
        {
            _client.Dispose();
        }

        [Test]
        public async Task UploadMultipleFilesNoParametersAndRemove()
        {
            string[] paths = this.GetExampleImages();
            UploadFileInfo[] files = paths.Select(path => new UploadFileInfo(path)).ToArray();
            var uploadedFiles = await _client.UploadFiles(files).ConfigureAwait(false);
            Assert.IsNotEmpty(uploadedFiles);
            Assert.AreEqual(paths.Length, files.Length);
            string[] publicIds = uploadedFiles.Select(f => f.PublicId).ToArray();
            var removedFiles = (await _client.RemoveFiles(publicIds).ConfigureAwait(false)).ToArray();
            Assert.NotNull(removedFiles);
            Assert.AreEqual(paths.Length, removedFiles.Length);
        }
        
        [Test]
        public async Task UploadMultipleFilesWithParametersAndRemove()
        {
            string[] paths = this.GetExampleImages();
            UploadFileInfo[] files = paths.Select(path => new UploadFileInfo(path) {IsPublic = false, Name = "testname"}).ToArray();
            var uploadedFiles = (await _client.UploadFiles(files).ConfigureAwait(false)).ToArray();
            Assert.IsNotEmpty(uploadedFiles);
            Assert.AreEqual(paths.Length, files.Length);
            Assert.IsFalse(uploadedFiles[0].IsPublic);
            Assert.AreEqual("testname", uploadedFiles[1].Name);
            string[] publicIds = uploadedFiles.Select(f => f.PublicId).ToArray();
            var removedFiles = (await _client.RemoveFiles(publicIds).ConfigureAwait(false)).ToArray();
            Assert.NotNull(removedFiles);
            Assert.AreEqual(paths.Length, removedFiles.Length);
        }

        [Test]
        public async Task UploadFileNoParametersAndRemove()
        {
            string path = this.GetExampleImage();
            var response = await _client.UploadFile(path).ConfigureAwait(false);
            Assert.NotNull(response);
            Assert.IsNotEmpty(response.Url);
            Assert.IsNull(response.AlbumId);
            Assert.IsTrue(response.IsPublic);
            Assert.AreEqual(response.PublicId, response.Name);
            await _client.RemoveFile(response.PublicId);
        }

        [Test]
        public async Task UploadFileWithParametersAndRemove()
        {
            string path = this.GetExampleImage();
            var response = await _client.UploadFile(path, "testname", false);
            Assert.NotNull(response);
            Assert.IsNotEmpty(response.Url);
            Assert.IsFalse(response.IsPublic);
            Assert.AreEqual("testname", response.Name);
            await _client.RemoveFile(response.PublicId);
        }

        private string GetExampleImage()
        {
            return Path.Combine(Directory.GetCurrentDirectory(),"ExamplePictures" ,"60181180_p0_master1200.jpg");
        }
        private string[] GetExampleImages()
        {
            return new []
            {
                Path.Combine(Directory.GetCurrentDirectory(),"ExamplePictures" ,"60181180_p0_master1200.jpg"),
                Path.Combine(Directory.GetCurrentDirectory(),"ExamplePictures" ,"60194974_p0_master1200.jpg"),
                Path.Combine(Directory.GetCurrentDirectory(),"ExamplePictures" ,"58240711_p0.png")
            };
        }
    }
}