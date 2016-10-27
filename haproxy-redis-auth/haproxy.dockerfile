FROM haproxy:1.6
COPY haproxy.cfg /usr/local/etc/haproxy/haproxy.cfg

EXPOSE 80