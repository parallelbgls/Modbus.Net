#region Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
//-----------------------------------------------------------------------------
// Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved
// Web: https://www.technosoftware.com 
// 
// The source code in this file is covered under a dual-license scenario:
//   - Owner of a purchased license: SCLA 1.0
//   - GPL V3: everybody else
//
// SCLA license terms accompanied with this source code.
// See SCLA 1.0: https://technosoftware.com/license/Source_Code_License_Agreement.pdf
//
// GNU General Public License as published by the Free Software Foundation;
// version 3 of the License are accompanied with this source code.
// See https://technosoftware.com/license/GPLv3License.txt
//
// This source code is distributed in the hope that it will be useful, but
// WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY
// or FITNESS FOR A PARTICULAR PURPOSE.
//-----------------------------------------------------------------------------
#endregion Copyright (c) 2011-2023 Technosoftware GmbH. All rights reserved

#region Using Directives
using System;
#endregion

namespace Technosoftware.DaAeHdaClient
{
	/// <summary>
	/// Defines functionality that is common to all OPC servers.
	/// </summary>
	public interface IOpcServer : IDisposable
	{
		/// <summary>
		/// An event to receive server shutdown notifications. This event can be used by the
		/// client so that the server can request that the client should disconnect from the
		/// server.
		/// </summary>
		/// <remarks>
		/// The OpcServerShutdownEvent event will be called when the server needs to
		/// shutdown. The client should release all connections and interfaces for this
		/// server.<br/>
		/// A client which is connected to multiple OPCServers (for example Data access and/or
		/// other servers such as Alarms and events servers from one or more vendors) should
		/// maintain separate shutdown callbacks for each server since any server can shut down
		/// independently of the others.
		/// </remarks>
        event OpcServerShutdownEventHandler ServerShutdownEvent;

		/// <summary>
		/// The locale used in any error messages or results returned to the client.
		/// </summary>
		/// <returns>The locale name in the format "[languagecode]-[country/regioncode]".</returns>
		string GetLocale();

		/// <summary>
		/// Sets the locale used in any error messages or results returned to the client.
		/// </summary>
		/// <param name="locale">The locale name in the format "[languagecode]-[country/regioncode]".</param>
		/// <returns>A locale that the server supports and is the best match for the requested locale.</returns>
		string SetLocale(string locale);

		/// <summary>
		/// Allows the client to optionally register a client name with the server. This is included primarily for debugging purposes. The recommended behavior is that the client set his Node name and EXE name here.
		/// </summary>
		void SetClientName(string clientName);

		/// <summary>
		/// Returns the locales supported by the server
		/// </summary>
		/// <remarks>The first element in the array must be the default locale for the server.</remarks>
		/// <returns>An array of locales with the format "[languagecode]-[country/regioncode]".</returns>
		string[] GetSupportedLocales();

		/// <summary>
		/// Returns the localized text for the specified result code.
		/// </summary>
		/// <param name="locale">The locale name in the format "[languagecode]-[country/regioncode]".</param>
		/// <param name="resultId">The result code identifier.</param>
		/// <returns>A message localized for the best match for the requested locale.</returns>
		string GetErrorText(string locale, OpcResult resultId);
    }

	/// <summary>
	/// A delegate to receive shutdown notifications from the server. This delegate can
	/// be used by the client so that the server can request that the client should disconnect
	/// from the server.
	/// </summary>
	/// <param name="reason">
	/// A text string provided by the server indicating the reason for the shutdown. The
	/// server may pass a null or empty string if no reason is provided.
	/// </param>
    public delegate void OpcServerShutdownEventHandler(string reason);
}
