using NTwain;
using NTwain.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using System.IO;
using System.Drawing;


namespace TwainCTSI.WinService
{
    public partial class TwainService : ServiceBase
    {
        string ip = "127.0.0.1";
        int port = 8083;

        TcpClient client;
        NetworkStream stream;

        void startWebSocketServer()
        {
            try
            {
                var server = new TcpListener(IPAddress.Parse(ip), port);
                server.Start();
                eventLog1.WriteEntry("Server has started on " + ip + " :" + port + " , Waiting for a connection...");

                client = server.AcceptTcpClient();
                stream = client.GetStream();

                Loop();
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Error starting tcp server! " + e.Message);
            }
        }

        public TwainService()
        {
            InitializeComponent();
            eventLog1 = new System.Diagnostics.EventLog();
            if (!System.Diagnostics.EventLog.SourceExists("TWAIN CTSI"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "TWAIN CTSI", "TWAIN Log");
            }
            eventLog1.Source = "TWAIN CTSI";
            eventLog1.Log = "TWAIN Log";
        }

        TwainSession twain;
        const string SAMPLE_SOURCE = "TWAIN2 FreeImage Software Scanner";

        protected override void OnStart(string[] args)
        {
            eventLog1.WriteEntry("TwainService starting...");
            ThreadPool.QueueUserWorkItem(o =>
            {
                try
                {
                    startWebSocketServer();
                }
                catch (Exception ex)
                {
                    eventLog1.WriteEntry("ERROR: " + ex.ToString());
                }
            });
        }


        void Loop()
        {
            while (true)
            {
                while (!stream.DataAvailable) ;
                while (client.Available < 3) ; // match against "get"

                byte[] bytes = new byte[client.Available];
                stream.Read(bytes, 0, client.Available);
                string s = Encoding.UTF8.GetString(bytes);

                if (Regex.IsMatch(s, "^GET", RegexOptions.IgnoreCase))
                {
                    try
                    {
                        //eventLog1.WriteEntry("=====Handshaking from client=====\n" + s);
                        string swk = Regex.Match(s, "Sec-WebSocket-Key: (.*)").Groups[1].Value.Trim();
                        string swka = swk + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
                        byte[] swkaSha1 = System.Security.Cryptography.SHA1.Create().ComputeHash(Encoding.UTF8.GetBytes(swka));
                        string swkaSha1Base64 = Convert.ToBase64String(swkaSha1);

                        byte[] response = Encoding.UTF8.GetBytes(
                            "HTTP/1.1 101 Switching Protocols\r\n" +
                            "Connection: Upgrade\r\n" +
                            "Upgrade: websocket\r\n" +
                            "Sec-WebSocket-Accept: " + swkaSha1Base64 + "\r\n\r\n");

                        stream.Write(response, 0, response.Length);

                        eventLog1.WriteEntry("A client connected.");

                        var res = EncodeOutgoingMessage("Hello there");
                        stream.Write(res, 0, res.Length);

                        DoTwainWork();
                    }
                    catch (Exception ex)
                    {
                        eventLog1.WriteEntry("Error in while loop 1: " + ex.Message + "-" + ex.StackTrace);
                    }
                }
                else
                {
                    try {
                        bool fin = (bytes[0] & 0b10000000) != 0,
                            mask = (bytes[1] & 0b10000000) != 0; // must be true, "All messages from the client to the server have this bit set"

                        int opcode = bytes[0] & 0b00001111, // expecting 1 - text message
                            msglen = bytes[1] - 128, // & 0111 1111
                            offset = 2;

                        if (msglen == 126)
                        {
                            // was ToUInt16(bytes, offset) but the result is incorrect
                            msglen = BitConverter.ToUInt16(new byte[] { bytes[3], bytes[2] }, 0);
                            offset = 4;
                        }
                        else if (msglen == 127)
                        {
                            eventLog1.WriteEntry("TODO: msglen == 127, needs qword to store msglen");
                            // i don't really know the byte order, please edit this
                            // msglen = BitConverter.ToUInt64(new byte[] { bytes[5], bytes[4], bytes[3], bytes[2], bytes[9], bytes[8], bytes[7], bytes[6] }, 0);
                            // offset = 10;
                        }

                        if (msglen == 0)
                            eventLog1.WriteEntry("msglen == 0");
                        else if (mask)
                        {
                            byte[] decoded = new byte[msglen];
                            byte[] masks = new byte[4] { bytes[offset], bytes[offset + 1], bytes[offset + 2], bytes[offset + 3] };
                            offset += 4;

                            for (int i = 0; i < msglen; ++i)
                                decoded[i] = (byte)(bytes[offset + i] ^ masks[i % 4]);

                            string text = Encoding.UTF8.GetString(decoded);
                            eventLog1.WriteEntry(text);
                        }
                        else
                            eventLog1.WriteEntry("mask bit not set");
                    }
                    catch (Exception ex)
                    {
                        eventLog1.WriteEntry("Error in while loop 2: " + ex.Message + "-" + ex.StackTrace);
                    }
                }

            }
        }

