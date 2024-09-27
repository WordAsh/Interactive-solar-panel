import paho.mqtt.client as mqtt
import time

broker = "127.0.0.1"
port = 1883
keepalive = 60
topic = "rod_length/msg"

client = mqtt.Client()
client.connect(broker,port)

send_count = 0
while True:
    msg = f"This is the {send_count} message."
    result = client.publish(topic,msg)
    status = result[0]
    if status == 0:
        print(f"Send '{msg}' to topic '{topic}'")
    else:
        print(f"Failed to send message to topic {topic}")
    send_count += 1
    time.sleep(1)