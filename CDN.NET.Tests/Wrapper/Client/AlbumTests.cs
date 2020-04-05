using System.Linq;
using System.Threading.Tasks;
using CDN.NET.Wrapper.Client;
using CDN.NET.Wrapper.Dtos.Album;
using NUnit.Framework;

namespace CDN.NET.Tests.Wrapper.Client
{
    public class AlbumTests
    {
        private CdnClient _client;

        private Album _albumToBeDeleted;
        private Album _albumUsedForTestingPrivate;
        private Album _albumUsedForTesting;
        
        [OneTimeSetUp]
        public async Task CreateAndAuthenticate()
        {
            _client = new CdnClient(Constants.BaseUrl);
            var resp = await _client.Login("admin", "password");
            Assert.IsTrue(resp.HasValue);
            Assert.IsNotEmpty(resp.Value.Token);

            // Create an album that is going to be deleted
            _albumToBeDeleted = (await _client.CreateAlbum("testAlbum").ConfigureAwait(false)).Value;
            Assert.NotNull(_albumToBeDeleted);
            _albumUsedForTestingPrivate = (await _client.CreateAlbum("testAlbum2", false).ConfigureAwait(false)).Value;
            Assert.NotNull(_albumUsedForTestingPrivate);
            _albumUsedForTesting = (await _client.CreateAlbum("testAlbum123").ConfigureAwait(false)).Value;
            Assert.NotNull(_albumUsedForTesting);
        }

        [OneTimeTearDown]
        public async Task DestroyClient()
        {
            // Remove album used for testing
            await _client.DeleteAlbum(_albumUsedForTestingPrivate.Id);
            await _client.DeleteAlbum(_albumUsedForTesting.Id);
            
            _client.Dispose();
        }

        [Test]
        public async Task GetAllAlbums()
        {
            var resp = (await _client.GetAllAlbums().ConfigureAwait(false)).Value.ToList();
            Assert.NotNull(resp);
            Assert.IsNotEmpty(resp);
        }

        [Test]
        public async Task GetAllAlbumsSparse()
        {
            var resp = await _client.GetAllAlbumsSparse();
            Assert.IsTrue(resp.HasValue);
            var albums = resp.Value.ToList();
            Assert.NotNull(albums);
            Assert.IsNotEmpty(albums);
        }

        [Test]
        public async Task GetPrivateAlbum()
        {
            var resp = await _client.GetPrivateAlbum(_albumUsedForTestingPrivate.Id).ConfigureAwait(false);
            Assert.IsTrue(resp.HasValue);
            Assert.NotNull(resp.Value);
            Assert.AreEqual(_albumUsedForTestingPrivate.Id, resp.Value.Id);
        }
        
        [Test]
        public async Task GetAlbum()
        {
            var resp = await _client.GetAlbum(_albumUsedForTesting.Id).ConfigureAwait(false);
            Assert.IsTrue(resp.HasValue);
            Assert.NotNull(resp.Value);
            Assert.AreEqual(_albumUsedForTesting.Id, resp.Value.Id);
        }
        
        [Test]
        public async Task DeleteAlbum()
        {
            var resp = await _client.DeleteAlbum(_albumToBeDeleted.Id).ConfigureAwait(false);
            Assert.IsTrue(resp.HasValue);
            Assert.IsTrue(resp.Value);
        }

        [Test]
        public async Task CreateAndThenRemoveAlbum()
        {
            var resp = await _client.CreateAlbum("test3").ConfigureAwait(false);
            Assert.IsTrue(resp.HasValue);
            Assert.NotNull(resp.Value);
            Assert.AreEqual("test3", resp.Value.Name);
            await _client.DeleteAlbum(resp.Value.Id);
        }
    }
}