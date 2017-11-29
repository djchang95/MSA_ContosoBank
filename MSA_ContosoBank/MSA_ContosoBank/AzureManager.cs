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
        private IMobileServiceTable<User2> userTable;

        private AzureManager()
        {
            this.client = new MobileServiceClient("http://contosobankbotservicedata.azurewebsites.net");
            this.userTable = this.client.GetTable<User2>();
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

        public async Task<List<User2>> GetUserInformation()
        {
            return await this.userTable.ToListAsync();
        }
        
        public async Task CreateUser(User2 user)
        {
            await this.userTable.InsertAsync(user);
        }

        public async Task UpdateUser(User2 user)
        {
            await this.userTable.UpdateAsync(user);
        }

        public async Task DeleteUser(User2 user)
        {
            await this.userTable.DeleteAsync(user);
        }
    }
}