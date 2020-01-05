using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CDN.NET.Wrapper.Client;
using CDN.NET.Wrapper.Dtos;
using CDN.NET.Wrapper.Models;
using NUnit.Framework;

namespace CDN.NET.Tests.Wrapper.Client
{
    public class FileTests
    {
        private CdnClient _client;

        private List<FileResponse> _uploadedFiles;
        
        [OneTimeSetUp]
        public async Task CreateAndAuthenticate()
        {
            _client = new CdnClient(Constants.BaseUrl);
            var resp = await _client.Login("daniele", "123456");
            Assert.IsNotEmpty(resp.Token);

            // Upload files to be used in further tests
            _uploadedFiles = (await this.UploadMultipleFilesNoParameters().ConfigureAwait(false)).ToList();
            Assert.IsNotEmpty(_uploadedFiles);
        }

        [OneTimeTearDown]
        public async Task DestroyClient()
        {
            string[] publicIds = _uploadedFiles.Select(f => f.PublicId).ToArray();
            var removedFiles = (await this.RemoveMultipleFiles(publicIds).ConfigureAwait(false)).ToList();
            Assert.IsNotEmpty(removedFiles);
            Assert.AreEqual(publicIds.Length, removedFiles.Count);
            _client.Dispose();
        }

        [Test]
        public async Task GetAllFilesTest()
        {
            var getallFiles = (await _client.GetAllFiles().ConfigureAwait(false)).ToList();
            Assert.IsNotEmpty(getallFiles);
            Assert.AreEqual(_uploadedFiles.Count, getallFiles.Count);
        }
        
        private async Task<IEnumerable<FileResponse>> UploadMultipleFilesNoParameters()
        {
            string[] paths = FileUploadTests.GetExampleImages();
            UploadFileInfo[] files = paths.Select(path => new UploadFileInfo(path)).ToArray();
            return await _client.UploadFiles(files).ConfigureAwait(false);
        }

        private async Task<IEnumerable<FileRemoveResponse>> RemoveMultipleFiles(string[] publicIds)
        {
            return await _client.RemoveFiles(publicIds).ConfigureAwait(false);
        }
    }
}