# HAProxy and Docker Zero Downtime Deployment Sample

This is a sample to demonstrate a deployment scenario where you have no downtime during the deployments.

![](./.media/demo-0.gif)

## Concepts Covered

This sample will cover a deployment scenario where you are deploying a minor or a patch change, which means that the existing clients are going to be compatible with the new changes.

## Structure

 - **lb0**: Will act as the load balancer, exposes port `80` and directs traffic to **api** instances.
 - **api0**: First node of the HTTP API service, exposes port `4000`.
 - **api1**: Second node of the HTTP API service, exposes port `4000`.
 - **api2**: Third node of the HTTP API service, exposes port `4000`.
 - **client0**: An instance of the client simulator.
 - **client1**: An instance of the client simulator.
 - **client2**: An instance of the client simulator.
 - **client3**: An instance of the client simulator.
 - **client4**: An instance of the client simulator.
 
## Running the Sample

### Requirements

 - Docker
 - Docker Compose

### Running

`cd` into the directory where this README file is located and from there, running should be as simple as invoking `docker-compose up` command. Now you have the entire sample up and you should see that the clients are woking as expected by logging the first occurance of a response from a different server.

### Doing the Deployment 

#### Communicating with HAProxy

HAProxy can communicate through a Unix socket. [You can add a Unix socket in your config, then interact with that](http://serverfault.com/a/249336). This is [a list of possible commands](http://cbonte.github.io/haproxy-dconv/configuration-1.5.html#9.2) that you can use. To be able to do this, you need to have `socat` installed:

```
sudo apt-get install socat
```

To take down a node, use [`disable server`](http://cbonte.github.io/haproxy-dconv/configuration-1.5.html#9.2-disable%20server) Unix socket command. This example takes down `api1` server under `api_nodes` backend while assuming the socket is on `./haproxy-data/haproxysock`:

```
echo "disable server api_nodes/api1" | sudo socat stdio ./haproxy-data/haproxysock
``` 

Use `enable server` to bring one up:

```
echo "enable server api_nodes/api1" | sudo socat stdio ./haproxy-data/haproxysock
``` 

A few useful links on this:

 - [haproxy and socat sudo](http://serverfault.com/questions/509934/haproxy-and-socat-sudo)
 - [Simple explanation of the Unix sockets](http://programmers.stackexchange.com/a/135972/22417)
 - [HAProxy doesn't start, can not bind UNIX socket "/run/haproxy/admin.sock"](http://stackoverflow.com/questions/30101075/haproxy-doesnt-start-can-not-bind-unix-socket-run-haproxy-admin-sock)
 - [HAProxy disabled server mode](https://cbonte.github.io/haproxy-dconv/configuration-1.5.html#5.2-disabled)

#### Before the Deployment

Before doing the deployment, run the below command (while being at the path of this README file) to see the current status of the cluster:

```bash
echo "show servers state api_nodes" | sudo socat stdio ./haproxy-data/haproxysock
```

You should see a smilar view to following:

```
3 api_nodes 1 api0 172.17.0.4 2 0 1 1 406 15 3 4 6 0 0 0
3 api_nodes 2 api1 172.17.0.5 2 0 1 1 407 15 3 4 6 0 0 0
3 api_nodes 3 api2 172.17.0.6 2 0 1 1 405 15 3 4 6 0 0 0
3 api_nodes 4 api3 172.17.0.7 0 5 1 1 420 1 0 0 14 0 0 0
3 api_nodes 5 api4 172.17.0.2 0 5 1 1 420 1 0 0 14 0 0 0
3 api_nodes 6 api5 172.17.0.3 0 5 1 1 420 1 0 0 14 0 0 0
```

6th column here shows the operational state of the server (`srv_op_state`, [see](http://www.haproxy.org/download/1.6/doc/management.txt)). `2` indicates [`RUNNING`](https://github.com/haproxy/haproxy/blob/v1.6.0/include/types/server.h#L50) and where `0` means [`STOPPED`](https://github.com/haproxy/haproxy/blob/v1.6.0/include/types/server.h#L48). So, this table of information shows our current server state and we will do a seemless switch to make last 3 servers active and first 3 disabled ([see](https://github.com/haproxy/haproxy/blob/v1.6.0/include/types/server.h#L47-L52) for all options).

#### Deployment Script

In order to simulate a deployment, we will put new servers (which has API 1.1 in them) in rotation and take the old onces out. During these transition, there should not be any connection loss and no request should fail to serve. This is script to handle this deployment simulation:

```bash
echo "enable server api_nodes/api3" | sudo socat stdio ./haproxy-data/haproxysock && \
echo "enable server api_nodes/api4" | sudo socat stdio ./haproxy-data/haproxysock && \
echo "enable server api_nodes/api5" | sudo socat stdio ./haproxy-data/haproxysock && \
echo "disable server api_nodes/api0" | sudo socat stdio ./haproxy-data/haproxysock && \
echo "disable server api_nodes/api1" | sudo socat stdio ./haproxy-data/haproxysock && \
echo "disable server api_nodes/api2" | sudo socat stdio ./haproxy-data/haproxysock
```

Now, running the below command to see the server states:

```bash
echo "show servers state api_nodes" | sudo socat stdio ./haproxy-data/haproxysock
```

We see that `api3`, `api4` and `api5` is now eanbled.

```
3 api_nodes 1 api0 172.17.0.4 0 1 1 1 5 15 3 0 14 0 0 0
3 api_nodes 2 api1 172.17.0.5 0 1 1 1 5 15 3 0 14 0 0 0
3 api_nodes 3 api2 172.17.0.6 0 1 1 1 5 15 3 0 14 0 0 0
3 api_nodes 4 api3 172.17.0.7 2 4 1 1 5 15 3 4 6 0 0 0
3 api_nodes 5 api4 172.17.0.2 2 4 1 1 5 15 3 4 6 0 0 0
3 api_nodes 6 api5 172.17.0.3 2 4 1 1 5 15 3 4 6 0 0 0
```

## Going Further

As you can see here, registering and deregistering the new nodes to the load balancer is handled manually for simplicity purposes here. However, you might consider handling this through service discovery mechanism (lik with [consul](https://www.consul.io/)). Keep in mind that the deployment scenario that you have will effect the way bring down and up the nodes on your load balancer.

This sample doesn't cover [the sticky session concept](http://blog.haproxy.com/2012/03/29/load-balancing-affinity-persistence-sticky-sessions-what-you-need-to-know/), which plays a vital role on web application deployments where you don't want to break the client while it's in the middle of a processing.

This sample doesn't cover deployments which involves a data storage technology.

We also have only one load balancer instance here which is a single point of failure. You might consider having at least 2 load balancers acting as the gateway. This will allow taking one out of the rotation for maintenance and bring it up later.

## Resources

 - [HAProxy 1.6 Docs](http://www.haproxy.org/download/1.6/doc/management.txt)
 - [Load Balancing with HAProxy](https://serversforhackers.com/load-balancing-with-haproxy)
 - [Using HAProxy and Consul for dynamic service discovery on Docker‏](http://sirile.github.io/2015/05/18/using-haproxy-and-consul-for-dynamic-service-discovery-on-docker.html)
 - [How to use Docker Compose to run complex multi container apps on your Raspberry Pi‏](http://blog.hypriot.com/post/docker-compose-nodejs-haproxy/)
 - [How can I remove balanced node from haproxy via command line?](http://serverfault.com/questions/249316/how-can-i-remove-balanced-node-from-haproxy-via-command-line)
 - [Zero Downtime Deployments haproxy docker‏](https://docs.quay.io/solution/zero-downtime-deployments.html)
 - [Automatically adding backend servers to HAProxy using docker-gen](https://dockify.io/haproxy-scale-automatically/)
 - [Question: using haproxy.socket to add new servers](http://haproxy.formilux.narkive.com/1OibZABp/using-haproxy-socket-to-add-new-servers)
 - [Actual Zero-Downtime with HAProxy](https://medium.com/@Drew_Stokes/actual-zero-downtime-with-haproxy-18318578fde6#.k1bx8sruu)
 - [Zero-Downtime Restarts with HAProxy](https://www.igvita.com/2008/12/02/zero-downtime-restarts-with-haproxy/)
 - [Yelp Blog: True Zero Downtime HAProxy Reloads](http://engineeringblog.yelp.com/2015/04/true-zero-downtime-haproxy-reloads.html)
