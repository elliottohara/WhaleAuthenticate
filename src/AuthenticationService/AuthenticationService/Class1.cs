using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Amazon.SimpleDB;
using Amazon.SimpleDB.Model;

namespace AuthenticationService
{
    public class AuthenticationServiceConfig
    {
        public string StoreName { get; set; }
    }
    public class UserManager
    {
        private readonly AuthenticationServiceConfig _config;
        private readonly AmazonSimpleDB _client;

        public UserManager(AuthenticationServiceConfig config, AmazonSimpleDB client)
        {
            _config = config;
            _client = client;
            EnsureDomain();
        }

        private void EnsureDomain()
        {
            _client.CreateDomain(new CreateDomainRequest().WithDomainName(_config.StoreName));
        }

        /// <summary>
        /// Creates a user. 
        /// </summary>
        /// <param name="request"></param>
        public void CreateUser(CreateUserRequest request)
        {
            var putRequest = new PutAttributesRequest().WithDomainName(_config.StoreName)
                .WithItemName(request.UserName)
                .WithAttribute(new ReplaceableAttribute().WithName("Password").WithValue(request.Password))
                ;
            _client.PutAttributes(putRequest);
        }
        public bool Authenticate(AutheticateRequest request)
        {
            var getAttribute =
                new GetAttributesRequest().WithDomainName(_config.StoreName).WithItemName(request.UserName);
            var result = _client.GetAttributes(getAttribute).GetAttributesResult;
            if (result == null) return false;
            return result.Attribute.Any(a => a.Name == "Password" && a.Value == request.Password);
        }
        public void AddToGroup(AddToGroupRequest request)
        {
            var putAttribute = new PutAttributesRequest().WithDomainName(_config.StoreName).WithItemName(
                request.UserName)
                .WithAttribute(new ReplaceableAttribute().WithName("Groups").WithValue(request.Group));
            _client.PutAttributes(putAttribute);
        }
        public GetGroupsResponse GetGroups(GetGroupsRequest request)
        {
            var getAttributesRequest =
                new GetAttributesRequest().WithDomainName(_config.StoreName).WithItemName(request.UserName);
            var results = _client.GetAttributes(getAttributesRequest).GetAttributesResult;
            var groups = new List<String>();
            foreach (var attribute in results.Attribute.Where(a => a.Name == "Groups"))
            {
                groups.Add(attribute.Value);
            }
            return new GetGroupsResponse {Groups = groups, UserName = request.UserName};
        }

                 
    }
    public class GetGroupsRequest
    {
        public string UserName { get; set; }
    }
    public class GetGroupsResponse
    {
        public string UserName { get; set; }
        public IEnumerable<string> Groups { get; set; } 
    }
    public class CreateUserRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class AutheticateRequest
    {
        public string UserName { get; set; }
        public string Password { get; set; }
    }
    public class AddToGroupRequest
    {
        public string UserName { get; set; }
        public string Group { get; set; }
    }
    
}
