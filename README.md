# CLIChat. A simple, CLI chat server and client, written in C#

This project has been made with the intention of learning more about C# and how a client and a server communicates with each other on a deeper level. I hope this repository can serve as a learning resource for other people.


# Features
- Sending messages (obviously)
- Setting an username
- Starting your own server

# Installation
```bash
git clone https://github.com/Shavixinio/CLIChat.git
cd CLIChat
dotnet restore ChatServer
dotnet restore ChatClient
```

# Running the project
Server
```
dotnet run --project ChatServer
```

Client (in another terminal)
```
dotnet run --project ChatClient
```
