using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Client;
using System.Linq;

namespace CRMPlugins
{
    public class ServiceContext : OrganizationServiceContext
    {
        public ServiceContext(IOrganizationService service) : base(service) { }

        public IQueryable<Contact> ContactSet
        {
            get { return this.CreateQuery<Contact>(); }
        }

        public IQueryable<Account> AccountSet
        {
            get { return this.CreateQuery<Account>(); }
        }

        public IQueryable<Email> EmailSet
        {
            get { return this.CreateQuery<Email>(); }
        }
    }
}