using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ModBus.Net.FBox
{
    public enum SignalRServer
    {
        FBoxServer = 0,
        DelianServer = 1
    }

    public class Constants
    {     
        public SignalRServer SignalRServer = SignalRServer.FBoxServer;

        public string BaseAddress
        {
            get
            {
                switch (SignalRServer)
                {
                    case SignalRServer.FBoxServer:
                    {
                        return "https://account.flexem.com/core";
                    }
                    case SignalRServer.DelianServer:
                    {
                        return "https://id.data.hzdelian.com/core";
                    }
                    default:
                    {
                        return "https://account.flexem.com/core";
                    }
                }
            }
        }

        public string AuthorizeEndpoint
        {
            get { return BaseAddress + "/connect/authorize"; }
        }

        public string LogoutEndpoint
        {
            get { return BaseAddress + "/connect/endsession"; }
        }

        public string TokenEndpoint
        {
            get { return BaseAddress + "/connect/token"; }
        }

        public string UserInfoEndpoint
        {
            get {return BaseAddress + "/connect/userinfo"; }
        }

        public string IdentityTokenValidationEndpoint
        {
            get {return BaseAddress + "/connect/identitytokenvalidation"; }
        }

        public string TokenRevocationEndpoint
        {
            get {return BaseAddress + "/connect/revocation"; } 
        } 

        public string AspNetWebApiSampleApi
        {
            get
            {
                switch (SignalRServer)
                {
                    case SignalRServer.FBoxServer:
                    {
                        return "http://fbox360.com/api/client/";
                    }
                    case SignalRServer.DelianServer:
                    {
                        return "http://wl.data.hzdelian.com/api/client/";
                    }
                    default:
                    {
                        return "http://fbox360.com/api/client/";
                    }
                }
            }
        } 

    }
}
