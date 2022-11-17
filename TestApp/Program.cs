// See https://aka.ms/new-console-template for more information
using Common.Logging;
using Newtonsoft.Json.Linq;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using System.Linq;

Console.WriteLine("Hello, World!");

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .MinimumLevel.Debug()
    .WriteTo.Logger(l => l
        .Filter.ByExcluding("@l in ['Verbose','Debug','Information'] and SourceContext like 'Makaretu.Dns%'")
        .Filter.ByExcluding("@l in ['Verbose','Debug','Information'] and SourceContext='PeerTalk.SecureCommunication.Noise.Noise'")
        .Filter.ByExcluding("@l in ['Verbose','Debug','Information'] and SourceContext='PeerTalk.Multiplex.Muxer'")
        .Filter.ByExcluding("@l in ['Verbose','Debug','Information'] and SourceContext='PeerTalk.Swarm'")
        .Filter.ByExcluding("@l in ['Verbose','Debug','Information'] and SourceContext='Ipfs.Engine.BlockExchange.Bitswap'")
        .Filter.ByExcluding("@l in ['Verbose','Debug','Information'] and SourceContext='Ipfs.Engine.BlockExchange.Bitswap11'")
        .Filter.ByExcluding("@l in ['Verbose','Debug','Information'] and SourceContext='PeerTalk.Protocols.Identify1'")
        .Filter.ByExcluding("@l in ['Verbose','Debug','Information'] and SourceContext='PeerTalk.PeerManager'")
        .Filter.ByExcluding("@l in ['Verbose','Debug','Information'] and SourceContext='PeerTalk.Protocols.Multistream1'")
        .Filter.ByExcluding("@l in ['Verbose','Debug','Information'] and SourceContext='PeerTalk.Routing.DistributedQuery'")
        .WriteTo.Console(outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}")
    )
    .WriteTo.Logger(l => l
        .Filter.ByIncludingOnly(Matching.FromSource<Ipfs.Engine.BlockExchange.Bitswap>())
        .Filter.ByIncludingOnly(Matching.FromSource<Ipfs.Engine.BlockExchange.Bitswap11>())
        .WriteTo.File("Logs/BitSwap.txt", outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level}] ({SourceContext}.{Method}) {Message}{NewLine}{Exception}")
    )
    .CreateLogger();
LogManager.Adapter = new Common.Logging.Serilog.SerilogFactoryAdapter(Log.Logger);

var engine = new Ipfs.Engine.IpfsEngine("1234".ToArray());
await engine.Config.SetAsync("Addresses.Swarm", JToken.FromObject(new [] { "/ip4/0.0.0.0/tcp/5555" }));

await engine.Bootstrap.AddAsync("/ip4/104.131.131.82/tcp/4001/p2p/QmaCpDMGvV2BGHeYERUEnRQAwe3N8SzbUtfsmvsqQLuvuJ");
await engine.Bootstrap.AddAsync("/ip4/143.244.179.113/tcp/30098/p2p/12D3KooWBxnNvfTnXKvqy7QnYqo7gm32YDMGq4S8RCeuVkek1NAX");
//await engine.Bootstrap.AddAsync("/ip4/127.0.0.1/tcp/4001/p2p/12D3KooWHKJqbSj1TLcTXSbzbBZoURwUxhk5dvjuFEKCxQdL6EH2");
var bs = await engine.Bootstrap.ListAsync();

engine.Start();

//Big buck bunny poster (low availability)
//var cid = await engine.ResolveIpfsPathToCidAsync("/ipfs/QmaK5Y969GeFqcBmu5BAPWgXfwkU9hpQCYJRJyQdYtCBjz");

//Planet Pic from ipfs wiki
var cid = await engine.ResolveIpfsPathToCidAsync("/ipfs/QmUZjmbow1ZyKKCyfz28Fw4tCrffpRpzr7vXEGCwfaywn4");

var addr = await engine.Swarm.AddressesAsync();


var builder = WebApplication.CreateBuilder(args);

var app = builder.Build();

app.MapGet("/", () => "Hello World!");

var webserver = app.RunAsync("http://localhost:3000");

_ = Task.Run(async () =>
{
    while(true)
    {
        var peers = await engine.Swarm.PeersAsync();
        Console.WriteLine($"Peers: {peers.Count()}");
        //foreach (var peer in peers)
        //{
        //    Console.WriteLine($"{peer.ConnectedAddress}");
        //}
        await Task.Delay(5000);
    }
});

await Task.Delay(20000);

var fileToRead = File.OpenRead("Program.cs");

var node = await engine.FileSystem.AddAsync(fileToRead, "Testfile.txt");

var obj = await engine.FileSystem.ReadFileAsync(cid);

var fileStream = File.Create("image.jpeg");
obj.CopyTo(fileStream);
fileStream.Close();


await webserver;