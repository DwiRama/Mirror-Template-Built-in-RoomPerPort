using System;
using System.Diagnostics;
using System.Linq;
using System.Net.NetworkInformation;

public static class PortHandler {
    public static int GetRandomAvailablePort(int min, int max, int maxTries = 15)
    {
        // Set the min and max port numbers
        int minPort = min;
        int maxPort = max;

        // Create a random number generator
        System.Random random = new System.Random();

        // Try up to 10 random ports
        for (int i = 0; i < maxTries; i++)
        {
            // Generate a random port number within the specified range
            int port = random.Next(minPort, maxPort + 1);
            if(IsPortAvailable(port))
                return port;
        }

        // All of the ports in the specified range are in use
        throw new Exception("Could not find an available port in the range " + minPort + "-" + maxPort);
    }

    public static bool IsPortAvailable(int port){
        #if !PLATFORM_WEBGL
        var properties = IPGlobalProperties.GetIPGlobalProperties();

        //getting active connections
        var tcpConnectionPorts = properties.GetActiveTcpConnections()
                            .Where(n => n.LocalEndPoint.Port == port)
                            .Select(n => n.LocalEndPoint.Port);


        ////getting active tcp listners - WCF service listening in tcp
        var tcpListenerPorts = properties.GetActiveTcpListeners()
                            .Where(n => n.Port == port)
                            .Select(n => n.Port);

        ////getting active udp listeners
        var udpListenerPorts = properties.GetActiveUdpListeners()
                            .Where(n => n.Port == port)
                            .Select(n => n.Port);

        return !tcpConnectionPorts.Contains(port);
        #else
        var properties = IPGlobalProperties.GetIPGlobalProperties();
        UnityEngine.Debug.Log("Get Active Port : ");
        ////getting active udp listeners
        try{
            var udpListenerPorts = properties.GetActiveUdpListeners()
                                .Where(n => n.Port == port)
                                .Select(n => n.Port);
            return !udpListenerPorts.Contains(port);
        }catch(Exception e){
            UnityEngine.Debug.Log(e.Source);
            return false;
        }
        #endif
    }
}
