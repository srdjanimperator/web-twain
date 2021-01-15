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
using System.Drawing.Imaging;
using vtortola.WebSockets;

namespace TwainCTSI.WinService
{
    public partial class TwainService : ServiceBase
    {
        string ip = "127.0.0.1";
        int port = 8083;

        WebSocket client;
        WebSocketMessageReadStream stream;

        static TwainSession twain = null;
        const string SAMPLE_SOURCE = "TWAIN2 FreeImage Software Scanner";

        void startWebSocketServer()
        {
            try
            {
                var server = new WebSocketListener(new IPEndPoint(IPAddress.Parse(ip), port));
                server.Standards.RegisterStandard(new WebSocketFactoryRfc6455());
                server.StartAsync();

                eventLog1.WriteEntry("Server has started on " + ip + " :" + port + " , Waiting for a connection...");

                // Trenutno radi samo za 1. klijenta koji se nakaci...
                client = server.AcceptWebSocketAsync(CancellationToken.None).Result;

                eventLog1.WriteEntry("Client connected");

                Loop();
            }
            catch (Exception e)
            {
                eventLog1.WriteEntry("Error starting tcp server!" + e.Message);
                eventLog1.WriteEntry("Error starting tcp server!" + e.StackTrace);
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
                    eventLog1.WriteEntry($"ERROR1: {ex.Message} - {ex.StackTrace}");
                }
            });
        }


        void Loop()
        {
            while (true)
            {
                stream = client?.ReadMessageAsync(CancellationToken.None).Result;
                if (stream?.MessageType == WebSocketMessageType.Text)
                {
                    using (var sr = new StreamReader(stream, Encoding.UTF8))
                    {
                        var msg = sr.ReadToEndAsync().Result;

                        eventLog1.WriteEntry($"Got command: {msg}");
                        SendMessage($"Echo command: {msg}");

                        if (msg.StartsWith("twain_"))
                        {
                            DoTwainWork(msg.Split('_')[1]);
                        }
                    }
                }
            }
        }

        void SendMessage(string msg, bool sleep = false)
        {
            try
            {
                using (var messageWriterStream = client.CreateMessageWriter(WebSocketMessageType.Text))
                {
                    using (var sw = new StreamWriter(messageWriterStream, Encoding.UTF8))
                    {
                        sw.WriteAsync(msg);

                        // Kad je slika salje se dugo, a posto se ne koristi await
                        // Ovo mu je da saceka malo pre nego sto zatvori stream na kraju using-a
                        // Dodato samo da radi
                        if (sleep) Thread.Sleep(2000);
                    }
                }
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry($"Msg send exception: {ex.Message} - {ex.StackTrace}");
            }
        }


        // Navodno ova websocket biblioteka moze da salje i binarne fajlove
        // ali nisam uspeo da namestim, pa se salje base64 string

        //void SendBinaryMessage(Stream bytes)
        //{
        //    try
        //    {
        //        using (var messageWriter = client.CreateMessageWriter(WebSocketMessageType.Binary))
        //        {
        //            bytes.CopyToAsync(messageWriter);
        //            eventLog1.WriteEntry($"Sending file!");
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        eventLog1.WriteEntry($"SendBinaryMessage error: {ex.Message} \n {ex.StackTrace}");
        //    }
        //}

        static string _cmd;
        void DoTwainWork(string cmd)
        {
            _cmd = cmd;
            eventLog1.WriteEntry($"Executin twain command: {cmd}");

            if (twain == null) InitTwain();

            ReturnCode rc;
            try
            {
                if (twain.State < 3)
                {
                    rc = twain.Open();
                    if (rc != ReturnCode.Success)
                    {
                        eventLog1.WriteEntry("Failed to open dsm with rc=" + rc);
                        return;
                    }
                }

                if (cmd.Equals("list"))
                {
                    var response = string.Join(", ", twain.Select(device => device.Name).ToList());
                    SendMessage(response);
                }
                else if (cmd.StartsWith("img"))
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
            }
            catch (Exception ex)
            {
                eventLog1.WriteEntry("Error opening twain: " + ex.Message);
            }
        }

        void InitTwain()
        {
            try
            {
                eventLog1.WriteEntry("Getting ready to do twain stuff on thread " + Thread.CurrentThread.ManagedThreadId);
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
                            Stream imgStream = e.GetNativeImageStream();

                            if (imgStream != null)
                            {
                                if (_cmd == "imglen") SendMessage(imgStream.Length.ToString());
                                else if (_cmd == "img")
                                {
                                    Image img = Image.FromStream(imgStream);

                                    byte[] pngBytes = null;

                                    using (MemoryStream stream = new MemoryStream())
                                    {
                                        img.Save(stream, ImageFormat.Png);
                                        pngBytes = stream.ToArray();
                                        var b64img = Convert.ToBase64String(pngBytes);
                                        eventLog1.WriteEntry($"Lenb64: {b64img.Length}");
                                        SendMessage("data:image/png;base64, " + b64img, true);
                                    }

                                    //Image img = null;
                                    //var stream = e.GetNativeImageStream();
                                    //if (stream != null)
                                    //{
                                    //    img = Image.FromStream(stream, );
                                    //}

                                    //using (var memoryStream = new MemoryStream())
                                    //{
                                    //    imgStream.CopyTo(memoryStream);
                                    //    var imgBytes = memoryStream.ToArray();
                                    //    var b64img = Convert.ToBase64String(imgBytes);

                                    //    SendMessage("data:image/tiff;base64, " + b64img);
                                    //}
                                }
                            }
                            else
                            {
                                SendMessage("Image not found!");
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
