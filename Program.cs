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
using System.IO;

namespace TinyCURL_002b
{
    class Program
    {
        static void Main(string[] args)
        {
            TinyCurl tc = new TinyCurl("http://roomavix.blogspot.com");
            tc.SetUserAgent("Mozilla/5.0");
            tc.SendGetRequest();
            Console.WriteLine(tc.GetResponseHeaders());
            Console.WriteLine(tc.GetResponseHeaders());

            Console.Read();
        }
    }
}
