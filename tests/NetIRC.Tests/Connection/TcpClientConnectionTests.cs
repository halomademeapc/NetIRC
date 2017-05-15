using NetIRC.Connection;
using System.Threading.Tasks;
using Xunit;
using System.IO;
using System.Threading;

namespace NetIRC.Tests.Connection
{
    public class TcpClientConnectionTests : IClassFixture<ConnectionFixture>
    {
        private readonly ConnectionFixture connectionFixture;

        public TcpClientConnectionTests(ConnectionFixture connectionFixture)
        {
            this.connectionFixture = connectionFixture;
        }

        [Fact]
        public async Task WhenConnected_TriggerConnectedEvent()
        {
            var pause = new ManualResetEvent(false);
            var connected = false;

            using (var tcpClient = new TcpClientConnection())
            {
                tcpClient.Connected += (s, e) =>
                {
                    connected = true;
                    pause.Set();
                };
                await tcpClient.ConnectAsync("127.0.0.1", 6667);

                await connectionFixture.TcpListener.AcceptTcpClientAsync();
            }

            Assert.True(pause.WaitOne(500));

            Assert.True(connected);
        }

        [Fact]
        public async Task WhenReceivingData_TriggerDataReceivedEvent()
        {
            var pause = new ManualResetEvent(false);
            var data = "test";
            var dataReceived = string.Empty;

            using (var tcpClient = new TcpClientConnection())
            {
                tcpClient.DataReceived += (s, e) =>
                {
                    dataReceived = e.Data;
                    pause.Set();
                };
                await tcpClient.ConnectAsync("127.0.0.1", 6667);

                using (var server = await connectionFixture.TcpListener.AcceptTcpClientAsync())
                {
                    using (var stream = new StreamWriter(server.GetStream()))
                    {
                        await stream.WriteLineAsync(data);
                        await stream.FlushAsync();
                    }
                }
            }

            Assert.True(pause.WaitOne(500));

            Assert.Equal(data, dataReceived);
        }

        [Fact]
        public async Task WhenSendingData_ServerShouldReceiveIt()
        {
            var data = "test";
            var dataReceived = string.Empty;

            using (var tcpClient = new TcpClientConnection())
            {
                await tcpClient.ConnectAsync("127.0.0.1", 6667);

                using (var server = await connectionFixture.TcpListener.AcceptTcpClientAsync())
                {
                    using (var stream = new StreamReader(server.GetStream()))
                    {
                        await tcpClient.SendAsync(data);
                        dataReceived = await stream.ReadLineAsync();
                    }
                }
            }

            Assert.Equal(data, dataReceived);
        }

        [Fact]
        public async Task WhenServerDisconnects_TrigerDisconnectedEvent()
        {
            var pause = new ManualResetEvent(false);
            var disconnected = false;

            using (var tcpClient = new TcpClientConnection())
            {
                tcpClient.Disconnected += (s, e) =>
                {
                    disconnected = true;
                    pause.Set();
                };
                await tcpClient.ConnectAsync("127.0.0.1", 6667);

                using (var server = await connectionFixture.TcpListener.AcceptTcpClientAsync())
                {
                    // Nothing exiting to do
                }
            }

            Assert.True(pause.WaitOne(5000)); // Is this enough to make Appveyor happy? Let's see

            Assert.True(disconnected);
        }
    }
}
