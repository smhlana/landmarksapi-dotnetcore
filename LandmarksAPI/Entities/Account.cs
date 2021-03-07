using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace LandmarksAPI.Entities
{
	public class Account
	{
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }
        [JsonProperty(PropertyName = "title")]
        public string Title { get; set; }
        [JsonProperty(PropertyName = "firstname")]
        public string FirstName { get; set; }
        [JsonProperty(PropertyName = "lastname")]
        public string LastName { get; set; }
        [JsonProperty(PropertyName = "username")]
        public string Username { get; set; }
        [JsonProperty(PropertyName = "passwordhash")]
        public string PasswordHash { get; set; }
        [JsonProperty(PropertyName = "acceptterms")]
        public bool AcceptTerms { get; set; }
        [JsonProperty(PropertyName = "verificationtoken")]
        public string VerificationToken { get; set; }
        [JsonProperty(PropertyName = "verified")]
        public DateTime? Verified { get; set; }
        [JsonProperty(PropertyName = "isverified")]
        public bool IsVerified => Verified.HasValue || PasswordReset.HasValue;
        [JsonProperty(PropertyName = "resettoken")]
        public string ResetToken { get; set; }
        [JsonProperty(PropertyName = "resettokenexpires")]
        public DateTime? ResetTokenExpires { get; set; }
        [JsonProperty(PropertyName = "passwordreset")]
        public DateTime? PasswordReset { get; set; }
        [JsonProperty(PropertyName = "created")]
        public DateTime Created { get; set; }
        [JsonProperty(PropertyName = "updated")]
        public DateTime? Updated { get; set; }
        [JsonProperty(PropertyName = "refreshtokens")]
        public List<RefreshToken> RefreshTokens { get; set; }

        public bool OwnsToken(string token)
        {
            return this.RefreshTokens?.Find(x => x.Token == token) != null;
        }
    }
}
