using System.Net;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Numerics;
using System.Text;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

//app.UseAuthorization();
app.UseWebSockets();
List<Player> players = new List<Player>();
List<WebSocket> sockets = new List<WebSocket>();
Player currentPlayer = null;
app.Map("/", async context =>
{

    if (!context.WebSockets.IsWebSocketRequest)
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
    else
    {

        
        using var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        sockets.Add(webSocket);
        var buffer = new byte[1024 * 10];
        WebSocketReceiveResult result;

        // Loop para manter a conexão aberta e receber mensagens
        while (webSocket.State == WebSocketState.Open)
        {
            // Receber a mensagem
            result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            

            if (result.MessageType == WebSocketMessageType.Text)
            {
                
                // Processar a mensagem recebida
                var receivedMessage = Encoding.ASCII.GetString(buffer, 0, result.Count) ?? null;
                if (receivedMessage != null) {


                    Player player = JsonSerializer.Deserialize<Player>(receivedMessage);
                    currentPlayer = players.Where(x => x.tag == player.tag).FirstOrDefault();
                    
                    if (currentPlayer == null)
                    {
                        players.Add(player);
                    }
                    else
                    {
                        currentPlayer.position = player.position;
                        currentPlayer.positionSprite = player.positionSprite;
                        currentPlayer.sizeFrame = player.sizeFrame;
                        currentPlayer.sizeFrameCanvas = player.sizeFrameCanvas;
                        currentPlayer.time = player.time;
                        currentPlayer.timeMin = player.timeMin;
                        currentPlayer.timeMax = player.timeMax;
                        currentPlayer.image = player.image;
                        //currentPlayer.src = player.src;
                    }
                    var responseMessage = players.Where(x => x.tag != player.tag).ToList();
                    var responseBytes = Encoding.ASCII.GetBytes(JsonSerializer.Serialize(responseMessage));
                   
                    await webSocket.SendAsync(new ArraySegment<byte>(responseBytes), WebSocketMessageType.Text, endOfMessage: true, CancellationToken.None);
                    
                }
            }

            if (result.MessageType == WebSocketMessageType.Close)
            {
                await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                var p = players.Where(x => x.tag == currentPlayer?.tag).FirstOrDefault();
                sockets.Remove(webSocket);
                players.RemoveAll(players => players.tag == p?.tag); // Remover jogador da lista
                currentPlayer = null;
            }

        }
    }


});

await app.RunAsync();




class Player
{
    public Player(string tag, Position position, Position positionSprite,
                  Position positionOffSet, sizeFrame sizeFrame, sizeFrame sizeFrameCanvas, 
                  sizeFrame sizeOffSet, int time, int timeMin, int timeMax, 
                  image image, string src)
    {
        this.tag = tag;
        this.position = position;
        this.positionSprite = positionSprite;
        this.positionOffSet = positionOffSet;
        this.sizeFrame = sizeFrame;
        this.sizeFrameCanvas = sizeFrameCanvas;
        this.sizeOffSet = sizeOffSet;
        this.time = time;
        this.timeMin = timeMin;
        this.timeMax = timeMax;
        this.image = image;
        this.src = src;
    }

    public string tag { get; set; }
    public Position position { get; set; }
    public Position positionSprite { get; set; }
    public Position positionOffSet { get; set; }
    public sizeFrame sizeFrame { get; set; }
    public sizeFrame sizeFrameCanvas { get; set; }
    public sizeFrame sizeOffSet { get; set; }
    public int time { get; set; } 
    public int timeMin { get; set; }
    public int timeMax { get; set; }
    public image image { get; set; }
    public string src {  get; set; }

    //public void atualizaTime(int value)
    //{
    //    if(this.time == null)
    //    {
    //        this.time = 1;
    //    }

    //    this.time += value;
    //}
}

class Position
{
    public float x { get; set; }
    public float y { get; set; }
}
class sizeFrame
{
    public float width { get; set; }
    public float height { get; set; }

}
class image
{
    public bool loaded { get; set; } 
    public string src {  set; get; }
}
