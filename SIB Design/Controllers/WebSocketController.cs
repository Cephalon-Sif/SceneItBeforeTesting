using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.WebSockets;
using System.Security;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace WebSocketsSample.Controllers;

#region snippet_Controller_Connect
public class WebSocketController : ControllerBase
{
    //connection to DB
    private static string cStr = "Data Source=(description=(retry_count=20)(retry_delay=3)(address=(protocol=tcps)(port=1521)(host=adb.us-chicago-1.oraclecloud.com))(connect_data=(service_name=gb1892133f78413_sibdb_high.adb.oraclecloud.com))(security=(ssl_server_dn_match=yes)))";

    // List to store all connected clients with their assigned group names/stages/projects
    private static ConcurrentDictionary<int, ClientEntry> allClients = [];
    private static ConcurrentDictionary<string, List<PropPlacements>> stageProps = [];

    [Route("/ws")]
    public async Task Get()
    {
        // Accept the websocket connection
        if (HttpContext.WebSockets.IsWebSocketRequest)
        {
            using var webSocket = await HttpContext.WebSockets.AcceptWebSocketAsync();
            await GameServer(webSocket, HttpContext);
        }
        else
        {
            // Error If not a WebSocket request
            HttpContext.Response.StatusCode = StatusCodes.Status400BadRequest;
        }
    }
    #endregion

