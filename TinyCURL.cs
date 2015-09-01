/*
 * 
 *      Author: ROOMAViX
 * 
 *      Copyright (c) 2015 ROOMAViX. All Rights Reserved.
 * 
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Net.Security;

namespace TinyCURL_002b
{
    class TinyCurl
    {
        public Uri URL;

        public Socket socket;

        public string userAgent;

        public List<string> requestHeaders;

        public string responseHeaders;

        public SslStream ssl;

        public TinyCurl(string url)
        {
            URL = new Uri(url);

            userAgent = "Mozilla/5.0 (Windows NT 6.1) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/44.0.2403.157 Safari/537.36";

            requestHeaders = new List<string>();

            TcpClient tc = new TcpClient(URL.Host, URL.Port);

            ssl = new SslStream(tc.GetStream());

            socket = tc.Client;
        }

        public void SslSendGetRequest()
        {
            if (!ssl.IsAuthenticated)
            {
                ssl.AuthenticateAsClient(URL.Host);
            }
            
            string request =
                "GET " + URL.AbsolutePath + " HTTP/1.1\r\n" +
                "Host: " + URL.Host + "\r\n" +
                "User-Agent: " + userAgent + "\r\n";

            foreach (string header in requestHeaders)
            {
                request += header + "\r\n";
            }

            request += "\r\n";

            ssl.Write(Encoding.ASCII.GetBytes(request));
        }

        public string SslGetResponseHeaders()
        {
            byte[] data;
            string headers = "";
            string check;
            char responseChar;

            while (true)
            {
                data = new byte[1];
                ssl.Read(data, 0, 1);
                responseChar = Encoding.ASCII.GetString(data)[0];
                headers += responseChar;
                if (responseChar == '\r')
                {
                    data = new byte[2];
                    ssl.Read(data, 0, 2);
                    check = Encoding.ASCII.GetString(data);
                    responseChar = check[1];
                    if (responseChar == '\r')
                    {
                        responseHeaders = headers;
                        return headers;
                    }
                    else
                    {
                        headers += check;
                    }
                }
            }
        }

        public void SetUserAgent(string useragent)
        {
            userAgent = useragent;
        }

        public void AddHeader(string header)
        {
            requestHeaders.Add(header);
        }

        public void SendGetRequest()
        {
            string request =
                "GET " + URL.AbsolutePath + " HTTP/1.1\r\n" +
                "Host: " + URL.Host + "\r\n" +
                "User-Agent: " + userAgent + "\r\n";

            foreach (string header in requestHeaders)
            {
                request += header + "\r\n";
            }

            request += "\r\n";

            byte[] data = Encoding.ASCII.GetBytes(request);
            socket.Send(data);
        }

        public string GetResponseHeaders()
        {
            byte[] data;
            string headers = "";
            string check;
            char responseChar;

            while (true)
            {
                data = new byte[1];
                socket.Receive(data);
                responseChar = Encoding.ASCII.GetString(data)[0];
                headers += responseChar;
                if (responseChar == '\r')
                {
                    data = new byte[2];
                    socket.Receive(data);
                    check = Encoding.ASCII.GetString(data);
                    responseChar = check[1];
                    if (responseChar == '\r')
                    {
                        responseHeaders = headers;
                        return headers;
                    }
                    else
                    {
                        headers += check;
                    }
                }
            }
        }

        public bool IsResponseChunked()
        {
            int chunkCheck = responseHeaders.IndexOf("\r\nTransfer-Encoding:");

            if (chunkCheck != -1)
            {
                chunkCheck += 20;

                bool cCharFound = false;

                while (responseHeaders[chunkCheck] != '\r')
                {
                    if (!char.IsWhiteSpace(responseHeaders[chunkCheck]))
                    {
                        if (responseHeaders[chunkCheck] == 'c')
                        {
                            cCharFound = true;
                        }

                        break;
                    }

                    chunkCheck++;
                }

                if (!cCharFound)
                {
                    return false;
                }
                else
                {
                    string tmp = responseHeaders.Substring(chunkCheck).ToLower();

                    if (tmp == "chunked\r")
                    {
                        return true;
                    }
                    else
                        return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
