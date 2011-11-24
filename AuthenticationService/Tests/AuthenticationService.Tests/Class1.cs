using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Amazon.SimpleDB;
using NUnit.Framework;

namespace AuthenticationService.Tests
{
    [TestFixture]
    public class authentication_tests
    {
        private string _key;
        private string _secret;
        private AmazonSimpleDB _client;
        private UserManager _manager;

        [SetUp]
        public void setup()
        {
            var streamReader = new StreamReader(@"C:\SpecialSuperSecret\elliott.ohara@gmail.com.txt");
            
            var dictionary = new Dictionary<string, string>();
            while (!streamReader.EndOfStream)
            {
                var line = streamReader.ReadLine();
                var keyValue = line.Split(':');
                dictionary.Add(keyValue[0], keyValue[1]);
            }
            _key = dictionary["UserName"];
            _secret = dictionary["Password"];
            _client = Amazon.AWSClientFactory.CreateAmazonSimpleDBClient(_key, _secret);
            _manager = new UserManager(new AuthenticationServiceConfig() {StoreName = "TEST_AUTHENTICATION"}, _client);

        }
        [Test]
        public void create_user()
        {
            _manager.CreateUser(new CreateUserRequest{UserName = "Elliott",Password = "test"});
        }
        [Test]
        public void authenticate_true()
        {
            Assert.True(_manager.Authenticate(new AutheticateRequest {UserName = "Elliott", Password = "test"}));
        }
        [Test]
        public void authenticate_false()
        {
            Assert.False(_manager.Authenticate(new AutheticateRequest{UserName = "Elliott",Password = "nope"}));
        }
        [Test]
        public void add_to_group()
        {
            _manager.AddToGroup(new AddToGroupRequest{UserName = "Elliott",Group = "ops"});
        }
        [Test]
        public void get_groups()
        {
            var respons = _manager.GetGroups(new GetGroupsRequest {UserName = "Elliott"});
            Assert.Contains("Admin",respons.Groups.ToList());
            Assert.Contains("ops", respons.Groups.ToList());
        }

    }
}
