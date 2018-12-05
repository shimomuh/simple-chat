const WebSocket = require ('ws').Server;

const webSocketServer = new WebSocket ({ port: 3000 });

webSocketServer.broadcast = (data) => {
  webSocketServer.clients.forEach((client) => {
    client.send(data);
  })
};

// Immutable じゃなさそうなので勝手に拡張

webSocketServer.count = () => {
  let clientNum = 0;
  webSocketServer.clients.forEach((client) => {
    clientNum += 1;
  })
  return clientNum;
}

webSocketServer.on ('connection', (webSocket) => {
  console.log(`connected new client. clients: ${webSocketServer.count()}`);
  webSocket.on('message', (message) => {
    let now = new Date();
    console.log(`${now.toLocaleString()} Received: ${message}`);
    webSocketServer.broadcast(message);
  });
  webSocket.on('close', () => {
    console.log(`close connection. clients: ${webSocketServer.count()}`);
  })
});
