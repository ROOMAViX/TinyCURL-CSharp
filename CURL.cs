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
using System.Net;
using System.Globalization;

namespace TinyCURL
{
    class CURL
    {
        public Uri URL;

        public Socket wSocket;

        public List<string> Headers;

        public CURL(string url)
        {
            Headers = new List<string>();

            URL = new Uri(url);

            TcpClient Client = new TcpClient();

            Client.Connect(URL.Host, URL.Port);

            wSocket = Client.Client;
        }

        public CURL(string url, string[] headers)
            : this(url)
        {
            Headers.AddRange(headers);
        }

        public void AddHeader(string header)
        {
            Headers.Add(header);
        }

        public void AddHeaderList(string[] headers)
        {
            Headers.AddRange(headers);
        }

        private void SendData(string data)
        {
            wSocket.Send(Encoding.ASCII.GetBytes(data));
        }

        private bool IsResponseChunked(string response)
        {
            int chunkCheck = response.IndexOf("Transfer-Encoding:");

            if (chunkCheck != -1)
            {
                chunkCheck += 18;

                bool cCharFound = false;

                while (response[chunkCheck] != '\n')
                {
                    if (!char.IsWhiteSpace(response[chunkCheck]))
                    {
                        if (response[chunkCheck] == 'c' || response[chunkCheck] == 'C')
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
                    string tmp = response.Substring(chunkCheck).ToLower();

                    if (tmp.StartsWith("chunked"))
                    {
                        for (int i = 7; i < tmp.Length; i++)
                        {
                            if (tmp[i] == '\r' && tmp[i + 1] == '\n')
                            {
                                return true;
                            }
                            else if (!char.IsWhiteSpace(tmp[i]))
                            {
                                return false;
                            }
                        }
                        return false;
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

        private string FixChunkedResponse(string response)
        {
            string rawResponse = response.Substring(response.IndexOf("\r\n\r\n") + 4);

            string fixedResponse = "";

            string sTmpChunkLen = "";

            int iTmpChunkLen = 0;

            int i = 0;

            do
            {
                sTmpChunkLen = "";

                while (rawResponse[i] != '\r')
                {
                    sTmpChunkLen += rawResponse[i];

                    i++;
                }

                i += 2;

                iTmpChunkLen = int.Parse(sTmpChunkLen, NumberStyles.HexNumber);

                if (iTmpChunkLen <= 0)
                {
                    break;
                }

                fixedResponse += rawResponse.Substring(i, iTmpChunkLen);

                i += iTmpChunkLen + 2;
            }
            while (iTmpChunkLen > 0);

            return fixedResponse;
        }

        public string SendGetRequest()
        {
            SendData("GET " + URL.AbsolutePath + " HTTP/1.1\r\n");

            SendData("Host: " + URL.Host + "\r\n");

            foreach (string header in Headers)
            {
                SendData(header + "\r\n");
            }

            SendData("\r\n");

            wSocket.Receive(new byte[0]);

            byte[] b = new byte[wSocket.Available];

            wSocket.Receive(b);

            string response = Encoding.ASCII.GetString(b);

            if (IsResponseChunked(response))
            {
                return FixChunkedResponse(response);
            }

            return response;
        }
    }
}
