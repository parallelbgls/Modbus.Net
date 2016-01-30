using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Modbus.Net.FBox
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

        public string AuthorizeEndpoint => BaseAddress + "/connect/authorize";

        public string LogoutEndpoint => BaseAddress + "/connect/endsession";

        public string TokenEndpoint => BaseAddress + "/connect/token";

        public string UserInfoEndpoint => BaseAddress + "/connect/userinfo";

        public string IdentityTokenValidationEndpoint => BaseAddress + "/connect/identitytokenvalidation";

        public string TokenRevocationEndpoint => BaseAddress + "/connect/revocation";

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
