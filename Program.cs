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

namespace TinyCURL
{
    class Program
    {
        static void Main(string[] args)
        {
            CURL c = new CURL("http://www.google.com");
            Console.WriteLine(c.SendGetRequest());
            Console.Read();
        }
    }
}