    private static async Task GameServer(WebSocket webSocket, HttpContext ctx)
    {
        var buffer = new byte[1024 * 4];
        ClaimsPrincipal username = ctx.User;
        Console.WriteLine("User: " + username?.Identity?.Name ?? "Nobody");
        // Find the current client in the list & If not found, add the client with a default group name "Stage1"
        ClientEntry? me = allClients.FirstOrDefault(c => c.Value.Ws == webSocket).Value;
        if (me == null)
        {
            SecureString pw = new SecureString();
            foreach (char c in "SceneItBeforeDB1") pw.AppendChar(c);
            pw.MakeReadOnly();
            OracleCredential cred = new("SIBUSER", pw);
            OracleConnection myConn = new(cStr, cred);
            myConn.Open();
            me = new(webSocket.GetHashCode().ToString(), "Lobby", webSocket, myConn);
            allClients.TryAdd(me.GetHashCode(), me);
        }

        // Print all connected clients and their unique WebSockets
        foreach (ClientEntry item in allClients.Values)
        {
            Console.WriteLine(item.Stage + " has username " + item.Username);
        }

        OracleCommand cmd = me.Conn.CreateCommand();
        cmd.CommandText = "SELECT COUNT(*) FROM PROJECT_TABLE";
        OracleDataReader rdr = cmd.ExecuteReader();
        while (rdr.Read())
        {
            int cnt = rdr.GetInt32(0);
            Console.WriteLine("There are " + cnt + " rows in the PROJECT table.");
        }
        // Receive an initial message from the WebSocket client
        //ensures clients are connected immediately rather than only after they send a message
        var receiveResult = await webSocket.ReceiveAsync(
            new ArraySegment<byte>(buffer), CancellationToken.None);

        // Continue receiving messages until the client requests to close the connection
        while (!receiveResult.CloseStatus.HasValue)
        {
            bool send = true;
            bool roomChange = false;
            string oldStage = string.Empty;
            string msg = System.Text.Encoding.Default.GetString(buffer)[..receiveResult.Count];
            string[] parts = msg.Split('^');
            if ("Hello".Equals(parts[0]))
            {
                me.Username = parts[1];
                send = false;
            }
            else if ("Go".Equals(parts[0]))
            {
                oldStage = me.Stage;
                me.Stage = parts[1];
                send = false;
                roomChange = true;
            }
            else if ("Leave".Equals(parts[0]))
            {
                oldStage = me.Stage;
                me.Stage = "Lobby";
                send = false;
                roomChange = true;
            }
            else if ("Create".Equals(parts[0]))
            {
                List<PropPlacements>? stgPropList;
                if (!stageProps.TryGetValue(me.Stage, out stgPropList))
                {
                    stgPropList = [];
                    while (!stageProps.TryAdd(me.Stage, stgPropList))
                    {
                        Thread.Sleep(10);
                    }
                }
                PropPlacements p = new()
                {
                    Name = parts[1],
                    Xcoord = float.Parse(parts[2]),
                    Ycoord = float.Parse(parts[3]),
                    Zcoord = float.Parse(parts[4]),
                    Xrot = float.Parse(parts[5]),
                    Yrot = float.Parse(parts[6]),
                    Zrot = float.Parse(parts[7]),
                    Wrot = float.Parse(parts[8]),
                    Xscale = float.Parse(parts[9]),
                    Yscale = float.Parse(parts[10]),
                    Zscale = float.Parse(parts[11]),
                };
                stgPropList.Add(p);
            }
            else if ("Delete".Equals(parts[0]))
            {
                List<PropPlacements>? stgPropList;
                if (stageProps.TryGetValue(me.Stage, out stgPropList))
                {
                    PropPlacements? p = stgPropList.FirstOrDefault(p => p.Name.Equals(parts[1]));
                    if (p != null) stgPropList.Remove(p);
                }
            }
            else if ("Move".Equals(parts[0]))
            {
                List<PropPlacements>? stgPropList;
                if (!stageProps.TryGetValue(me.Stage, out stgPropList))
                {
                    stgPropList = [];
                    while (!stageProps.TryAdd(me.Stage, stgPropList))
                    {
                        Thread.Sleep(10);
                    }

                }
                PropPlacements? p = stgPropList.Find(p => p.Name.Equals(parts[1]));
                if (p != null)
                {
                    p.Xcoord = float.Parse(parts[2]);
                    p.Ycoord = float.Parse(parts[3]);
                    p.Zcoord = float.Parse(parts[4]);
                }
            }
            else if ("Rotate".Equals(parts[0]))
            {
                List<PropPlacements>? stgPropList;
                if (!stageProps.TryGetValue(me.Stage, out stgPropList))
                {
                    stgPropList = [];
                    while (!stageProps.TryAdd(me.Stage, stgPropList))
                    {
                        Thread.Sleep(10);
                    }
                }
                PropPlacements? p = stgPropList.Find(p => p.Name.Equals(parts[1]));
                if (p != null)
                {
                    p.Xrot = float.Parse(parts[2]);
                    p.Yrot = float.Parse(parts[3]);
                    p.Zrot = float.Parse(parts[4]);
                    p.Wrot = float.Parse(parts[5]);
                }
            }
            else if ("Scale".Equals(parts[0]))
            {
                List<PropPlacements>? stgPropList;
                if (!stageProps.TryGetValue(me.Stage, out stgPropList))
                {
                    stgPropList = [];
                    while (!stageProps.TryAdd(me.Stage, stgPropList))
                    {
                        Thread.Sleep(10);
                    }
                }
                PropPlacements? p = stgPropList.Find(p => p.Name.Equals(parts[1]));
                if (p != null)
                {
                    p.Xscale = float.Parse(parts[2]);
                    p.Yscale = float.Parse(parts[3]);
                    p.Zscale = float.Parse(parts[4]);
                }
            }

            // Iterate over all clients to broadcast messages to those in the same group/project/stage1
            foreach (ClientEntry them in allClients.Values)
            {
                if (roomChange && them.Stage.Equals(oldStage) && them.Ws != webSocket)
                {
                    await them.Ws.SendAsync(
                        new ArraySegment<byte>(System.Text.Encoding.Default.GetBytes("Join^" + me.Username + "^Left")),
                        receiveResult.MessageType,
                        receiveResult.EndOfMessage,
                        CancellationToken.None);
                }
                if (roomChange && them.Stage.Equals(me.Stage) && them.Ws != webSocket)
                {
                    if (them.Ws.State == WebSocketState.Closed)
                    {
                        allClients.Remove(them.GetHashCode(), out _);
                    }
                    else try
                        {
                            await them.Ws.SendAsync(
                                new ArraySegment<byte>(System.Text.Encoding.Default.GetBytes("Join^" + me.Username + "^Joined")),
                                receiveResult.MessageType,
                                receiveResult.EndOfMessage,
                                CancellationToken.None);
                        }
                        catch (WebSocketException)
                        {
                            allClients.Remove(them.GetHashCode(), out _);
                        }
                }
                if (send && them.Stage.Equals(me.Stage) && them.Ws != webSocket)
                {
                    await them.Ws.SendAsync(
                        new ArraySegment<byte>(buffer, 0, receiveResult.Count),
                        receiveResult.MessageType,
                        receiveResult.EndOfMessage,
                        CancellationToken.None);
                }
            }

            if (roomChange)
            {
                List<PropPlacements>? stgPropList;
                if (!stageProps.TryGetValue(me.Stage, out stgPropList))
                {
                    stgPropList = [];
                    while (!stageProps.TryAdd(me.Stage, stgPropList))
                    {
                        Thread.Sleep(10);
                    }
                }
                foreach (PropPlacements p in stgPropList.AsReadOnly())
                {
                    Console.WriteLine("Stage: " + me.Stage + "  Prop: " + p.Name + "^" + p.Xcoord + "^" + p.Ycoord + "^" + p.Zcoord + "^" + p.Xrot + "^" + p.Yrot + "^" + p.Zrot + "^" + p.Wrot + "^" + p.Xscale + "^" + p.Yscale + "^" + p.Zscale);
                    await me.Ws.SendAsync(
                        new ArraySegment<byte>(System.Text.Encoding.Default.GetBytes("Create^" + p.Name + "^" + p.Xcoord + "^" + p.Ycoord + "^" + p.Zcoord + "^" + p.Xrot + "^" + p.Yrot + "^" + p.Zrot + "^" + p.Wrot + "^" + p.Xscale + "^" + p.Yscale + "^" + p.Zscale)),
                        receiveResult.MessageType,
                        receiveResult.EndOfMessage,
                        CancellationToken.None);
                }
            }

            //Console.Clear();
            foreach (ClientEntry item in allClients.Values)
            {
                Console.WriteLine(item.Stage + " has user " + item.Username);
            }
            foreach (KeyValuePair<string, List<PropPlacements>> e in stageProps.AsReadOnly())
            {
                foreach (PropPlacements p in e.Value.AsReadOnly())
                {
                    Console.WriteLine("Stage: " + e.Key + "  Prop: " + p.Name + "^" + p.Xcoord + "^" + p.Ycoord + "^" + p.Zcoord + "^" + p.Xrot + "^" + p.Yrot + "^" + p.Zrot + "^" + p.Wrot + "^" + p.Xscale + "^" + p.Yscale + "^" + p.Zscale);
                }
            }

            try
            {
                // Receive the next message
                receiveResult = await webSocket.ReceiveAsync(
                    new ArraySegment<byte>(buffer), CancellationToken.None);
            }
            catch
            {
                //await webSocket.CloseAsync(WebSocketCloseStatus.ProtocolError, "Error", CancellationToken.None);
                break;
            }
        }

        // If the client disconnects, find and remove the client from the list
        me = allClients.First(c => c.Value.Ws == webSocket).Value;
        if (me != null)
        {
            me.Conn.Close();
            allClients.Remove(me.GetHashCode(), out _);
            Console.WriteLine(me.Username + " has diconnected");
        }

        // Print the remaining clients after disconnection
        foreach (ClientEntry item in allClients.Values)
        {
            Console.WriteLine(item.Stage + " has user " + item.Username);
        }

        // Close the WebSocket connection
        if (webSocket.State == WebSocketState.Open) {
            await webSocket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Goodbye!",
                CancellationToken.None);
        }
    }
}

internal class ClientEntry
{
    private string username;
    private string stage;
    private WebSocket ws;
    private OracleConnection conn;

    public ClientEntry(string u, string s, WebSocket w, OracleConnection c)
    {
        username = u;
        stage = s;
        ws = w;
        conn = c;
    }

    public string Username { get => username; set => username = value; }
    public string Stage { get => stage; set => stage = value; }
    public WebSocket Ws { get => ws; set => ws = value; }
    public OracleConnection Conn { get => conn; set => conn = value; }
}

internal class PropPlacements
{
    public string Name { get; set; } = string.Empty;
    public float Xcoord { get; set; } = 0;
    public float Ycoord { get; set; } = 0;
    public float Zcoord { get; set; } = 0;
    public float Xrot { get; set; } = 0;
    public float Yrot { get; set; } = 0;
    public float Zrot { get; set; } = 0;
    public float Wrot { get; set; } = 0;
    public float Xscale { get; set; } = 1;
    public float Yscale { get; set; } = 1;
    public float Zscale { get; set; } = 1;

    //introduce scale attributes default 0f
    //mkae ints floats'
    //each prop needs a unique name
}