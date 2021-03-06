﻿/*
Copyright (c) 2007 Ian Obermiller and Hugo Persson 

Permission is hereby granted, free of charge, to any person
obtaining a copy of this software and associated documentation
files (the "Software"), to deal in the Software without
restriction, including without limitation the rights to use,
copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the
Software is furnished to do so, subject to the following
conditions:

The above copyright notice and this permission notice shall be
included in all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
OTHER DEALINGS IN THE SOFTWARE.
*/

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;

namespace Player
{
    public class Input
    {
        private Client client;
        internal const int WM_SIZE = 0x5;
        internal const int WM_KEYDOWN = 0x0100;
        internal const int WM_KEYUP = 0x0101;
        internal const int WM_CHAR = 0x0102;
        internal const int WM_SYSKEYDOWN = 0x0104;
        internal const int WM_SYSKEYUP = 0x0105;

        internal Input(Client c) { client = c; }

        /// <summary>
        /// Sends a string to the client
        /// </summary>
        /// <param name="s"></param>
        public void SendString(string s)
        {
            foreach (var c in s)
                SendKey(Convert.ToInt32(c));
        }

        /// <summary>
        /// Sends a key to the client
        /// </summary>
        /// <param name="key"></param>
        public void SendKey(Keys key)
        {
            SendMessage(WM_KEYDOWN, (int)key, 0);
            SendMessage(WM_CHAR, (int)key, 0);
            SendMessage(WM_KEYUP, (int)key, 0);
        }

        /// <summary>
        /// Sends a key to the client
        /// </summary>
        /// <param name="key"></param>
        public void SendKey(int key)
        {
            SendMessage(WM_KEYDOWN, key, 0);
            SendMessage(WM_CHAR, key, 0);
            SendMessage(WM_KEYUP, key, 0);
        }

        /// <summary>
        /// Wrapper for SendMessage function
        /// </summary>
        /// <param name="MessageId"></param>
        /// <param name="wParam"></param>
        /// <param name="lParam"></param>
        /// <returns></returns>
        public void SendMessage(uint MessageId, int wParam, int lParam)
        {
            WinApi.SendMessage(client.WindowHandle, MessageId, wParam, lParam);
        }

        /// <summary>
        /// Clicks with the mouse somewhere on the screen
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void Click(int x, int y)
        {
            SendMessage(WinApi.WM_LBUTTONUP, 0, 0);
            int lpara = WinApi.MakeLParam(x, y);
            SendMessage(WinApi.WM_LBUTTONDOWN, 0, lpara);
            SendMessage(WinApi.WM_LBUTTONUP, 0, lpara);
        }
    }
}
