using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnalData
{
    public class WebProxyService : System.Net.IWebProxy
    {


        protected System.Net.IWebProxy[] m_proxyList;
        ThreadSafeRandom ThreadSafeRandom { get; set; } = new();

        public System.Net.IWebProxy Proxy
        {
            get
            {
                // https://devblogs.microsoft.com/pfxteam/getting-random-numbers-in-a-thread-safe-way/
                if (this.m_proxyList != null)
                    return this.m_proxyList[ThreadSafeRandom.Next() % this.m_proxyList.Length];

                return System.Net.WebRequest.DefaultWebProxy;
            }
            set
            {
                throw new System.InvalidOperationException("It is not thread-safe to change the proxy-list.");
            }
        }

        System.Net.ICredentials System.Net.IWebProxy.Credentials
        {
            get { return this.Proxy.Credentials; }
            set { this.Proxy.Credentials = value; }
        }


        public WebProxyService()
        {
        } // Constructor 


        public WebProxyService(System.Net.IWebProxy[] proxyList)
        {
            this.m_proxyList = proxyList;
        } // Constructor 


        System.Uri System.Net.IWebProxy.GetProxy(System.Uri destination)
        {
            return this.Proxy.GetProxy(destination);
        }


        bool System.Net.IWebProxy.IsBypassed(System.Uri host)
        {
            return this.Proxy.IsBypassed(host);
        }


    }
}
