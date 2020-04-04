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
            var resp = await _client.Login("admin", "password");
            Assert.IsTrue(resp.HasValue);
            Assert.IsNotEmpty(resp.Value.Token);
        }

        [OneTimeTearDown]
        public void DestroyClient()
        {
            _client.Dispose();
        }

        [Test]
        public async Task UploadMultipleFilesNoParametersAndRemove()
        {
            string[] paths = GetExampleImages();
            UploadFileInfo[] files = paths.Select(path => new UploadFileInfo(path)).ToArray();
            var uploadedFiles = (await _client.UploadFiles(files).ConfigureAwait(false)).Value.ToList();
            Assert.IsNotEmpty(uploadedFiles);
            Assert.AreEqual(paths.Length, uploadedFiles.Count);
            string[] publicIds = uploadedFiles.Select(f => f.PublicId).ToArray();
            var removedFiles = (await _client.RemoveFiles(publicIds).ConfigureAwait(false)).Value.ToArray();
            Assert.NotNull(removedFiles);
            Assert.AreEqual(paths.Length, removedFiles.Length);
        }
        
        [Test]
        public async Task UploadMultipleFilesWithParametersAndRemove()
        {
            string[] paths = GetExampleImages();
            UploadFileInfo[] files = paths.Select(path => new UploadFileInfo(path) {IsPublic = false, Name = "testname"}).ToArray();
            var uploadedFiles = (await _client.UploadFiles(files).ConfigureAwait(false)).Value.ToArray();
            Assert.IsNotEmpty(uploadedFiles);
            Assert.AreEqual(paths.Length, files.Length);
            Assert.IsFalse(uploadedFiles[0].IsPublic);
            Assert.AreEqual("testname", uploadedFiles[1].Name);
            string[] publicIds = uploadedFiles.Select(f => f.PublicId).ToArray();
            var removedFiles = (await _client.RemoveFiles(publicIds).ConfigureAwait(false)).Value.ToArray();
            Assert.NotNull(removedFiles);
            Assert.AreEqual(paths.Length, removedFiles.Length);
        }

        [Test]
        public async Task UploadFileNoParametersAndRemove()
        {
            string path = GetExampleImage();
            var responseMaybe = await _client.UploadFile(path).ConfigureAwait(false);
            Assert.IsTrue(responseMaybe.HasValue);
            var response = responseMaybe.Value;
            Assert.NotNull(response);
            Assert.IsNotEmpty(response.Url);
            Assert.IsNull(response.AlbumId);
            Assert.IsTrue(response.IsPublic);
            await _client.RemoveFile(response.PublicId);
        }

        [Test]
        public async Task UploadFileWithParametersAndRemove()
        {
            string path = GetExampleImage();
            var responseM = await _client.UploadFile(path, "testname", false);
            Assert.IsTrue(responseM.HasValue);
            var response = responseM.Value;
            Assert.NotNull(response);
            Assert.IsNotEmpty(response.Url);
            Assert.IsFalse(response.IsPublic);
            Assert.AreEqual("testname", response.Name);
            await _client.RemoveFile(response.PublicId);
        }

        public static string GetExampleImage()
        {
            return Path.Combine(Directory.GetCurrentDirectory(),"ExamplePictures" ,"60181180_p0_master1200.jpg");
        }
        public static string[] GetExampleImages()
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