using System;
using System.Linq;
using System.Threading.Tasks;
using CDN.NET.Wrapper.Client;
using CDN.NET.Wrapper.Models;
using NUnit.Framework;

namespace CDN.NET.Tests.Wrapper.Client
{
    public class UserTests
    {
        private CdnClient _client;

        private User _userToRemove;
        private User _userToRename;
        
        [OneTimeSetUp]
        public async Task CreateAndAuthenticate()
        {
            _client = new CdnClient(Constants.BaseUrl);
            var resp = await _client.Login("admin", "password");
            Assert.IsTrue(resp.HasValue);
            Assert.IsNotEmpty(resp.Value.Token);
            // Add dummy user to remove
            var userToRemove = await _client.Register("removeMePlease", "password");
            var userToRename = await _client.Register("changeMyName", "password");
            Assert.IsTrue(userToRemove.HasValue);
            Assert.IsTrue(userToRename.HasValue);
            _userToRemove = userToRemove.Value;
            _userToRename = userToRename.Value;
        }
        
        [OneTimeTearDown]
        public async Task DestroyClient()
        {
            var succ =  await _client.AdminRemoveUser(_userToRename.Id);
            Assert.IsTrue(succ.HasValue);
            Assert.IsTrue(succ.Value);
            // Remove removeMePlease if it didnt happen yet but dont test it
            await _client.AdminRemoveUser(_userToRemove.Id);
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

        [Test]
        public async Task AdminRemoveUser()
        {
            var succ = await _client.AdminRemoveUser(_userToRemove.Id);
            Assert.IsTrue(succ.HasValue);
            Assert.IsTrue(succ.Value);
        }

        [Test]
        public async Task AdminRemoveUserThatDoesntExist()
        {
            var succ = await _client.AdminRemoveUser(99999999);
            Assert.False(succ.HasValue);
            Assert.True(succ.HasError);
            Assert.IsTrue(succ.Error.Message.Contains("404"));
        }

        [Test]
        public async Task AdminUpdateUserWithAllPropertiesNull()
        {
            UserUpdateInfo info = new UserUpdateInfo();
            var ret = await _client.AdminUpdateUser(_userToRename.Id, info);
            Assert.IsTrue(ret.HasError);
            Assert.IsFalse(ret.HasValue);
            Assert.IsTrue(ret.Error is ArgumentNullException);
        }
        
        [Test]
        public async Task AdminUpdateUserUsername()
        {
            UserUpdateInfo info = new UserUpdateInfo()
            {
                Username = "RENAMED"
            };
            var ret = await _client.AdminUpdateUser(_userToRename.Id, info);
            Assert.IsFalse(ret.HasError);
            Assert.IsTrue(ret.HasValue);
            Assert.AreEqual("RENAMED", ret.Value.Username);
        }
        
        [Test]
        public async Task AdminUpdateUserUsernameAndAdmin()
        {
            UserUpdateInfo info = new UserUpdateInfo()
            {
                Username = "RENAMED",
                IsAdmin = true
            };
            var ret = await _client.AdminUpdateUser(_userToRename.Id, info);
            Assert.IsFalse(ret.HasError);
            Assert.IsTrue(ret.HasValue);
            Assert.AreEqual("RENAMED", ret.Value.Username);
            Assert.IsTrue(ret.Value.IsAdmin);
        }
    }
}