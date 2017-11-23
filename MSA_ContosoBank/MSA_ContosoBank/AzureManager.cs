using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.MobileServices;
using MSA_ContosoBank.DataModel;
using System.Threading.Tasks;

namespace MSA_ContosoBank
{
    public class AzureManager
    {
        private static AzureManager instance;
        private MobileServiceClient client;
        private IMobileServiceTable<User> userTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://contosobankbotservicedata.azurewebsites.net");
            this.userTable = this.client.GetTable<User>();
        }

        public MobileServiceClient AzureClient
        {
            get { return client; }
        }

        public static AzureManager AzureManagerInstance
        {
            get
            {
                if (instance == null)
                {
                    instance = new AzureManager();
                }
                return instance;
            }
        }

        public async Task<List<User>> GetUserInformation()
        {
            return await this.userTable.ToListAsync();
        }
        
        public async Task CreateUser(User user)
        {
            await this.userTable.InsertAsync(user);
        }
    }
}