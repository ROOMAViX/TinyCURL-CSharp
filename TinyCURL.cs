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

        public string responseBody;

        public SslStream ssl;

        public TinyCurl(string url)
        {
            URL = new Uri(url);

            userAgent = "TinyCurl/0.0.2b";

            requestHeaders = new List<string>();

            Reset();
        }

        public void Reset()
        {
            TcpClient tc = new TcpClient(URL.Host, URL.Port);

            ssl = new SslStream(tc.GetStream());

            socket = tc.Client;
        }

        public void SslSendGetRequest()
        {
            Reset();

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
            Reset();

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

        public void SendPostRequest(Dictionary<string, string> postdata)
        {
            Reset();
            
            string request =
                "POST " + URL.AbsolutePath + " HTTP/1.1\r\n" +
                "Host: " + URL.Host + "\r\n" +
                "User-Agent: " + userAgent + "\r\n" +
                "Content-Type: application/x-www-form-urlencoded\r\n";

            foreach (string header in requestHeaders)
            {
                request += header + "\r\n";
            }

            string requestBody = "";

            int c = 0;
            int count = postdata.Count;

            foreach (var input in postdata)
            {
                requestBody += input.Key + "=" + input.Value;
                if (c++ < count)
                {
                    requestBody += "&";
                }
            }

            request += "Content-Length: " + requestBody.Length + "\r\n";

            request += "\r\n";

            request += requestBody;

            byte[] data = Encoding.ASCII.GetBytes(request);
            socket.Send(data);
        }

        public void SslSendPostRequest(Dictionary<string, string> postdata)
        {
            Reset();

            if (!ssl.IsAuthenticated)
            {
                ssl.AuthenticateAsClient(URL.Host);
            }

            string request =
                "POST " + URL.AbsolutePath + " HTTP/1.1\r\n" +
                "Host: " + URL.Host + "\r\n" +
                "User-Agent: " + userAgent + "\r\n" +
                "Content-Type: application/x-www-form-urlencoded\r\n";

            foreach (string header in requestHeaders)
            {
                request += header + "\r\n";
            }

            string requestBody = "";

            int c = 0;
            int count = postdata.Count;

            foreach (var input in postdata)
            {
                requestBody += input.Key + "=" + input.Value;
                if (c++ < count)
                {
                    requestBody += "&";
                }
            }

            request += "Content-Length: " + requestBody.Length + "\r\n";

            request += "\r\n";

            request += requestBody;

            byte[] data = Encoding.ASCII.GetBytes(request);
            ssl.Write(data);
        }

        public string SslGetResponseBody()
        {
            SslGetResponseHeaders();

            int contentLength = GetContentLength();

            byte[] data = new byte[contentLength];

            ssl.Read(data, 0, contentLength);

            responseBody = Encoding.UTF8.GetString(data);

            return responseBody;
        }

        public int GetContentLength()
        {
            int lengthCheck = responseHeaders.IndexOf("\r\nContent-Length:");

            if (lengthCheck != -1)
            {
                lengthCheck += 17;

                string sLength = "";

                while (responseHeaders[lengthCheck] != '\r')
                {
                    sLength += responseHeaders[lengthCheck];
                    lengthCheck++;
                }

                int iLength = int.Parse(sLength, System.Globalization.NumberStyles.Integer);

                return iLength;
            }
            return -1;
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
