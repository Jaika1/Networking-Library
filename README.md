# [Deprecated, please use this instead!](https://github.com/Jaika1/JaikasToolkit)

# Jaika★'s Networking Library
A UDP/IP networking library based on the client-server model which has been designed to be as easy-to-use as is possible, while also targeting .NET Standard 2.0 by default to enable compatability with the Unity engine by simply dragging the assembly into the assets folder. The library also includes a basic handshake stage involving a pre-defined 'secret' known by both client and server, along with the ability to send unordered, self-recovering packets (Also known as "reliable UDP", [explained here](#using-reliable-packets)).

## Examples and How-To
The examples provided below should give you all the basic information you'll need to implement the library. Every type that you should need access to from this library can be found within the `Jaika1.Networking` namespace, with a reference to `System.Reflection` being required for setup, and `System.Net` for any scenario where you aren't testing on a single machine.

The client and server types from this library both have a fixed buffer size and secret that can be modified. The buffer size should be adjusted if you plan on sending larger chunks of data in 1 packet (the default size for both ends is 1024, of which 12 is always reserved for the packet header, granting 1012 usable bytes by default). The secret is used when a client tries to verify with a server as a basic form of authenticity. The client sends across their secret to the server, the server compares this with their own and if they both match, the handshake succeeds. Because of this, the secret given to your clients and the server should be identical, or the server will refuse the connection. By default, this value is set to 0.

The behaviour of reliable data can be configured using the `MaxResendAttempts`, `DisconnectOnFailedResponse` and `ReliableResendDelay` fields that are exposed on every client and server instance.

### Setting up the server class

To create a server, we will need to create an instance of `Jaika1.Networking.UdpServer`, feed it a list of event handling methods from a `System.Reflection.Assembly` instance, then start the server on the user specified end-point.

Initializing an instance of `Jaika1.Networking.UdpServer` is extremely simple, as every parameter in the types constructor has a default value. You can choose to override these however you'd like to.

```cs
// Default initialization without any changes, giving us a buffer size of 1024 bytes and making our secret 0
UdpServer defaultServer = new UdpServer();

// Server initilization with an expanded buffer, but still the default secret of 0.
UdpServer bufferServer = new UdpServer(bufferSize: 2048);

// Server initialization with a modified secret
UdpServer secretServer = new UdpServer(1234);

// Server initialization with both a modified secret and buffer size
UdpServer modifiedServer = new UdpServer(1234, 2048);
```

Next, you'll need to parse through an assembly reference to this newly created instance, which will internally setup references to methods that will be called when data is received over the network. We will set up these methods later [here](#creating-event-handlers-and-adding-them-to-your-clientserver), but for now while we are setting up our server and client instances, we can just get this ready and out of the way.

```cs
// First parameter is the assembly to search through for said methods
// Second parameter is used to determine which group of events we'll be using (More on this later, default is 0)
server.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 0);
```

Finally, we call a single method to bind the server to a specified end-point and begin listening for data!

```cs
// Call this method if you'd like your binding address to simply be IPAddress.Any along with the specified port. (Used in most cases)
server.StartServer(7235);

// If for some reason you'd like to bind using a different address, there is an override available for that.
server.StartServer(IPAddress.Any, 7235);
```

That should be all the work we need to do to set up our server! Before we conclude this section, it's absolutely worth noting that there are events you can subscribe to for whenever a client connects or disconnects. While these aren't mandatory, they're sure to be extremely useful in most applications. If you'd like to implement these, I'd strongly suggest setting them up ***before*** you start your server. 

```cs
server.ClientConnected += Server_ClientConnected;
server.ClientDisconnected += Server_ClientDisconnected;

...

private static void Server_ClientConnected(UdpClient obj)
{
    // Write your code here! obj will be a reference to the newly verified client.
}

private static void Server_ClientDisconnected(UdpClient obj)
{
    // Write your code here! obj will be a reference to the client who disconnected. (Sending anything to them will obviously result in data going nowhere)
}
```

And that's all the basics to cover with server setup! There where a few instances where we dove a bit deeper into the library there, but hopefully nothing too extreme so far. To help recap all of this, here's the beginnings of a minimalistic application using our library!

```cs
using System;
using System.Reflection;
using System.Threading;
using Jaika1.Networking;

class Program
{
    static void Main()
    {
        // Just a random port I've decided on, you can use whatever you want.
        int port = 7235;

        UdpServer server = new UdpServer();
        server.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 0);
        server.ClientConnected += Server_ClientConnected;
        server.ClientDisconnected += Server_ClientDisconnected;
        server.StartServer(port);
        
        // Halt execution indefinitely so our application doesn't just immediately close.
        Thread.Sleep(-1); 
    }

    static void Server_ClientConnected(UdpClient obj)
    {
        Console.WriteLine($"Client at {obj.IPEndPoint} connected to the server!");
    }

    static void Server_ClientDisconnected(UdpClient obj)
    {
        Console.WriteLine($"Client at {obj.IPEndPoint} disconnected from the server!");
    }
}
```

The next section will cover setting up our client application.

### Setting up the client class

Now that we have our server out of the way, we'll need to create a client to connect to it. It would be pretty lonely without one, after all! Similarly to the server, we will need to create an instance of `Jaika1.Networking.UdpClient`, feed it a list of event handling methods from a `System.Reflection.Assembly` instance, then try to verify with a server at a given remote end-point.

Initializing an instance of `Jaika1.Networking.UdpClient` is virtually identical to `Jaika1.Networking.UdpServer`.

```cs
// Default initialization without any changes, giving us a buffer size of 1024 bytes and making our secret 0
UdpClient defaultClient = new UdpClient();

// Client initilization with an expanded buffer, but still the default secret of 0.
UdpClient bufferClient = new UdpClient(bufferSize: 2048);

// Client initialization with a modified secret
UdpClient secretClient = new UdpClient(1234);

// Client initialization with both a modified secret and buffer size
UdpClient modifiedClient = new UdpClient(1234, 2048);
```

Next, just like with our server, you'll need to parse through an assembly reference to this newly created instance. This will internally setup references to methods that will be called when data is received over the network. Again, we will set up these methods later [here](#creating-event-handlers-and-adding-them-to-your-clientserver), but for now while we are setting up our server and client instances, we can just get this ready and out of the way.

```cs
// First parameter is the assembly to search through for said methods
// Second parameter is used to determine which group of events we'll be using (More on this later, default is 0)
client.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 0);
```

Finally, we call a single method to verify with the server. If this succeeds, we will have successfully begun talking to the server!

```cs
// When fully implemented in a real-world situation, this will be the method you'll use to connect to a remote location.
client.VerifyAndListen(IPAddress.Parse("xxx.xxx.xxx.xxx"), 7235);

// Along with that, this method here is great for testing locally, as it simply attempts to connect to the local machine using the loopback address.
server.VerifyAndListen(7235);
```

With that all said and done, you should now have a client up and running, ready to communicate with a server! Before we conclude this section, it's absolutely worth noting that similarly like our server, there is an event you can subscribe to for when the client is disconnected. Setting this up isn't mandatory, but it's likely you'll find it extremely useful in your applications. If you'd like to implement this, I'd strongly suggest setting it up ***before*** you verify with a server. 

```cs
client.ClientDisconnected += Client_ClientDisconnected;

...

private static void Client_ClientDisconnected(UdpClient obj)
{
    // Write your code here! obj will be a reference to the client for any further reference you may need it for. 
}
```

To once again recap all of what we just went through for those following along, let's now update our minimalistic application so it can talk to itself using the loopback address.

```cs
using System;
using System.Reflection;
using System.Threading;
using Jaika1.Networking;

class Program
{
    static void Main()
    {
        // Just a random port I've decided on, you can use whatever you want.
        int port = 7235;

        UdpServer server = new UdpServer();
        server.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 0);
        server.ClientConnected += Server_ClientConnected;
        server.ClientDisconnected += Server_ClientDisconnected;
        server.StartServer(port);
        
        UdpClient client = new UdpClient();
        client.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 0);
        client.ClientDisconnected += Client_ClientDisconnected;
        client.VerifyAndListen(port);

        // Halt execution indefinitely so our application doesn't just immediately close.
        Thread.Sleep(-1); 
    }

    static void Server_ClientConnected(UdpClient obj)
    {
        Console.WriteLine($"Client at {obj.IPEndPoint} connected to the server!");
    }

    static void Server_ClientDisconnected(UdpClient obj)
    {
        Console.WriteLine($"Client at {obj.IPEndPoint} disconnected from the server!");
    }

    static void Client_ClientDisconnected(UdpClient obj)
    {
        Console.WriteLine($"Client instance has been disconnected from the server!");
    }
}
```

At this point in our application, you should be able to run it and see a message appear in the console informing us that a client has connected to our server! While this is very exciting at first, that excitment will quickly fade as after that, nothing will ever happen. Even if you tried to send data down the line from either server to client or client to server, you'd ultimately still never see anything happening what-so-ever. This is because we need to create methods in our application that deals with sent data so our client and server know how to deal with each specific request, which is exactly what will be covered in the next section. After that, you should be all set to begin effectively using the library!

### Creating event handlers and adding them to your client/server

Before reading through this section, I'd strongly reccommend that you should already have a client *and* server ready-to-go and talking to each-other. If you haven't already done that, please look through the [client](#setting-up-the-client-class) and [server](#setting-up-the-server-class) how-to sections first. 

Up to now, setup has been relatively simple! Here is where it may get ever so slightly more confusing at first for some, but all-in-all this shouldn't be too hard to implement, even if you don't fully understand why it works as of now. 

To mark methods as network events to be held by the client or server, we will be using the `Jaika1.Networking.NetDataEventAttribute` attribute. If you've never used attributes before, that's OK! This example should give you enough information to use them for this library.

First, we need to create a ***static*** method, where the ***type of the first parameter*** inherits `Jaika1.Networking.NetBase`. At the moment, the value parsed into here will only be of type `Jaika1.Networking.UdpClient`, so we shall specify that instead (NetBase is used internally for potential future expansion). The methods return type should be void, although you can make it whatever you want without issue, since the library will never touch the result.

```cs
static void PrintPong(UdpClient client)
{
    Console.WriteLine("Pong!");
}
```

Next, we will need to give this method our attribute. Due to how C# works, you can exclude `Attribute` from the type name when declaring it, as is clarified in the code below.

```cs
[NetDataEvent(0, 0)]
static void PrintPong(UdpClient client)
{
    Console.WriteLine("Pong!");
}
```

The first number we place within this attribute is the *event id*. When we send messages to our client or server, we have to specify a number first, which identifies our packet and lets us know what we want to do with it down the other end. Our attribute here is what determines the method to fire on said recieving end when this data arrives. The second number determines the group of events this method shall belong to. To help explain, lets take a look back at the `AddNetEventsFromAssembly` functions we called for our client and server instances. If you recall, we had the option of parsing a number into this method at the end, with the default being zero. That number is what determines which events will be added to that instance and which ones will be ignored based on what group id we give to our attributes. Just like the method that interacts with them, this attribute will also default the group id to 0 if it isn't specified.

To make more sense out of this, lets finally complete our minimalistic application below, then explain what's been added in more detail.

```cs
using System;
using System.Reflection;
using System.Threading;
using Jaika1.Networking;

class Program
{
    static void Main()
    {
        // Just a random port I've decided on, you can use whatever you want.
        int port = 7235;

        UdpServer server = new UdpServer();
        server.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 0);
        server.ClientConnected += Server_ClientConnected;
        server.ClientDisconnected += Server_ClientDisconnected;
        server.StartServer(port);

        UdpClient client = new UdpClient();
        client.AddNetEventsFromAssembly(Assembly.GetExecutingAssembly(), 1); // Take note that we've updated the group ID here to 1!
        client.ClientDisconnected += Client_ClientDisconnected;
        client.VerifyAndListen(port);

        // Send a "dummy" message to all connected clients
        server.Send(0);

        // Halt execution indefinitely so our application doesn't just immediately close.
        Thread.Sleep(-1);
    }

    // Event responding methods from before
    static void Server_ClientConnected(UdpClient obj)
    {
        Console.WriteLine($"Client at {obj.IPEndPoint} connected to the server!");
    }

    static void Server_ClientDisconnected(UdpClient obj)
    {
        Console.WriteLine($"Client at {obj.IPEndPoint} disconnected from the server!");
    }

    static void Client_ClientDisconnected(UdpClient obj)
    {
        Console.WriteLine($"Client instance has been disconnected from the server!");
    }

    // Net events for our server and client
    [NetDataEvent(0, 0)]
    static void PrintPong(UdpClient client)
    {
        Console.WriteLine("Pong!");
    }

    [NetDataEvent(0, 1)]
    static void PrintPingAndRespond(UdpClient client)
    {
        Console.WriteLine("Ping!");
        client.Send(0);
    }
}
```

While this is far from the prettiest application in the world, it now demonstrates all the basic functionality required to send a packet over the network from our server to our client(s), then for each client to be able to respond accordingly and process that back on our server.

Take note down the bottom how we've created 2 events, one for our server and the other our client. In the `Main` method after verifying our client, we call the `Send` method on our server, which sends a packet to every client that the server knows about. We send an event with an ID of 0, which just so happens to correspond with the attribute we set up on the `PrintPingAndRespond` method (Taking note that the group ID here is 1 on the attribute, and 1 when we call `AddNetEventsFromAssembly` on our client)! At the end of this method, it calls `Send` with an event ID of 0 on the client instance parsed through the function (which for a client event will always be a reference to the client). This is then processed by the server on the recieving end, which will call the `PrintPong` method on the other side (The parsed-in value here for our client will be a reference to the client who sent the message).

As an additional note, you can add parameters to your functions and `Send` calls to parse extra information through down the network! This should be valid for any simple value type, such as numbers, strings and structs. Most reference types such as classes, however, cannot be sent in whole, so you'll need to split up the nessecary data yourself. Reference types that are supported by default are `System.String`, `System.Enum`, and `System.Array` in cases where the array is one-dimensional.

```cs
[NetDataEvent(0, 0)]
static void ExampleMethod1(UdpClient client, int i)
{
    Console.WriteLine($"Received the number {i} from the server!");
    client.Send(0, i, i + 1);
}

[NetDataEvent(0, 1)]
static void ExampleMethod2(UdpClient sender, int i1, int i2)
{
    Console.WriteLine($"Received the numbers {i1} and {i2} from the client!");
}
```

## Extended knowledge

### Using reliable packets
One very useful feature that this library offers is the ability to send data reliably over the network. Any instance that inherits `Jaika1.Networking.NetBase` will have access to a set of fields and methods that grants quick and easy access to these features of the library.

To send reliable data either way, use the `NetBase.SendF()` method. This method has a virtually identical layout to `NetBase.Send()`, along with appropriate overloads to go, with the one addition being the inclusion of a `Jaika1.Networking.PacketFlags flags` parameter. To send reliable data down the network, simply call any overload of the `NetBase.SendF()` function with the `PacketFlags.Reliable` flag set.

***NetBase instance Fields***
 - **`int MaxResendAttempts = 10`** - Determines how many times dropped data will be resent before giving up.
 - **`bool DisconnectOnFailedResponse = true`** - If true, will disconnect the client if the maximum number of resend attempts is met, otherwise just stop sending the packet.
 - **`float ReliableResendDelay = 0.25f`** - The delay before a reliable packet is resent if no response is received.

 ### Simulating packet-loss
 If for any reason you need to simulate packet-loss, it can be done for any `UdpClient` instance by changing the `DropChance` field. A value of `0.0f` means no packets will ever be lost, and `1.0f` means all packets will be lost. *Please note that this functionality is omitted for release builds for optimized performance.*


## W.I.P Features
- Document ***everything*** with XML and create library documentation via the use of doxygen
- Use a `CancellationToken` to forcibly stop all asynchronous tasks when the client and/or server are closed for a cleaner shutdown.