        void DoWork()
        {
            try
            {
                DoTwainWork();
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry("ERROR: " + ex.ToString());
            }
        }

        void DoTwainWork()
        {
            try
            {
                eventLog1.WriteEntry("Getting ready to do twain stuff on thread " + Thread.CurrentThread.ManagedThreadId);
                //Thread.Sleep(1000);
                twain = new TwainSession(TWIdentity.CreateFromAssembly(DataGroups.Image, Assembly.GetExecutingAssembly()));
                twain.TransferReady += (s, e) =>
                {
                    eventLog1.WriteEntry("Got xfer ready on thread " + Thread.CurrentThread.ManagedThreadId);
                };
                twain.DataTransferred += (s, e) =>
                {
                    if (e.NativeData != IntPtr.Zero) //else if (!string.IsNullOrWhiteSpace(e.FileDataPath))
                    {
                        eventLog1.WriteEntry("SUCCESS! Got twain data on thread " + Thread.CurrentThread.ManagedThreadId);

                        try
                        {
                            byte[] msg = EncodeOutgoingMessage(e.DataSource.Name);

                            stream.Write(msg, 0, msg.Length);
                            stream.Flush();

                            eventLog1.WriteEntry("Attempting to read the image");

                            Stream imgStr = e.GetNativeImageStream();

                            if (imgStr != null)
                            {
                                eventLog1.WriteEntry("Attempting to read the image 1, length:" + imgStr.Length);

                                var imglen = EncodeOutgoingMessage("Img length:" + imgStr.Length.ToString());
                                stream.Write(imglen, 0, imglen.Length);
                                stream.Flush();

                                using (var memoryStream = new MemoryStream())
                                {
                                    // !!!
                                    imgStr.CopyTo(memoryStream);
                                    var imgBytes = memoryStream.ToArray();
                                    var b64img = Convert.ToBase64String(imgBytes);
                                    var preparedBytes = EncodeOutgoingMessage(b64img);
                                    stream.Write(preparedBytes, 0, preparedBytes.Length);
                                    stream.Flush();
                                    eventLog1.WriteEntry("Attempting to read the image 2");

                                }
                            }
                            else
                            {
                                eventLog1.WriteEntry("Attempting to read the image 3");

                                byte[] msgNoImg = EncodeOutgoingMessage("No image!?");

                                stream.Write(msgNoImg, 0, msgNoImg.Length);
                                stream.Flush();
                            }
                        }
                        catch (Exception ex)
                        {
                            eventLog1.WriteEntry("Error writing response! " + ex.Message + "\n" + ex.StackTrace);
                        }
                    }
                    else
                    {
                        eventLog1.WriteEntry("BUMMER! No twain data on thread " + Thread.CurrentThread.ManagedThreadId);
                    }
                };

                twain.SourceDisabled += (s, e) =>
                {
                    eventLog1.WriteEntry("Source disabled on thread " + Thread.CurrentThread.ManagedThreadId);
                    var rc = twain.CurrentSource.Close();
                    rc = twain.Close();
                };
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry("Error creating twain! " + ex.Message);
            }
            /////////////
            try
            {
                var rc = twain.Open();

                if (rc == ReturnCode.Success)
                {
                    var hit = twain.FirstOrDefault(s => string.Equals(s.Name, SAMPLE_SOURCE));
                    if (hit == null)
                    {
                        eventLog1.WriteEntry("The sample source \"" + SAMPLE_SOURCE + "\" is not installed.");
                        twain.Close();
                    }
                    else
                    {
                        rc = hit.Open();

                        if (rc == ReturnCode.Success)
                        {
                            eventLog1.WriteEntry("Starting capture from the sample source...");
                            rc = hit.Enable(SourceEnableMode.NoUI, false, IntPtr.Zero);
                        }
                        else
                        {
                            twain.Close();
                        }
                    }
                }
                else
                {
                    eventLog1.WriteEntry("Failed to open dsm with rc=" + rc);
                }
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry("Error opening twain: " + ex.Message);
            }
        }

