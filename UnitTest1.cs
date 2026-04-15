using FoodyExamPrep2.Models;
using NUnit.Framework.Constraints;
using RestSharp;
using RestSharp.Authenticators;
using System;
using System.Net;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;

namespace FoodyExamPrep2
{
    [TestFixture]
    public class Tests
    {
        private RestClient client;
        private static string createdFoodId;
        private const string BaseUrl = "http://144.91.123.158:81/";
        private const string StaticToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJKd3RTZXJ2aWNlQWNjZXNzVG9rZW4iLCJqdGkiOiIzNmFkMjRmOC1iOGNmLTRmZDctODFhYS0zNjQ1MzE4N2RiNTkiLCJpYXQiOiIwNC8xNS8yMDI2IDE1OjI0OjQwIiwiVXNlcklkIjoiNzU1NjIyZjEtOTQ2Yy00MDViLTc0NjUtMDhkZTc2OGU4Zjk3IiwiRW1haWwiOiJtaWVsZTIwMjZAZXhhbXBsZS5jb20iLCJVc2VyTmFtZSI6Im1pZWxlMjAyNiIsImV4cCI6MTc3NjI4ODI4MCwiaXNzIjoiRm9vZHlfQXBwX1NvZnRVbmkiLCJhdWQiOiJGb29keV9XZWJBUElfU29mdFVuaSJ9.chgYaD8zgEweB28Dm3Zg6tS1B6utiE9ud5Y_dyFZHS0";
        private const string Email = "miele2026";
        private const string Pass = "miele2026";



        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken;
            if (!string.IsNullOrEmpty(StaticToken))
            {
                jwtToken = StaticToken;
            }
            else
            {
                jwtToken = GetJwtToken(Email, Pass);
            }

            var options = new RestClientOptions(BaseUrl)
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };
            this.client = new RestClient(options);
        }
        private string GetJwtToken(string email, string pass)
        {
            var tempClient = new RestClient(BaseUrl);
            var request = new RestRequest("api/User/Authentication", Method.Post);
            request.AddJsonBody(email, pass);
            var response = tempClient.Execute(request);
            if (response.StatusCode == HttpStatusCode.OK)
            {
                var deserializedResponse = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = deserializedResponse.GetProperty("token").GetString();

                if (string.IsNullOrWhiteSpace(token))
                {
                    throw new InvalidOperationException("Token is missing");
                }
                return token;
            }
            else
            {
                throw new InvalidOperationException("Status different from 200");
            }

        }
        [Order(1)]
        [Test]
        public void CreateNewFoodWithRequiredField()
        {
            var request = new RestRequest("api/Food/Create", Method.Post);
            var body = new FoodDTO { Name = "Test", Description = "This is new food", Url = ""};
            request.AddJsonBody(body);

            var response = this.client.Execute(request);
            var deserializedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Created));
            Assert.That(deserializedResponse,Has.Property("FoodId"));
            createdFoodId = deserializedResponse.FoodId;


        }

        [Order(2)]
        [Test]
        public void EditTitleFoodCreated()
        {
            var request = new RestRequest($"api/Food/Edit/{createdFoodId}", Method.Patch);

            request.AddJsonBody(new[] { 
                new { 
                    path = "/name",
                    op = "replace",
                    value = "New"
                }
            }
            );

            var response = this.client.Execute(request);
            Console.WriteLine(response);
            var deserializedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);
            Console.WriteLine(deserializedResponse);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deserializedResponse.Msg, Is.EqualTo("Successfully edited"));


        }


        [Order(3)]
        [Test]
        public void GetAllFoods()
        {
            var request = new RestRequest("api/Food/All", Method.Get);

            var response = this.client.Execute(request);
            
            var deserializedResponse = JsonSerializer.Deserialize<List<ApiResponseDTO>>(response.Content);
           
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deserializedResponse, Is.Not.Null);
            Assert.That(deserializedResponse, Is.Not.Empty);
        }

        [Order(4)]
        [Test]
        public void DeleteEditedFood()
        {
            var request = new RestRequest($"api/Food/Delete/{createdFoodId}", Method.Delete);

            var response = this.client.Execute(request);

            var deserializedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            Assert.That(deserializedResponse.Msg, Is.EqualTo("Deleted successfully!"));
        }
        [Order(5)]
        [Test]
        public void CreateFoodWithoutRequiredFields()
        {
            var request = new RestRequest("api/Food/Create", Method.Post);
            var body = new FoodDTO { Name = "", Description = "", Url = "" };
            request.AddJsonBody(body);

            var response = this.client.Execute(request);

            var deserializedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }
        [Order(6)]
        [Test]
        public void EditNonExistingFood()
        {
            var request = new RestRequest("api/Food/Edit/-3", Method.Patch);
            request.AddJsonBody(new[] {
                new
            {
                    path = "/name",
                    op = "replace",
                    value = "New name"
            }
                });

            var response = this.client.Execute(request);

            var deserializedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.NotFound));
            Assert.That(deserializedResponse.Msg, Is.EqualTo("No food revues..."));
        }

        [Order(6)]
        [Test]
        public void DeleteNonExistingFood()
        {
            var request = new RestRequest("api/Food/Delete/-3", Method.Delete);

            var response = this.client.Execute(request);

            var deserializedResponse = JsonSerializer.Deserialize<ApiResponseDTO>(response.Content);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            Assert.That(deserializedResponse.Msg, Is.EqualTo("Unable to delete this food revue!"));
        }
        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            this.client.Dispose();
        }
    }
}
