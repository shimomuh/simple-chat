// https://www.npmjs.com/package/ws
//
const https = require('https');
const fs = require('fs');
const WebSocket = require('ws').Server;

const server = https.createServer({
    key: fs.readFileSync("public-static-directory/private-key.pem"),
    cert: fs.readFileSync("public-static-directory/public-key.crt")
}, (req, res) => {
    // ダミーリクエスト処理
    res.writeHead(200);
    res.end("All glory to WebSockets!\n");
}).listen(8080);

const webSocketServer = new WebSocket ({ server });

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

console.log('stand by ready!');
