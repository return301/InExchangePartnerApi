using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading.Tasks;

namespace InExchangePartnerApi
{
    public class Company
    {

        public string ErpId { get; set; } //unique ID for the registered company
        public string ErpProduct { get; set; } //name-version of ERP system
        public string Name { get; set; } //company legal name
        public string Email { get; set; } //email contact
        public string CountryCode { get; set; } //country code iso3166-alpha2
        public string OrgNo { get; set; } //organizatioal number, digits only
        public string VatNo { get; set; } //VAT Number
        public string[] Processes { get; set; } //list company cabalities, "SendInvoices" and/or "ReceiveInvoices
        public string ApiKey { get; set; } //API Key
        private string ClientToken { get; set; } //temp. store token for company status check

        public Company(string erpId, string erpProduct, string name, string email, string countryCode, string orgNo, string vatNo, params string[] processes)
        {
            ErpId = erpId;
            ErpProduct = erpProduct;
            Name = name;
            Email = email;
            CountryCode = countryCode;
            OrgNo = orgNo;
            VatNo = vatNo;
            Processes = processes;


        }


        public static void Create(Company c, string apiKey)
        {
            /*
            * --- 1. Register company in InExchange ---
            * Only once per company/customer.
            * read documentation of function first, uncomment to activate.
            * https://test.inexchange.se/docs/ApiReference#companies
            */

            string companyRegUrl = "https://testapi.inexchange.se/v1/api/companies/register";

            var companyDetails = new
            {
                ErpId = c.ErpId, 
                ErpProduct = c.ErpProduct, 
                Name = c.Name, 
                Email = c.Email, 
                CountryCode = c.CountryCode, 
                OrgNo = c.OrgNo, 
                VatNo = c.VatNo, 
                IsVatRegistered = "true", //true if VATNo is used
                Processes = c.Processes 
            };



            HttpClient h = new HttpClient();
            h.DefaultRequestHeaders.Add("APIKey", apiKey);
            var resp = h.PostAsJsonAsync(companyRegUrl, companyDetails).Result;


        }

        //creates a token for authentication, used for all calls after company has been registered
        public static async Task<string> CreateClientToken(string erpId, string apiKey)
        {

            string tokUrl = "https://testapi.inexchange.se/v1/api/clienttokens/create";

            HttpClient c = new HttpClient();
            c.DefaultRequestHeaders.Add("APIKey", apiKey);
            var req = new
            {
                ErpId = erpId
            };

            var payload = c.PostAsJsonAsync(tokUrl, req);
            var jsonResp = await payload.Result.Content.ReadFromJsonAsync<Dictionary<string, string>>();
            string? token = jsonResp?.GetValueOrDefault(key: "token");

            Console.WriteLine("Created token: " + token);

            return token;
        }

        //revokes token after each use, or implement a ValidTo-rule in the Create-function
        public static void RevokeClientToken(string clientToken, string apiKey)
        {
            HttpClient c = new HttpClient();
            c.DefaultRequestHeaders.Add("APIKey", apiKey);
            var req = new
            {
                ClientToken = clientToken
            };

            var payload = c.PostAsJsonAsync("https://testapi.inexchange.se/v1/api/clienttokens/revoke", req).Result;
            var resp = payload.Content.ReadAsStringAsync();

            if (payload.IsSuccessStatusCode)
                Console.WriteLine("Revoked token: " + clientToken);
            else
                Console.WriteLine(payload.Content);
        }


    }


}
