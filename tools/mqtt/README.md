# MQTT Tooling

## Client Tools

There are quite a few MQTT tools out there, but some of the most popular are:

- [MQTT Explorer](https://mqtt-explorer.com/)
- [MQTTX CLI](https://mqttx.app/cli)
- [MQTT CLI](https://hivemq.github.io/mqtt-cli/docs/installation/)
- Mosquitto client tools
  - For Linux, you can install them using `sudo apt install mosquitto-clients` without installing the broker.
  - For Windows, these tools are bundled with the broker installer and so cannot be installed separately.

## Mosquitto

For local development and testing, it is recommended to use the lightweight Mosquitto broker. It can be installed and run locally by downloading the appropriate binary from [mosquitto.org](mosquitto.org/download/), but the recommendation here is to run the container version using Docker. A simplified [mosquitto.conf](./mosquitto.conf) is included, but this should only be used for development and testing purposes. See the [documentation](https://mosquitto.org/man/mosquitto-conf-5.html) for setting up proper security, message persistence, logging and other advanced features for production environments.

### Windows

``` shell
docker run --rm -d --name mosquitto -p 1883:1883 -v .\mosquitto.conf:/mosquitto/config/mosquitto.conf eclipse-mosquitto
```

### Linux

``` shell
docker run --rm -d --name mosquitto -p 1883:1883  -v ./mosquitto.conf/:/mosquitto/config.mosquitto.conf eclipse-mosquitto
```

### Inspect container

A useful way to diagnose issues like what config file got mounted, you can open a shell on the container:

``` shell
docker exec -it {containerName or Id} sh
```

### Publish Multiple Messages

A useful capability of the mosquitto_pub tool is to publish messages in bulk from a file or other input. This can do with the following command:

```bash
    cat myMessages.json | mosquitto_pub -t device-registry/incoming -l
```
