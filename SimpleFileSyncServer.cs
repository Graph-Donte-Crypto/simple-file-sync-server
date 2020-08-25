using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Simple;

namespace Simple.FileSyncServer {
	class ClientHandler {
		SocketHelper socketHelper;

		public void get() {

			string path_source = socketHelper.receiveMessage();

			byte[] file = File.ReadAllBytes(path_source);

			socketHelper.sendBytesMessage(file);
		}

		public string replaceToDirectorySeparator(string path) {
			return Path.DirectorySeparatorChar == '/' ? path.Replace('\\', '/') : path.Replace('/', '\\');
		}

		public string getFullPath(string full_name) {
			return full_name.Substring(0, full_name.LastIndexOf(Path.DirectorySeparatorChar));
		}

		public void send() {
			
			string path_target = replaceToDirectorySeparator(socketHelper.receiveMessage());

			while (socketHelper.receiveMessage() == "continue") {
				string file = replaceToDirectorySeparator(socketHelper.receiveMessage());
				file = Path.Combine(path_target, file);

				Console.WriteLine("creating " + file + " ...");

				if (!File.Exists(file)) {

					string directory = getFullPath(file);

					if (!Directory.Exists(directory))
						Directory.CreateDirectory(directory);

					socketHelper.sendMessage("ok");

					byte[] bytes = socketHelper.receiveBytesMessage();
					FileStream stream = File.Create(file);

					stream.Write(bytes, 0, bytes.Length);
					stream.Close();
				}
				else {
					string message = "File alredy exist";
					socketHelper.sendMessage(message);
					Console.WriteLine(message);
				}
			}

			/*
			string path = socketHelper.receiveMessage();

			DirectoryInfo directory = new DirectoryInfo(path);

			if (!directory.Exists)
				directory.Create();

			string target = socketHelper.receiveMessage();

			Directory.SetCurrentDirectory(path);

			FileStream stream = new FileInfo(target).Create();

			byte[] file = socketHelper.receiveBytesMessage();

			stream.Write(file, 0, file.Length);
			stream.Close();
			*/
			

			/*
			string path_target = serverHelper.receiveSaveMessage();

			FileStream stream = new FileInfo(path_target).Create();

			byte[] file = serverHelper.receiveSaveBytesMessage();

			stream.Write(file, 0, file.Length);
			stream.Close();
			*/
		}

		public ClientHandler(TcpClient client) {
			//TODO: blet try blet blet
			try {
				socketHelper = new SocketHelper();
				socketHelper.accept(client);

				string command = socketHelper.receiveMessage();

				switch (command) {
					case "get":
					get();
					break;
					case "send":
					send();
					break;
					default:
					break;
				}

				socketHelper.close();
			}
			catch (Exception e) {
				Console.WriteLine(e.Message + "\nStackTrace:\n" + e.StackTrace);
			}
		}
	}

	class FileSyncServer {

		TcpListener Listener;

		void run() {
			//TODO: reaad port from file
			//TODO: make Dictionary "settings"
			Listener = new TcpListener(IPAddress.Any, 11211);
			Listener.Start();

			while (true) {
				Console.WriteLine("Wait to connection...");
				TcpClient client = Listener.AcceptTcpClient();
				Thread thread = new Thread(new ParameterizedThreadStart(ClientThread));
				thread.Start(new ClientInitInfo(client));
			}
		}
		static void Main(string[] args) {
			new FileSyncServer().run();
		}

		public struct ClientInitInfo {
			public TcpClient client;
			public ClientInitInfo(TcpClient client) {
				this.client = client;
			}
		}

		static void ClientThread(Object StateInfo) {
			Console.WriteLine("Connection Accepted");
			ClientInitInfo info = (ClientInitInfo)StateInfo;
			new ClientHandler(info.client);
		}

		~FileSyncServer() {
			if (Listener != null)
				Listener.Stop();
		}
	}
}
