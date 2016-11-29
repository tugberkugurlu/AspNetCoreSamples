## ASP.NET Core Authentication Behind a Load Balancer

This is a sample application to demonstrate ASP.NET Core authentication behind a load balancer. 
It demonstrates using HAProxy as the load balancer and Redis as the Data Protection key storage.

You can see the related blog post about this sample on my blog: 
[ASP.NET Core Authentication in a Load Balanced Environment with HAProxy and Redis](http://www.tugberkugurlu.com/archive/asp-net-core-authentication-in-a-load-balanced-environment-with-haproxy-and-redis)

### Running the Sample

To run the sample application, you need Docker and Docker Compose installed on your machine. Then, it's just running one simple `docker-compose up` under this path.