        protected override void OnStop()
        {
            eventLog1.WriteEntry("Service stopped...");
        }

        private byte[] EncodeOutgoingMessage(string text, bool masked = false)
        {
            /* this is how and header should be made:
             *   - first byte  -> FIN + RSV1 + RSV2 + RSV3 + OPCODE
             *   - second byte -> MASK + payload length (only 7 bits)
             *   - third, fourth, fifth and sixth bytes -> (optional) XOR encoding key bytes
             *   - following bytes -> the encoded (if a key has been used) payload
             *
             *   FIN    [1 bit]      -> 1 if the whole message is contained in this frame, 0 otherwise
             *   RSVs   [1 bit each] -> MUST be 0 unless an extension is negotiated that defines meanings for non-zero values
             *   OPCODE [4 bits]     -> defines the interpretation of the carried payload
             *
             *   MASK           [1 bit]  -> 1 if the message is XOR masked with a key, 0 otherwise
             *   payload length [7 bits] -> can be max 1111111 (127 dec), so, the payload cannot be more than 127 bytes per frame
             *
             * valid OPCODES:
             *   - 0000 [0]             -> continuation frame
             *   - 0001 [1]             -> text frame
             *   - 0010 [2]             -> binary frame
             *   - 0011 [3] to 0111 [7] -> reserved for further non-control frames
             *   - 1000 [8]             -> connection close
             *   - 1001 [9]             -> ping
             *   - 1010 [A]             -> pong
             *   - 1011 [B] to 1111 [F] -> reserved for further control frames
             */
            // in our case the first byte will be 10000001 (129 dec = 81 hex).
            // the length is going to be (masked)1 << 7 (OR) 0 + payload length.
            byte[] header = new byte[] { 0x81, (byte)((masked ? 0x1 << 7 : 0x0) + text.Length) };
            // by default the mask array is empty...
            byte[] maskKey = new byte[4];
            if (masked)
            {
                // but if needed, let's create it properly.
                Random rd = new Random();
                rd.NextBytes(maskKey);
            }
            // let's get the bytes of the message to send.
            byte[] payload = Encoding.UTF8.GetBytes(text);
            // this is going to be the whole frame to send.
            byte[] frame = new byte[header.Length + (masked ? maskKey.Length : 0) + payload.Length];
            // add the header.
            Array.Copy(header, frame, header.Length);
            // add the mask if necessary.
            if (maskKey.Length > 0)
            {
                Array.Copy(maskKey, 0, frame, header.Length, maskKey.Length);
                // let's encode the payload using the mask.
                for (int i = 0; i < payload.Length; i++)
                {
                    payload[i] = (byte)(payload[i] ^ maskKey[i % maskKey.Length]);
                }
            }
            // add the payload.
            Array.Copy(payload, 0, frame, header.Length + (masked ? maskKey.Length : 0), payload.Length);
            return frame;
        }
    }
}
