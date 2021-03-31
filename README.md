# Jaika★'s Networking Library
A UDP-based networking library based on the client-server model which has been designed to be as easy-to-use as is possible, while also targeting .NET Standard 2.0 by default to enable compatability with the Unity engine by simply dragging the assembly into the assets folder. The library also includes a basic handshake stage involving a pre-defined 'secret' known by both the server and client.

## Examples and How-To
The examples provided below should give you all the basic information you'll need to implement the library. Every type that you should need access to from this library can be found within the `NetworkingLibrary` namespace, with a reference to `System.Reflection` being required for setup, and `System.Net` for any scenario where you aren't testing on a single machine.

The client and server types from this library both have a fixed buffer size and secret that can be modified. The buffer size should be adjusted if you plan on sending larger chunks of data in 1 packet (the default size for both ends is 1024). The secret is used when a client tries to verify with a server as a basic form of authenticity. The client sends across their secret to the server, the server compares this with their own and if they both match, the handshake succeeds. Because of this, the secret given to your clients and the server should be identical, or the server will refuse data received from them. By default, this value is set to 0.

### Setting up the server class

To create a server, we will need to create an instance of `NetworkingLibrary.UdpClient`, feed it a list of event handling methods from a `System.Reflection.Assembly` instance, then start the server on the user specified end-point.

Initializing an instance of `NetworkingLibrary.UdpClient` is extremely simple, as every parameter in the types constructor has a default value. You can choose to override these however you'd like to.

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

And that's all the basics to cover with server setup! There where a few instances where we dove a bit deeper into the library there, but hopefully nothing too extreme so far. To help recap all of this, here's the beginnings of a bare-minimum application using our library!

```cs
using System;
using System.Reflection;
using System.Threading;
using NetworkingLibrary;

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



### Creating event handlers and adding them to your client/server

## W.I.P Features
- Document ***everything*** with XML and create library documentation via the use of doxygen
- Finish adding examples to this readme
- Find a way to forcibly stop all asynchronous tasks when the client and server are closed for cleaner shutdown.