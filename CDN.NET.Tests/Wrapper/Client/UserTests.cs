using System.Linq;
using System.Threading.Tasks;
using CDN.NET.Wrapper.Client;
using NUnit.Framework;

namespace CDN.NET.Tests.Wrapper.Client
{
    public class UserTests
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
        public async Task AdminGetAllAlbums()
        {
            var users = await _client.AdminGetAllUsers().ConfigureAwait(false);
            Assert.IsTrue(users.HasValue);
            Assert.IsFalse(users.HasError);
            var usersL = users.Value.ToList();
            Assert.NotZero(usersL.Count);
            var adminUser = usersL.FirstOrDefault(u => u.Username == "admin");
            Assert.NotNull(adminUser);
        }
    }
}