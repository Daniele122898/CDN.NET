using System.Threading.Tasks;
using CDN.NET.Wrapper.Client;
using CDN.NET.Wrapper.Enums;
using NUnit.Framework;

namespace CDN.NET.Tests.Wrapper.Client
{
    public class AuthTests
    {

        [Test, Explicit]
        public async Task SetupTestAccount()
        {
            using var client = new CdnClient(Constants.BaseUrl);
            await client.Register("daniele", "123456");
        }

        [Test]
        public async Task GetApiKeyAndTestAuthentication()
        {
            using var client = new CdnClient(Constants.BaseUrl);
            await client.Login("daniele", "123456");
            string token = await client.GetApiKey();
            Assert.IsNotEmpty(token);
            (bool success, string message) = await client.TestAuthentication();
            Assert.IsTrue(success);
            Assert.IsNotEmpty(message);
        }

        [Test]
        public async Task TestRemoveApiKey()
        {
            using var client = new CdnClient(Constants.BaseUrl);
            await client.Login("daniele", "123456");
            string token = await client.GetApiKey();
            Assert.IsNotEmpty(token);
            await client.DeleteApiKey();
            Assert.AreEqual(AuthenticationType.Jwt, client.CurrentAuthenticationType);
        }

        [Test]
        public async Task LoginAuthTest()
        {
            using var client = new CdnClient(Constants.BaseUrl);
            await client.Login("daniele", "123456");
            (bool success, string message) = await client.TestAuthentication();
            Assert.IsTrue(success);
            Assert.IsNotEmpty(message);
        }

        [Test]
        public async Task AuthTestWithoutLogin()
        {
            using var client = new CdnClient(Constants.BaseUrl);
            (bool success, string message) = await client.TestAuthentication();
            Assert.IsFalse(success);
            Assert.IsNull(message);
        }
        
        [Test, Explicit]
        public async Task RegisterTest()
        {
            using var client = new CdnClient(Constants.BaseUrl);
            var registerResponse = await client.Register("testUser", "123456");
            Assert.AreEqual("testuser", registerResponse.Username);
        }

        [Test]
        public async Task LoginTest()
        {
            using var client = new CdnClient(Constants.BaseUrl);
            var loginDto = await client.Login("daniele", "123456");
            Assert.AreEqual("daniele", loginDto.User.Username);
            Assert.IsNotNull(loginDto.Token);
            Assert.IsNotEmpty(loginDto.Token);
        }
    }
}